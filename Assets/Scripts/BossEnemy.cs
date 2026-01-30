using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    private static BossEnemy instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError(
                $"More than one BossEnemy in the scene! {gameObject.name} will be destroyed.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
