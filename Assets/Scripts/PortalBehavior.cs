using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PortalBehavior : MonoBehaviour
{
    // Static variables to store the player's position and rotation when transitioning between scenes
    public static bool isReturningToOutdoors = false;
    public static Vector3 savedOutdoorsPosition;
    public static Quaternion savedOutdoorsRotation;
    private bool isNear = false;
    private Collider playerCollider;

    void Update()
    {
        if (isNear && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TransitionToScene());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = true;
            playerCollider = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = false;
            playerCollider = null;
        }
    }

    private IEnumerator TransitionToScene()
    {
        isNear = false;
        yield return new WaitForSeconds(0.5f);

        PersistentObjectManager.NumGoldCoins = CoinBehaviour.NumCoins;

        if (SceneManager.GetActiveScene().name == "Outdoors")
        {
            // Save the player's position and rotation before transitioning to the Pub scene
            savedOutdoorsPosition = playerCollider.transform.position;
            savedOutdoorsRotation = playerCollider.transform.rotation;
            isReturningToOutdoors = true;

            if (PersistentObjectManager.Instance != null)
            {
                PersistentObjectManager.Instance.UpdateSpawnPointScene0(playerCollider.transform.position);
            }
        }

        if (SceneManager.GetActiveScene().name == "Outdoors")
        {
            SceneManager.LoadScene("Pub");
        }
        else
        {
            SceneManager.LoadScene("Outdoors");
        }
    }
}