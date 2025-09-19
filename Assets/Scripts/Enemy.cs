using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float patrolRange = 5f;
    public float waitTime = 2f;

    private string currentState = "Patrolling";
    private NavMeshAgent agent;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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

        Patrol();
    }


    void Update()
    {
        if (player == null)
        {
            Debug.LogError("Player reference not set on Enemy!");
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        Debug.Log("Distance to player: " + distance);

        if (distance <= attackRange)
        {
            if (currentState != "Attacking")
            {
                currentState = "Attacking";
                Debug.Log("Attacking!");
            }
        }
        else if (distance <= detectionRange)
        {
            if (currentState != "Chasing")
            {
                currentState = "Chasing";
                Debug.Log("Player detected! Chasing...");
            }
            agent.SetDestination(player.position);
        }
        else
        {
            if (currentState != "Patrolling")
            {
                currentState = "Patrolling";
                Debug.Log("Player lost. Resuming patrol...");
                Patrol();
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
            {
                StartCoroutine(WaitAndPatrol());
            }
            else
            {
            }
        }
    }

    void Patrol()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log("Patrolling to: " + hit.position);
        }
    }
    
    IEnumerator WaitAndPatrol()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        Patrol();
        isWaiting = false;
    }
}
