using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using System.Collections;

public class MenuAndGameManager : MonoBehaviour
{
    public enum PlayerRole { Robber, Police, Bystander }
    public enum GameDifficulty { Easy, Normal, Hard }

    public static bool GameHasStarted = false;
    public static PlayerRole SelectedRole = PlayerRole.Robber;         // התפקיד הפעיל במשחק הנוכחי
    private static PlayerRole PendingMenuRole = PlayerRole.Robber;     // התפקיד שנבחר בתפריט לקראת New Game

    // הגדרות משחק גלובליות (נשמרות בין סצנות)
    public static bool IsMusicMuted = false;
    public static GameDifficulty CurrentDifficulty = GameDifficulty.Normal;

    public static int PlayerHP = 160;
    public static string PlayerCodeOrName = "";

    private static Vector3 initialOutdoorsPlayerPos;
    private static Quaternion initialOutdoorsPlayerRot;
    private static bool hasSavedInitialPlayerPos = false;
    private static bool isStartingFreshNewGame = false;
    private static bool sceneNeedsReloadOnNewGame = false;
    private static int lastEscapeFrame = -1;

    [Header("Main UI Panels")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject optionsPanel;
    public GameObject pauseOverlayPanel;
    public GameObject defeatScreenPanel;
    public GameObject loadingScreenPanel;

    [Header("Victory Cutscene & Credits")]
    public GameObject victoryCutscenePanel;
    public RectTransform scrollingCreditsTransform;
    public TMP_Text victoryHeadlineText;
    public VideoPlayer victoryVideoPlayer;
    public float creditsScrollSpeed = 70f;

    [Header("Groups & Buttons Inside StartPanel")]
    public GameObject preGameSetupGroup;
    public GameObject mainActionButtonsGroup;
    public GameObject startNewGameButton;
    public GameObject continueGameButton;

    [Header("Texts & Inputs")]
    public TMP_Text selectedRoleStatusText;
    public TMP_InputField secretInputField;
    public TMP_Text hudRoleAndHPText;
    public TMP_Text loadingAnimatedText;
    public TMP_Text defeatReasonText;

    [Header("Music Tracks (Audio Clips)")]
    public AudioSource musicAudioSource;
    public AudioClip mainMenuMusic;
    public AudioClip robberVictoryMusic;
    public AudioClip policeVictoryMusic;
    public AudioClip bystanderSurvivalMusic;
    public AudioClip defeatMusic;

    [Header("Player Reference")]
    public MonoBehaviour playerMovementScript;

    [Header("Options Buttons Position (Adjust in Inspector if needed)")]
    public Vector2 optionsButtonsPos = new Vector2(0f, -340f);

    private bool isPaused = false;
    private bool isPlayingVictoryCutscene = false;
    private bool isDefeatButtonLocked = false;

    // רכיבי UI פנימיים של מסך ההגדרות (Options)
    private TMP_Text musicButtonLabel;
    private TMP_Text difficultyButtonLabel;

    void Awake()
    {
        MenuAndGameManager[] allManagers = FindObjectsByType<MenuAndGameManager>();
        if (allManagers.Length > 1)
        {
            if (this.startPanel == null || this.selectedRoleStatusText == null)
            {
                this.enabled = false;
                Destroy(this);
                return;
            }

            foreach (MenuAndGameManager mgr in allManagers)
            {
                if (mgr != this && (mgr.startPanel == null || mgr.selectedRoleStatusText == null))
                {
                    mgr.enabled = false;
                    Destroy(mgr);
                }
            }
        }

        UnityEngine.EventSystems.EventSystem[] eventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystems.Length == 0)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        else if (eventSystems.Length > 1)
        {
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Destroy(eventSystems[i].gameObject);
            }
        }

        SaveOrRestorePlayerInitialPosition();
        AutoBindPanelsAndButtons();
        SetupInteractiveOptionsPanelUI();

        DisableTextRaycastBlocking(defeatReasonText);
        DisableTextRaycastBlocking(selectedRoleStatusText);
        DisableTextRaycastBlocking(hudRoleAndHPText);
        DisableTextRaycastBlocking(loadingAnimatedText);
        DisableTextRaycastBlocking(victoryHeadlineText);
    }

    private void SaveOrRestorePlayerInitialPosition()
    {
        if (SceneManager.GetActiveScene().name != "Outdoors") return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        if (!hasSavedInitialPlayerPos && playerObj.transform.position.sqrMagnitude > 25f)
        {
            initialOutdoorsPlayerPos = playerObj.transform.position;
            initialOutdoorsPlayerRot = playerObj.transform.rotation;
            hasSavedInitialPlayerPos = true;
        }
    }

    private void TeleportPlayerToInitialOutdoorsSpawn()
    {
        if (!hasSavedInitialPlayerPos || SceneManager.GetActiveScene().name != "Outdoors") return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerObj.transform.position = initialOutdoorsPlayerPos;
        playerObj.transform.rotation = initialOutdoorsPlayerRot;

        if (cc != null) cc.enabled = true;
    }

    private IEnumerator EnforceInitialSpawnAfterLoadRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            TeleportPlayerToInitialOutdoorsSpawn();
            yield return null;
        }
        isStartingFreshNewGame = false;
    }

    private void AutoBindPanelsAndButtons()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        Transform root = canvas.transform;

        if (startPanel == null) startPanel = FindNode(root, "StartPanel");
        if (gamePanel == null) gamePanel = FindNode(root, "GamePanel");
        if (optionsPanel == null) optionsPanel = FindNode(root, "OptionsPanel");
        if (pauseOverlayPanel == null) pauseOverlayPanel = FindNode(root, "PauseOverlayPanel");
        if (defeatScreenPanel == null) defeatScreenPanel = FindNode(root, "Defeat Screen Panel");
        if (loadingScreenPanel == null) loadingScreenPanel = FindNode(root, "Loading Screen Panel");

        if (preGameSetupGroup == null) preGameSetupGroup = FindNode(root, "PreGameSetupGroup");
        if (mainActionButtonsGroup == null) mainActionButtonsGroup = FindNode(root, "MainActionButtonsGroup");
        if (startNewGameButton == null) startNewGameButton = FindNode(root, "NewGame");
        if (continueGameButton == null) continueGameButton = FindNode(root, "Continue");

        BindButton(startNewGameButton, OnNewGameButtonClicked);
        BindButton(continueGameButton, OnContinueButtonClicked);
        BindButton(FindNode(root, "OptionsButton"), OnOptionsButtonClicked);
        BindButton(FindNode(root, "ExitButton"), OnExitButtonClicked);

        if (pauseOverlayPanel != null)
        {
            BindButton(FindNode(pauseOverlayPanel.transform, "Resume"), ResumeGame);
            BindButton(FindNode(pauseOverlayPanel.transform, "Main Menu"), OnGoToMainMenuClicked);
        }

        if (defeatScreenPanel != null)
        {
            BindButton(FindNode(defeatScreenPanel.transform, "Main Menu"), OnGoToMainMenuClicked);
        }

        if (optionsPanel != null)
        {
            Button backBtn = optionsPanel.GetComponentInChildren<Button>(true);
            if (backBtn != null) BindButton(backBtn.gameObject, OnBackFromOptionsClicked);
        }
    }

    // בונה אוטומטית בתוך OptionsPanel כפתורים להשתקת מוזיקה ולשינוי דרגת קושי!
    private void SetupInteractiveOptionsPanelUI()
    {
        if (optionsPanel == null) return;

        Transform existingContainer = optionsPanel.transform.Find("AutoOptionsControlsContainer");
        if (existingContainer != null)
        {
            existingContainer.GetComponent<RectTransform>().anchoredPosition = optionsButtonsPos;
            return;
        }

        GameObject container = new GameObject("AutoOptionsControlsContainer", typeof(RectTransform));
        container.transform.SetParent(optionsPanel.transform, false);
        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = optionsButtonsPos; // יושב למטה מתחת לטקסט ההנחיות
        rt.sizeDelta = new Vector2(860f, 80f);

        // 1. כפתור מוזיקה בצד שמאל
        musicButtonLabel = CreateOptionsButton(
            container.transform,
            "MusicToggleButton",
            new Vector2(-215f, 0f),
            ToggleMusicSetting
        );

        // 2. כפתור דרגת קושי בצד ימין (לצד כפתור המוזיקה)
        difficultyButtonLabel = CreateOptionsButton(
            container.transform,
            "DifficultyCycleButton",
            new Vector2(215f, 0f),
            CycleDifficultySetting
        );

        RefreshOptionsUI();
    }

    private TMP_Text CreateOptionsButton(Transform parent, string name, Vector2 pos, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(400f, 60f);

        Image bg = btnObj.GetComponent<Image>();
        bg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = bg;
        btn.onClick.AddListener(onClickAction);

        GameObject txtObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return tmp;
    }

    public void ToggleMusicSetting()
    {
        IsMusicMuted = !IsMusicMuted;
        if (musicAudioSource != null)
        {
            musicAudioSource.mute = IsMusicMuted;
        }
        RefreshOptionsUI();
    }

    public void CycleDifficultySetting()
    {
        if (CurrentDifficulty == GameDifficulty.Easy) CurrentDifficulty = GameDifficulty.Normal;
        else if (CurrentDifficulty == GameDifficulty.Normal) CurrentDifficulty = GameDifficulty.Hard;
        else CurrentDifficulty = GameDifficulty.Easy;

        RefreshOptionsUI();
    }

    private void RefreshOptionsUI()
    {
        if (musicButtonLabel != null)
        {
            string stateStr = IsMusicMuted ? "<color=#FF4444>OFF (MUTED)</color>" : "<color=#33FF66>ON</color>";
            musicButtonLabel.text = "MUSIC: " + stateStr;
        }

        if (difficultyButtonLabel != null)
        {
            string diffColor = (CurrentDifficulty == GameDifficulty.Easy) ? "#33FF66" :
                               (CurrentDifficulty == GameDifficulty.Normal) ? "#FFD700" : "#FF4444";
            difficultyButtonLabel.text = $"DIFFICULTY: <color={diffColor}>{CurrentDifficulty.ToString().ToUpper()}</color>";
        }
    }

    private GameObject FindNode(Transform parent, string targetName)
    {
        if (parent == null) return null;
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName) return child.gameObject;
        }
        return null;
    }

    private void BindButton(GameObject btnObj, UnityEngine.Events.UnityAction action)
    {
        if (btnObj == null) return;
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    private void DisableTextRaycastBlocking(TMP_Text txt)
    {
        if (txt != null) txt.raycastTarget = false;
    }

    void Start()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.85f, 0.85f, 0.88f, 1f);
        RenderSettings.ambientIntensity = 1.2f;

        if (musicAudioSource != null) musicAudioSource.mute = IsMusicMuted;

        if (VictoryCinematicDirector.PendingOutdoorsCutscene && SceneManager.GetActiveScene().name == "Outdoors")
        {
            VictoryCinematicDirector.PendingOutdoorsCutscene = false;
            SelectedRole = VictoryCinematicDirector.CutsceneRole;
            TriggerEndOfMatch(true, VictoryCinematicDirector.CutsceneHeadlineMessage);
            return;
        }

        FindPlayerIfNeeded();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (SceneManager.GetActiveScene().name == "Outdoors" &&
            (isStartingFreshNewGame || (playerObj != null && playerObj.transform.position.sqrMagnitude < 25f)))
        {
            StartCoroutine(EnforceInitialSpawnAfterLoadRoutine());
        }

        if (GameHasStarted && !HealthSystem.MatchEnded)
        {
            ResumeGame();
            InitializeSceneSquads();
            UpdateHUD();
        }
        else
        {
            OpenMainMenuScreen();
        }
    }

    void Update()
    {
        if (isPaused || HealthSystem.MatchEnded || !GameHasStarted)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerMovementScript != null && playerMovementScript.enabled)
            {
                playerMovementScript.enabled = false;
            }
        }

        if (HealthSystem.MatchEnded && !isPlayingVictoryCutscene)
        {
            if (!isDefeatButtonLocked && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)))
            {
                OnGoToMainMenuClicked();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && Time.frameCount != lastEscapeFrame)
        {
            lastEscapeFrame = Time.frameCount;

            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                OnBackFromOptionsClicked();
                return;
            }

            if (!GameHasStarted) return;

            if (startPanel != null && startPanel.activeSelf)
            {
                OnContinueButtonClicked();
            }
            else if (pauseOverlayPanel != null && pauseOverlayPanel.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                OpenInGamePauseOverlay();
            }
        }
    }

    private void ShowExclusivePanel(GameObject panelToActivate)
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pauseOverlayPanel != null) pauseOverlayPanel.SetActive(false);
        if (defeatScreenPanel != null) defeatScreenPanel.SetActive(false);
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
        if (victoryCutscenePanel != null) victoryCutscenePanel.SetActive(false);

        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true);
            panelToActivate.transform.SetAsLastSibling();
        }
    }

    private void OpenInGamePauseOverlay()
    {
        isPaused = true;
        Time.timeScale = 0f;
        SetPlayerControl(false);

        if (pauseOverlayPanel != null)
        {
            ShowExclusivePanel(pauseOverlayPanel);
        }
        else
        {
            OpenMainMenuScreen();
        }
    }

    public void OnGoToMainMenuClicked()
    {
        StopAllCoroutines();
        isPlayingVictoryCutscene = false;
        isDefeatButtonLocked = false;
        if (victoryVideoPlayer != null) victoryVideoPlayer.Stop();

        OpenMainMenuScreen();
    }

    private void OpenMainMenuScreen()
    {
        isPaused = true;
        Time.timeScale = 0f;
        SetPlayerControl(false);

        PendingMenuRole = SelectedRole;

        ShowExclusivePanel(startPanel);

        if (mainActionButtonsGroup != null) mainActionButtonsGroup.SetActive(true);
        if (preGameSetupGroup != null) preGameSetupGroup.SetActive(true);

        bool canContinue = GameHasStarted && !HealthSystem.MatchEnded;
        if (continueGameButton != null) continueGameButton.SetActive(canContinue);
        if (startNewGameButton != null) startNewGameButton.SetActive(true);

        UpdateRoleSelectionText();
        PlayMusicTrack(mainMenuMusic, loop: true);
    }

    // לחיצה על New Game - כאן ורק כאן מוחל התפקיד שנבחר בתפריט (PendingMenuRole)!
    public void OnNewGameButtonClicked()
    {
        StopAllCoroutines();
        bool needCleanReload = sceneNeedsReloadOnNewGame || SceneManager.GetActiveScene().name != "Outdoors";

        SelectedRole = PendingMenuRole; // החלת התפקיד שנבחר עבור המשחק החדש
        ResetAllGameStaticData();
        ProcessSecretInput();
        GameHasStarted = true;
        isStartingFreshNewGame = true;
        sceneNeedsReloadOnNewGame = true;

        if (musicAudioSource != null) musicAudioSource.Stop();
        if (startPanel != null)
        {
            AudioSource panelAudio = startPanel.GetComponent<AudioSource>();
            if (panelAudio != null) panelAudio.Stop();
        }

        PersistentObjectManager oldPom = FindAnyObjectByType<PersistentObjectManager>();
        if (oldPom != null && needCleanReload)
        {
            Destroy(oldPom.gameObject);
        }

        if (needCleanReload)
        {
            LoadSceneWithAnimatedScreen("Outdoors");
            return;
        }

        TeleportPlayerToInitialOutdoorsSpawn();
        ResumeGame();
        InitializeSceneSquads();
        UpdateHUD();
    }

    // לחיצה על Continue - שומרת בקשיחות על התפקיד המקורי (SelectedRole) ולא מאפשרת החלפת תפקיד באמצע משחק!
    public void OnContinueButtonClicked()
    {
        if (!GameHasStarted || HealthSystem.MatchEnded)
        {
            OnNewGameButtonClicked();
            return;
        }

        PendingMenuRole = SelectedRole; // מבטל כל ניסיון לשנות תפקיד תוך כדי משחק פעיל!
        if (musicAudioSource != null) musicAudioSource.Stop();
        ResumeGame();
        UpdateHUD();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        ShowExclusivePanel(gamePanel);
        SetPlayerControl(true);
        UpdateHUD();
    }

    public void TriggerEndOfMatch(bool playerWon, string message)
    {
        if (playerWon && SceneManager.GetActiveScene().name != "Outdoors")
        {
            VictoryCinematicDirector.PendingOutdoorsCutscene = true;
            VictoryCinematicDirector.CutsceneRole = SelectedRole;
            VictoryCinematicDirector.CutsceneHeadlineMessage = message;
            Time.timeScale = 1f;
            SceneManager.LoadScene("Outdoors");
            return;
        }

        HealthSystem.MatchEnded = true;
        GameHasStarted = false;
        isPaused = true;
        Time.timeScale = 0f;
        SetPlayerControl(false);

        if (!playerWon)
        {
            PlayMusicTrack(defeatMusic, loop: false);

            if (defeatScreenPanel != null)
            {
                ShowExclusivePanel(defeatScreenPanel);
                StartCoroutine(DelayedDefeatButtonRoutine(message, 7f));
            }
            else
            {
                OpenMainMenuScreen();
            }
        }
        else
        {
            AudioClip winClip = bystanderSurvivalMusic;
            if (SelectedRole == PlayerRole.Robber) winClip = robberVictoryMusic;
            else if (SelectedRole == PlayerRole.Police) winClip = policeVictoryMusic;

            PlayMusicTrack(winClip, loop: false);

            float exactDuration = VictoryCinematicDirector.GetExactCutsceneDuration(SelectedRole);
            StartCoroutine(PlayVictoryCutsceneRoutine(message, exactDuration));
        }
    }

    private IEnumerator DelayedDefeatButtonRoutine(string message, float delaySeconds)
    {
        isDefeatButtonLocked = true;

        Button defeatMenuBtn = defeatScreenPanel.GetComponentInChildren<Button>(true);
        if (defeatMenuBtn != null)
        {
            defeatMenuBtn.gameObject.SetActive(false);
        }

        if (defeatReasonText != null)
        {
            defeatReasonText.text = message;
        }

        yield return new WaitForSecondsRealtime(delaySeconds);

        isDefeatButtonLocked = false;

        if (defeatMenuBtn != null)
        {
            defeatMenuBtn.gameObject.SetActive(true);
        }

        if (defeatReasonText != null)
        {
            defeatReasonText.text = message + "\n\n<size=75%>(Click Main Menu or press ESC)</size>";
        }
    }

    private IEnumerator PlayVictoryCutsceneRoutine(string victoryMessage, float duration)
    {
        isPlayingVictoryCutscene = true;

        if (victoryCutscenePanel == null)
        {
            CreateAutomaticVictoryCutsceneUI();
        }

        Image panelBg = victoryCutscenePanel.GetComponent<Image>();
        if (panelBg != null) panelBg.color = new Color(0f, 0f, 0f, 0.18f);

        ShowExclusivePanel(victoryCutscenePanel);

        VictoryCinematicDirector director = GetComponent<VictoryCinematicDirector>();
        if (director == null) director = gameObject.AddComponent<VictoryCinematicDirector>();
        director.StartRoleVictoryCutscene(SelectedRole, duration);

        if (victoryVideoPlayer != null && victoryVideoPlayer.clip != null)
        {
            victoryVideoPlayer.Play();
        }

        if (scrollingCreditsTransform != null)
        {
            scrollingCreditsTransform.gameObject.SetActive(false);
        }

        if (victoryHeadlineText != null)
        {
            RectTransform headlineRect = victoryHeadlineText.GetComponent<RectTransform>();
            if (headlineRect != null)
            {
                headlineRect.anchorMin = new Vector2(0.5f, 0.5f);
                headlineRect.anchorMax = new Vector2(0.5f, 0.5f);
                headlineRect.pivot = new Vector2(0.5f, 0.5f);
                headlineRect.anchoredPosition = Vector2.zero;
                headlineRect.sizeDelta = new Vector2(1500f, 300f);
            }

            victoryHeadlineText.alignment = TextAlignmentOptions.Center;
            victoryHeadlineText.text = victoryMessage;
            victoryHeadlineText.gameObject.SetActive(true);
        }

        float elapsed = 0f;
        float headlineDuration = Mathf.Min(7f, duration);
        while (elapsed < headlineDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (victoryHeadlineText != null)
        {
            victoryHeadlineText.gameObject.SetActive(false);
        }

        if (scrollingCreditsTransform != null)
        {
            scrollingCreditsTransform.gameObject.SetActive(true);
            scrollingCreditsTransform.anchoredPosition = new Vector2(0f, -750f);

            float scrollSpeed = Mathf.Max(creditsScrollSpeed, 135f);
            float creditsEndHeight = 1250f;

            while (elapsed < duration && scrollingCreditsTransform.anchoredPosition.y < creditsEndHeight)
            {
                scrollingCreditsTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.unscaledDeltaTime;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            scrollingCreditsTransform.gameObject.SetActive(false);
        }

        if (victoryHeadlineText != null)
        {
            victoryHeadlineText.text = "<b><size=56><color=#FFD700>THANK YOU FOR PLAYING!</color></size></b>\n\n<size=36>We hope you enjoyed The Great Bank Heist</size>";
            victoryHeadlineText.gameObject.SetActive(true);
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (victoryVideoPlayer != null)
        {
            victoryVideoPlayer.Stop();
        }

        isPlayingVictoryCutscene = false;
        sceneNeedsReloadOnNewGame = true;
        OpenMainMenuScreen();
    }

    private void CreateAutomaticVictoryCutsceneUI()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) parentCanvas = FindAnyObjectByType<Canvas>();
        if (parentCanvas == null) return;

        victoryCutscenePanel = new GameObject("AutoVictoryCutscenePanel", typeof(RectTransform), typeof(Image));
        victoryCutscenePanel.transform.SetParent(parentCanvas.transform, false);
        RectTransform panelRect = victoryCutscenePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        victoryCutscenePanel.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.06f, 0.22f);

        GameObject creditsObj = new GameObject("ScrollingCreditsText", typeof(RectTransform), typeof(TextMeshProUGUI));
        creditsObj.transform.SetParent(victoryCutscenePanel.transform, false);
        scrollingCreditsTransform = creditsObj.GetComponent<RectTransform>();
        scrollingCreditsTransform.sizeDelta = new Vector2(900f, 1200f);

        TextMeshProUGUI tmp = creditsObj.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 32;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.text = "<b><size=46><color=#FFD700>THE GREAT BANK HEIST</color></size></b>\n\n" +
                   "MISSION COMPLETED!\n\n" +
                   "<b>Created & Directed By:</b>\nDavid Ben-Yakov\n\n" +
                   "<b>Starring Squad Members:</b>\n" +
                   "Commander (Player)\n" +
                   "Robber Assault Team & SWAT Police Force\n" +
                   "The Brave Bank Customers\n\n" +
                   "<b>Game Architecture & AI:</b>\n" +
                   "3v3 NavMesh Tactical Combat System\n" +
                   "Dynamic Role Scenarios & Multi-Scene Pursuit\n\n" +
                   "<b>Thank You For Playing!</b>";

        victoryHeadlineText = tmp;
    }

    public void LoadSceneWithAnimatedScreen(string targetScene)
    {
        StopAllCoroutines();
        StartCoroutine(AnimatedLoadingRoutine(targetScene));
    }

    private IEnumerator AnimatedLoadingRoutine(string targetScene)
    {
        Time.timeScale = 1f;
        ShowExclusivePanel(loadingScreenPanel);

        string[] loadingStates = { "Loading", "Loading.", "Loading..", "Loading..." };
        float elapsed = 0f;
        int step = 0;

        while (elapsed < 1.5f)
        {
            if (loadingAnimatedText != null)
            {
                loadingAnimatedText.text = loadingStates[step % loadingStates.Length];
            }
            step++;
            elapsed += 0.3f;
            yield return new WaitForSecondsRealtime(0.3f);
        }

        SceneManager.LoadScene(targetScene);
    }

    private void ResetAllGameStaticData()
    {
        GameHasStarted = false;
        HealthSystem.MatchEnded = false;
        HealthSystem.SavedUnitHP.Clear();
        HealthSystem.RobberDeathsCount = 0;
        HealthSystem.PoliceDeathsCount = 0;
        SquadMemberAI.DeadUnitsRegistry.Clear();
        SquadMemberAI.EnemiesPursuingPlayer = false;
        SquadMemberAI.PersistentSquadState = SquadMemberAI.AIState.FollowCommander;
        HeistAlarm.IsHeistActive = false;
        HeistAlarm.IsSirenPlaying = false;
        HeistAlarm.SirenFinished = false;
        HeistAlarm.FirefightStarted = false;
        HeistAlarm.FirefightEnded = false;
        HeistAlarm.AllVaultCoinsLooted = false;
        HeistAlarm.RobberSquadOrderedToMoveOut = false;
        HeistAlarm.HeistCountdownEndTime = -1f;
        KeyPickup.HasKey = false;
        KeyPickup.KeyWasCollectedOnce = false;
        PersistentObjectManager.NumGoldCoins = 0;
        CoinBehaviour.NumCoins = 0;
        PortalBehavior.isReturningToOutdoors = false;
        PortalBehavior.isReturningFromVaultToBank = false;

        PlayerHP = GetMaxHPForRole(SelectedRole);
    }

    public static int GetMaxHPForRole(PlayerRole r)
    {
        if (r == PlayerRole.Police) return 180;
        if (r == PlayerRole.Robber) return 160;
        return 120;
    }

    public void OnOptionsButtonClicked()
    {
        SetupInteractiveOptionsPanelUI();
        RefreshOptionsUI();
        ShowExclusivePanel(optionsPanel);
    }

    public void OnBackFromOptionsClicked()
    {
        OpenMainMenuScreen();
    }

    public void OnExitButtonClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    // בחירת תפקיד בתפריט: אם יש משחק פעיל, התפקיד נשמר רק עבור New Game ולא משנה את התפקיד של המשחק הנוכחי!
    public void SelectRobber() { SetPendingRole(PlayerRole.Robber); }
    public void SelectPolice() { SetPendingRole(PlayerRole.Police); }
    public void SelectBystander() { SetPendingRole(PlayerRole.Bystander); }

    private void SetPendingRole(PlayerRole newRole)
    {
        PendingMenuRole = newRole;

        if (!GameHasStarted || HealthSystem.MatchEnded)
        {
            SelectedRole = newRole;
            PlayerHP = GetMaxHPForRole(newRole);
        }

        UpdateRoleSelectionText();
    }

    private void UpdateRoleSelectionText()
    {
        if (selectedRoleStatusText == null) return;

        int previewHP = GetMaxHPForRole(PendingMenuRole);
        if (GameHasStarted && !HealthSystem.MatchEnded && PendingMenuRole != SelectedRole)
        {
            selectedRoleStatusText.text = $"Active Role: {SelectedRole} (New Game Role: {PendingMenuRole} - Click NEW GAME to switch)";
        }
        else
        {
            selectedRoleStatusText.text = $"Selected Role: {PendingMenuRole} (HP: {previewHP})";
        }
    }

    public void ProcessSecretInput()
    {
        if (secretInputField == null) return;
        string input = secretInputField.text.Trim().ToUpper();
        PlayerCodeOrName = input;

        if (input == "HEIST" || input == "VIP")
        {
            PersistentObjectManager.NumGoldCoins += 50;
            CoinBehaviour.NumCoins = PersistentObjectManager.NumGoldCoins;
            PlayerHP += 50;
        }
        else if (input == "KEY")
        {
            KeyPickup.HasKey = true;
        }
    }

    public void UpdateHUD()
    {
        if (hudRoleAndHPText != null)
            hudRoleAndHPText.text = $"Role: {SelectedRole} | HP: {PlayerHP} | Diff: {CurrentDifficulty}";
    }

    private void PlayMusicTrack(AudioClip clip, bool loop)
    {
        if (musicAudioSource == null || clip == null) return;
        musicAudioSource.mute = IsMusicMuted;
        musicAudioSource.Stop();
        musicAudioSource.clip = clip;
        musicAudioSource.loop = loop;
        musicAudioSource.Play();
    }

    private void InitializeSceneSquads()
    {
        SquadMemberAI[] allUnits = FindObjectsByType<SquadMemberAI>();
        foreach (SquadMemberAI unit in allUnits) unit.InitializeRoleAndScenePlacement();
    }

    private void SetPlayerControl(bool active)
    {
        FindPlayerIfNeeded();
        if (playerMovementScript != null) playerMovementScript.enabled = active;
        Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !active;
    }

    private void FindPlayerIfNeeded()
    {
        if (playerMovementScript == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerMovementScript = playerObj.GetComponent<PlayerBehavior>();
        }
    }
}