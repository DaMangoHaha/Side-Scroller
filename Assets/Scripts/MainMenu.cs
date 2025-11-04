using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // Called by Play Button
    public void PlayGame()
    {
        // Load the next scene (Level Select)
        SceneManager.LoadScene("LevelSelect");
    }

    // Called by Settings Button
    public void OpenSettings()
    {
        Debug.Log("Settings menu not implemented yet!");
        // Later: open a UI panel
    }

    // Called by Shop Button
    public void OpenCredits()
    {
        SceneManager.LoadScene("Credits");
        // Credits scene for appropriate acknowledgments for assests used in game
    }

    // Called by Quit Button (if added later)
    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}

