using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public GameObject PlayerCamera; // public means that it must be connected in Unity
    public float walkSpeed = 7.5f;   // walking speed, currently set as half the sprinting speed
    public float sprintSpeed = 15f;  // original sprinting speed
    private float currentSpeed;      // helper variable to store the current speed
    float angular_speed = 3;

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
    }
}
