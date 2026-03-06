using UnityEngine;

public class EnemyGroupMember : MonoBehaviour
{
    [Header("Group / Room Assignment")]
    [SerializeField] private string groupTag;
    [SerializeField] private bool autoRegisterOnEnable = true;

    private bool registered;

    public string GroupTag => groupTag;

    private void OnEnable()
    {
        if (autoRegisterOnEnable && !registered && !string.IsNullOrWhiteSpace(groupTag))
        {
            EnemyGroupRegistry.RegisterAlive(groupTag);
            registered = true;
        }
    }

    private void OnDisable()
    {
        // Absichtlich leer.
        // Wir deregistrieren in OnDestroy, damit Gegner beim Deaktivieren
        // nicht fälschlich als tot gelten.
    }

    private void OnDestroy()
    {
        if (registered)
        {
            EnemyGroupRegistry.UnregisterAlive(groupTag);
            registered = false;
        }
    }

    public void InitializeAtRuntime(string newGroupTag)
    {
        if (registered)
        {
            EnemyGroupRegistry.UnregisterAlive(groupTag);
            registered = false;
        }

        groupTag = newGroupTag;

        if (!string.IsNullOrWhiteSpace(groupTag))
        {
            EnemyGroupRegistry.RegisterAlive(groupTag);
            registered = true;
        }
    }
}