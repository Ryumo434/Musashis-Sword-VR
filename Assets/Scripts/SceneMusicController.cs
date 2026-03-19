using UnityEngine;

public class SceneMusicController : MonoBehaviour
{
    [SerializeField] private AudioManager.SoundType musicType;

    private void Start()
    {
        Debug.Log($"Playing music: {musicType}");
        AudioManager.Instance.PlayLoop(musicType);
    }

    private void OnDestroy()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Stop(musicType);
        }
    }
}