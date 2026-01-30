using UnityEngine;
using TMPro;

public class TimeTrialManager : MonoBehaviour
{
    public static TimeTrialManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    private float startTime;
    private float elapsedTime;
    private bool isRunning = false;
    public bool IsRunning => isRunning;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Provisorischer Start
        if (!isRunning && Input.GetKeyDown(KeyCode.F1))
        {
            StartTimer();
        }

        if (isRunning)
        {
            elapsedTime = Time.time - startTime;
            UpdateTimerUI();
        }
    }

    public void StartTimer()
    {
        startTime = Time.time;
        elapsedTime = 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        if (!isRunning) return;
        isRunning = false;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100) % 100);

        timerText.text = $"{minutes:00}:{seconds:00},{milliseconds:00}";
    }
}
