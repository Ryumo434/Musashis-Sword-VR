using UnityEngine;
using TMPro;

public class MenuUI : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject startPanel;
    public GameObject namePanel;
    public GameObject victoryPanel;

    public TMP_InputField nameInputField;

    void Start()
    {
        startPanel.SetActive(true);
        namePanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    public void OpenNamePanel()
    {
        startPanel.SetActive(false);
        namePanel.SetActive(true);
    }

    public void PlayGame()
    {
        string playerName = nameInputField.text;
        Debug.Log("Player Name: " + playerName);

        // Menü schließen
        mainMenu.SetActive(false);

        // Timer starten
        TimeTrialManager.Instance.StartTimer();
    }

    public void ShowVictoryPanel()
    {
        Debug.Log("Victory Panel triggered");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}