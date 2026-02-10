using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
public class DoorOpenOnPress : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner spawner;   // muss gesetzt werden
    [SerializeField] private TextMeshPro text;       // muss gesetzt werden

    [Header("Door")]
    [SerializeField] private string openTriggerName = "OpenDoor";

    [Header("UI")]
    [SerializeField] private float feedbackSeconds = 2.0f; // wie lange Granted/Denied stehen bleibt
    [SerializeField] private bool showIdleCounter = true;

    private Animator anim;
    private bool opened = false;
    private float feedbackUntilTime = 0f;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (spawner == null)
        {
            Debug.LogError($"{name}: Spawner reference missing!", this);
            return;
        }

        spawner.OnAliveChanged += UpdateUI;

        UpdateUI(spawner.CurrentAlive);
    }

    void OnDisable()
    {
        if (spawner != null)
            spawner.OnAliveChanged -= UpdateUI;
    }

    public void Open()
    {
        if (spawner == null || text == null)
        {
            Debug.LogError($"{name}: Missing references (spawner/text).", this);
            return;
        }

        if (opened)
        {
            return;
        }

        int alive = spawner.CurrentAlive;

        if (alive <= 0)
        {
            opened = true;

            anim.ResetTrigger(openTriggerName);
            anim.SetTrigger(openTriggerName);

            text.color = Color.green;
            text.text = "Access Granted!";
            feedbackUntilTime = Time.time + feedbackSeconds;
        }
        else
        {
            text.color = Color.red;
            text.text = $"Access Denied! ({alive} alive)";
            feedbackUntilTime = Time.time + feedbackSeconds;
        }
    }

    void UpdateUI(int alive)
    {
        if (Time.time < feedbackUntilTime) return;

        if (!showIdleCounter) return;

        if (text == null) return;

        text.color = Color.white;
        text.text = $"Enemies alive: {alive}";
    }
}
