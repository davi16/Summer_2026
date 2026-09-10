using UnityEngine;
using ithappy.Animals_FREE; 

[RequireComponent(typeof(CreatureMover))]
public class AnimalNPC : MonoBehaviour
{
    [Header("AI Settings")]
    public Transform player;
    public float fleeDistance = 8f;
    public float obstacleDetectionDistance = 1.5f; // מרחק גילוי המכשולים מול החיה

    private CreatureMover m_Mover;
    private Vector2 currentMovement;
    private Vector3 currentLookTarget;
    private Vector3 currentWanderDirection;
    private float turnDrift = 0f;
    private float timer = 0f;
    private bool isRunning = false;

    void Awake()
    {
        m_Mover = GetComponent<CreatureMover>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        currentWanderDirection = transform.forward;
    }

    void Update()
    {
        bool isFleeing = false;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer < fleeDistance)
            {
                isFleeing = true;
            }
        }

        if (isFleeing)
        {
            FleeFromPlayer();
        }
        else
        {
            timer -= Time.deltaTime;
            
            // שיגור Raycast קדימה לבדיקת מכשולים (מופעל רק כשהחיה בתנועה)
            if (currentMovement.y > 0)
            {
                AvoidObstacles();
            }

            if (timer <= 0f)
            {
                ChooseNewDirection();
            }

            if (currentMovement.y > 0)
            {
                currentWanderDirection = Quaternion.Euler(0, turnDrift * Time.deltaTime, 0) * currentWanderDirection;
            }

            currentLookTarget = transform.position + currentWanderDirection;
        }

        // שולחים את הפקודות למנוע. פרמטר הקפיצה נשאר false מאחר והוא לא נתמך במנוע המקורי
        m_Mover.SetInput(currentMovement, currentLookTarget, isRunning, false);
    }

    private void AvoidObstacles()
    {
        // שולחים את הקרן מגובה חצי מטר כדי שלא תפגע ברצפה בטעות
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, currentWanderDirection);
        
        // אם הקרן פוגעת במשהו שהוא לא טריגר
        if (Physics.Raycast(ray, out RaycastHit hit, obstacleDetectionDistance))
        {
            if (!hit.collider.isTrigger && !hit.collider.CompareTag("Player"))
            {
                // מסתובבים בחדות הצידה ומקצרים את הטיימר כדי לחשב מסלול מחדש
                turnDrift = 180f; 
                timer = 1f; 
            }
        }
    }

    private void FleeFromPlayer()
    {
        Vector3 directionAway = (transform.position - player.position).normalized;
        directionAway.y = 0f; 

        currentMovement = new Vector2(0f, 1f); 
        isRunning = true;
        turnDrift = 0f; 
        
        currentWanderDirection = directionAway;
        currentLookTarget = transform.position + directionAway;
        
        timer = Random.Range(3f, 5f); 
    }

    private void ChooseNewDirection()
    {
        // הגרלת מספר מ-1 עד 100 ליצירת הסתברויות משוקללות
        int chance = Random.Range(0, 100);

        if (chance < 65) 
        {
            // 65% סיכוי: מנוחה ועמידה במקום
            currentMovement = Vector2.zero;
            isRunning = false;
            turnDrift = 0f;
            timer = Random.Range(4f, 10f); // נחות לזמן ממושך
        }
        else 
        {
            // 35% סיכוי: תנועה למקום חדש
            float randomAngle = Random.Range(0f, 360f);
            currentWanderDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            turnDrift = Random.Range(-20f, 20f);
            currentMovement = new Vector2(0f, 1f);
            
            if (chance < 90) 
            {
                // מתוך מצבי התנועה, רוב הפעמים זו תהיה הליכה רגועה (סיכוי של 25% סך הכל)
                isRunning = false; 
                timer = Random.Range(4f, 8f);
            }
            else 
            {
                // מתוך מצבי התנועה, מעט מאוד פעמים זו תהיה ריצה אקראית (סיכוי של 10% סך הכל)
                isRunning = true;  
                timer = Random.Range(1.5f, 3.5f); // רצות למרחק קצר בלבד
            }
        }
    }
}