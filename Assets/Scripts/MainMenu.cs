using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Map VR Scene");
    }

    public void CloseGame()
    {
        Debug.Log("Game closed");

        Application.Quit();
    }
}