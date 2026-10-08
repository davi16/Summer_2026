using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalBehavior : MonoBehaviour
{
    [Header("Scene Transition")]
    public string targetSceneName = "Outdoors"; // שם הסצנה שאליה עוברים (ניתן לשינוי ב-Inspector לכל דלת!)
    public GameObject portalPromptText; // אופציונלי: טקסט "Press E to enter/exit"

    // משתנים סטטיים ששומרים את מיקום הדלת האחרונה שדרכה נכנסנו מהכפר (עובד גם לפאב וגם לבית!)
    public static bool isReturningToOutdoors = false;
    public static Vector3 savedOutdoorsPosition;
    public static Quaternion savedOutdoorsRotation;

    private bool isNear = false;
    private Collider playerCollider;

    void Start()
    {
        if (portalPromptText != null) portalPromptText.SetActive(false);
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
        isNear = false;
        if (portalPromptText != null) portalPromptText.SetActive(false);

        yield return new WaitForSeconds(0.3f);

        // שמירת כמות המטבעות לפני מעבר הסצנה
        PersistentObjectManager.NumGoldCoins = CoinBehaviour.NumCoins;

        // אם אנחנו כרגע בכפר (Outdoors) ונכנסים למבנה (פאב או בית) - נשמור את המיקום המדויק של הדלת הזו!
        if (SceneManager.GetActiveScene().name == "Outdoors" && playerCollider != null)
        {
            savedOutdoorsPosition = playerCollider.transform.position;
            savedOutdoorsRotation = playerCollider.transform.rotation;
            isReturningToOutdoors = true;
        }

        SceneManager.LoadScene(targetSceneName);
    }
}