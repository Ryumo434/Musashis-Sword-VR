using UnityEngine;
using UnityEngine.AI;

public class EnemyAiXR : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Target (Player XR Camera)")]
    public Transform player; // wird automatisch Camera.main
    public LayerMask whatIsGround, whatIsPlayer;

    [Header("Enemy Stats")]
    public float health = 100f;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange = 10f;

    // Attacking
    public float timeBetweenAttacks = 2f;
    bool alreadyAttacked;
    public GameObject projectile;

    // States
    public float sightRange = 10f, attackRange = 2f;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Player = Headset Kamera (Main Camera in XR Origin)
        if (Camera.main != null)
            player = Camera.main.transform;
        else
            Debug.LogWarning("⚠️ Kein Camera.main gefunden! Stelle sicher, dass deine XR Origin → Main Camera das MainCamera Tag trägt.");
    }

    private void Update()
    {
        if (player == null) return;

        // Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // check ob auf begehbarem Boden
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        // Enemy bleibt stehen
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            /// Attack code here
            if (projectile != null)
            {
                Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
                rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
                rb.AddForce(transform.up * 8f, ForceMode.Impulse);
            }
            else
            {
                Debug.Log("💥 Enemy attacked player (nahkampf)!");
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
