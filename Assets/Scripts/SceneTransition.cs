using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Singleton that provides a smooth black fade-in/fade-out when transitioning
/// between scenes or toggling UI canvases. Persists across scene loads.
///
/// Usage:
///   SceneTransition.Instance.LoadScene("Level1");
///   SceneTransition.Instance.LoadScene("Level1", onBeforeLoad: () => MusicManager.Instance.PlayMusic("Level 1"));
///   SceneTransition.Instance.LoadSceneWithLoadingScreen("Level1", () => MusicManager.Instance.PlayMusic("Level 1"));
///   SceneTransition.Instance.FadeOut(() => { /* show new canvas */ }, () => SceneTransition.Instance.FadeIn());
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Fade Settings")]
    [Tooltip("Duration (seconds) for the fade-to-black and fade-from-black animations.")]
    public float fadeDuration = 0.4f;

    [Header("Loading Screen")]
    [Tooltip("When true, every LoadScene call automatically uses the loading screen instead of a simple fade.")]
    public bool useLoadingScreenByDefault = true;

    // Runtime-created overlay
    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateFadeOverlay();
        EnsureLoadingScreen();
    }

    /// <summary>
    /// Builds a full-screen black Image on a dedicated high-sort-order Canvas
    /// so the fade overlay renders on top of everything.
    /// </summary>
    private void CreateFadeOverlay()
    {
        // Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // always on top

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // We do NOT add a GraphicRaycaster so the overlay never blocks clicks
        // during the brief fade. If you want it to block input while fading,
        // uncomment the line below:
        // canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Full-screen black image
        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);

        fadeImage = imgGO.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f); // start fully transparent
        fadeImage.raycastTarget = false;

        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Creates the LoadingScreen singleton if one doesn't already exist.
    /// </summary>
    private void EnsureLoadingScreen()
    {
        if (LoadingScreen.Instance == null)
        {
            GameObject lsGO = new GameObject("LoadingScreen");
            lsGO.transform.SetParent(transform.parent); // keep at root alongside SceneTransition
            lsGO.AddComponent<LoadingScreen>();
        }
    }

    // ------------------------------------------------------------------
    //  PUBLIC API — Scene Loading
    // ------------------------------------------------------------------

    /// <summary>
    /// Loads a scene with either a loading screen or a simple fade, depending on
    /// <see cref="useLoadingScreenByDefault"/>.
    /// </summary>
    public void LoadScene(string sceneName, Action onBeforeLoad = null)
    {
        if (isTransitioning) return;

        if (useLoadingScreenByDefault && LoadingScreen.Instance != null)
        {
            LoadSceneWithLoadingScreen(sceneName, onBeforeLoad);
        }
        else
        {
            StartCoroutine(TransitionCoroutine(sceneName, onBeforeLoad));
        }
    }

    /// <summary>
    /// Explicitly loads a scene through the loading screen (ignores the default toggle).
    /// Fades to black first, shows the loading screen, loads async, then fades out.
    /// </summary>
    public void LoadSceneWithLoadingScreen(string sceneName, Action onBeforeLoad = null)
    {
        if (isTransitioning) return;
        StartCoroutine(LoadingScreenTransition(sceneName, onBeforeLoad));
    }

    /// <summary>
    /// Explicitly loads a scene with only the simple fade (ignores the default toggle).
    /// </summary>
    public void LoadSceneWithFade(string sceneName, Action onBeforeLoad = null)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionCoroutine(sceneName, onBeforeLoad));
    }

    private IEnumerator LoadingScreenTransition(string sceneName, Action onBeforeLoad)
    {
        isTransitioning = true;

        // Fade to black first
        yield return StartCoroutine(Fade(0f, 1f));

        // Hand off to the loading screen
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene(sceneName, onBeforeLoad);

            // Wait until the loading screen finishes
            while (LoadingScreen.Instance.IsLoading)
                yield return null;
        }
        else
        {
            // Fallback if LoadingScreen somehow missing
            onBeforeLoad?.Invoke();
            SceneManager.LoadScene(sceneName);
            yield return null;
        }

        // Fade from black (the loading screen fades itself out, so the
        // fade overlay just needs to go transparent)
        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
    }

    private IEnumerator TransitionCoroutine(string sceneName, Action onBeforeLoad)
    {
        isTransitioning = true;

        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f));

        // Callback (music, cleanup, etc.)
        onBeforeLoad?.Invoke();

        // Load the scene
        SceneManager.LoadScene(sceneName);

        // Wait one frame so the new scene's Start/Awake have fired
        yield return null;

        // Fade from black
        yield return StartCoroutine(Fade(1f, 0f));

        isTransitioning = false;
    }

    // ------------------------------------------------------------------
    //  PUBLIC API — Manual Fade (for canvas-to-canvas within a scene)
    // ------------------------------------------------------------------

    /// <summary>
    /// Fades the screen to black, invokes <paramref name="onFadedOut"/>,
    /// then optionally lets you call FadeIn yourself.
    /// </summary>
    public void FadeOut(Action onFadedOut = null, Action onComplete = null)
    {
        if (isTransitioning) return;
        StartCoroutine(ManualFadeOut(onFadedOut, onComplete));
    }

    /// <summary>
    /// Fades the screen back in from black.
    /// </summary>
    public void FadeIn(Action onComplete = null)
    {
        StartCoroutine(ManualFadeIn(onComplete));
    }

    private IEnumerator ManualFadeOut(Action onFadedOut, Action onComplete)
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(0f, 1f));
        onFadedOut?.Invoke();
        onComplete?.Invoke();
        isTransitioning = false;
    }

    private IEnumerator ManualFadeIn(Action onComplete)
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(1f, 0f));
        onComplete?.Invoke();
        isTransitioning = false;
    }

    // ------------------------------------------------------------------
    //  INTERNAL
    // ------------------------------------------------------------------

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        SetAlpha(startAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled so it works even when timeScale == 0
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, t));
            yield return null;
        }

        SetAlpha(endAlpha);
    }

    private void SetAlpha(float a)
    {
        if (fadeImage != null)
            fadeImage.color = new Color(0f, 0f, 0f, a);
    }

    /// <summary>Returns true while a fade/transition coroutine is running.</summary>
    public bool IsTransitioning => isTransitioning;
}
