using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalBehavior : MonoBehaviour
{
    [Header("Scene Transition")]
    public string targetSceneName = "Outdoors";
    public GameObject portalPromptText;

    // שמירת מיקום החזרה לרחוב (Outdoors)
    public static bool isReturningToOutdoors = false;
    public static Vector3 savedOutdoorsPosition;
    public static Quaternion savedOutdoorsRotation;

    // שמירת מיקום החזרה ללובי/מדרגות הבנק (כשחוזרים מסצנת Vault לסצנת Bank)
    public static bool isReturningFromVaultToBank = false;
    public static Vector3 savedBankVaultDoorPosition;
    public static Quaternion savedBankVaultDoorRotation;

    private bool isNear = false;
    private Collider playerCollider;

    void Start()
    {
        if (portalPromptText == null)
        {
            CreateAutoPortalPromptUI();
        }
        if (portalPromptText != null) portalPromptText.SetActive(false);

        // אם השחקן חזר הרגע מסצנת הכספת (Vault) אל סצנת הבנק - נמקם אותו מול דלת הכספת!
        if (SceneManager.GetActiveScene().name == "Bank" && isReturningFromVaultToBank)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                CharacterController cc = playerObj.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                playerObj.transform.position = savedBankVaultDoorPosition + (savedBankVaultDoorRotation * Vector3.forward * -1.8f);
                playerObj.transform.rotation = savedBankVaultDoorRotation * Quaternion.Euler(0f, 180f, 0f);
                if (cc != null) cc.enabled = true;
            }
            isReturningFromVaultToBank = false;
        }
    }

    private void CreateAutoPortalPromptUI()
    {
        MenuAndGameManager menu = FindAnyObjectByType<MenuAndGameManager>();
        Transform parentPanel = (menu != null && menu.gamePanel != null) ? menu.gamePanel.transform : null;
        if (parentPanel == null)
        {
            Canvas c = FindAnyObjectByType<Canvas>();
            if (c != null) parentPanel = c.transform;
        }
        if (parentPanel == null) return;

        Transform existing = parentPanel.Find("AutoDoorPortalPrompt");
        if (existing != null)
        {
            portalPromptText = existing.gameObject;
            return;
        }

        GameObject promptObj = new GameObject("AutoDoorPortalPrompt", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        promptObj.transform.SetParent(parentPanel, false);
        RectTransform rt = promptObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -110f);
        rt.sizeDelta = new Vector2(850f, 80f);

        TMPro.TextMeshProUGUI tmp = promptObj.GetComponent<TMPro.TextMeshProUGUI>();
        tmp.text = "Press <b><color=#FFD700>[E]</color></b> to Open Door";
        tmp.fontSize = 38;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        portalPromptText = promptObj;
    }

    void Update()
    {
        if (isNear && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Transition());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = true;
            playerCollider = other;
            if (portalPromptText != null) portalPromptText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = false;
            playerCollider = null;
            if (portalPromptText != null) portalPromptText.SetActive(false);
        }
    }

    private IEnumerator Transition()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 1. בדיקת חוקי יציאה מהבנק לרחוב (Outdoors)
        if (currentScene == "Bank" && targetSceneName == "Outdoors")
        {
            string blockReason;
            if (!HeistAlarm.CanPlayerLeaveBankToOutdoors(out blockReason))
            {
                if (StoryAndDialogueManager.Instance != null)
                {
                    StoryAndDialogueManager.Instance.ShowSingleDialogue("Bank Exit Locked", blockReason, 4f);
                }
                yield break;
            }

            if (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Bystander &&
                HeistAlarm.IsHeistActive && HeistAlarm.FirefightEnded)
            {
                MenuAndGameManager menuManager = FindAnyObjectByType<MenuAndGameManager>();
                if (menuManager != null)
                {
                    menuManager.TriggerEndOfMatch(true, "SURVIVED! You escaped the Bank safely after the firefight!");
                }
                yield break;
            }
        }

        // 2. בדיקת מפתח בכניסה מהבנק לסצנת הכספת (Vault)
        if (currentScene == "Bank" && HeistAlarm.IsVaultScene(targetSceneName) &&
            !KeyPickup.HasKey && !KeyPickup.KeyWasCollectedOnce &&
            MenuAndGameManager.SelectedRole != MenuAndGameManager.PlayerRole.Police)
        {
            if (StoryAndDialogueManager.Instance != null)
            {
                StoryAndDialogueManager.Instance.ShowSingleDialogue("Vault Door Locked", "You need the Vault Key! Aim your weapon at the Clerk at the counter first!", 4f);
            }
            yield break;
        }

        isNear = false;
        if (portalPromptText != null) portalPromptText.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        PersistentObjectManager.NumGoldCoins = CoinBehaviour.NumCoins;

        if (currentScene == "Outdoors" && playerCollider != null)
        {
            savedOutdoorsPosition = playerCollider.transform.position;
            savedOutdoorsRotation = playerCollider.transform.rotation;
            isReturningToOutdoors = true;
        }
        else if (currentScene == "Bank" && HeistAlarm.IsVaultScene(targetSceneName) && playerCollider != null)
        {
            savedBankVaultDoorPosition = playerCollider.transform.position;
            savedBankVaultDoorRotation = playerCollider.transform.rotation;
            isReturningFromVaultToBank = true;
        }

        MenuAndGameManager menu = FindAnyObjectByType<MenuAndGameManager>();
        if (menu != null)
        {
            menu.LoadSceneWithAnimatedScreen(targetSceneName);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}