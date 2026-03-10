using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class EnemyYBotAI : MonoBehaviour
{
    private enum EnemyState { Frozen, Patrolling, Chasing, Attacking, Dying }

    [Header("References")]
    public Transform player;
    private Animator animator;
    private NavMeshAgent agent;
    private bool isBoss;

    //[SerializeField] private floatingHealthBar healthBar;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private InputActionProperty attackAction;

    [Header("Stats")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float enemyAttackRange = 2f;
    [SerializeField] private float playerAttackRange = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float patrolRange = 5f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float maxHealth;

    [Header("Chase Pause")]
    [SerializeField] private float chaseDuration = 3f;
    [SerializeField] private float chasePause = 2f;
    [SerializeField] private float enemyDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;

    

    private float chaseTimer = 0f;
    private bool isChasePaused = false;
    //private float health;
    [SerializeField] private float currentHealth;
    private EnemyState currentState = EnemyState.Patrolling;
    private bool isWaiting = false;
    private bool isDead = false;

    // Animator parameter hashes
    private int isWalkingHash;
    private int isAttackingHash;
    private int isDyingHash;
    private int isRunningHash;

    private float lastAttackTime;
    private healthbar playerHealth;

    [Header("Boss Death Actions")]
    [SerializeField] private DoorOpenOnPress doorToOpen;
    [SerializeField] private GameObject spawnerToDisable;

    [Header("Boss Activation")]
    [SerializeField] private GameObject activationTriggerObject;
    [SerializeField] private bool freezeBossUntilTriggered = true;

    private bool bossActivated = false;




    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        healthSlider = GetComponentInChildren<Slider>();

        if (healthSlider == null)
        {
            Debug.LogError("Health Slider not found in children of " + gameObject.name);
        }
        //healthBar = GetComponentInChildren<floatingHealthBar>();

        isBoss = GetComponent<BossEnemy>() != null;

        isWalkingHash = Animator.StringToHash("isWalking");
        isAttackingHash = Animator.StringToHash("isAttacking");
        isDyingHash = Animator.StringToHash("isDying");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        agent.speed = moveSpeed;

        if (player == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                player = mainCam.transform;
                Debug.Log("Player set to Main Camera: " + player.name);
            }
        }

        playerHealth = player.GetComponentInChildren<healthbar>();

        if (playerHealth == null)
        {
            Debug.LogError("Player healthbar script not found!");
        }

        if (isBoss && freezeBossUntilTriggered)
        {
            bossActivated = false;
            currentState = EnemyState.Frozen;

            agent.isStopped = true;
            agent.ResetPath();

            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
            animator.SetBool(isAttackingHash, false);
            animator.SetBool(isDyingHash, false);

            if (activationTriggerObject == null)
            {
                Debug.LogWarning(gameObject.name + ": Boss is set to wait for trigger, but no activationTriggerObject is assigned.");
            }
            else
            {
                BossActivationTrigger trigger = activationTriggerObject.GetComponent<BossActivationTrigger>();

                if (trigger == null)
                {
                    trigger = activationTriggerObject.AddComponent<BossActivationTrigger>();
                }

                trigger.Initialize(this);
            }
        }
        else
        {
            bossActivated = true;
            currentState = EnemyState.Patrolling;
            Patrol();
        }
    }

    void Update()
    {
        if (isDead) return;
        if (!ValidatePlayer()) return;

        if (isBoss && freezeBossUntilTriggered && !bossActivated)
            return;

        if (currentState == EnemyState.Frozen)
            return;

        float distance = GetDistanceToPlayer();

        HandlePlayerAttackInput(distance);
        HandleDeathCheck();

        if (isDead) return;

        HandleStateByDistance(distance);
    }

    private bool ValidatePlayer()
    {
        if (player == null)
        {
            Debug.LogError("Player reference not set on Enemy!");
            return false;
        }
        return true;
    }

    private float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    private void HandlePlayerAttackInput(float distance)
    {
        bool attackInput =
            (attackAction.action != null && attackAction.action.WasPerformedThisFrame()) ||
            Input.GetMouseButtonDown(1);

        if (attackInput && distance <= playerAttackRange)
        {
            TakeDamage(1);
            AudioManager.Instance.Play(AudioManager.SoundType.EnemyMiddle_Damage_medium);
        }
    }

    private void HandleDeathCheck()
    {
        if (currentHealth <= 0 && !isDead)
        {
            StartCoroutine(Die());
        }
    }

    private void HandleStateByDistance(float distance)
    {
        if (distance <= enemyAttackRange)
        {
            EnterAttackingState();
        }
        else if (distance <= detectionRange)
        {
            EnterChasingState();
            HandleChaseBehaviour();
        }
        else
        {
            EnterPatrollingState();
            HandlePatrolWaiting();
        }
    }

    private void EnterAttackingState()
    {
        if (currentState == EnemyState.Attacking) return;

        AudioManager.Instance.Stop(AudioManager.SoundType.Running);

        currentState = EnemyState.Attacking;
        agent.isStopped = true;

        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isRunningHash, false);
        animator.SetBool(isAttackingHash, true);

        StartCoroutine(PlayAttackSoundDelayed(0.5f));
        Debug.Log("Attacking!");
    }

    private void EnterChasingState()
    {
        if (currentState == EnemyState.Chasing) return;

        AudioManager.Instance.Stop(AudioManager.SoundType.EnemySmall_Attack_light);

        currentState = EnemyState.Chasing;
        agent.isStopped = false;

        animator.SetBool(isAttackingHash, false);
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isRunningHash, true);

        AudioManager.Instance.PlayLoop(AudioManager.SoundType.Running);
        Debug.Log("Player detected! Chasing...");
    }

    private void HandleChaseBehaviour()
    {
        if (isChasePaused) return;

        chaseTimer += Time.deltaTime;

        agent.SetDestination(player.position);

        if (chaseTimer >= chaseDuration)
        {
            StartCoroutine(ChasePause());
        }
    }

    IEnumerator ChasePause()
    {
        isChasePaused = true;
        chaseTimer = 0f;

        agent.isStopped = true;

        animator.SetBool(isRunningHash, false);
        animator.SetBool(isWalkingHash, false);

        yield return new WaitForSeconds(chasePause);

        if (currentState != EnemyState.Chasing)
        {
            isChasePaused = false;
            yield break;
        }

        agent.isStopped = false;

        animator.SetBool(isRunningHash, true);

        isChasePaused = false;
    }

    private void EnterPatrollingState()
    {
        chaseTimer = 0f;
        isChasePaused = false;

        AudioManager.Instance.Stop(AudioManager.SoundType.Running);

        if (currentState != EnemyState.Patrolling)
        {
            currentState = EnemyState.Patrolling;
            Debug.Log("Player lost. Resuming patrol...");
        }

        agent.isStopped = false;

        animator.SetBool(isAttackingHash, false);
        animator.SetBool(isRunningHash, false);
        animator.SetBool(isWalkingHash, true);

        if (!agent.hasPath && !isWaiting)
        {
            Patrol();
        }
    }

    private void HandlePatrolWaiting()
    {
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance &&
            !isWaiting)
        {
            StartCoroutine(WaitAndPatrol());
        }
    }

    void Patrol()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange + transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log(gameObject.name + " patrol destination set to: " + hit.position);
            animator.SetBool(isWalkingHash, true);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " could not find patrol point on NavMesh.");
            animator.SetBool(isWalkingHash, false);
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

    /*
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health -= damage;
        healthBar.UpdateHealthBar(health, maxHealth);
        Debug.Log(gameObject.name + " took damage! Health: " + health);
    }
    */

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        Debug.Log(gameObject.name + " takes damage: " + damage);
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {

            StartCoroutine(Die());
        }
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

        Debug.Log("Die() gestartet");
        Debug.Log("animator: " + animator);
        Debug.Log("agent: " + agent);
        Debug.Log("AudioManager.Instance: " + AudioManager.Instance);
        Debug.Log("healthSlider: " + healthSlider);

        if (isBoss && TimeTrialManager.Instance != null)
        {
            if (TimeTrialManager.Instance.IsRunning)
            {
                TimeTrialManager.Instance.StopTimer();
            }
        }

        if (healthSlider != null)
        {
            Destroy(healthSlider.gameObject);
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

        if (isBoss)
        {
            if (doorToOpen != null)
            {
                doorToOpen.Open();
            }

            if (spawnerToDisable != null)
            {
                spawnerToDisable.SetActive(false);
            }
        }

        Debug.Log(gameObject.name + " destroyed!");
        Destroy(gameObject);
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void ActivateBoss()
    {
        if (!isBoss)
            return;

        if (bossActivated)
            return;

        bossActivated = true;
        currentState = EnemyState.Patrolling;

        isWaiting = false;
        isChasePaused = false;
        chaseTimer = 0f;

        agent.isStopped = false;
        agent.ResetPath();

        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isRunningHash, false);
        animator.SetBool(isAttackingHash, false);
        animator.SetBool(isDyingHash, false);

        Debug.Log(gameObject.name + ": Boss activated.");

        Patrol();
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("Enemy touched something: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemy touched PLAYER");
            if (currentState == EnemyState.Attacking)
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    playerHealth.PlayerTakeDamage(enemyDamage);
                    lastAttackTime = Time.time;

                    Debug.Log("Player hit! Damage: " + enemyDamage);
                }
            }
        }
    }
}


