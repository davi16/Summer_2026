using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public static bool HasKey = false;
    public static bool KeyWasCollectedOnce = false;
    public GameObject playerCamera;
    public GameObject pickUpText; // KeyPickupText מה-Canvas
    public AudioSource keySound;

    void Start()
    {
        if (HasKey || KeyWasCollectedOnce)
        {
            gameObject.SetActive(false);
        }
        if (HasKey)
        {
            gameObject.SetActive(false);
        }

        if (pickUpText != null)
        {
            pickUpText.SetActive(false);
        }
    }

    void Update()
    {
        if (playerCamera == null) return;

        RaycastHit hit;
        // שולחים קרן שמתעלמת מקוליידרים שהם Trigger!
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // בודק אם הקרן פגעה במפתח עצמו או בילד שלו (Tag7)
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                if (pickUpText != null && !pickUpText.activeSelf)
                {
                    pickUpText.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                {
                    CollectKey();
                }
                return;
            }
        }

        if (pickUpText != null && pickUpText.activeSelf)
        {
            pickUpText.SetActive(false);
        }
    }

    private void CollectKey()
    {
        HasKey = true;

        KeyWasCollectedOnce = true;

        if (pickUpText != null) pickUpText.SetActive(false);

        if (keySound != null)
        {
            keySound.Play();
        }

        gameObject.SetActive(false);
    }
}