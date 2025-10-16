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

            // Unlock Level 2
            if (LevelUnlockManager.Instance != null)
                LevelUnlockManager.Instance.UnlockLevel(2);

            // Return to Level Select
            SceneManager.LoadScene("LevelSelect");
        }
    }
}

