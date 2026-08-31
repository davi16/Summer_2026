using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public GameObject PlayerCamera; // public means that it must be connected in Unity
    public float walkSpeed = 7.5f;   // חצי מהמהירות הקודמת להליכה רגילה
    public float sprintSpeed = 15f;  // המהירות המקורית עבור ריצה
    private float currentSpeed;      // משתנה עזר שיחזיק את המהירות הפעילה כרגע
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
        // בדיקה האם מקש השיפט השמאלי לחוץ
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        float dx, dz;
        float RotationAboutY = Input.GetAxis("Mouse X") * 3f;
        float RotationAboutX = -Input.GetAxis("Mouse Y") * 3f;

        PlayerCamera.transform.Rotate(RotationAboutX, 0, 0);

        transform.Rotate(new Vector3(0, RotationAboutY,0));

        // שים לב: צריך להחליף את המילה 'speed' במילה 'currentSpeed' בשתי השורות הבאות:
        dz = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;
        dx = Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime;

        //        transform.Translate(new Vector3(dx, 0, dz));
        Vector3 motion = new Vector3(dx, -1, dz); // -1 is gravity
        motion = transform.TransformDirection(motion); // transforms motion to global coordinates
        controller.Move(motion);//global coordinates
    }
}
