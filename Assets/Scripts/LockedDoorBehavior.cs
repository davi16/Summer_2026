using UnityEngine;
using UnityEngine.UI; // או TMPro אם הטקסט שלך הוא TextMeshPro
using System.Collections;

public class LockedDoorBehavior : MonoBehaviour
{
    public GameObject playerCamera;
    
    [Header("UI Messages")]
    public GameObject lockedMessageObject;   // אובייקט טקסט: "You need a key to open this door"
    public GameObject unlockPromptObject;    // אובייקט טקסט: "Press E to use Key and open door"
    public GameObject keyUsedMessageObject;  // אובייקט טקסט: "Key used! Door unlocked"

    private bool isNear = false;
    private bool isUnlockedAndOpen = false;
    private Animator animator;
    private AudioSource doorAudio;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        doorAudio = GetComponent<AudioSource>();

        if (lockedMessageObject != null) lockedMessageObject.SetActive(false);
        if (unlockPromptObject != null) unlockPromptObject.SetActive(false);
        if (keyUsedMessageObject != null) keyUsedMessageObject.SetActive(false);
    }

    void Update()
    {
        // אם הדלת כבר נפתחה, לא מציגים יותר הודעות נעילה
        if (isUnlockedAndOpen) return;

        if (isNear && playerCamera != null)
        {
            RaycastHit hit;
            // בודק שהשחקן מסתכל על הדלת (מתעלם מקוליידר ה-Trigger)
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    // מצב 1: לשחקן יש את המפתח!
                    if (KeyPickup.HasKey)
                    {
                        if (lockedMessageObject != null) lockedMessageObject.SetActive(false);
                        if (unlockPromptObject != null) unlockPromptObject.SetActive(true);

                        // לחיצה על E או Space כדי להשתמש במפתח ולפתוח את הדלת
                        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                        {
                            UnlockAndOpenDoor();
                        }
                    }
                    // מצב 2: לשחקן אין מפתח!
                    else
                    {
                        if (unlockPromptObject != null) unlockPromptObject.SetActive(false);
                        if (lockedMessageObject != null) lockedMessageObject.SetActive(true);
                    }
                    return;
                }
            }
        }

        // אם השחקן התרחק או הסיט מבט מהדלת - מסתירים את ההודעות
        HidePrompts();
    }

    private void UnlockAndOpenDoor()
    {
        isUnlockedAndOpen = true;

        // הוצאת המפתח מהזיכרון של השחקן!
        KeyPickup.HasKey = false;

        HidePrompts();

        // הפעלת אנימציית פתיחת הדלת
        if (animator != null)
        {
            animator.SetBool("Open", true);
        }

        // השמעת סאונד פתיחת דלת (אם קיים)
        if (doorAudio != null)
        {
            doorAudio.Play();
        }

        // הצגת הודעת "השתמשת במפתח" למשך 3 שניות
        if (keyUsedMessageObject != null)
        {
            StartCoroutine(ShowKeyUsedNotification());
        }
    }

    private IEnumerator ShowKeyUsedNotification()
    {
        keyUsedMessageObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        keyUsedMessageObject.SetActive(false);
    }

    private void HidePrompts()
    {
        if (lockedMessageObject != null && lockedMessageObject.activeSelf)
            lockedMessageObject.SetActive(false);
        if (unlockPromptObject != null && unlockPromptObject.activeSelf)
            unlockPromptObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = false;
            HidePrompts();
        }
    }
}