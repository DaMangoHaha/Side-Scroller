using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    public string nextSceneName = "Level3";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameEvents.slimeExterminationActive)
            {
                Debug.Log("Event complete — loading next level.");
                MusicManager.Instance.PlayMusic("Level 3");
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("You can't leave yet — talk to the Thief first.");
            }
        }
    }
}

