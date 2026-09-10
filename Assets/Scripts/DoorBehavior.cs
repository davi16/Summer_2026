using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    Animator animator;
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    
    public GameObject actionText; 

    public BoxCollider solidCollider;

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
        // Check if the player is near the door and presses the "E" key to open it
        if (isNear && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            isOpen = true;
            animator.SetBool("Open", isOpen); 

            if (solidCollider != null) { solidCollider.enabled = false; }

            if (openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
            
            // Set the door state to open
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
            
            // Always show the action text when the player is near the door, regardless of whether it's open or closed
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
            
            // Always remove the action text when the player moves away from the door
            if (actionText != null)
            {
                actionText.SetActive(false); 
            }

            // Automatic closing when the player leaves the collider area
            if (isOpen)
            {
                isOpen = false;
                animator.SetBool("Open", isOpen);

                if (solidCollider != null) { solidCollider.enabled = true; }

                if (closeSound != null)
                {
                    audioSource.PlayOneShot(closeSound);
                }
            }
        }
    }
}