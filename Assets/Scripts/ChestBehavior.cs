using UnityEngine;

public class ChestBehavior : MonoBehaviour
{
    public GameObject playerCamera; 
    public Collider keyCollider; 
    public GameObject chestPromptText; 
    
    private bool isNear = false;
    public bool isOpen = false;
    private Animator animator;
    private AudioSource chestAudio; // משתנה עבור הסאונד של התיבה

    void Start()
    {
        // מחפש את ה-Animator על התיבה או על אחד הילדים שלה (כמו המכסה)!
        animator = GetComponentInChildren<Animator>();
        
        // תופס את ה-Audio Source שעל התיבה
        chestAudio = GetComponent<AudioSource>();
        
        if (keyCollider != null)
        {
            keyCollider.enabled = false;
        }

        if (chestPromptText != null)
        {
            chestPromptText.SetActive(false);
        }
    }

    void Update()
    {
        if (isOpen)
        {
            if (chestPromptText != null && chestPromptText.activeSelf)
            {
                chestPromptText.SetActive(false);
            }
            return;
        }

        if (isNear)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    if (chestPromptText != null && !chestPromptText.activeSelf)
                    {
                        chestPromptText.SetActive(true);
                    }

                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        isOpen = true;
                        if (chestPromptText != null) chestPromptText.SetActive(false);

                        // מעביר את התיבה ואת המכסה שלה ל-Ignore Raycast, אבל משאיר את המפתח רגיל!
                        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
                        foreach (Transform child in GetComponentsInChildren<Transform>(true))
                        {
                            // מוודאים שאנחנו לא משנים את השכבה של המפתח או הילדים שלו (Tag7)
                            if (keyCollider != null && (child == keyCollider.transform || child.IsChildOf(keyCollider.transform)))
                            {
                                continue;
                            }
                            child.gameObject.layer = ignoreRaycastLayer;
                        }
                        
                        // הפעלת האנימציה
                        if (animator != null) 
                        {
                            animator.SetBool("Open", true);
                        }
                        else
                        {
                            Debug.LogWarning("לא נמצא רכיב Animator על התיבה או על הילדים שלה!");
                        }

                        // הפעלת סאונד פתיחת התיבה
                        if (chestAudio != null)
                        {
                            chestAudio.Play();
                        }

                        // הפעלת הקוליידר של המפתח שבתוך התיבה
                        if (keyCollider != null) 
                        {
                            keyCollider.enabled = true;
                        }
                    }
                    return;
                }
            }
        }

        if (chestPromptText != null && chestPromptText.activeSelf)
        {
            chestPromptText.SetActive(false);
        }
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
            if (chestPromptText != null) chestPromptText.SetActive(false);
        }
    }
}