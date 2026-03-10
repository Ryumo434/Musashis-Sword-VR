using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject victoryMenu;

    [Header("Player")]
    public Transform playerCamera;
    public float menuDistance = 2f;

    void Start()
    {
        ShowMainMenu();
    }

    void PositionMenu(GameObject menu)
    {
        Vector3 spawnPos = playerCamera.position + playerCamera.forward * menuDistance;

        menu.transform.position = spawnPos;

        menu.transform.LookAt(playerCamera);
        menu.transform.Rotate(0, 180, 0);
    }

    public void ShowMainMenu()
    {
        PositionMenu(mainMenu);

        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        victoryMenu.SetActive(false);
    }

    public void PlayGame()
    {
        mainMenu.SetActive(false);

        TimeTrialManager.Instance.StartTimer();
    }

    public void ShowPauseMenu()
    {
        PositionMenu(pauseMenu);

        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
    }

    public void ShowVictoryMenu()
    {
        PositionMenu(victoryMenu);

        victoryMenu.SetActive(true);
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