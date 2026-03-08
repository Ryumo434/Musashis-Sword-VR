using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
public class DoorOpenOnPress : MonoBehaviour
{
    [Header("Room / Group")]
    [SerializeField] private string groupTag = "Room1";

    [Header("References")]
    [SerializeField] private TextMeshPro text;

    [Header("Door")]
    [SerializeField] private string openTriggerName = "OpenDoor";

    [Header("UI")]
    [SerializeField] private float feedbackSeconds = 2.0f;
    [SerializeField] private bool showIdleCounter = true;

    private Animator anim;
    private bool opened;
    private float feedbackUntilTime;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        EnemyGroupRegistry.OnGroupStateChanged += HandleGroupStateChanged;
        UpdateUI(EnemyGroupRegistry.GetState(groupTag));
    }

    private void OnDisable()
    {
        EnemyGroupRegistry.OnGroupStateChanged -= HandleGroupStateChanged;
    }

    private void HandleGroupStateChanged(string changedGroupTag, EnemyGroupRegistry.GroupState state)
    {
        if (changedGroupTag != groupTag)
            return;

        UpdateUI(state);
    }

    public void Open()
    {
        if (text == null)
        {
            Debug.LogError($"{name}: Text reference missing.", this);
            return;
        }

        if (opened)
            return;

        var state = EnemyGroupRegistry.GetState(groupTag);

        if (state.IsClear)
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
            text.text = $"Access Denied! ({state.aliveCount} alive, {state.remainingSpawnCount} pending)";
            feedbackUntilTime = Time.time + feedbackSeconds;
        }
    }

    private void UpdateUI(EnemyGroupRegistry.GroupState state)
    {
        if (Time.time < feedbackUntilTime)
            return;

        if (!showIdleCounter)
            return;

        if (text == null)
            return;

        text.color = Color.white;
        text.text = $"Alive: {state.aliveCount} | Pending: {state.remainingSpawnCount}";
    }
}
