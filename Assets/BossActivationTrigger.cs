using UnityEngine;

public class BossActivationTrigger : MonoBehaviour
{
    private EnemyYBotAI boss;
    private bool triggered = false;

    public void Initialize(EnemyYBotAI targetBoss)
    {
        boss = targetBoss;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.name + " | Tag: " + other.tag);

        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        if (boss != null)
        {
            boss.ActivateBoss();
        }
    }
}