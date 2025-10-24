using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public void OnContinuePressed()
    {
        string lastLevel = PlayerPrefs.GetString("LastLevel", "Level1"); // fallback if none is saved
        SceneManager.LoadScene(lastLevel);
    }

    public void OnMainMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}


