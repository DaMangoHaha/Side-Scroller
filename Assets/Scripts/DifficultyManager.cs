using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages progressive difficulty scaling as the player survives longer.
/// Every 90 seconds, ONE random difficulty increase is applied:
/// - Enemy damage +5
/// - Enemy speed +2
/// - Enemy spawn rate increases
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Difficulty Scaling Settings")]
    [Tooltip("Time in seconds between each difficulty increase")]
    public float difficultyInterval = 90f; // 1 minute 30 seconds

    [Tooltip("Damage increase per difficulty spike")]
    public float damageIncrease = 5f;

    [Tooltip("Speed increase per difficulty spike")]
    public float speedIncrease = 2f;

    [Tooltip("Spawn rate multiplier per difficulty spike (lower = faster spawns)")]
    public float spawnRateMultiplier = 0.85f; // 15% faster spawns each time

    [Header("Notification Settings")]
    [Tooltip("UI Text element for center-screen notifications (optional)")]
    public TextMeshProUGUI notificationText;

    [Tooltip("How long the notification stays on screen")]
    public float notificationDuration = 3f;

    [Tooltip("Color for difficulty increase notifications")]
    public Color notificationColor = new Color(1f, 0.3f, 0.3f, 1f); // Red warning

    // Current difficulty modifiers (applied to enemies)
    [HideInInspector] public float bonusDamage = 0f;
    [HideInInspector] public float bonusSpeed = 0f;
    [HideInInspector] public float spawnRateModifier = 1f; // 1 = normal, lower = faster

    // Track timing
    private float lastDifficultyTime = 0f;
    private int difficultyLevel = 0;

    // Notification coroutine
    private Coroutine notificationCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hide notification text at start
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Only run if LevelTimer exists and is tracking time
        if (LevelTimer.Instance == null) return;

        float elapsed = LevelTimer.Instance.GetElapsedTime();

        // Check if it's time for a difficulty increase
        float nextDifficultyTime = (difficultyLevel + 1) * difficultyInterval;

        if (elapsed >= nextDifficultyTime)
        {
            ApplyRandomDifficultyIncrease();
            difficultyLevel++;
            lastDifficultyTime = elapsed;
        }
    }

    /// <summary>
    /// Randomly selects and applies ONE difficulty increase.
    /// </summary>
    private void ApplyRandomDifficultyIncrease()
    {
        // 0 = Damage, 1 = Speed, 2 = Spawn Rate
        int choice = Random.Range(0, 3);

        string message = "";

        switch (choice)
        {
            case 0:
                bonusDamage += damageIncrease;
                message = $"Obstacles deal +{damageIncrease} damage!";
                Debug.Log($"[DifficultyManager] Enemy damage increased! Total bonus: +{bonusDamage}");
                break;

            case 1:
                bonusSpeed += speedIncrease;
                message = $"Obstacles are faster!";
                Debug.Log($"[DifficultyManager] Enemy speed increased! Total bonus: +{bonusSpeed}");
                break;

            case 2:
                spawnRateModifier *= spawnRateMultiplier;
                message = "Obstacles spawn more frequently!";
                Debug.Log($"[DifficultyManager] Spawn rate increased! Modifier: {spawnRateModifier:F2}");
                break;
        }

        // Show notification
        ShowNotification(message);
    }

    /// <summary>
    /// Displays a center-screen notification to warn the player.
    /// </summary>
    private void ShowNotification(string message)
    {
        // If no UI text assigned, create a floating popup at screen center instead
        if (notificationText == null)
        {
            // Find player position for popup, or use camera center
            Vector3 popupPos = Camera.main != null
                ? Camera.main.transform.position + new Vector3(0, 1f, 10f)
                : Vector3.zero;

            // Use existing popup system with larger text
            CreateDifficultyPopup(popupPos, message);
            return;
        }

        // Stop any existing notification
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine = StartCoroutine(ShowNotificationRoutine(message));
    }

    private IEnumerator ShowNotificationRoutine(string message)
    {
        notificationText.text = message;
        notificationText.color = notificationColor;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(notificationDuration);

        notificationText.gameObject.SetActive(false);
        notificationCoroutine = null;
    }

    /// <summary>
    /// Creates a larger, more prominent popup for difficulty warnings.
    /// </summary>
    private void CreateDifficultyPopup(Vector3 position, string message)
    {
        GameObject popupGO = new GameObject("DifficultyPopup");
        popupGO.transform.position = position;

        TextMeshPro tmp = popupGO.AddComponent<TextMeshPro>();
        tmp.text = "WARNING: " + message;
        tmp.fontSize = 5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = notificationColor;
        tmp.sortingOrder = 200;

        RectTransform rt = popupGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(12f, 3f);

        // Add a slightly longer-lasting popup behavior
        DifficultyPopup popup = popupGO.AddComponent<DifficultyPopup>();
        popup.lifetime = notificationDuration;
    }

    /// <summary>
    /// Resets all difficulty modifiers. Call this when starting a new run.
    /// </summary>
    public void ResetDifficulty()
    {
        bonusDamage = 0f;
        bonusSpeed = 0f;
        spawnRateModifier = 1f;
        difficultyLevel = 0;
        lastDifficultyTime = 0f;

        Debug.Log("[DifficultyManager] Difficulty reset to baseline.");
    }

    /// <summary>
    /// Gets the current difficulty level (number of increases applied).
    /// </summary>
    public int GetDifficultyLevel()
    {
        return difficultyLevel;
    }
}

/// <summary>
/// Simple popup component for difficulty warnings - stays longer and pulses.
/// </summary>
public class DifficultyPopup : MonoBehaviour
{
    public float lifetime = 3f;
    public float floatSpeed = 0.3f;
    public float pulseSpeed = 3f;

    private TextMeshPro textMesh;
    private float timer;
    private Color baseColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            baseColor = textMesh.color;
        }
    }

    void Update()
    {
        // Slow float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Pulse effect
        if (textMesh != null)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(0.9f, 1.1f, pulse);
            transform.localScale = Vector3.one * scale;
        }

        // Fade out near end
        timer += Time.deltaTime;
        if (timer > lifetime * 0.7f)
        {
            float fadeProgress = (timer - lifetime * 0.7f) / (lifetime * 0.3f);
            float alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            if (textMesh != null)
            {
                Color c = baseColor;
                c.a = alpha;
                textMesh.color = c;
            }
        }

        // Destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
