using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public void PlayLevel1()
    {
        // Loads the cutscene scene instead of directly the game
        SceneManager.LoadScene("Level1Cutscene");
    }

    public void PlayLevel2()
            {
        SceneManager.LoadScene("Level2");
    }

    public void PlayLevel3()
    {
               SceneManager.LoadScene("Level3");
    }

    public void PlayLevel4()
    {
        SceneManager.LoadScene("Level4");
    }
}

