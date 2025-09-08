using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public void PlayLevel1()
    {
        // Loads the cutscene scene instead of directly the game
        SceneManager.LoadScene("Level1Cutscene");
    }
}

