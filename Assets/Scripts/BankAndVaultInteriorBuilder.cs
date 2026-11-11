using UnityEngine;
using UnityEngine.SceneManagement;

public class BankAndVaultInteriorBuilder : MonoBehaviour
{
    public enum RoomType { AutoDetectBySceneName, BankLobbyWithVaultStairs, VaultRoomInterior }

    [Header("Room Selection")]
    public RoomType roomToBuild = RoomType.AutoDetectBySceneName;
    public bool buildAutomaticallyOnStart = true;

    [Header("Room Center & Dimensions")]
    public Vector3 roomCenter = Vector3.zero;
    public Vector2 lobbySize = new Vector2(22f, 26f);
    public float lobbyHeight = 4.8f;
    public Vector2 vaultSize = new Vector2(16f, 20f);
    public float vaultHeight = 4.2f;

    [Header("Scene Transition Names")]
    public string outdoorsSceneName = "Outdoors";
    public string vaultSceneName = "Vault";

    [Header("Money Bills & Coins Prefabs (In Vault Scene)")]
    public GameObject moneyBillPrefab;
    public int numberOfMoneyStacksToSpawn = 24;

    private GameObject generatedRoot;

    void Start()
    {
        if (buildAutomaticallyOnStart)
        {
            BuildSceneArchitectureIfNeeded();
        }
    }

    // --- פונקציות העזר ליצירת חומרים וקוביות (ממוקמות בראש הקובץ) ---
    private Material CreateMat(Color color, float metallic, float smoothness)
    {
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null || s.name == "Hidden/InternalErrorShader") s = Shader.Find("Standard");
        Material m = new Material(s);
        m.color = color;
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        return m;
    }

    private Material CreateTransparentMat(Color color)
    {
        Material m = CreateMat(color, 0.1f, 0.9f);
        m.color = color;
        return m;
    }

    private GameObject CreateBox(string boxName, Transform parent, Vector3 localPos, Vector3 scale, Material mat, bool keepCollider)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = boxName;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = localPos;
        box.transform.localScale = scale;
        box.GetComponent<Renderer>().sharedMaterial = mat;
        if (!keepCollider)
        {
            Collider c = box.GetComponent<Collider>();
            if (c != null)
            {
                if (Application.isPlaying) Destroy(c);
                else DestroyImmediate(c);
            }
        }
        return box;
    }

    [ContextMenu("Build Scene Interior Now")]
    public void BuildSceneArchitectureIfNeeded()
    {
        Transform existing = transform.Find("GeneratedArchitectureRoot");
        if (existing != null)
        {
            if (Application.isPlaying) return;
            DestroyImmediate(existing.gameObject);
        }

        generatedRoot = new GameObject("GeneratedArchitectureRoot");
        generatedRoot.transform.SetParent(transform, false);
        generatedRoot.transform.localPosition = roomCenter;

        string sceneName = SceneManager.GetActiveScene().name;
        bool buildBank = (roomToBuild == RoomType.BankLobbyWithVaultStairs) ||
                         (roomToBuild == RoomType.AutoDetectBySceneName && sceneName == "Bank");

        bool buildVault = (roomToBuild == RoomType.VaultRoomInterior) ||
                          (roomToBuild == RoomType.AutoDetectBySceneName && HeistAlarm.IsVaultScene(sceneName));

        if (buildBank)
        {
            ConstructGrandBankLobbyAndStairs(generatedRoot.transform);
        }
        else if (buildVault)
        {
            ConstructBankVaultInterior(generatedRoot.transform);
        }
    }

    // =========================================================================
    // 1. בניית לובי הבנק המלא: רצפה, תקרה, 4 קירות, דלפק הפקידה וירידה במדרגות
    // =========================================================================
    private void ConstructGrandBankLobbyAndStairs(Transform root)
    {
        Material floorMarble = CreateMat(new Color(0.86f, 0.84f, 0.80f), 0.15f, 0.85f);
        Material wallMarble = CreateMat(new Color(0.93f, 0.91f, 0.87f), 0.05f, 0.65f);
        Material darkMarble = CreateMat(new Color(0.22f, 0.24f, 0.28f), 0.2f, 0.85f);
        Material ceilingMat = CreateMat(new Color(0.96f, 0.95f, 0.92f), 0.0f, 0.4f);
        Material goldTrim = CreateMat(new Color(0.88f, 0.72f, 0.22f), 0.85f, 0.8f);
        Material woodPanel = CreateMat(new Color(0.38f, 0.22f, 0.12f), 0.1f, 0.6f);
        Material leatherSeat = CreateMat(new Color(0.52f, 0.12f, 0.14f), 0.1f, 0.5f);
        Material redCarpetMat = CreateMat(new Color(0.65f, 0.08f, 0.10f), 0f, 0.2f);
        Material glassMat = CreateTransparentMat(new Color(0.65f, 0.88f, 0.95f, 0.35f));

        float halfW = lobbySize.x * 0.5f;
        float halfL = lobbySize.y * 0.5f;
        float wallThick = 0.35f;

        // רצפה ותקרה
        CreateBox("BankLobbyFloor", root, new Vector3(0f, -0.15f, 0f), new Vector3(lobbySize.x, 0.3f, lobbySize.y), floorMarble, true);
        CreateBox("BankLobbyCeiling", root, new Vector3(0f, lobbyHeight + 0.15f, 0f), new Vector3(lobbySize.x + 0.4f, 0.3f, lobbySize.y + 0.4f), ceilingMat, true);

        for (int b = -2; b <= 2; b++)
        {
            CreateBox("CeilingBeam_" + b, root, new Vector3(0f, lobbyHeight - 0.1f, b * 4.8f), new Vector3(lobbySize.x, 0.2f, 0.35f), woodPanel, false);
        }

        // קירות מערב, מזרח ודרום (כניסה ראשית)
        CreateBox("WallWest", root, new Vector3(-halfW, lobbyHeight * 0.5f, 0f), new Vector3(wallThick, lobbyHeight, lobbySize.y), wallMarble, true);
        CreateBox("WallWestWoodBase", root, new Vector3(-halfW + 0.2f, 0.6f, 0f), new Vector3(0.08f, 1.2f, lobbySize.y), woodPanel, false);

        CreateBox("WallEast", root, new Vector3(halfW, lobbyHeight * 0.5f, 0f), new Vector3(wallThick, lobbyHeight, lobbySize.y), wallMarble, true);
        CreateBox("WallEastWoodBase", root, new Vector3(halfW - 0.2f, 0.6f, 0f), new Vector3(0.08f, 1.2f, lobbySize.y), woodPanel, false);

        CreateBox("WallSouth", root, new Vector3(0f, lobbyHeight * 0.5f, -halfL), new Vector3(lobbySize.x, lobbyHeight, wallThick), wallMarble, true);
        BuildBankMainEntranceDoor(root, new Vector3(0f, 0f, -halfL + 0.25f), woodPanel, goldTrim, darkMarble);

        // קיר צפוני (אחורי) עם פתח לירידה במדרגות לכספת
        float stairCenterX = halfW - 3.6f;
        float stairOpeningWidth = 3.2f;
        float leftEdgeOfOpening = stairCenterX - (stairOpeningWidth * 0.5f);
        float rightEdgeOfOpening = stairCenterX + (stairOpeningWidth * 0.5f);

        float northLeftWidth = leftEdgeOfOpening - (-halfW);
        float northLeftCenterX = -halfW + (northLeftWidth * 0.5f);
        CreateBox("WallNorth_LeftPart", root, new Vector3(northLeftCenterX, lobbyHeight * 0.5f, halfL), new Vector3(northLeftWidth, lobbyHeight, wallThick), wallMarble, true);
        CreateBox("WallNorth_WoodBase", root, new Vector3(northLeftCenterX, 0.6f, halfL - 0.2f), new Vector3(northLeftWidth, 1.2f, 0.08f), woodPanel, false);

        float northRightWidth = halfW - rightEdgeOfOpening;
        if (northRightWidth > 0.05f)
        {
            float northRightCenterX = rightEdgeOfOpening + (northRightWidth * 0.5f);
            CreateBox("WallNorth_RightPart", root, new Vector3(northRightCenterX, lobbyHeight * 0.5f, halfL), new Vector3(northRightWidth, lobbyHeight, wallThick), wallMarble, true);
        }

        float headerHeight = lobbyHeight - 3.1f;
        CreateBox("WallNorth_StairHeader", root, new Vector3(stairCenterX, 3.1f + headerHeight * 0.5f, halfL), new Vector3(stairOpeningWidth, headerHeight, wallThick), wallMarble, true);

        // שטיח אדום, עמודי שיש, ספסלי המתנה ועמודי חבלים
        CreateBox("RedCarpetRunner", root, new Vector3(-1.8f, 0.015f, -1.5f), new Vector3(2.8f, 0.02f, lobbySize.y * 0.68f), redCarpetMat, false);
        CreateBox("GoldCarpetBorderL", root, new Vector3(-3.25f, 0.018f, -1.5f), new Vector3(0.12f, 0.02f, lobbySize.y * 0.68f), goldTrim, false);
        CreateBox("GoldCarpetBorderR", root, new Vector3(-0.35f, 0.018f, -1.5f), new Vector3(0.12f, 0.02f, lobbySize.y * 0.68f), goldTrim, false);

        for (int row = -1; row <= 1; row++)
        {
            float zPos = row * 5.8f;
            CreateBankPillar(root, new Vector3(-halfW + 2.6f, 0f, zPos), wallMarble, goldTrim);
            CreateBankPillar(root, new Vector3(halfW - 2.6f, 0f, zPos), wallMarble, goldTrim);
        }

        for (int i = 0; i < 3; i++)
        {
            float zChair = -6.0f + i * 4.0f;
            CreateCustomerWaitingBench(root, new Vector3(-halfW + 1.4f, 0f, zChair), 90f, woodPanel, leatherSeat, goldTrim);
            CreateCustomerWaitingBench(root, new Vector3(halfW - 1.4f, 0f, zChair), -90f, woodPanel, leatherSeat, goldTrim);
        }

        for (int i = 0; i < 3; i++)
        {
            float zPost = -3.8f + i * 2.4f;
            CreateRopeStanchion(root, new Vector3(-3.8f, 0f, zPost), goldTrim, redCarpetMat);
            CreateRopeStanchion(root, new Vector3(0.2f, 0f, zPost), goldTrim, redCarpetMat);
        }

        // דלפק הפקידה המלא
        GameObject tellerCounter = new GameObject("TellerCounterComplete");
        tellerCounter.transform.SetParent(root, false);
        tellerCounter.transform.localPosition = new Vector3(-1.8f, 0f, halfL - 3.8f);

        CreateBox("CounterFrontDesk", tellerCounter.transform, new Vector3(0f, 0.55f, 0f), new Vector3(7.2f, 1.1f, 0.8f), woodPanel, true);
        CreateBox("CounterMarbleTop", tellerCounter.transform, new Vector3(0f, 1.14f, 0f), new Vector3(7.4f, 0.1f, 1.05f), darkMarble, true);
        CreateBox("CounterSideWingL", tellerCounter.transform, new Vector3(-3.5f, 0.55f, 1.8f), new Vector3(0.6f, 1.1f, 3.6f), woodPanel, true);
        CreateBox("CounterSideWingR", tellerCounter.transform, new Vector3(3.5f, 0.55f, 1.8f), new Vector3(0.6f, 1.1f, 3.6f), woodPanel, true);
        CreateBox("BulletproofGlass", tellerCounter.transform, new Vector3(0f, 1.95f, 0f), new Vector3(7.0f, 1.25f, 0.06f), glassMat, false);
        CreateBox("TopGoldFrame", tellerCounter.transform, new Vector3(0f, 2.62f, 0f), new Vector3(7.3f, 0.16f, 0.16f), goldTrim, false);

        CreateWarmCeilingLight(root, new Vector3(-1.5f, lobbyHeight - 0.35f, -4f), goldTrim);
        CreateWarmCeilingLight(root, new Vector3(-1.5f, lobbyHeight - 0.35f, 4f), goldTrim);
        CreateWarmCeilingLight(root, new Vector3(4.5f, lobbyHeight - 0.35f, 0f), goldTrim);

        // חדר המדרגות היורד לדלת הכספת
        BuildDownstairsVaultEntranceCorridor(root, new Vector3(stairCenterX, 0f, halfL), darkMarble, wallMarble, goldTrim);
    }

    private void BuildBankMainEntranceDoor(Transform root, Vector3 pos, Material wood, Material gold, Material frameMat)
    {
        GameObject mainEntrance = new GameObject("BankMainStreetEntrance");
        mainEntrance.transform.SetParent(root, false);
        mainEntrance.transform.localPosition = pos;

        CreateBox("DoorFrameL", mainEntrance.transform, new Vector3(-1.5f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 0.4f), frameMat, true);
        CreateBox("DoorFrameR", mainEntrance.transform, new Vector3(1.5f, 1.6f, 0f), new Vector3(0.35f, 3.2f, 0.4f), frameMat, true);
        CreateBox("DoorFrameTop", mainEntrance.transform, new Vector3(0f, 3.3f, 0f), new Vector3(3.4f, 0.35f, 0.45f), gold, true);

        CreateBox("DoubleDoorLeft", mainEntrance.transform, new Vector3(-0.68f, 1.55f, 0f), new Vector3(1.28f, 3.05f, 0.14f), wood, true);
        CreateBox("DoubleDoorRight", mainEntrance.transform, new Vector3(0.68f, 1.55f, 0f), new Vector3(1.28f, 3.05f, 0.14f), wood, true);
        CreateBox("GoldHandleL", mainEntrance.transform, new Vector3(-0.15f, 1.4f, 0.12f), new Vector3(0.06f, 0.45f, 0.08f), gold, false);
        CreateBox("GoldHandleR", mainEntrance.transform, new Vector3(0.15f, 1.4f, 0.12f), new Vector3(0.06f, 0.45f, 0.08f), gold, false);

        BoxCollider exitTrigger = mainEntrance.AddComponent<BoxCollider>();
        exitTrigger.isTrigger = true;
        exitTrigger.center = new Vector3(0f, 1.4f, 1.2f);
        exitTrigger.size = new Vector3(3.0f, 2.8f, 2.4f);

        PortalBehavior exitPortal = mainEntrance.AddComponent<PortalBehavior>();
        exitPortal.targetSceneName = outdoorsSceneName;
    }

    private void BuildDownstairsVaultEntranceCorridor(Transform root, Vector3 entranceLocalPos, Material darkStone, Material wallMat, Material goldMat)
    {
        GameObject stairAnnex = new GameObject("VaultStaircaseAnnex");
        stairAnnex.transform.SetParent(root, false);
        stairAnnex.transform.localPosition = entranceLocalPos;

        Material steelMat = CreateMat(new Color(0.36f, 0.39f, 0.43f), 0.85f, 0.75f);
        Material darkSteelMat = CreateMat(new Color(0.18f, 0.20f, 0.23f), 0.9f, 0.7f);

        CreateBox("StairGatePillarL", stairAnnex.transform, new Vector3(-1.55f, 1.55f, 0f), new Vector3(0.3f, 3.1f, 0.4f), darkStone, true);
        CreateBox("StairGatePillarR", stairAnnex.transform, new Vector3(1.55f, 1.55f, 0f), new Vector3(0.3f, 3.1f, 0.4f), darkStone, true);
        CreateBox("StairGateHeader", stairAnnex.transform, new Vector3(0f, 2.95f, 0f), new Vector3(3.4f, 0.35f, 0.45f), goldMat, true);

        int stepCount = 10;
        float stepDepth = 0.46f;
        float stepDrop = 0.22f;
        float stairWidth = 3.1f;

        for (int i = 0; i < stepCount; i++)
        {
            float yPos = -(i * stepDrop) - (stepDrop * 0.5f);
            float zPos = (i * stepDepth) + (stepDepth * 0.5f);
            CreateBox("Step_" + i, stairAnnex.transform, new Vector3(0f, yPos, zPos), new Vector3(stairWidth, stepDrop, stepDepth), darkStone, false);
        }

        float totalDepth = stepCount * stepDepth;
        float totalDrop = stepCount * stepDrop;
        GameObject smoothRamp = new GameObject("SmoothStairWalkRamp");
        smoothRamp.transform.SetParent(stairAnnex.transform, false);
        smoothRamp.transform.localPosition = new Vector3(0f, -totalDrop * 0.5f, totalDepth * 0.5f);
        float rampLength = Mathf.Sqrt(totalDepth * totalDepth + totalDrop * totalDrop);
        float rampAngle = Mathf.Atan2(totalDrop, totalDepth) * Mathf.Rad2Deg;
        smoothRamp.transform.localRotation = Quaternion.Euler(rampAngle, 0f, 0f);
        BoxCollider rampCol = smoothRamp.AddComponent<BoxCollider>();
        rampCol.size = new Vector3(stairWidth, 0.12f, rampLength);

        float bottomY = -totalDrop;
        float landingLength = 4.0f;
        float totalCorridorLength = totalDepth + landingLength;
        float landingCenterZ = totalDepth + (landingLength * 0.5f);

        CreateBox("BasementLandingFloor", stairAnnex.transform, new Vector3(0f, bottomY - 0.15f, landingCenterZ), new Vector3(stairWidth + 0.5f, 0.3f, landingLength + 0.4f), darkSteelMat, true);
        CreateBox("StairwellWallLeft", stairAnnex.transform, new Vector3(-1.65f, 0.5f, totalCorridorLength * 0.5f), new Vector3(0.3f, 6.2f, totalCorridorLength + 0.4f), wallMat, true);
        CreateBox("StairwellWallRight", stairAnnex.transform, new Vector3(1.65f, 0.5f, totalCorridorLength * 0.5f), new Vector3(0.3f, 6.2f, totalCorridorLength + 0.4f), wallMat, true);
        CreateBox("StairwellCeiling", stairAnnex.transform, new Vector3(0f, 3.25f, totalCorridorLength * 0.5f), new Vector3(stairWidth + 0.6f, 0.3f, totalCorridorLength + 0.4f), darkStone, true);
        CreateBox("StairwellBackWall", stairAnnex.transform, new Vector3(0f, 0.5f, totalCorridorLength + 0.1f), new Vector3(stairWidth + 0.6f, 6.2f, 0.35f), darkSteelMat, true);

        GameObject secLightObj = new GameObject("VaultCorridorSecurityLight");
        secLightObj.transform.SetParent(stairAnnex.transform, false);
        secLightObj.transform.localPosition = new Vector3(0f, bottomY + 2.8f, landingCenterZ);
        Light secLight = secLightObj.AddComponent<Light>();
        secLight.type = LightType.Point;
        secLight.color = new Color(1f, 0.45f, 0.3f);
        secLight.range = 11f;
        secLight.intensity = 2.4f;

        float vaultDoorZ = totalCorridorLength - 0.25f;
        GameObject vaultDoorRoot = BuildCircularSteelVaultDoor(stairAnnex.transform, new Vector3(0f, bottomY, vaultDoorZ), steelMat, darkSteelMat, goldMat);

        BoxCollider portalTrigger = vaultDoorRoot.AddComponent<BoxCollider>();
        portalTrigger.isTrigger = true;
        portalTrigger.center = new Vector3(0f, 1.4f, -1.3f);
        portalTrigger.size = new Vector3(3.0f, 2.8f, 2.6f);

        PortalBehavior portal = vaultDoorRoot.AddComponent<PortalBehavior>();
        portal.targetSceneName = vaultSceneName;
    }

    // =========================================================================
    // 2. בניית חדר הכספות המלא ("Vault"): רצפת פלדה, תקרה, 4 קירות, תאים ושלל
    // =========================================================================
    private void ConstructBankVaultInterior(Transform root)
    {
        Material steelWallMat = CreateMat(new Color(0.28f, 0.31f, 0.35f), 0.85f, 0.7f);
        Material steelFloorMat = CreateMat(new Color(0.20f, 0.22f, 0.25f), 0.75f, 0.65f);
        Material chromeMat = CreateMat(new Color(0.72f, 0.75f, 0.78f), 0.92f, 0.85f);
        Material darkSteelMat = CreateMat(new Color(0.15f, 0.17f, 0.20f), 0.8f, 0.6f);
        Material goldBarMat = CreateMat(new Color(1f, 0.80f, 0.12f), 0.95f, 0.9f);
        Material cashGreenMat = CreateMat(new Color(0.18f, 0.52f, 0.24f), 0.1f, 0.3f);

        float halfW = vaultSize.x * 0.5f;
        float halfL = vaultSize.y * 0.5f;
        float wallThick = 0.4f;

        CreateBox("VaultSteelFloor", root, new Vector3(0f, -0.15f, 0f), new Vector3(vaultSize.x, 0.3f, vaultSize.y), steelFloorMat, true);
        CreateBox("VaultSteelCeiling", root, new Vector3(0f, vaultHeight + 0.15f, 0f), new Vector3(vaultSize.x + 0.5f, 0.3f, vaultSize.y + 0.5f), darkSteelMat, true);

        CreateBox("VaultWallWest", root, new Vector3(-halfW, vaultHeight * 0.5f, 0f), new Vector3(wallThick, vaultHeight, vaultSize.y), steelWallMat, true);
        CreateBox("VaultWallEast", root, new Vector3(halfW, vaultHeight * 0.5f, 0f), new Vector3(wallThick, vaultHeight, vaultSize.y), steelWallMat, true);
        CreateBox("VaultWallNorth", root, new Vector3(0f, vaultHeight * 0.5f, halfL), new Vector3(vaultSize.x, vaultHeight, wallThick), steelWallMat, true);
        CreateBox("VaultWallSouth", root, new Vector3(0f, vaultHeight * 0.5f, -halfL), new Vector3(vaultSize.x, vaultHeight, wallThick), steelWallMat, true);

        for (int r = -2; r <= 2; r++)
        {
            CreateBox("VaultSteelRib_" + r, root, new Vector3(0f, vaultHeight - 0.12f, r * 3.6f), new Vector3(vaultSize.x, 0.24f, 0.35f), chromeMat, false);
        }

        BuildSafetyDepositBoxWall(root, new Vector3(-halfW + 0.38f, 0f, 0f), 90f, chromeMat, darkSteelMat, goldBarMat);
        BuildSafetyDepositBoxWall(root, new Vector3(halfW - 0.38f, 0f, 0f), -90f, chromeMat, darkSteelMat, goldBarMat);

        GameObject exitVaultDoor = BuildCircularSteelVaultDoor(root, new Vector3(0f, 0f, -halfL + 0.32f), chromeMat, darkSteelMat, goldBarMat);
        BoxCollider exitTrigger = exitVaultDoor.AddComponent<BoxCollider>();
        exitTrigger.isTrigger = true;
        exitTrigger.center = new Vector3(0f, 1.4f, 1.3f);
        exitTrigger.size = new Vector3(3.0f, 2.8f, 2.6f);

        PortalBehavior returnPortal = exitVaultDoor.AddComponent<PortalBehavior>();
        returnPortal.targetSceneName = "Bank";

        for (int t = 0; t < 3; t++)
        {
            float zTable = -2.5f + t * 3.2f;
            Vector3 tablePos = new Vector3(0f, 0f, zTable);

            CreateBox("VaultTable_" + t, root, tablePos + new Vector3(0f, 0.85f, 0f), new Vector3(3.4f, 0.1f, 1.6f), darkSteelMat, true);
            CreateBox("Leg1", root, tablePos + new Vector3(-1.5f, 0.42f, -0.65f), new Vector3(0.14f, 0.84f, 0.14f), steelWallMat, false);
            CreateBox("Leg2", root, tablePos + new Vector3(1.5f, 0.42f, -0.65f), new Vector3(0.14f, 0.84f, 0.14f), steelWallMat, false);
            CreateBox("Leg3", root, tablePos + new Vector3(-1.5f, 0.42f, 0.65f), new Vector3(0.14f, 0.84f, 0.14f), steelWallMat, false);
            CreateBox("Leg4", root, tablePos + new Vector3(1.5f, 0.42f, 0.65f), new Vector3(0.14f, 0.84f, 0.14f), steelWallMat, false);

            for (int b = 0; b < 5; b++)
            {
                CreateBox("GoldBullion_" + b, root, tablePos + new Vector3(-1.1f + b * 0.22f, 0.95f, -0.35f), new Vector3(0.18f, 0.09f, 0.42f), goldBarMat, false);
            }
        }

        SpawnCollectibleMoneyStacks(root, cashGreenMat, goldBarMat);

        CreateWarmCeilingLight(root, new Vector3(0f, vaultHeight - 0.3f, -2.5f), chromeMat);
        CreateWarmCeilingLight(root, new Vector3(0f, vaultHeight - 0.3f, 3.0f), chromeMat);
    }

    private void SpawnCollectibleMoneyStacks(Transform root, Material cashMat, Material bandMat)
    {
        CoinBehaviour existingCoin = FindAnyObjectByType<CoinBehaviour>();

        // 3 שולחנות * 6 ערימות מרווחות על כל שולחן (3 עמודות X 2 שורות)
        int cols = 3;
        int rows = 2;
        int spawned = 0;

        for (int t = 0; t < 3; t++)
        {
            float zTable = -2.5f + t * 3.2f;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (spawned >= numberOfMoneyStacksToSpawn) return;
                    spawned++;

                    // מרווח של 1.05 מטר בין ערימה לערימה כדי שלעולם לא יעלו זו על זו!
                    float xOffset = (c - 1) * 1.05f;
                    float zOffset = (r == 0) ? 0.18f : 0.52f;
                    Vector3 spawnPos = new Vector3(xOffset, 0.96f, zTable + zOffset);

                    GameObject billStack;
                    if (moneyBillPrefab != null)
                    {
                        billStack = Instantiate(moneyBillPrefab, root);
                        billStack.transform.localPosition = spawnPos;
                        billStack.transform.localRotation = Quaternion.Euler(0f, (spawned * 15f) % 45f, 0f);
                        billStack.name = "MoneyBillStack_" + spawned;
                    }
                    else
                    {
                        billStack = CreateBox("MoneyBillStack_" + spawned, root, spawnPos, new Vector3(0.28f, 0.08f, 0.14f), cashMat, false);
                        CreateBox("MoneyBand", billStack.transform, Vector3.zero, new Vector3(0.06f, 1.05f, 1.05f), bandMat, false);
                    }

                    SphereCollider trigger = billStack.GetComponent<SphereCollider>();
                    if (trigger == null) trigger = billStack.AddComponent<SphereCollider>();
                    trigger.isTrigger = true;
                    trigger.radius = 1.2f;

                    CoinBehaviour cb = billStack.GetComponent<CoinBehaviour>();
                    if (cb == null) cb = billStack.AddComponent<CoinBehaviour>();
                    if (existingCoin != null)
                    {
                        cb.CoinsText = existingCoin.CoinsText;
                        cb.AllCoins = existingCoin.AllCoins;
                    }
                }
            }
        }
    }

    private GameObject BuildCircularSteelVaultDoor(Transform parent, Vector3 localPos, Material steel, Material darkSteel, Material gold)
    {
        GameObject vaultDoorRoot = new GameObject("HeavyCircularVaultDoor");
        vaultDoorRoot.transform.SetParent(parent, false);
        vaultDoorRoot.transform.localPosition = localPos;

        CreateBox("VaultFrameTop", vaultDoorRoot.transform, new Vector3(0f, 2.75f, 0f), new Vector3(3.2f, 0.35f, 0.45f), darkSteel, true);
        CreateBox("VaultFrameL", vaultDoorRoot.transform, new Vector3(-1.45f, 1.35f, 0f), new Vector3(0.35f, 2.7f, 0.45f), darkSteel, true);
        CreateBox("VaultFrameR", vaultDoorRoot.transform, new Vector3(1.45f, 1.35f, 0f), new Vector3(0.35f, 2.7f, 0.45f), darkSteel, true);

        GameObject roundDoor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roundDoor.name = "SteelCircularDoorDisc";
        roundDoor.transform.SetParent(vaultDoorRoot.transform, false);
        roundDoor.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        roundDoor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        roundDoor.transform.localScale = new Vector3(2.4f, 0.18f, 2.4f);
        roundDoor.GetComponent<Renderer>().sharedMaterial = steel;

        for (int a = 0; a < 4; a++)
        {
            GameObject spoke = CreateBox("WheelSpoke_" + a, vaultDoorRoot.transform, new Vector3(0f, 1.35f, -0.22f), new Vector3(0.95f, 0.07f, 0.07f), gold, false);
            spoke.transform.localRotation = Quaternion.Euler(0f, 0f, a * 45f);
        }

        for (int b = -1; b <= 1; b++)
        {
            CreateBox("LockBoltL_" + b, vaultDoorRoot.transform, new Vector3(-1.05f, 1.35f + b * 0.55f, -0.18f), new Vector3(0.45f, 0.12f, 0.12f), gold, false);
            CreateBox("LockBoltR_" + b, vaultDoorRoot.transform, new Vector3(1.05f, 1.35f + b * 0.55f, -0.18f), new Vector3(0.45f, 0.12f, 0.12f), gold, false);
        }

        return vaultDoorRoot;
    }

    private void BuildSafetyDepositBoxWall(Transform parent, Vector3 localPos, float rotY, Material chrome, Material frameMat, Material gold)
    {
        GameObject wallRoot = new GameObject("SafetyDepositWall");
        wallRoot.transform.SetParent(parent, false);
        wallRoot.transform.localPosition = localPos;
        wallRoot.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);

        CreateBox("MainCabinetBacking", wallRoot.transform, new Vector3(0f, 1.5f, 0f), new Vector3(10.5f, 3.0f, 0.35f), frameMat, true);

        int cols = 12;
        int rows = 6;
        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                float x = -4.6f + c * 0.84f;
                float y = 0.32f + r * 0.45f;
                CreateBox("DepositBox_" + c + "_" + r, wallRoot.transform, new Vector3(x, y, 0.19f), new Vector3(0.78f, 0.39f, 0.04f), chrome, false);
                CreateBox("GoldPlate", wallRoot.transform, new Vector3(x + 0.22f, y, 0.22f), new Vector3(0.16f, 0.08f, 0.02f), gold, false);
            }
        }
    }

    private void CreateBankPillar(Transform parent, Vector3 pos, Material marble, Material gold)
    {
        GameObject p = new GameObject("MarblePillar");
        p.transform.SetParent(parent, false);
        p.transform.localPosition = pos;

        CreateBox("Base", p.transform, new Vector3(0f, 0.2f, 0f), new Vector3(0.85f, 0.4f, 0.85f), gold, true);
        GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        col.transform.SetParent(p.transform, false);
        col.transform.localPosition = new Vector3(0f, 2.35f, 0f);
        col.transform.localScale = new Vector3(0.65f, 2.15f, 0.65f);
        col.GetComponent<Renderer>().sharedMaterial = marble;
        CreateBox("Capital", p.transform, new Vector3(0f, 4.5f, 0f), new Vector3(0.9f, 0.35f, 0.9f), gold, false);
    }

    private void CreateCustomerWaitingBench(Transform parent, Vector3 pos, float rotY, Material wood, Material cushion, Material gold)
    {
        GameObject bench = new GameObject("CustomerWaitingBench");
        bench.transform.SetParent(parent, false);
        bench.transform.localPosition = pos;
        bench.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);

        CreateBox("SeatFrame", bench.transform, new Vector3(0f, 0.42f, 0f), new Vector3(2.2f, 0.12f, 0.65f), wood, true);
        CreateBox("LeatherCushion", bench.transform, new Vector3(0f, 0.50f, 0f), new Vector3(2.1f, 0.09f, 0.58f), cushion, false);
        CreateBox("Backrest", bench.transform, new Vector3(0f, 0.82f, -0.28f), new Vector3(2.1f, 0.55f, 0.10f), cushion, true);
        CreateBox("ArmL", bench.transform, new Vector3(-1.05f, 0.62f, 0f), new Vector3(0.10f, 0.35f, 0.65f), gold, false);
        CreateBox("ArmR", bench.transform, new Vector3(1.05f, 0.62f, 0f), new Vector3(0.10f, 0.35f, 0.65f), gold, false);
    }

    private void CreateRopeStanchion(Transform parent, Vector3 pos, Material gold, Material ropeMat)
    {
        GameObject st = new GameObject("GoldStanchion");
        st.transform.SetParent(parent, false);
        st.transform.localPosition = pos;

        CreateBox("Base", st.transform, new Vector3(0f, 0.04f, 0f), new Vector3(0.32f, 0.08f, 0.32f), gold, false);
        CreateBox("Pole", st.transform, new Vector3(0f, 0.52f, 0f), new Vector3(0.06f, 0.95f, 0.06f), gold, false);
        CreateBox("VelvetRope", st.transform, new Vector3(0f, 0.82f, 1.2f), new Vector3(0.05f, 0.05f, 2.4f), ropeMat, false);
    }

    private void CreateWarmCeilingLight(Transform parent, Vector3 pos, Material gold)
    {
        GameObject ch = CreateBox("CeilingChandelier", parent, pos, new Vector3(1.2f, 0.15f, 1.2f), gold, false);
        Light l = ch.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.94f, 0.80f);
        l.range = 18f;
        l.intensity = 2.2f;
    }
}