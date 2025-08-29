using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called by Play Button
    public void PlayGame()
    {
        // Load the next scene (Game Scene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Called by Settings Button
    public void OpenSettings()
    {
        Debug.Log("Settings menu not implemented yet!");
        // Later: open a UI panel
    }

    // Called by Character Select Button
    public void OpenCharacterSelect()
    {
        Debug.Log("Character select not implemented yet!");
        // Later: load CharacterSelect scene or UI panel
    }

    // Called by Quit Button (if you add one later)
    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}

