using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance;

    [Header("UI")]
    public TextMeshProUGUI timerText;

    private float elapsedTime;
    private bool isRunning;
    private string currentLevelName;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Auto-save best time when the timer is disabled (scene change, object destroyed, etc.)
        SaveBestTime();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-start timer on level scenes, stop on menus
        string sceneName = scene.name;

        if (sceneName.Contains("Level"))
        {
            currentLevelName = sceneName;
            ResetTimer();
            StartTimer();
        }
        else
        {
            StopTimer();
            currentLevelName = null;
        }
    }

    private void OnApplicationQuit()
    {
        // Safety net: save best time when the application is closing
        SaveBestTime();
    }

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        UpdateUI();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateUI();
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public string GetFormattedTime()
    {
        return FormatTime(elapsedTime);
    }

    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    /// <summary>
    /// Saves the current elapsed time as the best time for the current level if it beats the previous record.
    /// Returns true if a new personal best was set.
    /// </summary>
    public bool SaveBestTime()
    {
        if (string.IsNullOrEmpty(currentLevelName)) return false;

        // Check if player met the Level 1 unlock requirement
        if (currentLevelName == "Level1" && LevelUnlockManager.Instance != null)
        {
            LevelUnlockManager.Instance.CheckAndUnlockFromLevel1(elapsedTime);
        }

        SaveData data = SaveSystem.LoadData();
        float previousBest = GetBestTimeForLevel(data, currentLevelName);

        // Save if no previous record or current time is longer (longest survival)
        if (previousBest <= 0f || elapsedTime > previousBest)
        {
            SetBestTimeForLevel(data, currentLevelName, elapsedTime);
            SaveSystem.SaveData(data);
            Debug.Log($"New personal best for {currentLevelName}: {FormatTime(elapsedTime)}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the saved best time for a given level scene name.
    /// </summary>
    public static float GetBestTimeForLevel(SaveData data, string levelName)
    {
        return levelName switch
        {
            "Level1" => data.bestTimeLevel1,
            "Level2" => data.bestTimeLevel2,
            "Level3" => data.bestTimeLevel3,
            "Level4" => data.bestTimeLevel4,
            _ => 0f
        };
    }

    private static void SetBestTimeForLevel(SaveData data, string levelName, float time)
    {
        switch (levelName)
        {
            case "Level1": data.bestTimeLevel1 = time; break;
            case "Level2": data.bestTimeLevel2 = time; break;
            case "Level3": data.bestTimeLevel3 = time; break;
            case "Level4": data.bestTimeLevel4 = time; break;
        }
    }

    private void UpdateUI()
    {
        if (timerText != null)
            timerText.text = GetFormattedTime();
    }
}
