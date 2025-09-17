using UnityEngine;
using UnityEngine.SceneManagement;

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
    public void OpenPixels()
    {
        Debug.Log("Inventory not implemented yet!");
        // Later: Pixels(characters) scene or UI panel
    }

    // Called by Quit Button (if added later)
    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}

