using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SquadCommander : MonoBehaviour
{
    [Header("UI Notification")]
    public TMP_Text commandNotificationText;

    [Header("Radio Command Audio")]
    [Tooltip("ה-AudioSource שמשמיע את קול הפקודה בקשר (מזוהה אוטומטית אם נמצא על השחקן)")]
    public AudioSource commandRadioAudio;
    [Tooltip("אופציונלי: קובץ השמע של הפקודה בקשר")]
    public AudioClip commandRadioClip;

    private Coroutine notificationRoutine;

    void Start()
    {
        EnsureTopRightCommandTextExists();
        FindRadioAudioSourceIfNeeded();
    }

    private void FindRadioAudioSourceIfNeeded()
    {
        if (commandRadioAudio != null) return;

        // מחפש AudioSource שעל האובייקט הנוכחי (למעט זה של הצעדים ב-PlayerBehavior)
        PlayerBehavior pb = GetComponent<PlayerBehavior>();
        AudioSource[] sources = GetComponentsInChildren<AudioSource>(true);

        foreach (AudioSource src in sources)
        {
            if (pb != null && src == pb.footstepAudioSource) continue;
            if (src.gameObject.name.Contains("Weapon") || src.gameObject.name.Contains("Pistol")) continue;

            commandRadioAudio = src;
            break;
        }

        if (commandRadioAudio == null && sources.Length > 0)
        {
            commandRadioAudio = sources[0];
        }
    }

    void Update()
    {
        if (!MenuAndGameManager.GameHasStarted || Time.timeScale == 0f || HealthSystem.MatchEnded) return;
        if (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Bystander) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            IssueCommandToTeammates(1, "COMMAND [1]: Squad Open Fire & Attack!");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            IssueCommandToTeammates(2, "COMMAND [2]: Squad Throw Grenades!");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            IssueCommandToTeammates(3, "COMMAND [3]: Squad Tactical Retreat!");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            IssueCommandToTeammates(4, "COMMAND [4]: Squad Regroup & Follow Me!");
        }
    }

    private void IssueCommandToTeammates(int cmdIndex, string bannerMsg)
    {
        // השמעת קול הפקודה בקשר!
        PlayRadioCommandSound();

        SquadMemberAI[] allUnits = FindObjectsByType<SquadMemberAI>();
        foreach (SquadMemberAI unit in allUnits)
        {
            if (unit.isPlayerTeammate && !unit.isDead)
            {
                if (cmdIndex == 1) unit.CommandShoot();
                else if (cmdIndex == 2) unit.CommandThrowBomb();
                else if (cmdIndex == 3) unit.CommandRetreat();
                else if (cmdIndex == 4) unit.CommandFollow();
            }
        }

        ShowTopRightCommandNotification(bannerMsg, 3.5f);
    }

    private void PlayRadioCommandSound()
    {
        FindRadioAudioSourceIfNeeded();

        if (commandRadioAudio != null)
        {
            commandRadioAudio.spatialBlend = 0f; // 2D כדי שיישמע חזק וברור באוזנייה של השחקן
            if (commandRadioClip != null)
            {
                commandRadioAudio.PlayOneShot(commandRadioClip, 1f);
            }
            else if (commandRadioAudio.clip != null)
            {
                commandRadioAudio.PlayOneShot(commandRadioAudio.clip, 1f);
            }
        }
        else if (commandRadioClip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(commandRadioClip, Camera.main.transform.position, 1f);
        }
    }

    public void ShowTopRightCommandNotification(string msg, float duration)
    {
        EnsureTopRightCommandTextExists();
        if (commandNotificationText == null) return;

        if (notificationRoutine != null) StopCoroutine(notificationRoutine);
        notificationRoutine = StartCoroutine(DisplayRoutine(msg, duration));
    }

    private IEnumerator DisplayRoutine(string msg, float duration)
    {
        commandNotificationText.text = msg;
        commandNotificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        if (commandNotificationText != null)
            commandNotificationText.gameObject.SetActive(false);
    }

    private void EnsureTopRightCommandTextExists()
    {
        if (commandNotificationText != null) return;

        MenuAndGameManager menu = FindAnyObjectByType<MenuAndGameManager>();
        Transform parentPanel = (menu != null && menu.gamePanel != null) ? menu.gamePanel.transform : null;
        if (parentPanel == null)
        {
            Canvas c = FindAnyObjectByType<Canvas>();
            if (c != null) parentPanel = c.transform;
        }
        if (parentPanel == null) return;

        Transform existing = parentPanel.Find("AutoTopRightCommandText");
        if (existing != null)
        {
            commandNotificationText = existing.GetComponent<TMP_Text>();
            return;
        }

        GameObject cmdObj = new GameObject("AutoTopRightCommandText", typeof(RectTransform), typeof(TextMeshProUGUI));
        cmdObj.transform.SetParent(parentPanel, false);
        RectTransform rt = cmdObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-30f, -30f);
        rt.sizeDelta = new Vector2(750f, 90f);

        TextMeshProUGUI tmp = cmdObj.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 34;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.TopRight;
        tmp.color = new Color(0.2f, 1f, 0.4f);
        tmp.raycastTarget = false;

        commandNotificationText = tmp;
        cmdObj.SetActive(false);
    }
}