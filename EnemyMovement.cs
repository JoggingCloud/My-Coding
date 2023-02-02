using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement: MonoBehaviour
{
    [Header("Layer Mask:")]
    public LayerMask targetmask;
    public LayerMask obstructionmask;

    [Header("Health:")]
    public float enemyHealth = 100f;
    public bool isTakingDamage = false;

    [Header("Combat:")]
    [SerializeField] float attackCD = 3f;
    [SerializeField] float attackRange = 3f;
    [SerializeField] float aggroRange = 4f;

    [Header("Movement:")]
    GameObject player;
    NavMeshAgent enemy;
    Animator animator;
    float timePassed;
    float newDestinationCD = 0.5f;
    
    // An array of min and max of the direction the AI can travel at random. Can be adjusted in the inspector
    [Range(1, 500)] public float moveRadius;

    public void Start()
    {
        enemy = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
    }

    public void Update()
    {
        animator.SetFloat("speed", enemy.velocity.magnitude / enemy.speed);

        if (timePassed >= attackCD)
        {
            if (Vector3.Distance(player.transform.position, transform.position) <= attackRange)
            {
                animator.SetTrigger("attack");
                timePassed = 0;
            }
        }
        timePassed += Time.deltaTime;

        if (enemy != null && Vector3.Distance(player.transform.position, transform.position) > aggroRange)
        {
            enemy.SetDestination(RandomNavMeshLocation());
        }

        if (newDestinationCD <= 0 && Vector3.Distance(player.transform.position, transform.position) <= aggroRange)
        {
            //transform.LookAt(player.transform);
            if (isTakingDamage)
            {
                return;
            }
            newDestinationCD = 0.5f;
            enemy.SetDestination(player.transform.position);
        }
        newDestinationCD -= Time.deltaTime;
    }

    //Vector3 calculates random position on the NavMesh
    public Vector3 RandomNavMeshLocation()
    {
        Vector3 lastPosition = Vector3.zero;

        // Randome position is assigned inside the given space where the AI can walk on 
        Vector3 randomPosition = Random.insideUnitSphere * moveRadius;

        // A new random position is constantly made by the AI as it transforms its positon on the NavMesh
        randomPosition += transform.position;

        // "out" a function to return two values
        // If AI has reached destination "Hit" within the raycasting of the SamplePosition, then return previous postion as it records last postion it hit 
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, moveRadius, 1))
        {
            lastPosition = hit.position;
        }
        return lastPosition;
    }

    // Health 
    public void TakingDamage(int amount)
    {
        enemyHealth -= amount;
        //HitSound.Play();
        print("getting hit");
        if (enemyHealth <= 0f)
        {
            //EnemyDeath.Play();
            Die();
        }
    }

    // Death 
    void Die()
    {
        Destroy(this.gameObject);
    }

    // Damage to player 
    public void StartDealDamage()
    {
        GetComponentInChildren<EnemyDamage>().StartDealDamage();
    }

    public void EndDealDamage()
    {
        GetComponentInChildren<EnemyDamage>().EndDealDamage();
    }

    // Showing attack range and chase range
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}