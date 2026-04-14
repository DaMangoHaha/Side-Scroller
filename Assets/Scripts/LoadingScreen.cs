using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton loading screen that displays an animated progress bar and optional
/// gameplay tips while asynchronously loading the next scene.
///
/// Works alongside SceneTransition — the fade overlay handles the initial fade-to-black,
/// then this screen becomes visible, shows progress, and fades out once the scene is ready.
///
/// Attach to a persistent GameObject (or let SceneTransition create one for you).
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("Timing")]
    [Tooltip("Minimum seconds the loading screen stays visible (so it doesn't just flash).")]
    public float minimumDisplayTime = 2f;

    [Tooltip("How quickly the displayed progress bar catches up to actual progress.")]
    public float progressLerpSpeed = 3f;

    [Header("Numeric Counter")]
    [Tooltip("Speed at which the cosmetic number counter ticks up (units per second). Higher = faster counting.")]
    public float counterSpeed = 80f;

    [Header("Tips (Optional)")]
    [Tooltip("Random tips shown at the bottom of the loading screen. Leave empty to hide.")]
    [TextArea(2, 4)]
    public string[] tips = new string[]
    {
        "Tip: Collect coins to unlock new abilities!",
        "Tip: Each character has unique skills that can be upgraded.",
        "Tip: The Sticky! debuff makes it harder to jump.",
        "Tip: The Burning! debuff does damage over time.",
        "Tip: The Soggy! debuff makes inputs register later than usual.",
        "Tip: The Cursed! debuff causes all sorts of weird effects!",
        "Tip: Bits' Bit Buff lets him tank incoming damage!",
        "Tip: Thief can use Sticky Fingers to pull in nearby coins!",
        "Tip: Ninja can fire her Electric Bolt to destroy obstacles!",
        "Tip: Wiz Kid can give himself energy!",
        "Tip: Crystal can use Glaciate to freeze certain enemies in place!",
        "Tip: Cubit can pause his energy depletion for a short time!",
        "Tip: Cubit is immune to the Cursed! debuff, as he is an antivirus!",
        "Fun Fact: Bits' name is a play on 'bit' as in computer bits!",
        "Fun Fact: Despite being a thief, Thief never actually steals anything!",
        "Fun Fact: Ninja's hometown, Ninjavalley, is home to all women!",
        "Fun Fact: Wiz Kid's real name is Wizzleton McGuffin!",
        "Fun Fact: Crystal is actually a frost spirit, not a living creature!",
        "Fun Fact: Cubit is a sentient cube that grew limbs from another dimension!",
        "Lore Fact: The planet Spritz was once a barren wasteland until the five founders arrived and built the first cities!",
        "Lore Fact: Axiom Byte, the founder of Pixelville and ancestor of Bits, was the first to create society!",
        "Lore Fact: Sylro Vex, the founder of Greenwood and ancestor of Thief, was a master of stealth and trickery!",
        "Lore Fact: Kairi Shin, the founder of Ninja Valley and ancestor of Ninja, was a leader of women who valued strength and unity.",
        "Lore Fact: Glace Frost, the founder of Frosty Outpost and ancestor of Crystal, was an elegant yet composed frost spirit who preserved resources in the harsh climate.",
        "Lore Fact: Orin Lux, the founder of Wizardspeak and father of Wiz Kid, was a brilliant inventor who discovered how to harness magic and technology together!",
    };

    // --- Runtime UI references (built in code) ---
    private Canvas loadingCanvas;
    private Image backgroundImage;
    private Image progressBarBackground;
    private Image progressBarFill;
    private TextMeshProUGUI loadingLabel;
    private TextMeshProUGUI tipLabel;
    private TextMeshProUGUI percentLabel;
    private TextMeshProUGUI counterLabel;
    private CanvasGroup canvasGroup;

    private bool isLoading = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildUI();
        loadingCanvas.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------------
    //  PUBLIC API
    // ------------------------------------------------------------------

    /// <summary>
    /// Shows the loading screen, asynchronously loads <paramref name="sceneName"/>,
    /// and invokes the optional callback just before the scene activates.
    /// </summary>
    public void LoadScene(string sceneName, Action onBeforeActivation = null)
    {
        if (isLoading) return;
        StartCoroutine(LoadRoutine(sceneName, onBeforeActivation));
    }

    public bool IsLoading => isLoading;

    // ------------------------------------------------------------------
    //  CORE ROUTINE
    // ------------------------------------------------------------------

    private IEnumerator LoadRoutine(string sceneName, Action onBeforeActivation)
    {
        isLoading = true;

        // Pick a random tip
        if (tips != null && tips.Length > 0 && tipLabel != null)
        {
            tipLabel.text = tips[UnityEngine.Random.Range(0, tips.Length)];
            tipLabel.gameObject.SetActive(true);
        }
        else if (tipLabel != null)
        {
            tipLabel.gameObject.SetActive(false);
        }

        // Reset progress bar
        if (progressBarFill != null)
            progressBarFill.fillAmount = 0f;
        if (percentLabel != null)
            percentLabel.text = "0%";
        if (counterLabel != null)
            counterLabel.text = "0";

        // Show the loading screen canvas
        loadingCanvas.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        // Fade in the loading screen
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, 0.3f));

        // Begin async load
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = false;

        float displayedProgress = 0f;
        float cosmeticCounter = 0f;
        float elapsed = 0f;

        // Animate progress bar while loading
        while (!asyncOp.isDone)
        {
            elapsed += Time.unscaledDeltaTime;

            // Unity reports 0..0.9 while loading; 0.9 means ready to activate
            float targetProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress,
                progressLerpSpeed * Time.unscaledDeltaTime);

            // Cosmetic counter — ticks up independently at counterSpeed, clamped to 0-100.
            // It races ahead but never exceeds the displayed progress percentage,
            // so it always "agrees" with the bar visually.
            float progressPercent = displayedProgress * 100f;
            cosmeticCounter = Mathf.MoveTowards(cosmeticCounter, progressPercent,
                counterSpeed * Time.unscaledDeltaTime);

            if (progressBarFill != null)
                progressBarFill.fillAmount = displayedProgress;
            if (percentLabel != null)
                percentLabel.text = Mathf.RoundToInt(displayedProgress * 100f) + "%";
            if (counterLabel != null)
                counterLabel.text = Mathf.FloorToInt(cosmeticCounter).ToString();

            // Wait until async is ready AND we've shown the screen long enough
            if (asyncOp.progress >= 0.9f && elapsed >= minimumDisplayTime && displayedProgress >= 0.99f)
            {
                // Snap to 100%
                if (progressBarFill != null)
                    progressBarFill.fillAmount = 1f;
                if (percentLabel != null)
                    percentLabel.text = "100%";
                if (counterLabel != null)
                    counterLabel.text = "100";

                // Callback (e.g. music change)
                onBeforeActivation?.Invoke();

                // Activate the loaded scene
                asyncOp.allowSceneActivation = true;
            }

            yield return null;
        }

        // Wait a frame so the new scene's Awake/Start run
        yield return null;

        // Fade out the loading screen
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, 0.3f));

        loadingCanvas.gameObject.SetActive(false);
        isLoading = false;
    }

    // ------------------------------------------------------------------
    //  FADE HELPER
    // ------------------------------------------------------------------

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    // ------------------------------------------------------------------
    //  UI CONSTRUCTION
    // ------------------------------------------------------------------

    private void BuildUI()
    {
        // --- Canvas ---
        GameObject canvasGO = new GameObject("LoadingScreenCanvas");
        canvasGO.transform.SetParent(transform);

        loadingCanvas = canvasGO.AddComponent<Canvas>();
        loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        loadingCanvas.sortingOrder = 10000; // above the fade overlay

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>(); // block input while loading

        canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // --- Full-screen dark background ---
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);

        backgroundImage = bgGO.AddComponent<Image>();
        backgroundImage.color = new Color(0.05f, 0.05f, 0.08f, 1f); // near-black with slight blue
        backgroundImage.raycastTarget = true;

        RectTransform bgRT = backgroundImage.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // --- "Loading..." label ---
        GameObject labelGO = new GameObject("LoadingLabel");
        labelGO.transform.SetParent(canvasGO.transform, false);

        loadingLabel = labelGO.AddComponent<TextMeshProUGUI>();
        loadingLabel.text = "Loading...";
        loadingLabel.fontSize = 52;
        loadingLabel.alignment = TextAlignmentOptions.Center;
        loadingLabel.color = Color.white;

        RectTransform labelRT = loadingLabel.rectTransform;
        labelRT.anchorMin = new Vector2(0.5f, 0.5f);
        labelRT.anchorMax = new Vector2(0.5f, 0.5f);
        labelRT.anchoredPosition = new Vector2(0f, 60f);
        labelRT.sizeDelta = new Vector2(600f, 80f);

        // --- Progress bar background ---
        GameObject barBgGO = new GameObject("ProgressBarBG");
        barBgGO.transform.SetParent(canvasGO.transform, false);

        progressBarBackground = barBgGO.AddComponent<Image>();
        progressBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        RectTransform barBgRT = progressBarBackground.rectTransform;
        barBgRT.anchorMin = new Vector2(0.5f, 0.5f);
        barBgRT.anchorMax = new Vector2(0.5f, 0.5f);
        barBgRT.anchoredPosition = new Vector2(0f, -10f);
        barBgRT.sizeDelta = new Vector2(600f, 30f);

        // --- Progress bar fill ---
        GameObject barFillGO = new GameObject("ProgressBarFill");
        barFillGO.transform.SetParent(barBgGO.transform, false);

        progressBarFill = barFillGO.AddComponent<Image>();
        progressBarFill.color = new Color(0.3f, 0.8f, 0.4f, 1f); // green fill
        progressBarFill.type = Image.Type.Filled;
        progressBarFill.fillMethod = Image.FillMethod.Horizontal;
        progressBarFill.fillOrigin = 0; // left to right
        progressBarFill.fillAmount = 0f;

        RectTransform barFillRT = progressBarFill.rectTransform;
        barFillRT.anchorMin = Vector2.zero;
        barFillRT.anchorMax = Vector2.one;
        barFillRT.offsetMin = Vector2.zero;
        barFillRT.offsetMax = Vector2.zero;

        // --- Percentage label (below the bar) ---
        GameObject pctGO = new GameObject("PercentLabel");
        pctGO.transform.SetParent(canvasGO.transform, false);

        percentLabel = pctGO.AddComponent<TextMeshProUGUI>();
        percentLabel.text = "0%";
        percentLabel.fontSize = 28;
        percentLabel.alignment = TextAlignmentOptions.Center;
        percentLabel.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        RectTransform pctRT = percentLabel.rectTransform;
        pctRT.anchorMin = new Vector2(0.5f, 0.5f);
        pctRT.anchorMax = new Vector2(0.5f, 0.5f);
        pctRT.anchoredPosition = new Vector2(0f, -50f);
        pctRT.sizeDelta = new Vector2(200f, 40f);

        // --- Numeric counter label (right side of the bar) ---
        GameObject counterGO = new GameObject("CounterLabel");
        counterGO.transform.SetParent(canvasGO.transform, false);

        counterLabel = counterGO.AddComponent<TextMeshProUGUI>();
        counterLabel.text = "0";
        counterLabel.fontSize = 36;
        counterLabel.alignment = TextAlignmentOptions.Right;
        counterLabel.color = new Color(0.3f, 0.8f, 0.4f, 1f); // match the bar fill color

        RectTransform counterRT = counterLabel.rectTransform;
        counterRT.anchorMin = new Vector2(0.5f, 0.5f);
        counterRT.anchorMax = new Vector2(0.5f, 0.5f);
        counterRT.anchoredPosition = new Vector2(330f, -10f); // just right of the 600px-wide bar
        counterRT.sizeDelta = new Vector2(100f, 40f);

        // --- Tip label ---
        GameObject tipGO = new GameObject("TipLabel");
        tipGO.transform.SetParent(canvasGO.transform, false);

        tipLabel = tipGO.AddComponent<TextMeshProUGUI>();
        tipLabel.text = "";
        tipLabel.fontSize = 26;
        tipLabel.fontStyle = FontStyles.Italic;
        tipLabel.alignment = TextAlignmentOptions.Center;
        tipLabel.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        tipLabel.enableWordWrapping = true;

        RectTransform tipRT = tipLabel.rectTransform;
        tipRT.anchorMin = new Vector2(0.5f, 0f);
        tipRT.anchorMax = new Vector2(0.5f, 0f);
        tipRT.anchoredPosition = new Vector2(0f, 80f);
        tipRT.sizeDelta = new Vector2(800f, 60f);
    }
}