using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehavior : MonoBehaviour
{
    public GameObject PlayerCamera; // public means that it must be connected in Unity
    public float walkSpeed = 7.5f;   // walking speed, currently set as half the sprinting speed
    public float sprintSpeed = 15f;  // original sprinting speed
    private float currentSpeed;      // helper variable to store the current speed
    float angular_speed = 3;
    public GameObject Drawer; // only in pub scene
    public GameObject RegularCrosshair; // only in pub scene
    public GameObject TouchCrosshair; // only in pub scene
    public GameObject Drawertext; // only in pub scene
    public GameObject GunInDrawer; // only in pub scene
    public GameObject GunInHand; // only in pub scene
    public GameObject PickGunText; // only in pub scene
    public GameObject Alarm; // only in pub scene

    CharacterController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>(); // initialization
    }

    // Update is called once per frame
    void Update()
    {
        // setting the current speed based on whether LeftShift is pressed or not. If LeftShift is pressed, the player will sprint; otherwise, they will walk.
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // set the speed to walkSpeed if LeftShift is not pressed, otherwise set it to sprintSpeed
        float dx = Input.GetAxis("Horizontal") * currentSpeed;
        float dz = Input.GetAxis("Vertical") * currentSpeed;

        float RotationAboutY = Input.GetAxis("Mouse X") * 3f;
        float RotationAboutX = -Input.GetAxis("Mouse Y") * 3f;

        PlayerCamera.transform.Rotate(RotationAboutX, 0, 0);
        transform.Rotate(new Vector3(0, RotationAboutY, 0));

        Vector3 motion = new Vector3(dx, 0, dz); 
        motion = transform.TransformDirection(motion); 
        
        // smart gravity: if the player is grounded, we apply a small downward force to keep them grounded; if they are in the air, we apply a realistic gravity force.
        if (controller.isGrounded)
        {
            motion.y = -2f; // weak force to keep the player grounded when they are on the ground
        }
        else
        {
            motion.y = -9.81f; // realistic gravity force when the player is in the air
        }

        // the multiplication by Time.deltaTime is necessary to make the movement frame-rate independent, ensuring consistent movement speed regardless of the frame rate.
        controller.Move(motion * Time.deltaTime);

        // check the sight of player
        if (SceneManager.GetActiveScene().buildIndex == 1) // Assuming "Pub" scene has build index 1
        {
            RaycastHit hit;
            if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit, 3f))
            {
                if (hit.collider.gameObject == Drawer)
                {
                    RegularCrosshair.SetActive(false);
                    TouchCrosshair.SetActive(true);
                    Drawertext.SetActive(true);
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        GameObject Cabinet = Drawer.transform.parent.gameObject; // get the parent of the drawer, which is the cabinet
                        Animator animator = Cabinet.GetComponent<Animator>();
                        animator.SetBool("Open", !animator.GetBool("Open")); // toggle the "Open" parameter
                        if (animator.GetBool("Open"))
                        {
                            Collider GunCollider = GunInDrawer.GetComponent<Collider>();
                            GunCollider.enabled = true; // enable the collider of the gun when the cabinet is open
                        }
                    }
                }
                else{
                    RegularCrosshair.SetActive(true);
                    TouchCrosshair.SetActive(false);
                    Drawertext.SetActive(false);
                    if (hit.collider.gameObject == GunInDrawer)
                    {
                        if (GunInDrawer.gameObject.activeSelf) // check if the gun is active in the drawer
                        {
                            PickGunText.SetActive(true);
                            if (Input.GetKeyDown(KeyCode.E))
                            {
                                GunInDrawer.SetActive(false); // hide the gun when picked up
                                GunInHand.SetActive(true); // show the gun in hand when picked up
                                Alarm.SetActive(true); // show the alarm when the gun is picked up
                                AudioSource alarmAudio = Alarm.GetComponent<AudioSource>();
                                alarmAudio.Play(); // play the alarm sound when the gun is picked up
                                PickGunText.SetActive(false); // hide the pick gun text when the gun is picked up
                                PersistentObjectManager.HasGun = true; // set the HasGun variable in PersistentObjectManager to true
                            }
                        }
                    }
                }
            }
        }
    }
}
