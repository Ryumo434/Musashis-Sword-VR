using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class healthbar : MonoBehaviour
{
    public Slider healthslider;
    public Slider easeHealthSlider;

    public float maxHealth = 100f;
    public float health;

    [SerializeField] private float lerpSpeed = 100f;

    private bool isDead = false;

    void Start()
    {
        health = maxHealth;

        healthslider.maxValue = maxHealth;
        easeHealthSlider.maxValue = maxHealth;

        healthslider.value = maxHealth;
        easeHealthSlider.value = maxHealth;
    }

    void Update()
    {
        healthslider.value = health;

        if (health <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(RestartDelay());
        }

        if (easeHealthSlider.value != health)
        {
            easeHealthSlider.value = Mathf.Lerp(
                easeHealthSlider.value,
                health,
                lerpSpeed * Time.deltaTime
            );
        }
    }

    public void PlayerTakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);
    }

    IEnumerator RestartDelay()
    {
        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;

#if UNITY_EDITOR
        EditorApplication.isPaused = false;
#endif

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}