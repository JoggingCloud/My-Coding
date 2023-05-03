using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement: MonoBehaviour
{
    [Header("Reference to Player")]
    public GameObject player;
    public PlayerHealth playerHealth;

    [Header("Movement:")]
    NavMeshAgent enemy;
    Animator animator;
    public float randomPointRange;
    float timePassed;
    public float newDestinationCD;

    [Header("FOV:")]
    public GameObject raycastPoint;
    public float radius;
    [Range(0, 360)]
    public float angle;
    public bool canSeePlayer;

    [Header("Rotation:")]
    // Smooth look ortation when in line of site radius
    public float LookSpeed = 1f;

    [Header("Combat:")]
    [SerializeField] float attackSpeed = 3f;
    [SerializeField] float attackRange = 3f;
    public bool isChasing = false;

    [Header("Layer Mask:")]
    public LayerMask targetmask;
    public LayerMask obstructionmask;

    [Header("Health:")]
    public float enemyHealth = 100f;
    public bool isTakingDamage = false;

    [Header("Audio:")]
    public bool isAttacking = false;
    public AudioSource attackSource;
    public AudioClip[] attackClips;

    public bool isRoaming = false;
    //public AudioSource roamSource;
    
    //public AudioSource painSource;

    public AudioSource deathSource;

    public void Start()
    {
        enemy = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");

        enemyHealth = 100f;
    }

    public void Update()
    {
        //Debug.Log(enemyHealth);
        if (playerHealth.livesRemaining == 0)
        {
            return;
        }

        if (enemyHealth == 0)
        {
            return;
        }

        animator.SetFloat("speed", enemy.velocity.magnitude / enemy.speed);

        if (timePassed >= attackSpeed)
        {
            if (Vector3.Distance(player.transform.position, transform.position) <= attackRange)
            {
                timePassed = 0;
                animator.SetTrigger("attack");
                isAttacking = true;
                AttackHandler();
            }
        }
        timePassed += Time.deltaTime;
        
        if (/*newDestinationCD < 0 &&*/ Vector3.Distance(player.transform.position, transform.position) <= radius)
        {
            StartCoroutine(TurnToPlayer());
            StartCoroutine(FOVRoutine());
            if (canSeePlayer)
            {
                Debug.Log("Going to player" + " " + canSeePlayer);
                enemy.SetDestination(player.transform.position);
            }
            newDestinationCD = 1f;
        }
        else if (!canSeePlayer && Vector3.Distance(player.transform.position, transform.position) <= radius)
        {
            RandomNavPoint();
        }
        newDestinationCD -= Time.deltaTime;

        if (enemy != null && Vector3.Distance(player.transform.position, transform.position) > radius)
        {
            RandomNavPoint();
        }
    }

    bool RandomPostion(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    void RandomNavPoint()
    {
        if (canSeePlayer)
        {
            return;
        }
        if (enemy.remainingDistance <= enemy.stoppingDistance)
        {
            isRoaming = true;
            Vector3 position;
            if (RandomPostion(enemy.transform.position, randomPointRange, out position))
            {
                Debug.DrawRay(position, Vector3.up, Color.yellow, 1.0f); //so you can see with gizmos
                enemy.SetDestination(position);
            }
        }
    }

    private IEnumerator TurnToPlayer()
    {
        Quaternion lookRotation = Quaternion.LookRotation(player.transform.position - transform.position);

        float time = 0;

        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);

            time += Time.deltaTime * LookSpeed;

            yield return null;
        }
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(raycastPoint.transform.position, radius, targetmask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - raycastPoint.transform.position).normalized;

            if (Vector3.Angle(raycastPoint.transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(raycastPoint.transform.position, target.position);

                if (!Physics.Raycast(raycastPoint.transform.position, directionToTarget, distanceToTarget, obstructionmask))
                {
                    canSeePlayer = true;
                    if (canSeePlayer)
                    {
                        Physics.Raycast(raycastPoint.transform.position, directionToTarget, distanceToTarget, obstructionmask);
                    }
                    //enemy.SetDestination(player.transform.position);
                }
                else
                {
                    canSeePlayer = false;
                }
            }
            else
            {
                canSeePlayer = false;
            }
        }
        else if (canSeePlayer)
        {
            canSeePlayer = false;
        }
    }

    // Health 
    public void LosingHealth(int amount)
    {
        if (enemyHealth == 0)
        {
            Die();
        }
        enemyHealth -= amount;
        animator.SetTrigger("damage");
        Debug.Log("getting hit" + " " + enemyHealth);
        isTakingDamage = true;
    }

    // Death 
    void Die()
    {
        animator.SetTrigger("death");
        deathSource.Play();
        StartCoroutine(DeleteFromScene());
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

    IEnumerator DeleteFromScene()
    {
        yield return new WaitForSeconds(2);
        Destroy(this.gameObject);
    }

    // Showing attack range and chase range
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, randomPointRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, angle);
        
        if (canSeePlayer)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(raycastPoint.transform.position, player.transform.position);
        }
    }

    private void AttackHandler()
    {
        switch (isAttacking)
        {
            default:
                attackSource.PlayOneShot(attackClips[Random.Range(0, attackClips.Length - 1)]);
                break;
        }
    }
}
