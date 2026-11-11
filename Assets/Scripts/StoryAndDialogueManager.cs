using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StoryAndDialogueManager : MonoBehaviour
{
    public static StoryAndDialogueManager Instance;

    private static bool introDialogueShown = false;
    private static bool keyPickedDialogueShown = false;
    private static bool chestLootedDialogueShown = false;
    private static bool bankEnteredDialogueShown = false;

    private GameObject dialogueBoxPanel;
    private TMP_Text speakerNameText;
    private TMP_Text dialogueBodyText;
    private TMP_Text objectiveTrackerText;
    private Coroutine activeDialogueRoutine;
    private int talkCycleIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateStoryUIElementsIfNeeded();
        AssignPersonalNamesToSquadMembers();
        StartCoroutine(TriggerSceneStoryBeatsRoutine());
    }

    void Update()
    {
        if (!MenuAndGameManager.GameHasStarted || Time.timeScale == 0f || HealthSystem.MatchEnded) return;

        UpdateDynamicObjectiveText();

        // בדיקת התקדמות עלילתית אוטומטית (איסוף מפתח / פתיחת תיבה)
        if (KeyPickup.HasKey && !keyPickedDialogueShown)
        {
            keyPickedDialogueShown = true;
            TriggerKeyCollectedDialogue();
        }

        if (PersistentObjectManager.NumGoldCoins > 0 && !chestLootedDialogueShown && SceneManager.GetActiveScene().name != "Bank")
        {
            chestLootedDialogueShown = true;
            TriggerChestOpenedDialogue();
        }

        // לחיצה על מקש T לשיחה יזומה עם חברי הצוות בכל רגע
        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerInteractiveSquadTalk();
        }
    }

    // מעניק אוטומטית לכל חבר צוות את השם העלילתי שלו ב-HealthSystem
    private void AssignPersonalNamesToSquadMembers()
    {
        SquadMemberAI[] units = FindObjectsByType<SquadMemberAI>();
        foreach (SquadMemberAI unit in units)
        {
            HealthSystem hp = unit.GetComponent<HealthSystem>();
            if (hp == null) continue;

            if (unit.myTeam == SquadMemberAI.Team.Robber)
            {
                if (unit.squadMemberIndex == 0) hp.characterDisplayName = "Viktor (Robber Boss)";
                else if (unit.squadMemberIndex == 1) hp.characterDisplayName = "Daniel (Your Younger Brother)";
                else if (unit.squadMemberIndex == 2) hp.characterDisplayName = "Max (Demolitions & Childhood Friend)";
            }
            else if (unit.myTeam == SquadMemberAI.Team.Police)
            {
                if (unit.squadMemberIndex == 0) hp.characterDisplayName = "Captain Arnon (SWAT Lead)";
                else if (unit.squadMemberIndex == 1) hp.characterDisplayName = "Sgt. Roi Levi (Veteran Partner)";
                else if (unit.squadMemberIndex == 2) hp.characterDisplayName = "Officer Maya Cohen (Rookie)";
            }
        }
    }

    private IEnumerator TriggerSceneStoryBeatsRoutine()
    {
        // ממתין שהשחקן ילחץ על New Game בתפריט
        while (!MenuAndGameManager.GameHasStarted)
        {
            yield return null;
        }

        AssignPersonalNamesToSquadMembers();
        yield return new WaitForSeconds(0.8f);

        string sceneName = SceneManager.GetActiveScene().name;
        var role = MenuAndGameManager.SelectedRole;

        if (sceneName == "Outdoors" && !introDialogueShown)
        {
            introDialogueShown = true;
            if (role == MenuAndGameManager.PlayerRole.Robber)
            {
                ShowDialogueSequence(new string[] {
                    "Daniel (Brother): Bro, my hands are shaking... If we don't pay off the loan sharks tonight, I'm dead.",
                    "Max (Best Friend): Easy, Daniel. Your brother planned this down to the second. I just need my cut for Mia's surgery.",
                    "Commander (You): Nobody dies today. First, let's enter the Safehouse to grab the Master Key and the thermal gear from our stash chest. (Press [T] anytime to talk)"
                }, 5.5f);
            }
            else if (role == MenuAndGameManager.PlayerRole.Police)
            {
                ShowDialogueSequence(new string[] {
                    "Sgt. Roi Levi: Hard to believe this is my last patrol before retirement! Tomorrow I'm on a yacht, partner.",
                    "Officer Maya Cohen: Don't jinx it, Roi! It's my first day out of the police academy, I still need you watching my back.",
                    "Commander (You): Stay sharp both of you. Intel reported suspicious activity near the safehouse before we head to the Bank. (Press [T] anytime to talk)"
                }, 5.5f);
            }
            else
            {
                ShowDialogueSequence(new string[] {
                    "You (Alex): What a beautiful morning! Still can't believe the wedding is next week.",
                    "You (Alex): Better go inside my house first, grab the key, and take the wedding savings out of the locked chest before walking to the Bank."
                }, 5.5f);
            }
        }
        else if (sceneName == "Bank" && !bankEnteredDialogueShown)
        {
            bankEnteredDialogueShown = true;
            if (role == MenuAndGameManager.PlayerRole.Robber)
            {
                ShowDialogueSequence(new string[] {
                    "Max (Best Friend): We're inside the lobby. Guards are ahead. Give the word, boss!",
                    "Daniel (Brother): Press [F] or fire your weapon when you're ready to start the heist, or press [1] to order us to open fire!"
                }, 5f);
            }
            else if (role == MenuAndGameManager.PlayerRole.Police)
            {
                ShowDialogueSequence(new string[] {
                    "Sgt. Roi Levi: Armed hostiles in the lobby! Civilians on the floor!",
                    "Officer Maya Cohen: Commander, press [1] to order us to engage or [2] for stun grenades!"
                }, 5f);
            }
            else
            {
                ShowDialogueSequence(new string[] {
                    "Bank Teller: Good morning sir! Step up to the counter and press [E] whenever you're ready to make your transaction."
                }, 5f);
            }
        }
    }

    private void TriggerKeyCollectedDialogue()
    {
        var role = MenuAndGameManager.SelectedRole;
        if (role == MenuAndGameManager.PlayerRole.Robber)
            ShowSingleDialogue("Daniel (Brother)", "Got the Master Key! Now let's unlock the back room and open the equipment chest.", 4.5f);
        else if (role == MenuAndGameManager.PlayerRole.Police)
            ShowSingleDialogue("Officer Maya Cohen", "Found a key at the suspect's house! Let's check what they hid behind that locked door.", 4.5f);
        else
            ShowSingleDialogue("You (Alex)", "Got my house key! Now I can unlock the storage room and grab the chest with the wedding gold.", 4.5f);
    }

    private void TriggerChestOpenedDialogue()
    {
        var role = MenuAndGameManager.SelectedRole;
        if (role == MenuAndGameManager.PlayerRole.Robber)
            ShowSingleDialogue("Max (Best Friend)", "Gear and initial funds secured! Let's head back outside and hit the City Bank!", 5f);
        else if (role == MenuAndGameManager.PlayerRole.Police)
            ShowSingleDialogue("Sgt. Roi Levi", "Look at this stash... They're targeting the City Bank right now! Move out to the Bank!", 5f);
        else
            ShowSingleDialogue("You (Alex)", "Got the gold coins! Time to walk over to the City Bank and deposit them safely.", 5f);
    }

    // שיחה יזומה בלחיצה על מקש T
    private void TriggerInteractiveSquadTalk()
    {
        talkCycleIndex++;
        var role = MenuAndGameManager.SelectedRole;

        if (role == MenuAndGameManager.PlayerRole.Robber)
        {
            if (!HeistAlarm.IsHeistActive)
            {
                if (talkCycleIndex % 2 == 1)
                    ShowSingleDialogue("Daniel (Brother)", "Thanks for not leaving me to face those sharks alone, bro. I've got your back.", 4f);
                else
                    ShowSingleDialogue("Max (Best Friend)", "My explosives are primed. Press [2] in combat and I'll clear the room!", 4f);
            }
            else
            {
                ShowSingleDialogue("Max (Best Friend)", "Keep pushing to the Vault! Don't let the SWAT team pin us down!", 4f);
            }
        }
        else if (role == MenuAndGameManager.PlayerRole.Police)
        {
            if (!HeistAlarm.IsHeistActive)
            {
                if (talkCycleIndex % 2 == 1)
                    ShowSingleDialogue("Sgt. Roi Levi", "25 years on the force and my knees still work! Let's keep this city safe.", 4f);
                else
                    ShowSingleDialogue("Officer Maya Cohen", "Ready for orders, Commander! Use [1]-[4] if things get loud.", 4f);
            }
            else
            {
                ShowSingleDialogue("Sgt. Roi Levi", "Watch the crossfire! Protect the bank customers!", 4f);
            }
        }
        else
        {
            if (!HeistAlarm.IsHeistActive)
                ShowSingleDialogue("You (Alex)", "Just a quiet day in the city... Nothing could possibly go wrong today!", 3.5f);
            else
                ShowSingleDialogue("You (Alex)", "Stay low! While the Police and Robbers fight each other, maybe I can sneak into the Vault!", 4f);
        }
    }

    // נקרא אוטומטית מ-HealthSystem כשחבר צוות נהרג - יוצר אימפקט רגשי חזק!
    public void OnCharacterKilledNarrativeReaction(string deadCharacterName)
    {
        if (deadCharacterName.Contains("Daniel"))
        {
            ShowSingleDialogue("Commander (You)", "DANIEL!! NO! Stay with me little brother, please don't close your eyes!", 5.5f);
        }
        else if (deadCharacterName.Contains("Max"))
        {
            ShowSingleDialogue("Commander (You)", "MAX IS DOWN! Damn it, Max... I swear I'll take care of little Mia for you!", 5.5f);
        }
        else if (deadCharacterName.Contains("Roi"))
        {
            ShowSingleDialogue("Commander (You)", "ROI!! Officer down! He was one day away from retirement... Hold on partner!", 5.5f);
        }
        else if (deadCharacterName.Contains("Maya"))
        {
            ShowSingleDialogue("Sgt. Roi Levi", "MAYA IS HIT! Kid, stay with us! Commander, we have to avenge her!", 5.5f);
        }
    }

    private void UpdateDynamicObjectiveText()
    {
        if (objectiveTrackerText == null) return;

        var role = MenuAndGameManager.SelectedRole;
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string obj = "";

        if (role == MenuAndGameManager.PlayerRole.Robber)
        {
            if (scene == "Outdoors" && !HeistAlarm.RobberSquadOrderedToMoveOut && !HeistAlarm.IsHeistActive)
                obj = "1. Press [4] (or [F]) to order Daniel & Max to move out from the house\n2. Head to the City Bank";
            else if (!HeistAlarm.IsHeistActive)
                obj = "1. Enter the Bank & physically approach the Clerk at the counter\n2. Aim your gun at her so she places the Vault Key on the counter, then pick it up [E]!";
            else if (HeistAlarm.IsSirenPlaying && !HeistAlarm.AllVaultCoinsLooted)
                obj = "<color=#FF2222><size=44><b>SWAT ARRIVAL COUNTDOWN: " + Mathf.CeilToInt(HeistAlarm.GetRemainingCountdownSeconds()) + " SECONDS!</b></size></color>\nEnter the Vault & loot ALL Gold to escape early!";
            else if (HeistAlarm.FirefightStarted && !HeistAlarm.FirefightEnded)
                obj = "SWAT HAS ARRIVED! Eliminate the Police squad ([1] Fire [2]/[G] Grenade)! (Lose 2 teammates = Defeat!)";
            else
                obj = "ESCAPE NOW! Exit the Bank to the street and reach the Getaway Vehicle to win!";
        }
        else if (role == MenuAndGameManager.PlayerRole.Police)
        {
            if (scene == "Outdoors" && !HeistAlarm.FirefightStarted)
                obj = "DISPATCH ALERT: Follow the Bank alarm sound with Sgt. Roi & Maya and enter the City Bank!";
            else if (scene == "Bank" && !HeistAlarm.FirefightStarted)
                obj = "Hostages say the Robbers are inside the Vault! Breach the Vault door immediately!";
            else
                obj = "VAULT FIREFIGHT: Neutralize the Robber squad! ([1] Fire [2]/[G] Grenade [3] Retreat [4] Follow)";
        }
        else
        {
            if (!HeistAlarm.IsHeistActive)
                obj = "1. Enter the City Bank\n2. Approach the Clerk at the counter and press [E] to withdraw cash.";
            else if (!HeistAlarm.FirefightEnded)
                obj = "ROBBERY & SHOOTOUT IN PROGRESS! Take cover in the lobby and stay out of their line of fire!";
            else
                obj = "THE SHOOTOUT IS OVER! Exit through the Bank front door to the street to survive and win!";
        }

        objectiveTrackerText.text = "<b><color=#FFD700>MISSION OBJECTIVE (Press [T] to Talk | [G] Throw Grenade):</color></b>\n" + obj;
    }

    public void ShowSingleDialogue(string speaker, string line, float duration)
    {
        if (activeDialogueRoutine != null) StopCoroutine(activeDialogueRoutine);
        activeDialogueRoutine = StartCoroutine(SingleDialogueRoutine(speaker, line, duration));
    }

    public void ShowDialogueSequence(string[] lines, float secondsPerLine)
    {
        if (activeDialogueRoutine != null) StopCoroutine(activeDialogueRoutine);
        activeDialogueRoutine = StartCoroutine(DialogueSequenceRoutine(lines, secondsPerLine));
    }

    private IEnumerator SingleDialogueRoutine(string speaker, string line, float duration)
    {
        if (dialogueBoxPanel == null) yield break;
        dialogueBoxPanel.SetActive(true);
        speakerNameText.text = speaker;
        dialogueBodyText.text = line;

        // מפעיל את הדיבוב הקולי של הדמות!
        TriggerCharacterVoice(speaker, line);

        yield return new WaitForSeconds(duration);
        dialogueBoxPanel.SetActive(false);
    }

    private IEnumerator DialogueSequenceRoutine(string[] lines, float secondsPerLine)
    {
        if (dialogueBoxPanel == null) yield break;
        dialogueBoxPanel.SetActive(true);

        foreach (string fullLine in lines)
        {
            string[] parts = fullLine.Split(new char[] { ':' }, 2);
            if (parts.Length == 2)
            {
                speakerNameText.text = parts[0].Trim();
                dialogueBodyText.text = parts[1].Trim();
            }
            else
            {
                speakerNameText.text = "Story";
                dialogueBodyText.text = fullLine;
            }

            // מפעיל את הדיבוב הקולי לכל משפט ברצף!
            TriggerCharacterVoice(speakerNameText.text, dialogueBodyText.text);

            yield return new WaitForSeconds(secondsPerLine);
        }

        dialogueBoxPanel.SetActive(false);
    }

    private void TriggerCharacterVoice(string speaker, string line)
    {
        if (DialogueVoiceSynthesizer.Instance == null)
        {
            gameObject.AddComponent<DialogueVoiceSynthesizer>();
        }
        if (DialogueVoiceSynthesizer.Instance != null)
        {
            DialogueVoiceSynthesizer.Instance.SpeakLine(speaker, line);
        }
    }

    // בונה אוטומטית את תיבת המשימות ותיבת הדיאלוגים בתוך GamePanel
    private void CreateStoryUIElementsIfNeeded()
    {
        MenuAndGameManager menu = GetComponent<MenuAndGameManager>();
        if (menu == null || menu.gamePanel == null) return;

        Transform gamePanelTransform = menu.gamePanel.transform;

        // 1. תיבת משימות מוגדלת פי 3 וממוקמת מתחת לטקסט הכסף והתפקיד (Y = -260)
        GameObject objTracker = new GameObject("ObjectiveTrackerBox", typeof(RectTransform), typeof(Image));
        objTracker.transform.SetParent(gamePanelTransform, false);
        RectTransform objRect = objTracker.GetComponent<RectTransform>();
        objRect.anchorMin = new Vector2(0f, 1f);
        objRect.anchorMax = new Vector2(0f, 1f);
        objRect.pivot = new Vector2(0f, 1f);
        objRect.anchoredPosition = new Vector2(25f, -260f); // מתחת ל-HUD של הכסף והתפקיד!
        objRect.sizeDelta = new Vector2(1150f, 290f);      // מוגדל פי 3!
        Image objBg = objTracker.GetComponent<Image>();
        objBg.color = new Color(0f, 0f, 0f, 0.72f);
        objBg.raycastTarget = false;

        GameObject objTxtGO = new GameObject("ObjectiveText", typeof(RectTransform), typeof(TextMeshProUGUI));
        objTxtGO.transform.SetParent(objTracker.transform, false);
        RectTransform txtRect = objTxtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(24f, 16f);
        txtRect.offsetMax = new Vector2(-24f, -16f);
        objectiveTrackerText = objTxtGO.GetComponent<TextMeshProUGUI>();
        objectiveTrackerText.fontSize = 38;
        objectiveTrackerText.color = Color.white;
        objectiveTrackerText.raycastTarget = false;

        // 2. תיבת דיאלוגים קולנועית מוגדלת פי 3 בתחתית המסך
        dialogueBoxPanel = new GameObject("CinematicDialogueBox", typeof(RectTransform), typeof(Image));
        dialogueBoxPanel.transform.SetParent(gamePanelTransform, false);
        RectTransform dlgRect = dialogueBoxPanel.GetComponent<RectTransform>();
        dlgRect.anchorMin = new Vector2(0.5f, 0f);
        dlgRect.anchorMax = new Vector2(0.5f, 0f);
        dlgRect.pivot = new Vector2(0.5f, 0f);
        dlgRect.anchoredPosition = new Vector2(0f, 40f);
        dlgRect.sizeDelta = new Vector2(1850f, 330f);      // מוגדל פי 3!
        Image dlgBg = dialogueBoxPanel.GetComponent<Image>();
        dlgBg.color = new Color(0.05f, 0.07f, 0.12f, 0.90f);
        dlgBg.raycastTarget = false;

        GameObject nameGO = new GameObject("SpeakerName", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGO.transform.SetParent(dialogueBoxPanel.transform, false);
        RectTransform nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.62f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = new Vector2(32f, 0f);
        nameRect.offsetMax = new Vector2(-32f, -12f);
        speakerNameText = nameGO.GetComponent<TextMeshProUGUI>();
        speakerNameText.fontSize = 48;
        speakerNameText.fontStyle = FontStyles.Bold;
        speakerNameText.color = new Color(0.3f, 0.85f, 1f);
        speakerNameText.raycastTarget = false;

        GameObject bodyGO = new GameObject("DialogueBody", typeof(RectTransform), typeof(TextMeshProUGUI));
        bodyGO.transform.SetParent(dialogueBoxPanel.transform, false);
        RectTransform bodyRect = bodyGO.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 0.64f);
        bodyRect.offsetMin = new Vector2(32f, 16f);
        bodyRect.offsetMax = new Vector2(-32f, 0f);
        dialogueBodyText = bodyGO.GetComponent<TextMeshProUGUI>();
        dialogueBodyText.fontSize = 40;
        dialogueBodyText.color = Color.white;
        dialogueBodyText.raycastTarget = false;

        dialogueBoxPanel.SetActive(false);
    }
}