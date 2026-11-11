using UnityEngine;

public class BankClerkBehavior : MonoBehaviour
{
    [Header("References")]
    [Tooltip("גרור לכאן את אובייקט המפתח (KeyPickup) שמונח על הדלפק")]
    public GameObject vaultKeyOnCounter;
    public float interactionDistance = 4.5f;

    private bool keyPlacedOnCounter = false;
    private bool alarmTriggeredByKeyPickup = false;
    private bool bystanderWithdrawalDone = false;
    private bool isClerkDead = false;
    private Animator animator;
    private Transform playerTransform;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        // מוצא אוטומטית את המפתח בסצנה אם לא נגרר ידנית
        if (vaultKeyOnCounter == null)
        {
            KeyPickup kp = FindAnyObjectByType<KeyPickup>(FindObjectsInactive.Include);
            if (kp != null) vaultKeyOnCounter = kp.gameObject;
        }

        // כשמשחקים כשודד ועוד לא איימנו על הפקידה - המפתח מוסתר!
        if (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Robber &&
            !KeyPickup.HasKey && !KeyPickup.KeyWasCollectedOnce)
        {
            if (vaultKeyOnCounter != null) vaultKeyOnCounter.SetActive(false);
        }
    }

    void Update()
    {
        if (!MenuAndGameManager.GameHasStarted || Time.timeScale == 0f || isClerkDead || playerTransform == null) return;

        var role = MenuAndGameManager.SelectedRole;
        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // --- תרחיש 1: שודד ניגש פיזית לפקידה ומכוון עליה את הנשק ---
        if (role == MenuAndGameManager.PlayerRole.Robber)
        {
            if (!keyPlacedOnCounter && distToPlayer <= interactionDistance)
            {
                if (IsPlayerAimingAtClerk())
                {
                    RevealKeyOnCounter();
                }
            }

            // ברגע שהשודד לקח את המפתח מהדלפק - הפקידה מפעילה את האזעקה ומתחיל טיימר של דקה!
            if (keyPlacedOnCounter && !alarmTriggeredByKeyPickup && (KeyPickup.HasKey || KeyPickup.KeyWasCollectedOnce))
            {
                alarmTriggeredByKeyPickup = true;
                HeistAlarm alarm = FindAnyObjectByType<HeistAlarm>();
                if (alarm != null) alarm.StartRobberHeistAfterKeyPickup();
            }
        }
        // --- תרחיש 2: עובר אורח ניגש לדלפק ולוחץ E למשיכת כספים ---
        else if (role == MenuAndGameManager.PlayerRole.Bystander)
        {
            if (!bystanderWithdrawalDone && distToPlayer <= interactionDistance && Input.GetKeyDown(KeyCode.E))
            {
                bystanderWithdrawalDone = true;
                PersistentObjectManager.NumGoldCoins += 10;
                CoinBehaviour.NumCoins = PersistentObjectManager.NumGoldCoins;

                HeistAlarm alarm = FindAnyObjectByType<HeistAlarm>();
                if (alarm != null) alarm.TriggerBystanderHeistSequence();
            }
        }
    }

    private bool IsPlayerAimingAtClerk()
    {
        if (Camera.main == null) return false;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionDistance + 1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                return true;
            }
        }
        return false;
    }

    private void RevealKeyOnCounter()
    {
        keyPlacedOnCounter = true;

        if (vaultKeyOnCounter != null)
        {
            vaultKeyOnCounter.SetActive(true);
        }

        // כל האזרחים בבנק נשכבים מפחד
        BankCustomerBehavior[] customers = FindObjectsByType<BankCustomerBehavior>();
        foreach (var c in customers) c.TriggerPanicAndLieDown();

        if (StoryAndDialogueManager.Instance != null)
        {
            StoryAndDialogueManager.Instance.ShowSingleDialogue(
                "Bank Clerk (Terrified)",
                "Please don't shoot!! I put the Vault Key right here on the counter! Take it with [E]!",
                5f
            );
        }
    }

    // מאפשר לשודד לחסל את הפקידה אם הוא מתעצבן אחרי שהיא הפעילה את האזעקה
    public void OnClerkShotByPlayer()
    {
        if (isClerkDead) return;
        isClerkDead = true;

        if (animator != null)
        {
            animator.SetBool("LieDown", true);
            animator.SetTrigger("Die");
        }
        else
        {
            transform.localRotation = Quaternion.Euler(90f, transform.localEulerAngles.y, 0f);
        }

        if (StoryAndDialogueManager.Instance != null)
        {
            StoryAndDialogueManager.Instance.ShowSingleDialogue(
                "Commander (You)",
                "That's what you get for pressing the silent alarm! Guys, into the Vault NOW - we have 60 seconds!",
                4.5f
            );
        }
    }
}