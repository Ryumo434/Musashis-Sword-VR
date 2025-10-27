using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemyYBotAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody rb;
    [SerializeField] private floatingHealthBar healthBar;
    [SerializeField] private InputActionProperty attackAction; 

    [Header("Stats")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float enemyAttackRange = 2f;
    [SerializeField] private float playerAttackRange = 2f;
   [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolRange = 5f;
    [SerializeField] private float waitTime = 2f;
   [SerializeField] private float maxHealth = 10f;
    private float health;

    private string currentState = "Patrolling";
    private bool isWaiting = false;
    private bool isDead = false;

    // Animator parameter hashes
    private int isWalkingHash;
    private int isAttackingHash;
    private int isDyingHash;
    private int isRunningHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        healthBar = GetComponentInChildren<floatingHealthBar>();

        // Animator parameter IDs
        isWalkingHash = Animator.StringToHash("isWalking");
        isAttackingHash = Animator.StringToHash("isAttacking");
        isDyingHash = Animator.StringToHash("isDying");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    void Start()
    {
        health = maxHealth;
        agent.speed = moveSpeed;
        healthBar.UpdateHealthBar(health, maxHealth);

        if (player == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                player = mainCam.transform;
                Debug.Log("Player set to Main Camera: " + player.name);
            }
        }

       if (healthBar == null)
        {
            healthBar = FindObjectOfType<floatingHealthBar>();
        }

        Patrol();
    }

    void Update()
    {
        if (isDead) return;
        if (player == null)
        {
            Debug.LogError("Player reference not set on Enemy!");
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // Damage input test (Player attacking Enemy)
        if ((attackAction.action != null && attackAction.action.WasPerformedThisFrame() || Input.GetMouseButtonDown(1)) && distance <= playerAttackRange)
        {
            TakeDamage(1);
        }

        if (health <= 0 && !isDead)
        {
            StartCoroutine(Die());
            return;
        }

        // --- Player detection logic ---
        if (distance <= enemyAttackRange)
        {
            if (currentState != "Attacking")
            {
                currentState = "Attacking";
                agent.isStopped = true;
                animator.SetBool(isWalkingHash, false);
                animator.SetBool(isRunningHash, false);
                animator.SetBool(isAttackingHash, true);
                Debug.Log("Attacking!");
            }
        }
        else if (distance <= detectionRange)
        {
            if (currentState != "Chasing")
            {
                currentState = "Chasing";
                agent.isStopped = false;
                animator.SetBool(isAttackingHash, false);
                animator.SetBool(isWalkingHash, false);
                animator.SetBool(isRunningHash, true);
                Debug.Log("Player detected! Chasing...");
            }
            agent.SetDestination(player.position);
        }
        else
        {
            if (currentState != "Patrolling")
            {
                currentState = "Patrolling";
                agent.isStopped = false;
                animator.SetBool(isAttackingHash, false);
                animator.SetBool(isRunningHash, false);
                animator.SetBool(isWalkingHash, true);
                Debug.Log("Player lost. Resuming patrol...");
                Patrol();
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
            {
                StartCoroutine(WaitAndPatrol());
            }
        }
    }

    void Patrol()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            animator.SetBool(isWalkingHash, true);
        }
    }

    IEnumerator WaitAndPatrol()
    {
        isWaiting = true;
        animator.SetBool(isWalkingHash, false);
        yield return new WaitForSeconds(waitTime);
        Patrol();
        isWaiting = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health -= damage;
        healthBar.UpdateHealthBar(health, maxHealth);
        Debug.Log(gameObject.name + " took damage! Health: " + health);
    }

    IEnumerator Die()
    {
        isDead = true;
        currentState = "Dying";
        agent.isStopped = true;
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isAttackingHash, false);
        animator.SetBool(isDyingHash, true);
        Debug.Log(gameObject.name + " is dying...");

        // Warten bis die Death-Animation abgespielt ist
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"));
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }


        Debug.Log(gameObject.name + " destroyed!");
        Destroy(gameObject);
    }
}
