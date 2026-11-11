using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Role-Specific Weapon Models")]
    public GameObject robberPistolModel;
    public GameObject policeWeaponModel;

    [Header("Weapon Settings")]
    public int weaponDamage = 25;
    public float weaponRange = 45f;
    public float fireCooldown = 0.22f;

    [Header("Recoil Animation")]
    public Vector3 recoilKickbackOffset = new Vector3(0f, 0.04f, -0.11f);
    public Vector3 recoilRotationOffset = new Vector3(-12f, 0f, 0f);
    public float recoilReturnSpeed = 12f;

    [Header("Dynamic Crosshair Settings")]
    public Color normalCrosshairColor = new Color(0.2f, 1f, 0.35f, 0.95f);
    public Color enemyCrosshairColor = new Color(1f, 0.15f, 0.15f, 1f);
    public float baseCrosshairGap = 16f;
    public float shootSpreadKick = 22f;
    public float spreadRecoverySpeed = 10f;

    [Header("Audio, Light & Physical Muzzle Flash")]
    public AudioSource gunshotAudio;
    public Light muzzleFlashLight;
    public GameObject physicalMuzzleFlashObject;
    public Vector3 autoFlashLocalOffset = new Vector3(0f, 0.06f, 0.45f);

    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private Vector3 currentPosOffset;
    private Vector3 currentRotOffset;
    private float nextShootTime = 0f;

    // רכיבי הכוונת שנוצרים אוטומטית במרכז המסך
    private GameObject crosshairRoot;
    private Image[] crosshairParts = new Image[5]; // 0=Dot, 1=Top, 2=Bottom, 3=Left, 4=Right
    private RectTransform[] crosshairRects = new RectTransform[5];
    private float currentSpread;

    void Start()
    {
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
        currentSpread = baseCrosshairGap;

        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;

        if (physicalMuzzleFlashObject == null)
        {
            physicalMuzzleFlashObject = CreateAutoPhysicalMuzzleFlash(transform, autoFlashLocalOffset);
        }
        physicalMuzzleFlashObject.SetActive(false);

        CreateAutoCrosshairUI();
        UpdateWeaponModelsByRole();
    }

    void Update()
    {
        UpdateWeaponModelsByRole();
        UpdateCrosshairBehavior();

        if (!MenuAndGameManager.GameHasStarted || Time.timeScale == 0f || HealthSystem.MatchEnded) return;
        if (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Bystander) return;

        currentPosOffset = Vector3.Lerp(currentPosOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
        currentRotOffset = Vector3.Lerp(currentRotOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);

        transform.localPosition = initialLocalPos + currentPosOffset;
        transform.localRotation = initialLocalRot * Quaternion.Euler(currentRotOffset);

        if (Input.GetMouseButtonDown(0) && Time.time >= nextShootTime)
        {
            nextShootTime = Time.time + fireCooldown;
            FireWeapon();
        }
    }

    private void UpdateWeaponModelsByRole()
    {
        bool inActiveGame = MenuAndGameManager.GameHasStarted && !HealthSystem.MatchEnded && Time.timeScale > 0f;
        var role = MenuAndGameManager.SelectedRole;

        if (robberPistolModel != null && policeWeaponModel != null)
        {
            robberPistolModel.SetActive(inActiveGame && role == MenuAndGameManager.PlayerRole.Robber);
            policeWeaponModel.SetActive(inActiveGame && role == MenuAndGameManager.PlayerRole.Police);
        }
        else
        {
            bool showDefault = inActiveGame && role != MenuAndGameManager.PlayerRole.Bystander;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (physicalMuzzleFlashObject != null && r.transform.IsChildOf(physicalMuzzleFlashObject.transform)) continue;
                r.enabled = showDefault;
            }
        }
    }

    private void UpdateCrosshairBehavior()
    {
        if (crosshairRoot == null) CreateAutoCrosshairUI();
        if (crosshairRoot == null) return;

        bool shouldShowCrosshair = MenuAndGameManager.GameHasStarted &&
                                   !HealthSystem.MatchEnded &&
                                   Time.timeScale > 0f &&
                                   MenuAndGameManager.SelectedRole != MenuAndGameManager.PlayerRole.Bystander;

        if (crosshairRoot.activeSelf != shouldShowCrosshair)
        {
            crosshairRoot.SetActive(shouldShowCrosshair);
        }

        if (!shouldShowCrosshair) return;

        // חזרה חלקה של קווי הכוונת למרחק הבסיסי לאחר ירייה
        currentSpread = Mathf.Lerp(currentSpread, baseCrosshairGap, Time.deltaTime * spreadRecoverySpeed);
        if (crosshairRects[1] != null) crosshairRects[1].anchoredPosition = new Vector2(0f, currentSpread);
        if (crosshairRects[2] != null) crosshairRects[2].anchoredPosition = new Vector2(0f, -currentSpread);
        if (crosshairRects[3] != null) crosshairRects[3].anchoredPosition = new Vector2(-currentSpread, 0f);
        if (crosshairRects[4] != null) crosshairRects[4].anchoredPosition = new Vector2(currentSpread, 0f);

        // בדיקה אם השחקן מכוון על אויב כדי לצבוע את הכוונת באדום
        bool aimingAtEnemy = false;
        Camera cam = GetComponentInParent<Camera>();
        if (cam == null) cam = Camera.main;

        if (cam != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, weaponRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                HealthSystem targetHealth = hit.collider.GetComponentInParent<HealthSystem>();
                if (targetHealth != null && !targetHealth.isPlayer && targetHealth.currentHP > 0)
                {
                    aimingAtEnemy = true;
                }
            }
        }

        Color targetColor = aimingAtEnemy ? enemyCrosshairColor : normalCrosshairColor;
        for (int i = 0; i < crosshairParts.Length; i++)
        {
            if (crosshairParts[i] != null) crosshairParts[i].color = targetColor;
        }
    }

    private void FireWeapon()
    {
        currentPosOffset = recoilKickbackOffset;
        currentRotOffset = recoilRotationOffset;
        currentSpread = baseCrosshairGap + shootSpreadKick; // פתיחת הכוונת בירי

        if (gunshotAudio != null) gunshotAudio.Play();
        StartCoroutine(MuzzleFlashRoutine());

        Camera cam = GetComponentInParent<Camera>();
        if (cam == null) cam = Camera.main;

        if (cam != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, weaponRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                HealthSystem targetHealth = hit.collider.GetComponentInParent<HealthSystem>();
                BankClerkBehavior clerk = hit.collider.GetComponentInParent<BankClerkBehavior>();
                if (clerk != null && HeistAlarm.IsHeistActive)
                {
                    clerk.OnClerkShotByPlayer();
                }
                if (targetHealth != null && !targetHealth.isPlayer)
                {
                    targetHealth.TakeDamage(weaponDamage, "Commander (" + MenuAndGameManager.SelectedRole + ")");
                }
            }
        }

        HeistAlarm alarm = FindAnyObjectByType<HeistAlarm>();
        if (alarm != null && !HeistAlarm.IsHeistActive &&
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Bank")
        {
            alarm.StartHeist();
        }
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        if (muzzleFlashLight != null) muzzleFlashLight.enabled = true;
        if (physicalMuzzleFlashObject != null)
        {
            physicalMuzzleFlashObject.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            physicalMuzzleFlashObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.06f);

        if (muzzleFlashLight != null) muzzleFlashLight.enabled = false;
        if (physicalMuzzleFlashObject != null) physicalMuzzleFlashObject.SetActive(false);
    }

    // מייצר אוטומטית את הכוונת במרכז ה-GamePanel
    private void CreateAutoCrosshairUI()
    {
        Transform parentPanel = null;
        MenuAndGameManager menu = FindAnyObjectByType<MenuAndGameManager>();
        if (menu != null && menu.gamePanel != null)
        {
            parentPanel = menu.gamePanel.transform;
        }
        else
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null) parentPanel = canvas.transform;
        }

        if (parentPanel == null) return;

        Transform existing = parentPanel.Find("TacticalCrosshairRoot");
        if (existing != null) Destroy(existing.gameObject);

        crosshairRoot = new GameObject("TacticalCrosshairRoot", typeof(RectTransform));
        crosshairRoot.transform.SetParent(parentPanel, false);
        RectTransform rootRect = crosshairRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(100f, 100f);

        // 0: נקודה מרכזית, 1: קו עליון, 2: קו תחתון, 3: קו שמאלי, 4: קו ימני
        CreateCrosshairBar(0, "CenterDot", new Vector2(5f, 5f), Vector2.zero);
        CreateCrosshairBar(1, "TopBar", new Vector2(4f, 16f), new Vector2(0f, baseCrosshairGap));
        CreateCrosshairBar(2, "BottomBar", new Vector2(4f, 16f), new Vector2(0f, -baseCrosshairGap));
        CreateCrosshairBar(3, "LeftBar", new Vector2(16f, 4f), new Vector2(-baseCrosshairGap, 0f));
        CreateCrosshairBar(4, "RightBar", new Vector2(16f, 4f), new Vector2(baseCrosshairGap, 0f));
    }

    private void CreateCrosshairBar(int index, string partName, Vector2 size, Vector2 pos)
    {
        GameObject barObj = new GameObject(partName, typeof(RectTransform), typeof(Image));
        barObj.transform.SetParent(crosshairRoot.transform, false);

        RectTransform rect = barObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;

        Image img = barObj.GetComponent<Image>();
        img.color = normalCrosshairColor;
        img.raycastTarget = false;

        crosshairRects[index] = rect;
        crosshairParts[index] = img;
    }

    public static GameObject CreateAutoPhysicalMuzzleFlash(Transform parentTransform, Vector3 localPos)
    {
        GameObject flashRoot = new GameObject("PhysicalMuzzleFlash");
        flashRoot.transform.SetParent(parentTransform, false);
        flashRoot.transform.localPosition = localPos;

        Material flashMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        if (flashMat.shader == null || flashMat.shader.name == "Hidden/InternalErrorShader")
            flashMat = new Material(Shader.Find("Sprites/Default"));
        flashMat.color = new Color(1f, 0.78f, 0.15f, 1f);

        for (int i = 0; i < 2; i++)
        {
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(blade.GetComponent<Collider>());
            blade.transform.SetParent(flashRoot.transform, false);
            blade.transform.localScale = (i == 0) ? new Vector3(0.14f, 0.04f, 0.22f) : new Vector3(0.04f, 0.14f, 0.22f);
            blade.GetComponent<Renderer>().sharedMaterial = flashMat;
        }

        return flashRoot;
    }
}