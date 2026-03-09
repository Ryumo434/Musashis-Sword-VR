using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    //[SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private Slider healthSlider;

    private bool isDead = false;
    private float maxHealth;

    EnemyYBotAI enemyYBotAI;
    private void Start()
    {
        enemyYBotAI = GetComponent<EnemyYBotAI>();

        maxHealth = enemyYBotAI.GetMaxHealth();
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
           
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " ist besiegt");

        // Erstmal simpel:
        Destroy(gameObject);
    }
}