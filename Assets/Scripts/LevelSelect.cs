using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public void PlayLevel1()
    {
        MusicManager.Instance.PlayMusic("Level 1");
        SceneManager.LoadScene("Level1");
    }

    public void PlayLevel2()
    {
        MusicManager.Instance.PlayMusic("Level 2");
        SceneManager.LoadScene("Level2");
    }

    public void PlayLevel3()
    {
        MusicManager.Instance.PlayMusic("Level 3");
        SceneManager.LoadScene("Level3");
    }

    public void PlayLevel4()
    {
        MusicManager.Instance.PlayMusic("Level 4");
        SceneManager.LoadScene("Level4");
    }
}

