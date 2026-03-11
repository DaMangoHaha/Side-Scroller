using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public void Start()
    {
        MusicManager.Instance.PlayMusic("Game Over");
    }
    public void OnContinuePressed()
    {
        string lastLevel = PlayerPrefs.GetString("LastLevel", "Level1"); // fallback if none is saved
        SceneTransition.Instance.LoadScene(lastLevel);
    }

    public void OnMainMenuPressed()
    {
        SceneTransition.Instance.LoadScene("MainMenu");
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}


