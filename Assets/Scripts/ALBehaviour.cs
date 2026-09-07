using UnityEngine;
using UnityEngine.AI;

public class ALBehaviour : MonoBehaviour
{
    Animator animator;
    NavMeshAgent NMAgent;
    public GameObject target;
    public GameObject target_2nd_floor;
    LineRenderer path;
    bool isTarget1 = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        NMAgent = GetComponent<NavMeshAgent>();
        path = GetComponent<LineRenderer>();
        path.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
 //           path.positionCount = NMAgent.path.corners.Length;
  //          path.SetPositions(NMAgent.path.corners);

        if (!NMAgent.isStopped)
        {
            float distance;
            // in any case show the path
            path.positionCount = NMAgent.path.corners.Length;
            path.SetPositions(NMAgent.path.corners);    

            if(isTarget1)
                distance = Vector3.Distance(transform.position,target.transform.position);
            else distance = Vector3.Distance(transform.position, target_2nd_floor.transform.position);
            
            if (distance < 2)
            {
                if (isTarget1)
                {
                    NMAgent.SetDestination(target_2nd_floor.transform.position);
                    isTarget1 = false;
                }
                else
                {
                    NMAgent.SetDestination(target.transform.position);
                    isTarget1 = true;

                }

            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            animator.SetInteger("State", 1);
            // Starts computing the path
            NMAgent.SetDestination(target.transform.position);
            // create line renderer
            path.positionCount = NMAgent.path.corners.Length;
            path.SetPositions(NMAgent.path.corners);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            animator.SetInteger("State", 0);
        }
    }
}
