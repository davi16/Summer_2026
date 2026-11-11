using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class VictoryCinematicDirector : MonoBehaviour
{
    public static VictoryCinematicDirector Instance;

    public static bool PendingOutdoorsCutscene = false;
    public static MenuAndGameManager.PlayerRole CutsceneRole;
    public static string CutsceneHeadlineMessage = "";

    public const float ROBBER_ESCAPE_DURATION = 83.904f;      // 1:23.904
    public const float POLICE_ARREST_DURATION = 107.112f;     // 1:47.112
    public const float BYSTANDER_GOODLIFE_DURATION = 131.544f; // 2:11.544

    private Camera cutsceneCam;
    private GameObject spawnedPropsRoot;
    private List<Light> policeSirenLights = new List<Light>();

    // מערכת הכביש המהיר האינסופי (מונעת התנגשות בבתים ובהרים בזמן הנסיעה)
    private Vector3 highwayOrigin = new Vector3(0f, 550f, 0f);
    private List<Transform> highwaySegments = new List<Transform>();
    private const float SEGMENT_LENGTH = 24f;
    private const int SEGMENT_COUNT = 16;

    void Awake()
    {
        Instance = this;
    }

    public static float GetExactCutsceneDuration(MenuAndGameManager.PlayerRole role)
    {
        switch (role)
        {
            case MenuAndGameManager.PlayerRole.Robber: return ROBBER_ESCAPE_DURATION;
            case MenuAndGameManager.PlayerRole.Police: return POLICE_ARREST_DURATION;
            default: return BYSTANDER_GOODLIFE_DURATION;
        }
    }

    public void StartRoleVictoryCutscene(MenuAndGameManager.PlayerRole role, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(MasterCutsceneRoutine(role, duration));
    }

    private IEnumerator MasterCutsceneRoutine(MenuAndGameManager.PlayerRole role, float duration)
    {
        spawnedPropsRoot = new GameObject("CutsceneTempProps");

        // כיבוי הקפסולה והנשקים של השחקן
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            foreach (Renderer r in playerObj.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled = false;
            }
        }

        if (Camera.main != null) Camera.main.gameObject.SetActive(false);

        GameObject camObj = new GameObject("CinematicDirectorCamera");
        camObj.transform.SetParent(spawnedPropsRoot.transform);
        cutsceneCam = camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>();
        cutsceneCam.fieldOfView = 50f;

        // בניית הכביש הארוך עבור קטעי הנסיעה
        BuildEndlessHighwayStrip();

        GameObject getawayCar = FindVehicleByName("Getaway Vehicle");
        List<GameObject> policeCars = FindAllPoliceCars();

        if (getawayCar != null) getawayCar.SetActive(true);
        foreach (var pc in policeCars)
        {
            if (pc != null)
            {
                pc.SetActive(true);
                AttachFlashingSirenLights(pc.transform);
            }
        }

        StartCoroutine(FlashPoliceSirensRoutine());

        if (role == MenuAndGameManager.PlayerRole.Robber)
        {
            yield return StartCoroutine(PlayRobberPurpleCarEscape(getawayCar, policeCars, duration));
        }
        else if (role == MenuAndGameManager.PlayerRole.Police)
        {
            yield return StartCoroutine(PlayPoliceHandcuffArrest(policeCars, duration));
        }
        else
        {
            yield return StartCoroutine(PlayBystanderGoodLife(getawayCar, policeCars, duration));
        }
    }

    // =========================================================================
    // בניית כביש ארוך ואינסופי (Endless Highway) עם אספלט, סימונים, תאורה ועצים
    // =========================================================================
    private void BuildEndlessHighwayStrip()
    {
        highwaySegments.Clear();
        GameObject highwayRoot = new GameObject("CinematicEndlessHighway");
        highwayRoot.transform.SetParent(spawnedPropsRoot.transform, false);
        highwayRoot.transform.position = highwayOrigin;

        Material asphaltMat = CreateSimpleMaterial(new Color(0.16f, 0.17f, 0.19f));
        Material whiteLineMat = CreateSimpleMaterial(new Color(0.95f, 0.95f, 0.95f));
        Material yellowLineMat = CreateSimpleMaterial(new Color(0.95f, 0.78f, 0.15f));
        Material sidewalkMat = CreateSimpleMaterial(new Color(0.55f, 0.56f, 0.58f));
        Material guardrailMat = CreateSimpleMaterial(new Color(0.72f, 0.75f, 0.78f));
        Material trunkMat = CreateSimpleMaterial(new Color(0.38f, 0.24f, 0.12f));
        Material leavesMat = CreateSimpleMaterial(new Color(0.18f, 0.48f, 0.22f));

        for (int i = 0; i < SEGMENT_COUNT; i++)
        {
            GameObject seg = new GameObject("HighwaySegment_" + i);
            seg.transform.SetParent(highwayRoot.transform, false);
            seg.transform.localPosition = new Vector3(0f, 0f, (i - 2) * SEGMENT_LENGTH);

            // כביש אספלט רחב (14 מטר רוחב)
            CreateHighwayBox("Asphalt", seg.transform, new Vector3(0f, -0.1f, 0f), new Vector3(14f, 0.2f, SEGMENT_LENGTH), asphaltMat);

            // מדרכות וגדרות הפרדה בצדדים
            CreateHighwayBox("SidewalkL", seg.transform, new Vector3(-8.5f, 0.05f, 0f), new Vector3(3f, 0.2f, SEGMENT_LENGTH), sidewalkMat);
            CreateHighwayBox("SidewalkR", seg.transform, new Vector3(8.5f, 0.05f, 0f), new Vector3(3f, 0.2f, SEGMENT_LENGTH), sidewalkMat);
            CreateHighwayBox("GuardrailL", seg.transform, new Vector3(-6.8f, 0.55f, 0f), new Vector3(0.2f, 0.35f, SEGMENT_LENGTH), guardrailMat);
            CreateHighwayBox("GuardrailR", seg.transform, new Vector3(6.8f, 0.55f, 0f), new Vector3(0.2f, 0.35f, SEGMENT_LENGTH), guardrailMat);

            // פסי שוליים צהובים וקו הפרדה לבן מקווקו במרכז הכביש
            CreateHighwayBox("YellowLineL", seg.transform, new Vector3(-6.2f, 0.02f, 0f), new Vector3(0.18f, 0.02f, SEGMENT_LENGTH), yellowLineMat);
            CreateHighwayBox("YellowLineR", seg.transform, new Vector3(6.2f, 0.02f, 0f), new Vector3(0.18f, 0.02f, SEGMENT_LENGTH), yellowLineMat);

            for (int d = -1; d <= 1; d++)
            {
                CreateHighwayBox("CenterDash_" + d, seg.transform, new Vector3(0f, 0.02f, d * 8f), new Vector3(0.22f, 0.02f, 4.2f), whiteLineMat);
            }

            // עמודי תאורה ועצים לאורך הכביש לתחושת מהירות מציאותית
            CreateHighwayBox("LampPostL", seg.transform, new Vector3(-7.4f, 2.5f, 0f), new Vector3(0.15f, 5f, 0.15f), guardrailMat);
            CreateHighwayBox("LampPostR", seg.transform, new Vector3(7.4f, 2.5f, 0f), new Vector3(0.15f, 5f, 0.15f), guardrailMat);

            CreateHighwayTree(seg.transform, new Vector3(-11.5f, 0f, -6f), trunkMat, leavesMat);
            CreateHighwayTree(seg.transform, new Vector3(11.5f, 0f, 6f), trunkMat, leavesMat);

            highwaySegments.Add(seg.transform);
        }
    }

    private void UpdateEndlessHighwaySegments(float leadVehicleZ)
    {
        float totalHighwaySpan = SEGMENT_COUNT * SEGMENT_LENGTH;
        foreach (Transform seg in highwaySegments)
        {
            if (seg == null) continue;
            // אם מקטע כביש נשאר יותר מ-35 מטר מאחורי הרכב, מקפיצים אותו קדימה להמשך הכביש!
            if (leadVehicleZ - seg.position.z > 35f)
            {
                seg.position += new Vector3(0f, 0f, totalHighwaySpan);
            }
        }
    }

    // =========================================================================
    // סרטון 1: השודדים עומדים ליד הרכב הסגול, נעלמים לתוכו, ונוסעים על הכביש הארוך
    // =========================================================================
    private IEnumerator PlayRobberPurpleCarEscape(GameObject purpleCar, List<GameObject> policeCars, float totalDuration)
    {
        if (purpleCar == null)
        {
            purpleCar = CreateFallbackCar(highwayOrigin, new Color(0.55f, 0.1f, 0.75f));
        }

        purpleCar.SetActive(true);
        foreach (Renderer r in purpleCar.GetComponentsInChildren<Renderer>(true)) r.enabled = true;

        Vector3 cityCarPos = purpleCar.transform.position;
        Quaternion cityCarRot = purpleCar.transform.rotation;

        // שליפת שלושת השודדים המקוריים (0, 1, 2) ללא בר חיים
        List<SquadMemberAI> robbers = PrepareAllTeamUnitsForCutscene(SquadMemberAI.Team.Robber);
        HideTeamUnits(SquadMemberAI.Team.Police);

        // העמדת 3 השודדים במרחק סביר וטבעי ליד הרכב הסגול (לפני הכניסה לרכב)
        Vector3[] standOffsets = new Vector3[]
        {
            new Vector3(-2.4f, 0f, 0.6f),  // שודד 0 משמאל לרכב
            new Vector3(2.4f, 0f, 0.6f),   // שודד 1 מימין לרכב
            new Vector3(0f, 0f, -3.4f)     // שודד 2 מאחורי הרכב
        };

        for (int i = 0; i < robbers.Count; i++)
        {
            Vector3 worldStandPos = cityCarPos + (cityCarRot * standOffsets[i % standOffsets.Length]);
            robbers[i].transform.position = worldStandPos;
            robbers[i].transform.LookAt(cityCarPos + Vector3.up * 0.8f);
        }

        List<Transform> fallingGold = SpawnFloatingGoldCoins(cityCarPos + Vector3.up * 2.5f, 28);

        float elapsed = 0f;
        float act1End = 15f; // 0:00 - 0:15: שלושת השודדים עומדים ליד הרכב הסגול ברחוב לפני שנעלמים לתוכו
        float act2End = 58f; // 0:15 - 0:58: מרדף מהיר על הכביש הארוך (ללא בתים או הרים שחוסמים)
        bool transitionedToHighway = false;

        while (elapsed < totalDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            if (elapsed <= act1End)
            {
                // מערכה 1: השודדים עומדים ליד הרכב הסגול במרחק סביר, והמצלמה מקיפה אותם
                UpdateFallingGoldAroundTarget(fallingGold, cityCarPos, dt);

                float angle = elapsed * 22f * Mathf.Deg2Rad;
                Vector3 orbitOffset = new Vector3(Mathf.Sin(angle) * 6.5f, 2.2f, Mathf.Cos(angle) * 6.5f);
                cutsceneCam.transform.position = cityCarPos + orbitOffset;
                cutsceneCam.transform.LookAt(cityCarPos + Vector3.up * 1.1f);
            }
            else
            {
                // ברגע שמתחילה הנסיעה: השודדים נעלמים לתוך הרכב (ללא ראשים שמציצים מהגג!) והרכב עובר לכביש המהיר הארוך!
                if (!transitionedToHighway)
                {
                    transitionedToHighway = true;
                    foreach (var r in robbers)
                    {
                        if (r != null) r.gameObject.SetActive(false);
                    }

                    purpleCar.transform.position = highwayOrigin;
                    purpleCar.transform.rotation = Quaternion.identity;

                    for (int i = 0; i < policeCars.Count; i++)
                    {
                        if (policeCars[i] == null) continue;
                        policeCars[i].SetActive(true);
                        policeCars[i].transform.position = highwayOrigin + new Vector3((i - 1) * 3.2f, 0f, -14f - i * 5f);
                        policeCars[i].transform.rotation = Quaternion.identity;
                    }
                }

                float driveTime = elapsed - act1End;
                float speed = 15f;
                float swerveX = Mathf.Sin(driveTime * 1.8f) * 2.2f;

                purpleCar.transform.position = new Vector3(
                    highwayOrigin.x + swerveX,
                    highwayOrigin.y,
                    purpleCar.transform.position.z + speed * dt
                );
                purpleCar.transform.rotation = Quaternion.Euler(0f, Mathf.Cos(driveTime * 1.8f) * 5f, 0f);

                UpdateEndlessHighwaySegments(purpleCar.transform.position.z);
                UpdateFallingGoldAroundTarget(fallingGold, purpleCar.transform.position, dt);

                if (elapsed <= act2End)
                {
                    // מערכה 2: ניידות המשטרה רודפות אחרי הרכב הסגול על הכביש הארוך
                    for (int i = 0; i < policeCars.Count; i++)
                    {
                        if (policeCars[i] == null) continue;
                        Vector3 targetPolicePos = new Vector3(
                            highwayOrigin.x + (i - 1) * 3.0f + Mathf.Sin(driveTime * 1.5f + i) * 0.8f,
                            highwayOrigin.y,
                            purpleCar.transform.position.z - (12f + i * 5.5f)
                        );
                        policeCars[i].transform.position = Vector3.Lerp(policeCars[i].transform.position, targetPolicePos, dt * 6f);
                        policeCars[i].transform.rotation = Quaternion.identity;
                    }

                    int shotType = Mathf.FloorToInt(driveTime / 9.5f) % 3;
                    if (shotType == 0)
                    {
                        cutsceneCam.transform.position = purpleCar.transform.position + new Vector3(1.8f, 1.5f, 5.5f);
                        cutsceneCam.transform.LookAt(purpleCar.transform.position + new Vector3(0f, 1.0f, -4f));
                    }
                    else if (shotType == 1)
                    {
                        cutsceneCam.transform.position = purpleCar.transform.position + new Vector3(-3.8f, 1.6f, -2.5f);
                        cutsceneCam.transform.LookAt(purpleCar.transform.position + new Vector3(0f, 1.1f, -8f));
                    }
                    else
                    {
                        cutsceneCam.transform.position = purpleCar.transform.position + new Vector3(0f, 6.5f, -9.5f);
                        cutsceneCam.transform.LookAt(purpleCar.transform.position + new Vector3(0f, 1.0f, 6f));
                    }
                }
                else
                {
                    // מערכה 3: הניידות נשארות מאחור והרכב הסגול דוהר קדימה על הכביש הארוך
                    float finaleTime = elapsed - act2End;
                    Vector3 camPos = purpleCar.transform.position + new Vector3(0f, 2.2f + finaleTime * 0.12f, -6.5f - finaleTime * 0.2f);
                    cutsceneCam.transform.position = camPos;
                    cutsceneCam.transform.LookAt(purpleCar.transform.position + new Vector3(0f, 1.2f, 8f));
                }
            }

            yield return null;
        }
    }

    // =========================================================================
    // סרטון 2: אזיקת השודדים והובלתם בניידות על הכביש הארוך (1:47.112)
    // =========================================================================
    private IEnumerator PlayPoliceHandcuffArrest(List<GameObject> policeCars, float totalDuration)
    {
        Vector3 stageCenter = highwayOrigin + new Vector3(0f, 0f, 10f);

        for (int i = 0; i < policeCars.Count; i++)
        {
            if (policeCars[i] == null) continue;
            policeCars[i].SetActive(true);
            float angle = (i - 1) * 35f;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * 6.5f);
            policeCars[i].transform.position = stageCenter + offset;
            policeCars[i].transform.LookAt(stageCenter);
        }

        List<SquadMemberAI> robbers = PrepareAllTeamUnitsForCutscene(SquadMemberAI.Team.Robber);
        List<SquadMemberAI> officers = PrepareAllTeamUnitsForCutscene(SquadMemberAI.Team.Police);

        for (int i = 0; i < robbers.Count; i++)
        {
            Vector3 rPos = stageCenter + new Vector3((i - 1) * 2.2f, 0f, 0f);
            robbers[i].transform.position = rPos;
            robbers[i].transform.LookAt(stageCenter + Vector3.forward * 6f);
            AttachHandcuffsModel(robbers[i].transform);
        }

        for (int i = 0; i < officers.Count; i++)
        {
            Vector3 pPos = stageCenter + new Vector3((i - 1) * 2.2f, 0f, -2.2f);
            officers[i].transform.position = pPos;
            officers[i].transform.LookAt(stageCenter + Vector3.forward * 6f);
        }

        float elapsed = 0f;
        float act1End = 36f;
        float act2End = 72f;
        bool convoyStarted = false;

        while (elapsed < totalDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            if (elapsed <= act1End)
            {
                float angle = elapsed * 14f * Mathf.Deg2Rad;
                Vector3 camOffset = new Vector3(Mathf.Sin(angle) * 6.2f, 1.85f, Mathf.Cos(angle) * 6.2f);
                cutsceneCam.transform.position = stageCenter + camOffset;
                cutsceneCam.transform.LookAt(stageCenter + Vector3.up * 1.1f);
            }
            else if (elapsed <= act2End)
            {
                float walkProgress = (elapsed - act1End) / (act2End - act1End);

                for (int i = 0; i < robbers.Count; i++)
                {
                    GameObject targetCar = (policeCars.Count > 0) ? policeCars[i % policeCars.Count] : null;
                    Vector3 carDoorPos = (targetCar != null) ? (targetCar.transform.position - targetCar.transform.forward * 1.8f) : (stageCenter + Vector3.forward * 5f);

                    if (walkProgress < 0.82f)
                    {
                        robbers[i].transform.position = Vector3.MoveTowards(robbers[i].transform.position, carDoorPos, dt * 1.1f);
                        robbers[i].transform.LookAt(carDoorPos);

                        if (i < officers.Count)
                        {
                            officers[i].transform.position = robbers[i].transform.position - robbers[i].transform.forward * 1.2f;
                            officers[i].transform.rotation = robbers[i].transform.rotation;
                        }
                    }
                    else
                    {
                        robbers[i].gameObject.SetActive(false);
                        if (i < officers.Count) officers[i].gameObject.SetActive(false);
                    }
                }

                cutsceneCam.transform.position = stageCenter + new Vector3(4.5f, 2.4f, -3.5f);
                cutsceneCam.transform.LookAt(stageCenter + Vector3.forward * 2.5f + Vector3.up * 1f);
            }
            else
            {
                // מערכה 3: שיירת הניידות נוסעת על הכביש המהיר הארוך עם השודדים העצורים
                if (!convoyStarted)
                {
                    convoyStarted = true;
                    foreach (var r in robbers) if (r != null) r.gameObject.SetActive(false);
                    foreach (var o in officers) if (o != null) o.gameObject.SetActive(false);

                    for (int i = 0; i < policeCars.Count; i++)
                    {
                        if (policeCars[i] == null) continue;
                        policeCars[i].transform.position = highwayOrigin + new Vector3((i - 1) * 3.0f, 0f, -i * 7f);
                        policeCars[i].transform.rotation = Quaternion.identity;
                    }
                }

                for (int i = 0; i < policeCars.Count; i++)
                {
                    if (policeCars[i] == null) continue;
                    policeCars[i].transform.position += Vector3.forward * (13f * dt);
                }

                if (policeCars.Count > 0 && policeCars[0] != null)
                {
                    UpdateEndlessHighwaySegments(policeCars[0].transform.position.z);
                    cutsceneCam.transform.position = policeCars[0].transform.position + new Vector3(3.2f, 2.2f, 6.5f);
                    cutsceneCam.transform.LookAt(policeCars[0].transform.position + new Vector3(0f, 1.0f, -5f));
                }
            }

            yield return null;
        }
    }

    // =========================================================================
    // סרטון 3: עובר האורח נוסע על הכביש הארוך כשניידות המשטרה מאחוריו והכסף נופל
    // =========================================================================
    private IEnumerator PlayBystanderGoodLife(GameObject luxuryCar, List<GameObject> policeCars, float totalDuration)
    {
        HideTeamUnits(SquadMemberAI.Team.Robber);
        HideTeamUnits(SquadMemberAI.Team.Police);

        if (luxuryCar == null)
        {
            luxuryCar = CreateFallbackCar(highwayOrigin, new Color(0.55f, 0.1f, 0.75f));
        }

        luxuryCar.SetActive(true);
        foreach (Renderer r in luxuryCar.GetComponentsInChildren<Renderer>(true)) r.enabled = true;

        luxuryCar.transform.position = highwayOrigin;
        luxuryCar.transform.rotation = Quaternion.identity;

        for (int i = 0; i < policeCars.Count; i++)
        {
            if (policeCars[i] == null) continue;
            policeCars[i].SetActive(true);
            foreach (Renderer r in policeCars[i].GetComponentsInChildren<Renderer>(true)) r.enabled = true;
        }

        List<Transform> goldRain = SpawnFloatingGoldCoins(highwayOrigin + Vector3.up * 2.5f, 32);

        Light sun = FindAnyObjectByType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.color = new Color(1f, 0.92f, 0.72f);
            sun.intensity = 1.4f;
        }

        float elapsed = 0f;
        float act1End = 45f;
        float act2End = 95f;

        while (elapsed < totalDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;

            if (!luxuryCar.activeSelf) luxuryCar.SetActive(true);

            float swerveX = Mathf.Sin(elapsed * 1.2f) * 1.4f;
            luxuryCar.transform.position = new Vector3(
                highwayOrigin.x + swerveX,
                highwayOrigin.y,
                luxuryCar.transform.position.z + 12f * dt
            );
            luxuryCar.transform.rotation = Quaternion.Euler(0f, Mathf.Cos(elapsed * 1.2f) * 3.5f, 0f);

            UpdateEndlessHighwaySegments(luxuryCar.transform.position.z);

            // ניידות המשטרה המהבהבות נוסעות ברקע מאחורי הרכב של עובר האורח על הכביש הארוך
            for (int i = 0; i < policeCars.Count; i++)
            {
                if (policeCars[i] == null) continue;
                if (!policeCars[i].activeSelf) policeCars[i].SetActive(true);

                policeCars[i].transform.position = new Vector3(
                    highwayOrigin.x + (i - 1) * 3.4f,
                    highwayOrigin.y,
                    luxuryCar.transform.position.z - (11f + (i % 2) * 4f)
                );
                policeCars[i].transform.rotation = Quaternion.identity;
            }

            UpdateFallingGoldAroundTarget(goldRain, luxuryCar.transform.position, dt);

            if (elapsed <= act1End)
            {
                cutsceneCam.transform.position = luxuryCar.transform.position + new Vector3(Mathf.Sin(elapsed * 0.5f) * 1.6f, 1.8f, 6.2f);
                cutsceneCam.transform.LookAt(luxuryCar.transform.position + new Vector3(0f, 1.1f, -3f));
            }
            else if (elapsed <= act2End)
            {
                float orbit = (elapsed - act1End) * 0.65f;
                Vector3 orbitOffset = new Vector3(Mathf.Sin(orbit) * 6.5f, 2.3f, Mathf.Cos(orbit) * 6.5f);
                cutsceneCam.transform.position = luxuryCar.transform.position + orbitOffset;
                cutsceneCam.transform.LookAt(luxuryCar.transform.position + Vector3.up * 1.1f);
            }
            else
            {
                float vipTime = elapsed - act2End;
                cutsceneCam.transform.position = luxuryCar.transform.position + new Vector3(0f, 2.5f + vipTime * 0.08f, 6.8f);
                cutsceneCam.transform.LookAt(luxuryCar.transform.position + new Vector3(0f, 1.1f, -4f));
            }

            yield return null;
        }
    }

    // --- פונקציות עזר ---
    private GameObject CreateHighwayBox(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        Destroy(box.GetComponent<Collider>());
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPos;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = mat;
        return box;
    }

    private void CreateHighwayTree(Transform parent, Vector3 localPos, Material trunkMat, Material leavesMat)
    {
        CreateHighwayBox("Trunk", parent, localPos + new Vector3(0f, 1.2f, 0f), new Vector3(0.45f, 2.4f, 0.45f), trunkMat);
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(crown.GetComponent<Collider>());
        crown.transform.SetParent(parent, false);
        crown.transform.localPosition = localPos + new Vector3(0f, 3.2f, 0f);
        crown.transform.localScale = new Vector3(2.6f, 2.8f, 2.6f);
        crown.GetComponent<Renderer>().sharedMaterial = leavesMat;
    }

    private List<SquadMemberAI> PrepareAllTeamUnitsForCutscene(SquadMemberAI.Team team)
    {
        List<SquadMemberAI> list = new List<SquadMemberAI>();
        foreach (SquadMemberAI unit in FindObjectsByType<SquadMemberAI>(FindObjectsInactive.Include))
        {
            if (unit.myTeam == team) list.Add(unit);
        }

        list.Sort((a, b) => a.squadMemberIndex.CompareTo(b.squadMemberIndex));

        foreach (SquadMemberAI unit in list)
        {
            unit.gameObject.SetActive(true);
            unit.isDead = false;
            unit.enabled = false;

            if (unit.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var ag)) ag.enabled = false;
            if (unit.TryGetComponent<HealthSystem>(out var hp)) hp.enabled = false;

            foreach (Canvas c in unit.GetComponentsInChildren<Canvas>(true)) c.gameObject.SetActive(false);
            foreach (Slider s in unit.GetComponentsInChildren<Slider>(true)) s.gameObject.SetActive(false);

            Animator anim = unit.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.Rebind();
                anim.Update(0f);
                anim.SetBool("IsMoving", false);
                anim.SetBool("IsShooting", false);
            }
        }

        return list;
    }

    private void HideTeamUnits(SquadMemberAI.Team team)
    {
        foreach (SquadMemberAI unit in FindObjectsByType<SquadMemberAI>(FindObjectsInactive.Include))
        {
            if (unit.myTeam == team) unit.gameObject.SetActive(false);
        }
    }

    private void UpdateFallingGoldAroundTarget(List<Transform> coins, Vector3 targetCenter, float dt)
    {
        for (int i = 0; i < coins.Count; i++)
        {
            if (coins[i] == null) continue;

            coins[i].Rotate(90f * dt, 180f * dt, 0f);
            coins[i].position += Vector3.down * 2.2f * dt;

            float horizontalDist = Vector2.Distance(
                new Vector2(coins[i].position.x, coins[i].position.z),
                new Vector2(targetCenter.x, targetCenter.z)
            );

            if (coins[i].position.y < targetCenter.y + 0.1f || horizontalDist > 6.5f)
            {
                coins[i].position = targetCenter + new Vector3(
                    Random.Range(-3.5f, 3.5f),
                    Random.Range(2.4f, 5.2f),
                    Random.Range(-3.5f, 3.5f)
                );
            }
        }
    }

    private void AttachFlashingSirenLights(Transform carTransform)
    {
        GameObject redObj = new GameObject("RedSirenLight");
        redObj.transform.SetParent(carTransform, false);
        redObj.transform.localPosition = new Vector3(-0.4f, 1.9f, 0f);
        Light redL = redObj.AddComponent<Light>();
        redL.type = LightType.Point;
        redL.color = Color.red;
        redL.range = 12f;
        redL.intensity = 5f;
        policeSirenLights.Add(redL);

        GameObject blueObj = new GameObject("BlueSirenLight");
        blueObj.transform.SetParent(carTransform, false);
        blueObj.transform.localPosition = new Vector3(0.4f, 1.9f, 0f);
        Light blueL = blueObj.AddComponent<Light>();
        blueL.type = LightType.Point;
        blueL.color = Color.blue;
        blueL.range = 12f;
        blueL.intensity = 5f;
        policeSirenLights.Add(blueL);
    }

    private IEnumerator FlashPoliceSirensRoutine()
    {
        bool toggle = false;
        while (true)
        {
            toggle = !toggle;
            for (int i = 0; i < policeSirenLights.Count; i++)
            {
                if (policeSirenLights[i] != null)
                {
                    policeSirenLights[i].enabled = (i % 2 == 0) ? toggle : !toggle;
                }
            }
            yield return new WaitForSecondsRealtime(0.18f);
        }
    }

    private void AttachHandcuffsModel(Transform robberTransform)
    {
        GameObject cuffs = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(cuffs.GetComponent<Collider>());
        cuffs.name = "MetalHandcuffs";
        cuffs.transform.SetParent(robberTransform, false);
        cuffs.transform.localPosition = new Vector3(0f, 0.95f, -0.26f);
        cuffs.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        cuffs.transform.localScale = new Vector3(0.16f, 0.22f, 0.16f);

        Renderer r = cuffs.GetComponent<Renderer>();
        r.sharedMaterial = CreateSimpleMaterial(new Color(0.75f, 0.78f, 0.82f));
    }

    private List<Transform> SpawnFloatingGoldCoins(Vector3 center, int count)
    {
        List<Transform> list = new List<Transform>();
        Material goldMat = CreateSimpleMaterial(new Color(1f, 0.82f, 0.1f));

        for (int i = 0; i < count; i++)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(coin.GetComponent<Collider>());
            coin.transform.SetParent(spawnedPropsRoot.transform);
            coin.transform.position = center + new Vector3(Random.Range(-3.5f, 3.5f), Random.Range(0.2f, 4.5f), Random.Range(-3.5f, 3.5f));
            coin.transform.localScale = new Vector3(0.35f, 0.04f, 0.35f);
            coin.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            coin.GetComponent<Renderer>().sharedMaterial = goldMat;
            list.Add(coin.transform);
        }
        return list;
    }

    private GameObject CreateFallbackCar(Vector3 pos, Color color)
    {
        GameObject car = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(car.GetComponent<Collider>());
        car.transform.SetParent(spawnedPropsRoot.transform);
        car.transform.position = pos;
        car.transform.localScale = new Vector3(2.1f, 1.3f, 4.4f);
        car.GetComponent<Renderer>().sharedMaterial = CreateSimpleMaterial(color);
        return car;
    }

    private Material CreateSimpleMaterial(Color c)
    {
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader")
            m = new Material(Shader.Find("Sprites/Default"));
        m.color = c;
        return m;
    }

    private GameObject FindVehicleByName(string partialName)
    {
        HeistAlarm alarm = FindAnyObjectByType<HeistAlarm>();
        if (alarm != null && alarm.getawayVehicleOutside != null)
        {
            return alarm.getawayVehicleOutside;
        }

        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (t.name.Contains(partialName) || t.name.Contains("Getaway")) return t.gameObject;
        }
        return null;
    }

    private List<GameObject> FindAllPoliceCars()
    {
        List<GameObject> cars = new List<GameObject>();

        HeistAlarm alarm = FindAnyObjectByType<HeistAlarm>();
        if (alarm != null && alarm.policeCarsOutside != null && alarm.policeCarsOutside.Length > 0)
        {
            foreach (GameObject c in alarm.policeCarsOutside)
            {
                if (c != null) cars.Add(c);
            }
            if (cars.Count > 0) return cars;
        }

        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (t.name.Contains("Police Car") && (t.parent == null || !t.parent.name.Contains("Police Car")))
            {
                cars.Add(t.gameObject);
            }
        }
        return cars;
    }
}