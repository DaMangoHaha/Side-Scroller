using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Attach this to an empty GameObject in a level scene.
/// Assign a TextMeshPro (or TextMeshProUGUI) element to display
/// the player's personal best time for the current level.
/// </summary>
public class PersonalBestDisplay : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The TextMeshProUGUI element that will show the personal best time.")]
    public TextMeshProUGUI bestTimeText;

    [Header("Formatting")]
    [Tooltip("Prefix shown before the time value.")]
    public string prefix = "Personal Best: ";

    [Tooltip("Text shown when no best time has been recorded yet.")]
    public string noRecordText = "Personal Best: --:--.--";

    void Start()
    {
        RefreshDisplay();
    }

    void OnEnable()
    {
        RefreshDisplay();
    }

    /// <summary>
    /// Reads the saved best time for the active scene and updates the text element.
    /// </summary>
    public void RefreshDisplay()
    {
        if (bestTimeText == null) return;

        string levelName = SceneManager.GetActiveScene().name;
        SaveData data = SaveSystem.LoadData();
        float bestTime = LevelTimer.GetBestTimeForLevel(data, levelName);

        if (bestTime > 0f)
        {
            bestTimeText.text = $"{prefix}{LevelTimer.FormatTime(bestTime)}";
        }
        else
        {
            bestTimeText.text = noRecordText;
        }
    }
}
