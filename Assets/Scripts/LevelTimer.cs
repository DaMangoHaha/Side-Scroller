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
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-start timer on level scenes, stop on menus
        string sceneName = scene.name;

        if (sceneName.Contains("Level"))
        {
            ResetTimer();
            StartTimer();
        }
        else
        {
            StopTimer();
        }
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
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    private void UpdateUI()
    {
        if (timerText != null)
            timerText.text = GetFormattedTime();
    }
}
