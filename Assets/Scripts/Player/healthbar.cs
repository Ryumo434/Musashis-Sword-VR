using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class healthbar : MonoBehaviour
{
    public Slider healthslider;
    public Slider easeHealthSlider;
    public float maxHealth = 100f;
    public float health;
    [SerializeField] private float lerpSpeed = 100f;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;


        healthslider.maxValue = maxHealth;
        easeHealthSlider.maxValue = maxHealth;

        healthslider.value = maxHealth;
        easeHealthSlider.value = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
     
            healthslider.value = health;
        

        if (Input.GetKeyDown(KeyCode.Space))
        {
            takeDamage(10f);
        }

        if(easeHealthSlider.value != health)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, health, lerpSpeed * Time.deltaTime);
        }
    }


    void takeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);
    }
}
