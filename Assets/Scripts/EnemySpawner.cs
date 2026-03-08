using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

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
    [Header("Spawn Setup")]
    [Tooltip("Mindestens 1, typischerweise 2 bis 3 Prefabs. Daraus wird zufällig gewählt.")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    public Transform spawnPoint;
    public float intervalSeconds = 15f;
    public float firstDelay = 0f;

    [Tooltip("-1 = unbegrenzt gleichzeitig aktiv")]
    public int maxAlive = -1;

    [Tooltip("Wie viele Gegner dieser Spawner insgesamt im ganzen Spiel erzeugen darf.")]
    public int maxTotalEnemies = 15;

    [Header("Room / Group")]
    [Tooltip("Logischer Raum-Tag. Alle Gegner dieses Spawners bekommen diesen Wert.")]
    public string groupTag = "Room1";

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

    [Tooltip("Optional: überschreibt EnemyAI.player.")]
    public Transform playerOverride;

    [Tooltip("Optional: falls du Angriffsinput weiterreichen willst.")]
    public InputActionProperty attackAction;

    public int CurrentAlive { get { return alive.Count; } }
    public int SpawnedEnemies { get { return spawnedEnemies; } }
    public int RemainingToSpawn { get { return Mathf.Max(0, maxTotalEnemies - spawnedEnemies); } }

    public event System.Action<int> OnAliveChanged;

    private readonly List<GameObject> alive = new List<GameObject>();
    private Coroutine loop;
    private bool plannedSpawnsRegistered;
    private int spawnedEnemies;

    private void OnEnable()
    {
        RegisterRemainingPlannedSpawns();
        loop = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (loop != null)
            StopCoroutine(loop);

        loop = null;

        UnregisterRemainingPlannedSpawns();
    }

    private void RegisterRemainingPlannedSpawns()
    {
        if (plannedSpawnsRegistered)
            return;

        int remaining = RemainingToSpawn;
        if (remaining > 0)
        {
            EnemyGroupRegistry.AddPlannedSpawns(groupTag, remaining);
            plannedSpawnsRegistered = true;
        }
    }

    private void UnregisterRemainingPlannedSpawns()
    {
        if (!plannedSpawnsRegistered)
            return;

        int remaining = RemainingToSpawn;
        if (remaining > 0)
            EnemyGroupRegistry.RemovePlannedSpawns(groupTag, remaining);

        plannedSpawnsRegistered = false;
    }

    private IEnumerator SpawnLoop()
    {
        if (firstDelay > 0f)
            yield return new WaitForSeconds(firstDelay);

        WaitForSeconds wait = new WaitForSeconds(intervalSeconds);

        while (spawnedEnemies < maxTotalEnemies)
        {
            if (!HasValidPrefab())
            {
                Debug.LogError(name + ": EnemySpawner hat keine gültigen enemyPrefabs gesetzt!", this);
                yield break;
            }

            if (maxAlive < 0 || alive.Count < maxAlive)
                SpawnOne();

            yield return wait;
        }
    }

    private bool HasValidPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
            return false;

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab != null)
                return true;
        }

        return false;
    }

    private GameObject GetRandomPrefab()
    {
        List<GameObject> valid = new List<GameObject>();

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab != null)
                valid.Add(prefab);
        }

        if (valid.Count == 0)
            return null;

        int index = Random.Range(0, valid.Count);
        return valid[index];
    }

    public GameObject SpawnOne()
    {
        if (spawnedEnemies >= maxTotalEnemies)
            return null;

        GameObject selectedPrefab = GetRandomPrefab();
        if (selectedPrefab == null)
        {
            Debug.LogError(name + ": Kein gültiges Enemy Prefab gefunden.", this);
            return null;
        }

        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, 2f, NavMesh.AllAreas))
            pos = hit.position;

        GameObject go = Instantiate(selectedPrefab, pos, rot);

        alive.Add(go);
        spawnedEnemies++;

        EnemyGroupRegistry.ConsumePlannedSpawn(groupTag, 1);

        DespawnTracker despawnTracker = go.GetComponent<DespawnTracker>();
        if (despawnTracker == null)
            despawnTracker = go.AddComponent<DespawnTracker>();

        despawnTracker.Init(() =>
        {
            alive.Remove(go);
            if (OnAliveChanged != null)
                OnAliveChanged(alive.Count);
        });

        EnemyGroupMember groupMember = go.GetComponent<EnemyGroupMember>();
        if (groupMember == null)
            groupMember = go.AddComponent<EnemyGroupMember>();

        groupMember.InitializeAtRuntime(groupTag);

        if (OnAliveChanged != null)
            OnAliveChanged(alive.Count);

        ApplyEnemyParameters(go);

        return go;
    }

    private void ApplyEnemyParameters(GameObject go)
    {
        EnemyAI ai = go.GetComponent<EnemyAI>();
        if (ai == null)
        {
            Debug.LogWarning(name + ": Prefab '" + go.name + "' hat kein EnemyAI.", go);
            return;
        }

        if (playerOverride != null)
            ai.player = playerOverride;

        ai.detectionRange = config.detectionRange;
        ai.enemyAttackRange = config.enemyAttackRange;
        ai.playerAttackRange = config.playerAttackRange;
        ai.moveSpeed = config.moveSpeed;
        ai.patrolRange = config.patrolRange;
        ai.waitTime = config.waitTime;
        ai.maxHealth = config.maxHealth;
        ai.health = config.maxHealth;

        NavMeshAgent agent = go.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.speed = config.moveSpeed;
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        Gizmos.DrawWireSphere(pos, 0.25f);
    }

    private class DespawnTracker : MonoBehaviour
    {
        private System.Action onDestroyed;

        public void Init(System.Action callback)
        {
            onDestroyed = callback;
        }

        private void OnDestroy()
        {
            if (onDestroyed != null)
                onDestroyed();
        }
    }
}