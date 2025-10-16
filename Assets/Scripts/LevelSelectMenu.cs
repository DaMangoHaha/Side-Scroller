using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button level1Button;
    public Button level2Button;

    void Start()
    {
        // Ensure buttons exist
        if (level1Button != null)
            level1Button.onClick.AddListener(() => LoadLevel("Level1"));

        if (level2Button != null)
            level2Button.onClick.AddListener(() => LoadLevel("Level2"));

        UpdateButtons();
    }

    void UpdateButtons()
    {
        if (LevelUnlockManager.Instance != null)
        {
            level1Button.interactable = LevelUnlockManager.Instance.level1Unlocked;
            level2Button.interactable = LevelUnlockManager.Instance.level2Unlocked;
        }
    }

    void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
