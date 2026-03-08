using UnityEngine;
using System.Collections.Generic;

public class SwordDamage : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float minVelocityForDamage = 1.5f;

    [SerializeField] private AudioSource hitAudio;

    private HashSet<EnemyHealth> alreadyHitEnemies = new HashSet<EnemyHealth>();

    private Vector3 lastPosition;
    private float currentVelocity;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        currentVelocity = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("EnemyHitbox"))
            return;

        if (currentVelocity < minVelocityForDamage)
            return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy != null && !alreadyHitEnemies.Contains(enemy))
        {
            enemy.TakeDamage(damage);
            alreadyHitEnemies.Add(enemy);

            hitAudio.Play(); // Laser Treffer Sound

            Debug.Log("Gegner getroffen: " + other.name + " | Velocity: " + currentVelocity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("EnemyHitbox"))
            return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            alreadyHitEnemies.Remove(enemy);
        }
    }
}