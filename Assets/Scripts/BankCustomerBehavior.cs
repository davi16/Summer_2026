using UnityEngine;
using System.Collections;

public class BankCustomerBehavior : MonoBehaviour
{
    private Animator animator;
    private bool isOnFloor = false;

    [Header("Optional Reaction Audio")]
    public AudioSource panicVoiceAudio; // אופציונלי: צעקת בהלה כשמתחיל השוד

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        // אם השוד כבר התחיל לפני כן (למשל כשחוזרים מחדר הכספות לבנק), הלקוח כבר שוכב
        if (HeistAlarm.IsHeistActive)
        {
            DropToFloorImmediate();
        }
    }

    public void TriggerPanicAndLieDown()
    {
        if (isOnFloor) return;
        isOnFloor = true;

        // השהיה אקראית קטנה (בין 0 ל-0.4 שניות) כדי שהלקוחות בתור יגיבו בצורה טבעית
        float randomReactionDelay = Random.Range(0f, 0.4f);
        StartCoroutine(LieDownRoutine(randomReactionDelay));
    }

    private IEnumerator LieDownRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (animator != null)
        {
            animator.SetBool("LieDown", true);
        }

        if (panicVoiceAudio != null)
        {
            panicVoiceAudio.Play();
        }
    }

    private void DropToFloorImmediate()
    {
        isOnFloor = true;
        if (animator != null)
        {
            animator.SetBool("LieDown", true);
        }
    }
}