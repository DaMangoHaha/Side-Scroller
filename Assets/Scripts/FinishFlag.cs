using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishFlag : MonoBehaviour
{
    private bool levelComplete = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!levelComplete && collision.CompareTag("Player"))
        {
            levelComplete = true;
            Debug.Log("Tutorial Complete!");

            // Stop the timer (best time will auto-save on scene exit)
            if (LevelTimer.Instance != null)
            {
                LevelTimer.Instance.StopTimer();
            }

            // Unlock Level 2
            if (LevelUnlockManager.Instance != null)
                LevelUnlockManager.Instance.UnlockLevel(2);

            MusicManager.Instance.PlayMusic("Main Menu");
            // Return to Level Select
            SceneManager.LoadScene("LevelSelect");
        }

        if (levelComplete && collision.CompareTag("Player"))
        {
            Debug.Log("Level 2 already unlocked.");

            // Stop the timer (best time will auto-save on scene exit)
            if (LevelTimer.Instance != null)
            {
                LevelTimer.Instance.StopTimer();
            }

            // Unlock Level 3
            if (LevelUnlockManager.Instance != null)
                LevelUnlockManager.Instance.UnlockLevel(3);
            // Return to Level Select
            SceneManager.LoadScene("LevelSelect");
        }
        if (levelComplete && collision.CompareTag("Player"))
        {
            Debug.Log("Level 2 already unlocked.");

            // Stop the timer (best time will auto-save on scene exit)
            if (LevelTimer.Instance != null)
            {
                LevelTimer.Instance.StopTimer();
            }

            // Unlock Level 4
            if (LevelUnlockManager.Instance != null)
                LevelUnlockManager.Instance.UnlockLevel(4);
            // Return to Level Select
            SceneManager.LoadScene("LevelSelect");
        }
    }
}

