using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    Animator animator;
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    
    public GameObject actionText; 

    private bool isNear = false;
    private bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (actionText != null)
        {
            actionText.SetActive(false);
        }
    }

    void Update()
    {
        // הפתיחה תתבצע רק אם השחקן קרוב, לחץ על E, והדלת עדיין סגורה
        if (isNear && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            isOpen = true;
            animator.SetBool("Open", isOpen); 

            if (openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
            
            // מעלימים את הטקסט מיד עם הפתיחה
            if (actionText != null) 
            {
                actionText.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = true;
            
            // מציגים את הטקסט כשמתקרבים רק אם הדלת סגורה
            if (actionText != null && !isOpen)
            {
                actionText.SetActive(true); 
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = false;
            
            // תמיד מעלימים את הטקסט כשמתרחקים
            if (actionText != null)
            {
                actionText.SetActive(false); 
            }

            // סגירה אוטומטית ברגע שהשחקן עוזב את אזור ה-Collider
            if (isOpen)
            {
                isOpen = false;
                animator.SetBool("Open", isOpen);

                if (closeSound != null)
                {
                    audioSource.PlayOneShot(closeSound);
                }
            }
        }
    }
}