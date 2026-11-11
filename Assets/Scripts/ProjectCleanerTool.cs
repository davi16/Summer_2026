#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ProjectCleanerTool
{
    [MenuItem("Tools/Clean Unused Project Assets (Keep 3 Scenes Only)")]
    public static void CleanUnusedAssets()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Please exit Play Mode before cleaning the project.", "OK");
            return;
        }

        // 1. איסוף כל הסצנות שמוגדרות ב-Build Settings + חיפוש סצנות Outdoors, Bank, Vault
        HashSet<string> scenePaths = new HashSet<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!string.IsNullOrEmpty(scene.path) && File.Exists(scene.path))
                scenePaths.Add(scene.path);
        }

        string[] allSceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        foreach (string guid in allSceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (fileName == "Outdoors" || fileName == "Bank" || fileName == "Vault" || fileName == "HouseWithChest")
            {
                scenePaths.Add(path);
            }
        }

        if (scenePaths.Count == 0)
        {
            EditorUtility.DisplayDialog("No Scenes Found", "Could not find Outdoors, Bank, or Vault scenes!", "OK");
            return;
        }

        // 2. חישוב כל הקבצים שבהם הסצנות האלו משתמשות (Dependencies)
        string[] scenesArray = new List<string>(scenePaths).ToArray();
        string[] usedDependencies = AssetDatabase.GetDependencies(scenesArray, true);
        HashSet<string> usedFiles = new HashSet<string>(usedDependencies);

        foreach (string s in scenesArray) usedFiles.Add(s);

        // 3. סריקת כל הקבצים בתיקיית Assets וזיהוי קבצים שלא נמצאים בשימוש
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        List<string> unusedAssetPaths = new List<string>();

        foreach (string path in allAssetPaths)
        {
            if (!path.StartsWith("Assets/")) continue;
            if (AssetDatabase.IsValidFolder(path)) continue;

            // שומרים תמיד על כל קובצי הקוד (.cs), השיידרים, ה-Resources והגדרות ה-URP/Input
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".cs" || ext == ".shader" || ext == ".cginc" || ext == ".hlsl" || ext == ".asmdef" || ext == ".inputactions")
                continue;

            if (path.Contains("/Resources/") || path.Contains("/Settings/") || path.Contains("/Editor/") || path.Contains("TextMesh Pro"))
                continue;

            if (!usedFiles.Contains(path))
            {
                unusedAssetPaths.Add(path);
            }
        }

        if (unusedAssetPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Project Is Clean!", "All files in Assets are already in use by your scenes!", "Awesome");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "Clean Unused Assets",
            $"Found {unusedAssetPaths.Count} unused files (models, textures, audio, demo scenes) that are NOT used in your scenes ({string.Join(", ", scenesArray)}).\n\n" +
            "All your C# scripts and active scene assets will be kept safe.\n\n" +
            "Do you want to delete the unused files now?",
            "Yes, Clean Project",
            "Cancel"
        );

        if (!confirm) return;

        // 4. מחיקת הקבצים המיותרים ותיקיות ריקות
        int deletedCount = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string unusedPath in unusedAssetPaths)
            {
                if (AssetDatabase.DeleteAsset(unusedPath))
                {
                    deletedCount++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        RemoveEmptyFolders("Assets");
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Cleanup Complete!", $"Successfully removed {deletedCount} unused files and cleaned empty folders!", "Great!");
    }

    private static void RemoveEmptyFolders(string rootPath)
    {
        foreach (string dir in Directory.GetDirectories(rootPath))
        {
            RemoveEmptyFolders(dir);
            if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
            {
                AssetDatabase.DeleteAsset(dir.Replace("\\", "/"));
            }
        }
    }
}
#endif