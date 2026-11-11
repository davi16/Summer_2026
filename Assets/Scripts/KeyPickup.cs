using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public static bool HasKey = false;
    public static bool KeyWasCollectedOnce = false;
    public GameObject playerCamera;
    public GameObject pickUpText;
    public AudioSource keySound;
    public float pickupDistance = 3.2f;

    private Transform playerTransform;

    void OnEnable()
    {
        // מוודא שהקוליידר של המפתח דלוק ברגע שהפקידה מניחה אותו על הדלפק!
        foreach (Collider col in GetComponentsInChildren<Collider>(true))
        {
            col.enabled = true;
        }
    }

    void Start()
    {
        if (HasKey || KeyWasCollectedOnce)
        {
            gameObject.SetActive(false);
            return;
        }

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.gameObject;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        if (pickUpText != null) pickUpText.SetActive(false);
    }

    void Update()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.gameObject;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        bool canPickUp = false;

        // 1. בדיקת קרן (כולל תמיכה ב-Trigger כדי שהשולחן לא יחסום)
        if (playerCamera != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    canPickUp = true;
                }
            }
        }

        // 2. גיבוי מרחק: אם השחקן עומד צמוד לדלפק מול המפתח (מונע חסימה על ידי הקוליידר של השולחן)
        if (!canPickUp && playerTransform != null)
        {
            float dist = Vector3.Distance(playerTransform.position, transform.position);
            if (dist <= pickupDistance)
            {
                canPickUp = true;
            }
        }

        if (canPickUp)
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
        if (keySound != null) keySound.Play();

        if (StoryAndDialogueManager.Instance != null)
        {
            StoryAndDialogueManager.Instance.ShowSingleDialogue("Vault Key Collected", "You grabbed the Vault Key! Open the locked door to enter the Vault!", 4f);
        }

        gameObject.SetActive(false);
    }
}