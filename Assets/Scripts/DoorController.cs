using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
public class DoorOpenOnPress : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private TextMeshPro text;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Open()
    {
        if (spawner.CurrentAlive < 0)
        {
            anim.SetTrigger("OpenDoor");
            text.color = Color.green;
            text.text = "Access Granted!";
        } else
        {
            text.color = Color.red;
            GetComponent<TextFlash>().ShowTemporaryMessage("Access Denied!", 3f);
        }
    }

    void OnEnable()
    {
        if (spawner != null)
        {
            // Initial auslesen
            UpdateUI(spawner.CurrentAlive);
            // Live-Updates
            spawner.OnAliveChanged += UpdateUI;
        }
    }
    void OnDisable()
    {
        if (spawner != null) spawner.OnAliveChanged -= UpdateUI;
    }

    void UpdateUI(int alive)
    {
        text.color = Color.white;
        text.text = $"Current Enemies: {alive}";
    }
}