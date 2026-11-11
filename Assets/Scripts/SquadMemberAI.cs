using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class SquadMemberAI : MonoBehaviour
{
    public enum Team { Robber, Police }
    public enum AIState { FollowCommander, AttackShooting, Retreat }

    public static AIState PersistentSquadState = AIState.FollowCommander;
    public static bool EnemiesPursuingPlayer = false;
    public static HashSet<string> DeadUnitsRegistry = new HashSet<string>();

    [Header("Team & 3v3 Formation")]
    public Team myTeam = Team.Robber;
    public int squadMemberIndex = 0;

    [Header("Scene Navigation & Retreat")]
    public Transform retreatDoorPoint;

    [Header("Combat, Weapons & 3D Audio")]
    public float attackRange = 12f;
    public float fireRate = 1.2f;
    public Transform npcWeaponBarrelTransform;
    public AudioSource weaponAudio;
    public AudioSource footstepAudio;
    public AudioClip footstepClip;
    public AudioSource explosionAudio;
    public GameObject bombExplosionEffect;

    [HideInInspector] public bool isPlayerTeammate = false;
    [HideInInspector] public bool isDead = false;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform activeCommander;
    private Transform currentTarget;
    private AIState currentState = AIState.FollowCommander;
    private float nextFireTime = 0f;
    private float nextFootstepTime = 0f;
    private string uniqueUnitID;
    private GameObject npcPhysicalFlash;
    private Light npcFlashLight;

    public string GetUniqueID()
    {
        if (string.IsNullOrEmpty(uniqueUnitID))
            uniqueUnitID = myTeam.ToString() + "_" + squadMemberIndex;
        return uniqueUnitID;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        uniqueUnitID = GetUniqueID();
    }

    void Start()
    {
        if (animator != null) animator.applyRootMotion = false;

        if (weaponAudio != null)
        {
            weaponAudio.spatialBlend = 1.0f;
            weaponAudio.minDistance = 3f;
            weaponAudio.maxDistance = 45f;
        }

        if (footstepAudio != null)
        {
            footstepAudio.spatialBlend = 1.0f;
            footstepAudio.minDistance = 2f;
            footstepAudio.maxDistance = 22f;
        }

        if (agent != null)
        {
            agent.speed = 6.5f;
            agent.acceleration = 16.0f;
            agent.angularSpeed = 360f;
            agent.stoppingDistance = 2.4f;
            agent.radius = 0.45f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 30 + (squadMemberIndex * 10) + (myTeam == Team.Police ? 5 : 0);
        }

        // סעיף 4: ביטול דחיפה פיזית בין הדמות לבין השחקן כדי שלא ידחפו אותו דרך קירות!
        IgnoreCollisionWithPlayer();

        nextFireTime = Time.time + 1.5f;

        if (MenuAndGameManager.GameHasStarted)
        {
            InitializeRoleAndScenePlacement();
        }
    }

    private void IgnoreCollisionWithPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Collider[] playerColliders = playerObj.GetComponentsInChildren<Collider>(true);
        Collider[] myColliders = GetComponentsInChildren<Collider>(true);

        foreach (Collider pCol in playerColliders)
        {
            foreach (Collider mCol in myColliders)
            {
                if (pCol != null && mCol != null)
                {
                    Physics.IgnoreCollision(pCol, mCol, true);
                }
            }
        }
    }

    public void InitializeRoleAndScenePlacement()
    {
        uniqueUnitID = GetUniqueID();

        if (DeadUnitsRegistry.Contains(uniqueUnitID))
        {
            gameObject.SetActive(false);
            return;
        }

        IgnoreCollisionWithPlayer();

        var playerRole = MenuAndGameManager.SelectedRole;
        string sceneName = SceneManager.GetActiveScene().name;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        isPlayerTeammate = (myTeam == Team.Robber && playerRole == MenuAndGameManager.PlayerRole.Robber) ||
                           (myTeam == Team.Police && playerRole == MenuAndGameManager.PlayerRole.Police);

        if (squadMemberIndex == 0 && isPlayerTeammate)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!isPlayerTeammate)
        {
            bool shouldEnemyBeVisible = false;

            if (EnemiesPursuingPlayer || HeistAlarm.FirefightStarted)
            {
                shouldEnemyBeVisible = true;
            }
            else if (playerRole == MenuAndGameManager.PlayerRole.Police)
            {
                shouldEnemyBeVisible = HeistAlarm.IsVaultScene(sceneName);
            }
            else if (playerRole == MenuAndGameManager.PlayerRole.Robber)
            {
                shouldEnemyBeVisible = HeistAlarm.SirenFinished;
            }
            else if (playerRole == MenuAndGameManager.PlayerRole.Bystander)
            {
                if (sceneName == "Bank")
                {
                    if (myTeam == Team.Robber) shouldEnemyBeVisible = HeistAlarm.IsHeistActive;
                    else if (myTeam == Team.Police) shouldEnemyBeVisible = HeistAlarm.SirenFinished;
                }
            }

            if (!shouldEnemyBeVisible)
            {
                gameObject.SetActive(false);
                return;
            }
        }

        if (isPlayerTeammate && playerObj != null)
        {
            activeCommander = playerObj.transform;
            currentState = PersistentSquadState;

            if (agent != null && agent.isOnNavMesh && sceneName != "Outdoors")
            {
                agent.Warp(GetFormationPosition(playerObj.transform));
            }
        }
        else
        {
            activeCommander = FindTeamNPCLeader();
            if (EnemiesPursuingPlayer || HeistAlarm.FirefightStarted)
            {
                currentState = AIState.AttackShooting;
            }
        }
    }

    private Transform FindTeamNPCLeader()
    {
        SquadMemberAI[] allUnits = FindObjectsByType<SquadMemberAI>();
        foreach (SquadMemberAI unit in allUnits)
        {
            if (unit.myTeam == this.myTeam && unit.squadMemberIndex == 0 && unit.gameObject.activeInHierarchy)
            {
                return unit.transform;
            }
        }
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        return playerObj != null ? playerObj.transform : transform;
    }

    void Update()
    {
        if (!MenuAndGameManager.GameHasStarted || isDead) return;

        bool onNavMesh = (agent != null && agent.enabled && agent.isOnNavMesh);

        if (isPlayerTeammate &&
            MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Robber &&
            SceneManager.GetActiveScene().name == "Outdoors" &&
            !HeistAlarm.RobberSquadOrderedToMoveOut &&
            !HeistAlarm.IsHeistActive)
        {
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.F))
            {
                HeistAlarm.RobberSquadOrderedToMoveOut = true;
                if (StoryAndDialogueManager.Instance != null)
                {
                    StoryAndDialogueManager.Instance.ShowSingleDialogue(
                        "Daniel & Max",
                        "Copy that boss! Moving out with you toward the City Bank!",
                        4f
                    );
                }
            }
            else
            {
                if (onNavMesh) agent.isStopped = true;
                UpdateAnimationsAndFootsteps();
                return;
            }
        }

        if (!isPlayerTeammate && (HeistAlarm.FirefightStarted || EnemiesPursuingPlayer))
        {
            currentState = AIState.AttackShooting;
        }

        switch (currentState)
        {
            case AIState.FollowCommander:
                if (onNavMesh) FollowCommanderWithSpacing();
                break;

            case AIState.AttackShooting:
                EngageEnemies(onNavMesh);
                break;

            case AIState.Retreat:
                if (onNavMesh) MoveToRetreatDoor();
                break;
        }

        UpdateAnimationsAndFootsteps();
    }

    private Vector3 GetFormationPosition(Transform commander)
    {
        if (squadMemberIndex == 0) return commander.position;
        float sideOffset = (squadMemberIndex == 1) ? -2.2f : 2.2f;
        float backOffset = -2.5f;
        return commander.position + (commander.right * sideOffset) + (commander.forward * backOffset);
    }

    private void FollowCommanderWithSpacing()
    {
        if (activeCommander == null) return;

        if (squadMemberIndex == 0 && !isPlayerTeammate && !HeistAlarm.FirefightStarted)
        {
            agent.isStopped = true;
            return;
        }

        // שומר מרחק בטוח מהשחקן כדי לא להיצמד אליו ליד קירות
        float distToCommander = Vector3.Distance(transform.position, activeCommander.position);
        if (distToCommander < 2.2f)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return;
        }

        Vector3 targetPos = GetFormationPosition(activeCommander);
        if (Vector3.Distance(transform.position, targetPos) > 0.85f)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPos);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    private void EngageEnemies(bool onNavMesh)
    {
        currentTarget = FindNearestEnemy();

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // סעיף 1: כשמשחקים כעובר אורח, טווח הירי בין השוטרים לשודדים בלובי מוגדל ל-30 מטר כדי שיירו מיד מכל נקודה בחדר!
            float effectiveRange = (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Bystander)
                ? Mathf.Max(attackRange, 30f)
                : attackRange;

            if (distance > effectiveRange && onNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.position);
            }
            else
            {
                if (onNavMesh)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }

                LookAtTarget(currentTarget.position);

                if (Time.time >= nextFireTime)
                {
                    float cooldown;
                    if (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Bystander)
                    {
                        // קצב אש מהיר בקרב של עובר האורח כדי שהעימות יוכרע מהר מול עיניו
                        cooldown = Random.Range(0.85f, 1.25f);
                    }
                    else
                    {
                        cooldown = isPlayerTeammate ? Mathf.Max(1.3f, fireRate) : Mathf.Max(2.4f, fireRate * 2.2f);
                    }

                    nextFireTime = Time.time + cooldown;
                    ShootAtTarget();
                }
            }
        }
        else if (onNavMesh)
        {
            FollowCommanderWithSpacing();
        }
    }

    private void ShootAtTarget()
    {
        if (weaponAudio != null) weaponAudio.Play();

        StartCoroutine(NpcWeaponMuzzleFlashRoutine());

        if (currentTarget != null)
        {
            CheckIfBystanderCrossedLineOfFire(transform.position + Vector3.up * 1.2f, currentTarget.position + Vector3.up * 1.2f);

            HealthSystem targetHealth = currentTarget.GetComponentInParent<HealthSystem>();
            if (targetHealth != null)
            {
                int damageToDeal;
                if (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Bystander)
                {
                    damageToDeal = Random.Range(24, 34);
                }
                else if (isPlayerTeammate)
                {
                    damageToDeal = 18;
                }
                else
                {
                    // נזק אויבים מותאם לדרגת הקושי שנבחרה ב-OptionsPanel!
                    if (MenuAndGameManager.CurrentDifficulty == MenuAndGameManager.GameDifficulty.Easy)
                        damageToDeal = 4;
                    else if (MenuAndGameManager.CurrentDifficulty == MenuAndGameManager.GameDifficulty.Normal)
                        damageToDeal = 7;
                    else
                        damageToDeal = 13; // Hard
                }

                targetHealth.TakeDamage(damageToDeal, myTeam.ToString() + " #" + squadMemberIndex);
            }
        }
    }

    private void CheckIfBystanderCrossedLineOfFire(Vector3 shooterPos, Vector3 targetPos)
    {
        if (MenuAndGameManager.SelectedRole != MenuAndGameManager.PlayerRole.Bystander) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Vector3 playerPos = playerObj.transform.position + Vector3.up * 1.0f;
        Vector3 lineDir = targetPos - shooterPos;
        float lineLen = lineDir.magnitude;
        if (lineLen < 0.1f) return;

        float projection = Vector3.Dot(playerPos - shooterPos, lineDir.normalized);
        if (projection > 0f && projection < lineLen)
        {
            Vector3 closestPointOnLine = shooterPos + lineDir.normalized * projection;
            float distFromLaser = Vector3.Distance(playerPos, closestPointOnLine);

            if (distFromLaser <= 1.35f)
            {
                HealthSystem playerHP = playerObj.GetComponent<HealthSystem>();
                if (playerHP != null)
                {
                    playerHP.TakeDamage(35, "Crossfire (" + myTeam.ToString() + ")");
                }
            }
        }
    }

    private IEnumerator NpcWeaponMuzzleFlashRoutine()
    {
        if (npcPhysicalFlash == null)
        {
            Transform attachPoint = (npcWeaponBarrelTransform != null) ? npcWeaponBarrelTransform : transform;
            Vector3 offset = (npcWeaponBarrelTransform != null) ? new Vector3(0f, 0.05f, 0.35f) : new Vector3(0.2f, 1.35f, 0.7f);
            npcPhysicalFlash = PlayerWeaponController.CreateAutoPhysicalMuzzleFlash(attachPoint, offset);

            npcFlashLight = npcPhysicalFlash.AddComponent<Light>();
            npcFlashLight.type = LightType.Point;
            npcFlashLight.color = new Color(1f, 0.75f, 0.2f);
            npcFlashLight.range = 4f;
            npcFlashLight.intensity = 2.5f;
        }

        npcPhysicalFlash.SetActive(true);
        yield return new WaitForSeconds(0.06f);
        if (npcPhysicalFlash != null) npcPhysicalFlash.SetActive(false);
    }

    private void MoveToRetreatDoor()
    {
        if (retreatDoorPoint != null)
        {
            agent.isStopped = false;
            agent.SetDestination(retreatDoorPoint.position);

            if (Vector3.Distance(transform.position, retreatDoorPoint.position) <= 2.0f &&
                SceneManager.GetActiveScene().name != "Outdoors")
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            FollowCommanderWithSpacing();
        }
    }

    public void CommandShoot()
    {
        if (isDead) return;
        HeistAlarm.RobberSquadOrderedToMoveOut = true;
        currentState = AIState.AttackShooting;
        if (isPlayerTeammate) PersistentSquadState = AIState.AttackShooting;
    }

    public void CommandThrowBomb()
    {
        if (isDead) return;
        HeistAlarm.RobberSquadOrderedToMoveOut = true;
        StartCoroutine(ThrowPhysicalGrenadeRoutine());
    }

    public void CommandRetreat()
    {
        if (isDead) return;
        HeistAlarm.RobberSquadOrderedToMoveOut = true;
        currentState = AIState.Retreat;
        if (isPlayerTeammate) PersistentSquadState = AIState.Retreat;
    }

    public void CommandFollow()
    {
        if (isDead) return;
        HeistAlarm.RobberSquadOrderedToMoveOut = true;
        currentState = AIState.FollowCommander;
        if (isPlayerTeammate) PersistentSquadState = AIState.FollowCommander;
    }

    private IEnumerator ThrowPhysicalGrenadeRoutine()
    {
        Transform enemy = FindNearestEnemy();
        Vector3 targetPos = (enemy != null) ? enemy.position : (transform.position + transform.forward * 8f);

        LookAtTarget(targetPos);
        if (animator != null) animator.SetTrigger("ThrowBomb");

        yield return new WaitForSeconds(0.55f);

        Vector3 handSpawnPos = transform.position + (Vector3.up * 1.5f) + (transform.forward * 0.6f);
        AudioClip boomClip = (explosionAudio != null) ? explosionAudio.clip : null;

        GrenadeProjectile.LaunchPhysicalGrenade(
            handSpawnPos,
            targetPos,
            myTeam,
            myTeam.ToString() + " Grenade",
            bombExplosionEffect,
            boomClip
        );
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;
        DeadUnitsRegistry.Add(GetUniqueID());

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsShooting", false);
            animator.SetTrigger("Die");
        }
    }

    private Transform FindNearestEnemy()
    {
        SquadMemberAI[] allUnits = FindObjectsByType<SquadMemberAI>();
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (SquadMemberAI unit in allUnits)
        {
            if (unit != this && !unit.isDead && unit.gameObject.activeInHierarchy && unit.myTeam != this.myTeam)
            {
                float dist = Vector3.Distance(transform.position, unit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = unit.transform;
                }
            }
        }

        if (!isPlayerTeammate && MenuAndGameManager.SelectedRole != MenuAndGameManager.PlayerRole.Bystander)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                float playerDist = Vector3.Distance(transform.position, playerObj.transform.position);
                if (playerDist < minDistance)
                {
                    closest = playerObj.transform;
                }
            }
        }

        return closest;
    }

    private void LookAtTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
        }
    }

    private void UpdateAnimationsAndFootsteps()
    {
        if (animator == null || isDead) return;

        bool isMoving = (agent != null && agent.enabled && agent.isOnNavMesh) && agent.velocity.sqrMagnitude > 0.08f && !agent.isStopped;
        bool isShooting = (currentState == AIState.AttackShooting) && !isMoving && (currentTarget != null);

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsShooting", isShooting);

        if (isMoving && Time.time >= nextFootstepTime)
        {
            nextFootstepTime = Time.time + 0.42f;
            if (footstepAudio != null)
            {
                if (footstepClip != null) footstepAudio.PlayOneShot(footstepClip, 0.55f);
                else footstepAudio.Play();
            }
        }
    }
}