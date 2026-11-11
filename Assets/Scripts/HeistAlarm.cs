using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class HeistAlarm : MonoBehaviour
{
    public static bool IsHeistActive = false;
    public static bool IsSirenPlaying = false;
    public static bool SirenFinished = false;
    public static bool FirefightStarted = false;
    public static bool FirefightEnded = false;
    public static bool AllVaultCoinsLooted = false;
    public static bool RobberSquadOrderedToMoveOut = false;

    // שמירת טיימר הדקה של השודד במעבר בין הבנק לכספת
    public static float HeistCountdownEndTime = -1f;
    private static bool policeDispatchTriggered = false;

    [Header("Audio Sources")]
    public AudioSource alarmAudioSource;
    public AudioSource approachingSirenAudio;

    [Header("UI Banner")]
    public GameObject heistAlertBanner;
    public TMP_Text heistAlertText;

    [Header("Outdoors Vehicles (Assign in Outdoors Scene)")]
    public GameObject[] policeCarsOutside;
    public GameObject getawayVehicleOutside;

    private bool wasPausedLastFrame = false;

    // פונקציית עזר שמזהה את סצנת הכספת (תומכת גם בשם "Vault" וגם בשם הישן "HouseWithChest")
    public static bool IsVaultScene(string sceneName)
    {
        return sceneName == "Vault" || sceneName == "HouseWithChest";
    }

    public static float GetRemainingCountdownSeconds()
    {
        if (!IsSirenPlaying || HeistCountdownEndTime < 0f) return 0f;
        return Mathf.Max(0f, HeistCountdownEndTime - Time.time);
    }

    void Start()
    {
        if (heistAlertBanner != null) heistAlertBanner.SetActive(false);

        string currentScene = SceneManager.GetActiveScene().name;
        var role = MenuAndGameManager.SelectedRole;

        // הגדרת סאונד תלת-ממדי שמתחזק כשמתקרבים לבנק מהרחוב או לדלת מהכספת
        if (alarmAudioSource != null && currentScene != "Bank")
        {
            alarmAudioSource.spatialBlend = 1.0f;
            alarmAudioSource.rolloffMode = AudioRolloffMode.Linear;
            alarmAudioSource.minDistance = 4f;
            alarmAudioSource.maxDistance = 85f;
        }

        if (currentScene == "Outdoors")
        {
            UpdateOutdoorsVehiclesVisibility();
        }

        // כששוטר נכנס ללובי הבנק לפני שהתחיל הקרב בכספת - האזרחים על הרצפה צועקים לו שהשודדים בכספת!
        if (role == MenuAndGameManager.PlayerRole.Police && currentScene == "Bank" && !FirefightStarted)
        {
            MakeAllCustomersLieDown();
            KeyPickup.HasKey = true; // מאפשר לשוטר להיכנס מיד לכספת
            if (StoryAndDialogueManager.Instance != null)
            {
                StoryAndDialogueManager.Instance.ShowSingleDialogue(
                    "Terrified Hostages (Lobby)",
                    "OFFICERS! HELP! The armed robbers locked themselves inside the Vault! Hurry through the Vault door!",
                    6.5f
                );
            }
        }

        // כששוטר נכנס לכספת (HouseWithChest) - מתחיל קרב היריות ומכאן הם עוקבים אחריו לכל סצנה!
        if (role == MenuAndGameManager.PlayerRole.Police && IsVaultScene(currentScene) && !FirefightStarted)
        {
            FirefightStarted = true;
            SquadMemberAI.EnemiesPursuingPlayer = true;
            WakeUpAllSceneUnitsForCombat();
        }

        // המשך השמעת האזעקה והסירנה אם כבר הופעלו בסצנה קודמת
        if (IsHeistActive && alarmAudioSource != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.loop = true;
            alarmAudioSource.Play();
        }

        if (IsSirenPlaying && approachingSirenAudio != null && !approachingSirenAudio.isPlaying)
        {
            approachingSirenAudio.loop = true;
            approachingSirenAudio.Play();
        }
    }

    void Update()
    {
        // 1. השתקה אוטומטית של האזעקה והסירנה כשיוצאים לתפריט (ESC) או בסיום המשחק
        bool isMenuOpenOrPaused = !MenuAndGameManager.GameHasStarted || Time.timeScale == 0f || HealthSystem.MatchEnded;
        if (isMenuOpenOrPaused)
        {
            if (!wasPausedLastFrame)
            {
                if (alarmAudioSource != null && alarmAudioSource.isPlaying) alarmAudioSource.Pause();
                if (approachingSirenAudio != null && approachingSirenAudio.isPlaying) approachingSirenAudio.Pause();
                wasPausedLastFrame = true;
            }
            return;
        }
        else if (wasPausedLastFrame)
        {
            if (IsHeistActive && alarmAudioSource != null) alarmAudioSource.UnPause();
            if (IsSirenPlaying && approachingSirenAudio != null) approachingSirenAudio.UnPause();
            wasPausedLastFrame = false;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        var role = MenuAndGameManager.SelectedRole;

        if (currentScene == "Outdoors")
        {
            UpdateOutdoorsVehiclesVisibility();
        }

        // 2. תרחיש שוטר ברחוב: מקבל אות בקשר שהשוד פרץ בבנק והאזעקה נשמעת מרחוק!
        if (role == MenuAndGameManager.PlayerRole.Police && currentScene == "Outdoors" && !policeDispatchTriggered)
        {
            policeDispatchTriggered = true;
            StartCoroutine(PoliceDispatchStartRoutine());
        }

        // 3. בדיקת סיום טיימר הדקה (60 שניות) והסירנה אצל השודד או עובר האורח
        if (IsSirenPlaying && Time.time >= HeistCountdownEndTime)
        {
            OnSirenAndCountdownFinished();
        }
    }

    private void UpdateOutdoorsVehiclesVisibility()
    {
        // מונע מ-HeistAlarm לכבות את הרכבים בזמן שרץ סרטון הסיום והקרדיטים!
        if (HealthSystem.MatchEnded || VictoryCinematicDirector.PendingOutdoorsCutscene) return;

        var role = MenuAndGameManager.SelectedRole;

        // רכב המילוט מופיע ברחוב רק אחרי שהשודד ביצע את השוד בבנק!
        if (getawayVehicleOutside != null)
        {
            bool showGetaway = (role == MenuAndGameManager.PlayerRole.Robber) &&
                               IsHeistActive &&
                               (AllVaultCoinsLooted || FirefightEnded);
            if (getawayVehicleOutside.activeSelf != showGetaway)
                getawayVehicleOutside.SetActive(showGetaway);
        }

        // ניידות המשטרה מופיעות ברחוב רק אחרי שהסירנה הסתיימה והעימות החל!
        if (policeCarsOutside != null)
        {
            bool showPoliceCars = (role == MenuAndGameManager.PlayerRole.Robber && SirenFinished && FirefightStarted) ||
                                  (role == MenuAndGameManager.PlayerRole.Bystander && SirenFinished);
            foreach (GameObject car in policeCarsOutside)
            {
                if (car != null && car.activeSelf != showPoliceCars)
                    car.SetActive(showPoliceCars);
            }
        }
    }

    private IEnumerator PoliceDispatchStartRoutine()
    {
        yield return new WaitForSeconds(2.5f);
        IsHeistActive = true;
        KeyPickup.HasKey = true;

        if (alarmAudioSource != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.loop = true;
            alarmAudioSource.Play();
        }

        if (StoryAndDialogueManager.Instance != null)
        {
            StoryAndDialogueManager.Instance.ShowSingleDialogue(
                "Police Radio Dispatch",
                "ALL UNITS: Silent alarm triggered at the City Bank! Your squad is the closest unit - respond immediately while backup is en route!",
                6.5f
            );
        }
    }

    // מופעל כששודד לוקח את המפתח מהדלפק: הפקידה מפעילה אזעקה ומתחיל טיימר של 60 שניות!
    public void StartRobberHeistAfterKeyPickup()
    {
        if (IsHeistActive) return;
        IsHeistActive = true;
        IsSirenPlaying = true;
        SirenFinished = false;
        HeistCountdownEndTime = Time.time + 60f; // דקה אחת קצובה!

        if (alarmAudioSource != null)
        {
            alarmAudioSource.loop = true;
            alarmAudioSource.Play();
        }

        if (approachingSirenAudio != null)
        {
            approachingSirenAudio.loop = true;
            approachingSirenAudio.Play();
        }

        MakeAllCustomersLieDown();

        if (StoryAndDialogueManager.Instance != null)
        {
            StoryAndDialogueManager.Instance.ShowSingleDialogue(
                "Daniel (Brother)",
                "DAMN IT! That clerk hit the alarm button! Shoot her if you want, then get into the Vault! We have 60 seconds to empty it before SWAT storms in!",
                6.5f
            );
        }
    }

    // מופעל כשעובר אורח מושך כסף מהפקידה: השודדים מופיעים בלובי ומתחיל הרצף!
    public void TriggerBystanderHeistSequence()
    {
        if (IsHeistActive) return;
        StartCoroutine(BystanderRobberyCutsceneRoutine());
    }

    private IEnumerator BystanderRobberyCutsceneRoutine()
    {
        if (StoryAndDialogueManager.Instance != null)
        {
            StoryAndDialogueManager.Instance.ShowSingleDialogue(
                "Bank Clerk",
                "Here is your cash withdrawal, sir... Wait—who are those masked men at the entrance?!",
                4f
            );
        }

        yield return new WaitForSeconds(2.5f);

        // השודדים מופיעים בלובי הבנק ומאיימים על הפקידה!
        IsHeistActive = true;
        MakeAllCustomersLieDown();
        ActivateTeamInCurrentScene(SquadMemberAI.Team.Robber);

        if (alarmAudioSource != null)
        {
            alarmAudioSource.loop = true;
            alarmAudioSource.Play();
        }

        if (StoryAndDialogueManager.Instance != null)
        {
            StoryAndDialogueManager.Instance.ShowSingleDialogue(
                "Viktor (Robber Boss)",
                "NOBODY MOVE! THIS IS A ROBBERY! Clerk, open the Vault! Wait—she triggered the alarm! Police sirens are approaching!",
                5.5f
            );
        }

        IsSirenPlaying = true;
        SirenFinished = false;
        HeistCountdownEndTime = Time.time + 16f; // סירנה של 16 שניות עד שהשוטרים פורצים ללובי מול עובר האורח

        if (approachingSirenAudio != null)
        {
            approachingSirenAudio.loop = true;
            approachingSirenAudio.Play();
        }
    }

    // מופעל ברגע שטיימר הדקה / הסירנה מסתיים: השוטרים מופיעים באותו חדר שבו השחקן נמצא!
    private void OnSirenAndCountdownFinished()
    {
        IsSirenPlaying = false;
        SirenFinished = true;

        if (approachingSirenAudio != null && approachingSirenAudio.isPlaying)
        {
            approachingSirenAudio.Stop();
        }

        var role = MenuAndGameManager.SelectedRole;

        // אם השודד כבר רוקן את כל הכספת ויצא לרחוב לפני שהסירנה נגמרה - הוא כבר בדרך לרכב המילוט
        if (role == MenuAndGameManager.PlayerRole.Robber &&
            AllVaultCoinsLooted &&
            SceneManager.GetActiveScene().name == "Outdoors")
        {
            return;
        }

        // אחרת - השוטרים פורצים מיד לחדר הנוכחי (כספת או לובי) ומתחיל קרב יריות!
        FirefightStarted = true;
        SquadMemberAI.EnemiesPursuingPlayer = true;
        ActivateTeamInCurrentScene(SquadMemberAI.Team.Police);
        ActivateTeamInCurrentScene(SquadMemberAI.Team.Robber);
        WakeUpAllSceneUnitsForCombat();

        if (StoryAndDialogueManager.Instance != null)
        {
            if (role == MenuAndGameManager.PlayerRole.Robber)
            {
                StoryAndDialogueManager.Instance.ShowSingleDialogue(
                    "SWAT Commander",
                    "POLICE! YOU ARE SURROUNDED! (Eliminate the Police squad before you can escape to the Getaway Vehicle!)",
                    6f
                );
            }
            else if (role == MenuAndGameManager.PlayerRole.Bystander)
            {
                StoryAndDialogueManager.Instance.ShowSingleDialogue(
                    "SWAT Police Force",
                    "SWAT BREACHING THE LOBBY! OPEN FIRE! (Stay out of their line of fire until the shootout ends!)",
                    6f
                );
            }
        }
    }

    private void ActivateTeamInCurrentScene(SquadMemberAI.Team teamToActivate)
    {
        SquadMemberAI[] allUnits = FindObjectsByType<SquadMemberAI>(FindObjectsInactive.Include);
        foreach (SquadMemberAI unit in allUnits)
        {
            if (unit.myTeam == teamToActivate && !SquadMemberAI.DeadUnitsRegistry.Contains(unit.GetUniqueID()))
            {
                unit.gameObject.SetActive(true);
                unit.InitializeRoleAndScenePlacement();
            }
        }
    }

    private void WakeUpAllSceneUnitsForCombat()
    {
        SquadMemberAI[] activeUnits = FindObjectsByType<SquadMemberAI>();
        foreach (SquadMemberAI unit in activeUnits)
        {
            if (!unit.isPlayerTeammate)
            {
                unit.CommandShoot();
            }
        }
    }

    public void StartHeist()
    {
        if (!IsHeistActive)
        {
            StartRobberHeistAfterKeyPickup();
        }
    }

    // בדיקת חוקי היציאה מהבנק לרחוב (Outdoors)
    public static bool CanPlayerLeaveBankToOutdoors(out string reason)
    {
        reason = "";
        var role = MenuAndGameManager.SelectedRole;

        if (role == MenuAndGameManager.PlayerRole.Robber)
        {
            if (!IsHeistActive)
            {
                reason = "You haven't robbed the Bank yet! Aim your weapon at the Clerk to get the Vault Key!";
                return false;
            }

            // אם השודד רוקן 100% מהכספת לפני שעברה הדקה - מותר לו לברוח מהבנק עוד לפני שהמשטרה מגיעה!
            if (AllVaultCoinsLooted && !FirefightStarted)
            {
                return true;
            }

            // אם עוד לא עברה הדקה והוא לא רוקן את כל הכספת - אי אפשר לצאת!
            if (IsSirenPlaying && !AllVaultCoinsLooted)
            {
                reason = "LOCKED: Empty all Vault gold to escape early, or wait " + Mathf.CeilToInt(GetRemainingCountdownSeconds()) + "s for Police arrival!";
                return false;
            }

            // אם העימות עם השוטרים התחיל - חייבים לחסל את כולם לפני שנמלטים לרחוב!
            if (FirefightStarted && !FirefightEnded)
            {
                reason = "SWAT BLOCKADE! You must eliminate the Police squad before escaping to the street!";
                return false;
            }
        }
        else if (role == MenuAndGameManager.PlayerRole.Bystander)
        {
            if (!IsHeistActive)
            {
                reason = "Go to the Bank Clerk at the counter and press [E] to withdraw your money first!";
                return false;
            }
            if (!FirefightEnded)
            {
                reason = "DANGER! You cannot leave the Bank until the shootout between Police and Robbers is over!";
                return false;
            }
        }

        return true;
    }

    private void MakeAllCustomersLieDown()
    {
        BankCustomerBehavior[] allCustomers = FindObjectsByType<BankCustomerBehavior>();
        foreach (BankCustomerBehavior customer in allCustomers)
        {
            customer.TriggerPanicAndLieDown();
        }
    }
}