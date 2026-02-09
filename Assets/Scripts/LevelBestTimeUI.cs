using UnityEngine;
using TMPro;

public class LevelBestTimeUI : MonoBehaviour
{
    [Header("Best Time Text Elements")]
    public TextMeshProUGUI level1BestTimeText;
    public TextMeshProUGUI level2BestTimeText;
    public TextMeshProUGUI level3BestTimeText;
    public TextMeshProUGUI level4BestTimeText;

    void Start()
    {
        RefreshBestTimes();
    }

    void OnEnable()
    {
        RefreshBestTimes();
    }

    public void RefreshBestTimes()
    {
        SaveData data = SaveSystem.LoadData();

        SetBestTimeText(level1BestTimeText, data.bestTimeLevel1, "Level 1");
        SetBestTimeText(level2BestTimeText, data.bestTimeLevel2, "Level 2");
        SetBestTimeText(level3BestTimeText, data.bestTimeLevel3, "Level 3");
        SetBestTimeText(level4BestTimeText, data.bestTimeLevel4, "Level 4");
    }

    private void SetBestTimeText(TextMeshProUGUI textElement, float bestTime, string levelName)
    {
        if (textElement == null) return;

        if (bestTime > 0f)
        {
            textElement.text = $"Best: {LevelTimer.FormatTime(bestTime)}";
        }
        else
        {
            textElement.text = "Best: --:--.--";
        }
    }
}
