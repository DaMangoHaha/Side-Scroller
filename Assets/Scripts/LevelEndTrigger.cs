using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    [Header("Scene Transition")]
    public string nextSceneName = "Level3";

    [Header("Music")]
    [Tooltip("The track name to play when transitioning (must match a name in MusicLibrary).")]
    public string musicTrackName = "Level 3";

    [Header("Event Gate (Optional)")]
    [Tooltip("If true, the player must complete the event (e.g., talk to Thief) before leaving.")]
    public bool requiresEventGate = true;

    [Tooltip("Message shown in the console when the player tries to leave before the event is complete.")]
    public string blockedMessage = "You can't leave yet — talk to the Thief first.";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!requiresEventGate || GameEvents.slimeExterminationActive)
            {
                Debug.Log("Event complete — loading next level.");
                SceneTransition.Instance.LoadScene(nextSceneName, () => MusicManager.Instance.PlayMusic(musicTrackName));
            }
            else
            {
                Debug.Log(blockedMessage);
            }
        }
    }
}

