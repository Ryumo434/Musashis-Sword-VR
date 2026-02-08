using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemyYBotAI : MonoBehaviour
{
    private enum EnemyState { Patrolling, Chasing, Attacking, Dying }

    [Header("References")]
    public Transform player;
    private Animator animator;
    private NavMeshAgent agent;
    private bool isBoss;

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

    private EnemyState currentState = EnemyState.Patrolling;
    private bool isWaiting = false;
    private bool isDead = false;

    // Animator parameter hashes
    private int isWalkingHash;
    private int isAttackingHash;
    private int isDyingHash;
    private int isRunningHash;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        healthBar = GetComponentInChildren<floatingHealthBar>();

        isBoss = GetComponent<BossEnemy>() != null;

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

        if ((attackAction.action != null && attackAction.action.WasPerformedThisFrame() || Input.GetMouseButtonDown(1))
            && distance <= playerAttackRange)
        {
            TakeDamage(1);
            AudioManager.Instance.Play(AudioManager.SoundType.EnemyMiddle_Damage_medium);
        }

        if (health <= 0 && !isDead)
        {
            StartCoroutine(Die());
            return;
        }

        if (distance <= enemyAttackRange)
        {
            if (currentState != EnemyState.Attacking)
            {
                AudioManager.Instance.Stop(AudioManager.SoundType.Running);

                currentState = EnemyState.Attacking;
                agent.isStopped = true;
                animator.SetBool(isWalkingHash, false);
                animator.SetBool(isRunningHash, false);
                animator.SetBool(isAttackingHash, true);
                StartCoroutine(PlayAttackSoundDelayed(0.5f));
                Debug.Log("Attacking!");
            }
        }
        else if (distance <= detectionRange)
        {
            if (currentState != EnemyState.Chasing)
            {
                AudioManager.Instance.Stop(AudioManager.SoundType.EnemySmall_Attack_light);

                currentState = EnemyState.Chasing;
                agent.isStopped = false;
                animator.SetBool(isAttackingHash, false);
                animator.SetBool(isWalkingHash, false);
                animator.SetBool(isRunningHash, true);
                AudioManager.Instance.PlayLoop(AudioManager.SoundType.Running);
                Debug.Log("Player detected! Chasing...");
            }
            agent.SetDestination(player.position);
        }
        else
        {
            if (currentState != EnemyState.Patrolling)
            {
                AudioManager.Instance.Stop(AudioManager.SoundType.Running);

                currentState = EnemyState.Patrolling;
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

    IEnumerator PlayAttackSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        while (currentState == EnemyState.Attacking)
        {
            AudioManager.Instance.Play(AudioManager.SoundType.EnemySmall_Attack_light);
            Debug.Log("Played attack sound");
            yield return new WaitForSeconds(2.7f);
        }
    }

    IEnumerator Die()
    {
        if (isDead) yield break;
        isDead = true;

        if (isBoss && TimeTrialManager.Instance != null)
        {
            if (TimeTrialManager.Instance.IsRunning)
            {
                TimeTrialManager.Instance.StopTimer();
            }
        }

        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        AudioManager.Instance.Stop(AudioManager.SoundType.EnemySmall_Attack_light);
        AudioManager.Instance.Play(AudioManager.SoundType.EnemyMiddle_Die_medium);

        currentState = EnemyState.Dying;
        agent.isStopped = true;
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isAttackingHash, false);
        animator.SetBool(isDyingHash, true);
        Debug.Log(gameObject.name + " is dying...");

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"));
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        Debug.Log(gameObject.name + " destroyed!");
        Destroy(gameObject);
    }
}
