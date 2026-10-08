using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;
public class MonsterBehavior : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    public GameObject player;
    float MaxHP = 100f;
    float currentHP;
    float damage = 10f;
    public Slider healthBar; // Reference to the UI Slider for health bar
    public GameObject PlayerCamera; // Reference to the player's camera
    public GameObject monsterCanvas; // Reference to the monster's canvas for health bar
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.isStopped = true;
        currentHP = MaxHP;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.isStopped)
        {
            agent.SetDestination(player.transform.position);
        }
        healthBar.value = currentHP / MaxHP; // Update the health bar value based on current HP
        monsterCanvas.transform.LookAt(PlayerCamera.transform.position); // Make the health bar face the player's camera
    }

    public void startWalking()
    {
        agent.SetDestination(player.transform.position);
        animator.SetInteger("State", 1);
    }

    public void gettingShot()
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            agent.isStopped = true;
            Falling();
            monsterCanvas.SetActive(false); // Hide the health bar when the monster dies
        }
    }

    public void Falling()
    {
        animator.SetInteger("State", 2);
        StartCoroutine(WaitAndDie());
        
    }

    IEnumerator WaitAndDie()
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetInteger("State", 3);
        yield return new WaitForSeconds(1.5f);
    }

}
