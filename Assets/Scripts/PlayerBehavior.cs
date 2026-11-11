using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    public GameObject PlayerCamera;
    public float walkSpeed = 7.5f;
    public float sprintSpeed = 15f;

    [Header("Player Footstep Audio (Rule 8)")]
    public AudioSource footstepAudioSource;
    public AudioClip footstepClip;

    private float currentSpeed;
    private float verticalRotation = 0f;
    private float nextFootstepTime = 0f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (PlayerCamera == null && Camera.main != null)
        {
            PlayerCamera = Camera.main.gameObject;
        }

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
        if (!MenuAndGameManager.GameHasStarted || Time.timeScale == 0f || HealthSystem.MatchEnded) return;

        HandleMovementAndLook();

        // סעיף 9: השחקן עצמו יכול לזרוק רימון פיזי בלחיצה על G!
        if (Input.GetKeyDown(KeyCode.G) && MenuAndGameManager.SelectedRole != MenuAndGameManager.PlayerRole.Bystander)
        {
            SquadMemberAI.Team myTeam = (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Police)
                ? SquadMemberAI.Team.Police : SquadMemberAI.Team.Robber;

            Transform cam = (PlayerCamera != null) ? PlayerCamera.transform : transform;
            Vector3 startPos = cam.position + cam.forward * 0.8f;
            Vector3 targetPos = cam.position + cam.forward * 11f;

            GrenadeProjectile.LaunchPhysicalGrenade(startPos, targetPos, myTeam, "Commander Grenade");
        }
    }

    private void HandleMovementAndLook()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        float dx = Input.GetAxis("Horizontal") * currentSpeed;
        float dz = Input.GetAxis("Vertical") * currentSpeed;

        float mouseX = Input.GetAxis("Mouse X") * 3f;
        float mouseY = Input.GetAxis("Mouse Y") * 3f;

        transform.Rotate(0, mouseX, 0);

        if (PlayerCamera != null)
        {
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
            PlayerCamera.transform.localEulerAngles = new Vector3(verticalRotation, 0, 0);
        }

        Vector3 motion = transform.TransformDirection(new Vector3(dx, 0, dz));
        motion.y = (controller != null && controller.isGrounded) ? -2f : -9.81f;

        if (controller != null && controller.enabled)
        {
            controller.Move(motion * Time.deltaTime);

            // סעיף 8: השמעת צליל צעדים כשהשחקן הולך או רץ על הקרקע
            bool isWalking = (Mathf.Abs(dx) > 0.1f || Mathf.Abs(dz) > 0.1f) && controller.isGrounded;
            if (isWalking && Time.time >= nextFootstepTime)
            {
                nextFootstepTime = Time.time + (isSprinting ? 0.30f : 0.46f);
                if (footstepAudioSource != null)
                {
                    if (footstepClip != null) footstepAudioSource.PlayOneShot(footstepClip, 0.5f);
                    else footstepAudioSource.Play();
                }
            }
        }
    }
}