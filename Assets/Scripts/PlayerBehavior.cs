using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    public GameObject PlayerCamera;
    public float walkSpeed = 7.5f;
    public float sprintSpeed = 15f;
    private float currentSpeed;
    private float verticalRotation = 0f;

    [Header("Pub Scene References")]
    public GameObject Drawer;
    public GameObject RegularCrosshair;
    public GameObject TouchCrosshair;
    public GameObject Drawertext;
    public GameObject GunInDrawer;
    public GameObject GunInHand;
    public GameObject PickGunText;
    public GameObject Alarm;
    public MonsterBehavior monster;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // נעילת והעלמת סמן העכבר
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // שמירה על הנשק ביד אם השחקן כבר אסף אותו קודם
        if (PersistentObjectManager.HasGun && GunInHand != null)
        {
            GunInHand.SetActive(true);
        }

        // שיגור השחקן אל מחוץ למבנה עם הפנים קדימה (תומך גם בפאב וגם בבית החדש)
        if (SceneManager.GetActiveScene().name == "Outdoors")
        {
            if (PortalBehavior.isReturningToOutdoors)
            {
                RepositionPlayer(PortalBehavior.savedOutdoorsPosition, PortalBehavior.savedOutdoorsRotation);
                PortalBehavior.isReturningToOutdoors = false;
            }
        }
    }

    private void RepositionPlayer(Vector3 savedPos, Quaternion savedRot)
    {
        if (controller != null) controller.enabled = false;
        transform.position = savedPos + (savedRot * Vector3.forward * -1.5f);
        transform.rotation = savedRot * Quaternion.Euler(0, 180f, 0);
        if (controller != null) controller.enabled = true;
    }

    void Update()
    {
        HandleMovementAndLook();

        // בדיקת אינטראקציה רק אם אנחנו בפאב
        if (SceneManager.GetActiveScene().name == "Pub")
        {
            HandlePubInteractions();
        }
    }

    private void HandleMovementAndLook()
    {
        // מהירות ריצה / הליכה
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        float dx = Input.GetAxis("Horizontal") * currentSpeed;
        float dz = Input.GetAxis("Vertical") * currentSpeed;

        // סיבוב מצלמה ושחקן (כולל הגנה אם PlayerCamera ריק)
        float mouseX = Input.GetAxis("Mouse X") * 3f;
        float mouseY = Input.GetAxis("Mouse Y") * 3f;

        transform.Rotate(0, mouseX, 0);

        if (PlayerCamera != null)
        {
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f); // מונע היפוך של הראש
            PlayerCamera.transform.localEulerAngles = new Vector3(verticalRotation, 0, 0);
        }

        // פיזיקה ותנועה
        Vector3 motion = transform.TransformDirection(new Vector3(dx, 0, dz));
        motion.y = (controller != null && controller.isGrounded) ? -2f : -9.81f;

        if (controller != null && controller.enabled)
        {
            CollisionFlags flags = controller.Move(motion * Time.deltaTime);
            Debug.Log($"Pos: {transform.position} | Flags: {flags} | isGrounded: {controller.isGrounded}");
            // Debug.Log($"Pos: {transform.position} | Motion: {motion} | dt: {Time.deltaTime} | CC Enabled: {controller.enabled}");
            // controller.Move(motion * Time.deltaTime);
        }
    }

    private void HandlePubInteractions()
    {
        if (PlayerCamera == null) return;

        RaycastHit hit;
        bool hitSomething = Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hit, 3f);

        if (hitSomething)
        {
            if (Drawer != null && hit.collider.gameObject == Drawer)
            {
                SetCrosshairState(isInteracting: true);
                if (Drawertext != null) Drawertext.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    GameObject cabinet = Drawer.transform.parent.gameObject;
                    Animator animator = cabinet.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetBool("Open", !animator.GetBool("Open"));
                        if (animator.GetBool("Open") && GunInDrawer != null)
                        {
                            Collider gunCollider = GunInDrawer.GetComponent<Collider>();
                            if (gunCollider != null) gunCollider.enabled = true;
                        }
                    }
                }
                return;
            }
            else if (GunInDrawer != null && hit.collider.gameObject == GunInDrawer && GunInDrawer.activeSelf)
            {
                SetCrosshairState(isInteracting: true);
                if (PickGunText != null) PickGunText.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    GunInDrawer.SetActive(false);
                    if (GunInHand != null) GunInHand.SetActive(true);
                    if (Alarm != null)
                    {
                        Alarm.SetActive(true);
                        AudioSource alarmAudio = Alarm.GetComponent<AudioSource>();
                        if (alarmAudio != null) alarmAudio.Play();
                    }
                    if (monster != null) monster.startWalking();
                    if (PickGunText != null) PickGunText.SetActive(false);
                    PersistentObjectManager.HasGun = true;
                }
                return;
            }
        }

        // איפוס תצוגה אם השחקן לא מביט באובייקט רלוונטי
        SetCrosshairState(isInteracting: false);
        if (Drawertext != null) Drawertext.SetActive(false);
        if (PickGunText != null) PickGunText.SetActive(false);
    }

    private void SetCrosshairState(bool isInteracting)
    {
        if (RegularCrosshair != null) RegularCrosshair.SetActive(!isInteracting);
        if (TouchCrosshair != null) TouchCrosshair.SetActive(isInteracting);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.name != "Floor" && hit.gameObject.name != "Plane")
        {
            Debug.Log("Blocked while moving by: " + hit.gameObject.name);
        }
    }
}