using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalBehavior : MonoBehaviour
{
    private bool isNear = false;
    private Collider playerCollider;

    void Update()
    {
        // נבדוק אם השחקן נמצא באזור הטריגר של הדלת ולחץ על המקש E
        if (isNear && Input.GetKeyDown(KeyCode.E))
        {
            // מפעיל את תהליך המעבר הכולל השהיה קלה
            StartCoroutine(TransitionToScene());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // מזהים שהשחקן התקרב לדלת
        if (other.CompareTag("Player"))
        {
            isNear = true;
            playerCollider = other; // שומרים את השחקן כדי שניקח ממנו את המיקום שלו בהמשך
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // מזהים שהשחקן התרחק מהדלת ומונעים מעבר מרחוק
        if (other.CompareTag("Player"))
        {
            isNear = false;
            playerCollider = null;
        }
    }

    private IEnumerator TransitionToScene()
    {
        // מכבים מיד את האפשרות ללחוץ שוב פעמיים
        isNear = false;

        // ממתינים חצי שנייה כדי לתת לאנימציית הפתיחה והסאונד זמן להתנגן
        yield return new WaitForSeconds(0.5f);

        // עדכון המטבעות
        PersistentObjectManager.NumGoldCoins = CoinBehaviour.NumCoins;

        // שמירת נקודת החזרה
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Outdoors"))
        {
            if (PersistentObjectManager.Instance != null)
            {
                // נשמור את המיקום המדויק שבו השחקן עמד כשלחץ על E
                PersistentObjectManager.Instance.UpdateSpawnPointScene0(playerCollider.transform.position);
            }
            else
            {
                Debug.LogWarning("PersistentObjectManager is missing in this scene!");
            }
        }

        // טעינת הסצנה הבאה
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Outdoors"))
        {
            SceneManager.LoadScene("Pub");
        }
        else
        {
            SceneManager.LoadScene("Outdoors");
        }
    }
}