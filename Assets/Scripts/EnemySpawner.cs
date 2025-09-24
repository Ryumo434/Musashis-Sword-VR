using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; // nur falls du AttackAction übergeben willst

[System.Serializable]
public struct EnemyConfig
{
    [Header("Perception & Combat")]
    public float detectionRange;
    public float enemyAttackRange;
    public float playerAttackRange;

    [Header("Movement & Patrol")]
    public float moveSpeed;
    public float patrolRange;
    public float waitTime;

    [Header("Health")]
    public float maxHealth;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Spawn")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;                 // wenn leer -> Position dieses GameObjects
    public float intervalSeconds = 15f;
    public float firstDelay = 0f;
    public int maxAlive = -1;                    // -1 = unbegrenzt

    [Header("Enemy Parameters")]
    public EnemyConfig config = new EnemyConfig
    {
        detectionRange = 10f,
        enemyAttackRange = 2f,
        playerAttackRange = 2f,
        moveSpeed = 3f,
        patrolRange = 5f,
        waitTime = 2f,
        maxHealth = 10f
    };

    [Tooltip("Optional: überschreibt EnemyAI.player (z. B. HMD Transform). Wenn leer, nutzt der EnemyAI MainCamera.")]
    public Transform playerOverride;

    [Tooltip("Optional: falls du das Angriffs-Input vom Spieler an den Enemy weiterleiten willst (dein Script hat ein privates Feld).")]
    public InputActionProperty attackAction;

    private readonly List<GameObject> alive = new();
    private Coroutine loop;

    void OnEnable() { loop = StartCoroutine(SpawnLoop()); }
    void OnDisable() { if (loop != null) StopCoroutine(loop); loop = null; }

    IEnumerator SpawnLoop()
    {
        if (firstDelay > 0f) yield return new WaitForSeconds(firstDelay);
        var wait = new WaitForSeconds(intervalSeconds);

        while (true)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemySpawner: enemyPrefab nicht gesetzt!");
                yield break;
            }

            if (maxAlive < 0 || alive.Count < maxAlive)
                SpawnOne();

            yield return wait;
        }
    }

    public GameObject SpawnOne()
    {
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

        // Optional: auf NavMesh einrasten
        if (NavMesh.SamplePosition(pos, out var hit, 2f, NavMesh.AllAreas))
            pos = hit.position;

        var go = Instantiate(enemyPrefab, pos, rot);
        alive.Add(go);
        go.AddComponent<DespawnTracker>().Init(() => alive.Remove(go));

        // --- Parameter setzen ---
        var ai = go.GetComponent<EnemyAI>();
        if (ai != null)
        {
            // Player-Ziel
            if (playerOverride) ai.player = playerOverride;

            // Basis-Parameter (vor Start() gesetzt → werden in Start() genutzt)
            ai.detectionRange = config.detectionRange;
            ai.enemyAttackRange = config.enemyAttackRange;
            ai.playerAttackRange = config.playerAttackRange;
            ai.moveSpeed = config.moveSpeed;
            ai.patrolRange = config.patrolRange;
            ai.waitTime = config.waitTime;
            ai.maxHealth = config.maxHealth;
            ai.health = config.maxHealth; // falls du direkt voll starten willst

            // NavMeshAgent Speed synchron halten
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent) agent.speed = config.moveSpeed;

            // Optional: dein privates Feld via Helper (siehe Teil 2)
            //if (attackAction.action != null)
            //    ai.SetAttackAction(attackAction);
        }
        else
        {
            Debug.LogWarning("EnemySpawner: Prefab hat kein EnemyAI.");
        }

        return go;
    }

    void OnDrawGizmos()
    {
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Gizmos.DrawWireSphere(pos, 0.25f);
    }

    private class DespawnTracker : MonoBehaviour
    {
        private System.Action onDestroyed;
        public void Init(System.Action cb) => onDestroyed = cb;
        void OnDestroy() => onDestroyed?.Invoke();
    }
}