using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HealthSystem : MonoBehaviour
{
    public static Dictionary<string, int> SavedUnitHP = new Dictionary<string, int>();
    public static bool MatchEnded = false;
    public static int RobberDeathsCount = 0;
    public static int PoliceDeathsCount = 0;

    [Header("Character Identity & Health")]
    public string characterDisplayName = "Unit #1";
    public bool isPlayer = false;
    public int maxHP = 100;
    public int currentHP = 100;

    [Header("Audio Clips (Hurt & Death Sounds - Rule 8)")]
    public AudioSource characterAudioSource;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("UI References")]
    public Slider hpSlider;
    public Transform npcCanvasTransform;
    public TMP_Text combatStatusBannerText;

    private SquadMemberAI squadAI;
    private string uniqueSaveKey;
    private static Coroutine activeMessageRoutine;

    void Start()
    {
        squadAI = GetComponent<SquadMemberAI>();
        if (characterAudioSource == null) characterAudioSource = GetComponent<AudioSource>();

        foreach (MonoBehaviour mb in GetComponents<MonoBehaviour>())
        {
            if (mb != null && mb != this && mb != squadAI)
            {
                string typeName = mb.GetType().Name;
                if (typeName.Contains("Monster") || typeName.Contains("AIBehav") || typeName.Contains("Health"))
                {
                    mb.enabled = false;
                }
            }
        }

        if (isPlayer)
        {
            maxHP = (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Police) ? 180 :
                    (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Robber) ? 160 : 120;
            currentHP = MenuAndGameManager.PlayerHP;
            characterDisplayName = "Commander (" + MenuAndGameManager.SelectedRole.ToString() + ")";
        }
        else if (squadAI != null)
        {
            uniqueSaveKey = squadAI.GetUniqueID();
            if (SavedUnitHP.ContainsKey(uniqueSaveKey))
            {
                currentHP = SavedUnitHP[uniqueSaveKey];
            }
            else
            {
                currentHP = maxHP;
                SavedUnitHP[uniqueSaveKey] = currentHP;
            }
        }

        // סעיף 2: איתור בר החיים הראשי וכיבוי אוטומטי של כל בר חיים / Canvas כפול (סטטי) מעל הראש של הדמות!
        if (!isPlayer)
        {
            Slider[] allSliders = GetComponentsInChildren<Slider>(true);
            if (allSliders.Length > 0)
            {
                if (hpSlider == null) hpSlider = allSliders[0];

                for (int i = 0; i < allSliders.Length; i++)
                {
                    if (allSliders[i] != hpSlider)
                    {
                        allSliders[i].gameObject.SetActive(false);
                    }
                }
            }

            if (hpSlider != null)
            {
                Canvas activeCanvas = hpSlider.GetComponentInParent<Canvas>();
                if (activeCanvas != null) npcCanvasTransform = activeCanvas.transform;

                Canvas[] allCanvases = GetComponentsInChildren<Canvas>(true);
                foreach (Canvas c in allCanvases)
                {
                    if (c != activeCanvas)
                    {
                        c.gameObject.SetActive(false);
                    }
                }
            }
        }

        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = maxHP;
            hpSlider.wholeNumbers = true;
            hpSlider.value = currentHP;
        }
    }

    void LateUpdate()
    {
        // כופה בכל פריים ב-LateUpdate את ערך החיים האמיתי על ה-Slider כדי שלעולם לא יתאפס למעלה!
        if (hpSlider != null)
        {
            if (hpSlider.maxValue != maxHP) hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        if (!isPlayer && npcCanvasTransform != null && Camera.main != null)
        {
            npcCanvasTransform.LookAt(
                npcCanvasTransform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }

    public void TakeDamage(int damage, string attackerName)
    {
        if (currentHP <= 0 || MatchEnded) return;

        currentHP = Mathf.Max(0, currentHP - damage);

        if (isPlayer)
        {
            MenuAndGameManager.PlayerHP = currentHP;
            MenuAndGameManager menu = FindAnyObjectByType<MenuAndGameManager>();
            if (menu != null) menu.UpdateHUD();

            // סעיף 8: השמעת קול פציעה אצל השחקן!
            if (currentHP > 0 && hurtSound != null)
            {
                if (characterAudioSource != null) characterAudioSource.PlayOneShot(hurtSound);
                else AudioSource.PlayClipAtPoint(hurtSound, transform.position, 1.0f);
            }
        }
        else if (!string.IsNullOrEmpty(uniqueSaveKey))
        {
            SavedUnitHP[uniqueSaveKey] = currentHP;
        }

        UpdateHealthUI();

        if (currentHP > 0)
        {
            ShowCombatBanner(characterDisplayName + " hit by " + attackerName + "! (HP: " + currentHP + ")", 2.5f);
        }
        else
        {
            HandleDeath(attackerName);
        }
    }

    private void UpdateHealthUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    private void HandleDeath(string killerName)
    {
        // סעיף 8: השמעת קול מוות של חבר הצוות / הדמות!
        if (deathSound != null)
        {
            if (characterAudioSource != null) characterAudioSource.PlayOneShot(deathSound);
            else AudioSource.PlayClipAtPoint(deathSound, transform.position, 1.0f);
        }

        ShowCombatBanner(characterDisplayName + " was eliminated by " + killerName + "!", 3.5f);

        if (StoryAndDialogueManager.Instance != null)
        {
            StoryAndDialogueManager.Instance.OnCharacterKilledNarrativeReaction(characterDisplayName);
        }

        if (isPlayer)
        {
            DeclareMatchOutcome(playerWon: false, "DEFEAT! You were killed in action!");
            return;
        }

        if (squadAI != null)
        {
            if (squadAI.myTeam == SquadMemberAI.Team.Robber) RobberDeathsCount++;
            else if (squadAI.myTeam == SquadMemberAI.Team.Police) PoliceDeathsCount++;

            squadAI.TriggerDeath();
        }

        if (npcCanvasTransform != null)
        {
            npcCanvasTransform.gameObject.SetActive(false);
        }

        CheckTeamVictoryOrDefeat();
    }

    public void CheckTeamVictoryOrDefeat()
    {
        if (MatchEnded) return;

        var role = MenuAndGameManager.SelectedRole;

        // --- סעיף 7: אם 2 חברי צוות בקבוצה שלך נהרגו - הקבוצה שלך מפסידה מיד כי אדם אחד לא יכול להשלים את המשימה לבד! ---
        if (role == MenuAndGameManager.PlayerRole.Robber && RobberDeathsCount >= 2)
        {
            DeclareMatchOutcome(false, "DEFEAT! Both Daniel & Max were killed! One person cannot complete the heist alone!");
            return;
        }
        if (role == MenuAndGameManager.PlayerRole.Police && PoliceDeathsCount >= 2)
        {
            DeclareMatchOutcome(false, "DEFEAT! Both Sgt. Roi & Officer Maya fell in battle! You cannot complete the mission alone!");
            return;
        }

        // --- סעיף 7 ליריבים: אם 2 חברי צוות של הקבוצה היריבה נהרגו, הקבוצה היריבה מפסידה (והשלישי מנוטרל) ---
        if (RobberDeathsCount >= 2)
        {
            NeutralizeRemainingLoneSurvivor(SquadMemberAI.Team.Robber);
        }
        if (PoliceDeathsCount >= 2)
        {
            NeutralizeRemainingLoneSurvivor(SquadMemberAI.Team.Police);
        }

        bool robbersDefeated = (RobberDeathsCount >= 2);
        bool policeDefeated = (PoliceDeathsCount >= 2);

        if (robbersDefeated || policeDefeated)
        {
            HeistAlarm.FirefightEnded = true;
        }

        if (role == MenuAndGameManager.PlayerRole.Police && robbersDefeated)
        {
            DeclareMatchOutcome(true, "VICTORY! The Robber squad was neutralized and the Bank heist is foiled!");
        }
        else if (role == MenuAndGameManager.PlayerRole.Robber && policeDefeated)
        {
            ShowCombatBanner("SWAT Squad neutralized! Exit the Bank to the street and reach the Getaway Vehicle!", 6f);
        }
        else if (role == MenuAndGameManager.PlayerRole.Bystander && HeistAlarm.FirefightEnded)
        {
            ShowCombatBanner("THE SHOOTOUT IS OVER! You can now exit the Bank to the street to win!", 6f);
        }
    }

    private void NeutralizeRemainingLoneSurvivor(SquadMemberAI.Team defeatedTeam)
    {
        SquadMemberAI[] units = FindObjectsByType<SquadMemberAI>();
        foreach (SquadMemberAI unit in units)
        {
            if (unit.myTeam == defeatedTeam && !unit.isDead)
            {
                unit.TriggerDeath();
                HealthSystem hp = unit.GetComponent<HealthSystem>();
                if (hp != null && hp.npcCanvasTransform != null)
                    hp.npcCanvasTransform.gameObject.SetActive(false);
            }
        }
    }

    public void DeclareMatchOutcome(bool playerWon, string outcomeMessage)
    {
        MatchEnded = true;
        ShowCombatBanner(outcomeMessage, 8f);

        MenuAndGameManager menu = FindAnyObjectByType<MenuAndGameManager>();
        if (menu != null)
        {
            menu.TriggerEndOfMatch(playerWon, outcomeMessage);
        }
    }

    public void ShowCombatBanner(string message, float duration)
    {
        TMP_Text targetText = combatStatusBannerText;

        if (targetText == null)
        {
            SquadCommander cmd = FindAnyObjectByType<SquadCommander>();
            if (cmd != null) targetText = cmd.commandNotificationText;
        }

        if (targetText != null && targetText.gameObject.scene.IsValid())
        {
            MonoBehaviour runner = FindAnyObjectByType<MenuAndGameManager>();
            if (runner == null) runner = this;

            if (activeMessageRoutine != null) runner.StopCoroutine(activeMessageRoutine);
            activeMessageRoutine = runner.StartCoroutine(BannerRoutine(targetText, message, duration));
        }
    }

    private static IEnumerator BannerRoutine(TMP_Text textUI, string msg, float duration)
    {
        textUI.text = msg;
        textUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        if (!MatchEnded && textUI != null)
        {
            textUI.gameObject.SetActive(false);
        }
    }
}