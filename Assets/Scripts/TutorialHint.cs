using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Displays a platform-specific gameplay hint in the tutorial level.
/// Fades in, stays visible for a set duration, then fades out and destroys itself.
/// 
/// Usage:
///   1. Create a UI Canvas in your tutorial scene (Screen Space – Overlay or Camera).
///   2. Add a TextMeshPro – Text (UI) child and attach this script to it.
///   3. Fill in the hint messages for each platform in the Inspector.
///   4. Optionally assign a CanvasGroup on the same GameObject for smooth fade (one is added automatically if missing).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class TutorialHint : MonoBehaviour
{
    [Header("Hint Messages")]
    [Tooltip("Message shown on PC (keyboard / mouse).")]
    [TextArea] public string pcHint = "Press Space to jump. Press Control to slide.";

    [Tooltip("Message shown when a gamepad is detected.")]
    [TextArea] public string gamepadHint = "Press A to jump. Press X to slide.";

    [Tooltip("Message shown on mobile / touchscreen devices.")]
    [TextArea] public string mobileHint = "Tap the screen to jump. Press the slide button to slide.";

    [Header("Timing")]
    [Tooltip("Seconds to wait before the hint appears.")]
    public float delayBeforeShow = 0.5f;

    [Tooltip("How long (seconds) the fade-in takes.")]
    public float fadeInDuration = 0.5f;

    [Tooltip("How long (seconds) the hint stays fully visible.")]
    public float displayDuration = 15f;

    [Tooltip("How long (seconds) the fade-out takes.")]
    public float fadeOutDuration = 1f;

    [Tooltip("If true, the entire GameObject is destroyed after the hint finishes. " +
             "If false, it is simply deactivated.")]
    public bool destroyAfterFade = true;

    private CanvasGroup canvasGroup;
    private TextMeshProUGUI hintText;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        hintText = GetComponent<TextMeshProUGUI>();

        // Start fully transparent
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        // Set the text to the correct platform hint
        if (hintText != null)
            hintText.text = GetPlatformHint();

        StartCoroutine(HintLifecycle());
    }

    /// <summary>
    /// Returns the appropriate hint string based on the active input device / platform.
    /// </summary>
    private string GetPlatformHint()
    {
        // Active touchscreen means mobile
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return mobileHint;

#if UNITY_IOS || UNITY_ANDROID
        return mobileHint;
#else
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            return gamepadHint;

        return pcHint;
#endif
    }

    private IEnumerator HintLifecycle()
    {
        // Optional delay before showing
        if (delayBeforeShow > 0f)
            yield return new WaitForSeconds(delayBeforeShow);

        // Fade in
        yield return Fade(0f, 1f, fadeInDuration);

        // Stay visible
        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;

            // Continuously update text in case the player switches input device
            if (hintText != null)
                hintText.text = GetPlatformHint();

            yield return null;
        }

        // Fade out
        yield return Fade(1f, 0f, fadeOutDuration);

        // Clean up
        if (destroyAfterFade)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
