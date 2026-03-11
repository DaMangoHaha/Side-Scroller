using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class OpeningCutscene : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public CanvasGroup dialogueCanvasGroup;
    public Image portraitImage;

    [Header("Slideshow Image")]
    [Tooltip("Full-screen or panel Image used to display slideshow frames during the cutscene.")]
    public Image slideImage;

    [Header("Typing Effect")]
    public float typingSpeed = 0.03f;

    [Header("Continue Prompt")]
    public TextMeshProUGUI continueText;
    public float continueFadeSpeed = 2f;
    private bool showContinuePrompt = false;

    [Header("Continue Prompt Messages")]
    public string pcPrompt = "Left click to continue. (Right click to skip.)";
    public string gamepadPrompt = "Press A to continue. (Press Y to skip.)";
    public string mobilePrompt = "Tap to continue. (Press and hold to skip.)";

    [Header("Start Settings")]
    public bool showOnStart = true;
    [TextArea] public string startingText;
    public Sprite startingPortrait;

    [Header("Input (New Input System)")]
    public InputActionReference advanceActionRef; // optional: assign an action in the Inspector
    private InputAction advanceAction;
    private bool createdLocalAction = false;

    private bool dialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string fullText;

    // Prevent repeatedly attempting to load the main menu every frame
    private bool mainMenuLoaded = false;

    public bool IsDialogueActive => dialogueActive;
    public bool IsTyping => isTyping;
    public void SetContinuePromptVisible(bool visible)
    {
        showContinuePrompt = visible;
        if (!visible && continueText != null)
            continueText.alpha = 0; // hide when not used
    }

    void OnEnable()
    {
        // Prefer assigned InputActionReference, otherwise create a small fallback action
        if (advanceActionRef != null && advanceActionRef.action != null)
        {
            advanceAction = advanceActionRef.action;
        }
        else
        {
            advanceAction = new InputAction("AdvanceDialogue", InputActionType.Button);
            advanceAction.AddBinding("<Keyboard>/space");
            advanceAction.AddBinding("<Gamepad>/buttonSouth");
            createdLocalAction = true;
        }

        if (advanceAction != null)
        {
            advanceAction.performed += OnAdvancePerformed;
            advanceAction.Enable();
        }
    }

    void OnDisable()
    {
        if (advanceAction != null)
        {
            advanceAction.performed -= OnAdvancePerformed;
            advanceAction.Disable();
        }

        if (createdLocalAction && advanceAction != null)
        {
            advanceAction.Dispose();
            advanceAction = null;
            createdLocalAction = false;
        }
    }

    // Public helper to temporarily enable/disable the internal advance input.
    // Other systems (like OpeningCutsceneTextSkip) can call this to avoid duplicate handling.
    public void SetAdvanceInputEnabled(bool enabled)
    {
        if (advanceAction == null) return;

        if (enabled && !advanceAction.enabled)
            advanceAction.Enable();
        else if (!enabled && advanceAction.enabled)
            advanceAction.Disable();
    }

    void Start()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (portraitImage != null)
            portraitImage.enabled = false;

        if (slideImage != null)
            slideImage.enabled = false;

        if (dialogueCanvasGroup != null)
            dialogueCanvasGroup.alpha = 0;

        if (continueText != null)
            continueText.alpha = 255; // keep original value as in file

        // Show dialogue on start if requested. Use startingText if provided,
        // otherwise use the existing dialogueText.text (if any).
        if (showOnStart)
        {
            string textToShow = !string.IsNullOrEmpty(startingText)
                ? startingText
                : (dialogueText != null && !string.IsNullOrEmpty(dialogueText.text) ? dialogueText.text : null);

            if (!string.IsNullOrEmpty(textToShow))
                ShowDialogue(textToShow, startingPortrait);
        }
    }

    private void Update()
    {
        // If the dialogue box object exists and is not active in the scene,
        // load the MainMenu scene once (guarded by mainMenuLoaded).
        if (!mainMenuLoaded && dialogueBox != null && !dialogueBox.activeInHierarchy)
        {
            mainMenuLoaded = true;
            SceneTransition.Instance.LoadScene("MainMenu");
        }

        // Update continue prompt text based on active input device
        if (showContinuePrompt && continueText != null && continueText.alpha > 0)
        {
            continueText.text = GetPlatformPrompt();
        }

        // Legacy fallback when InputAction is not assigned or enabled
        if ((advanceAction == null || !advanceAction.enabled) && Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                HandleAdvanceInput();
        }
    }

    /// <summary>
    /// Returns the appropriate continue prompt string based on the
    /// currently active input device / platform.
    /// </summary>
    private string GetPlatformPrompt()
    {
        // Mobile check first — if a touchscreen is the most recent device, show mobile prompt
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return mobilePrompt;

        // On mobile platforms, default to the mobile prompt even without active touch
#if UNITY_IOS || UNITY_ANDROID
        return mobilePrompt;
#else
        // Gamepad check — if a gamepad is connected and was used most recently
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
            return gamepadPrompt;

        // Default to PC prompt
        return pcPrompt;
#endif
    }

    private void OnAdvancePerformed(InputAction.CallbackContext ctx)
    {
        HandleAdvanceInput();
    }

    private void HandleAdvanceInput()
    {
        if (!dialogueActive)
            return;

        if (isTyping)
            FinishTyping();
        else
        {
            // close the dialogue box (same behavior as previous skip)
            HideDialogue();
            SetContinuePromptVisible(false);
        }
    }

    public void ShowDialogue(string text, Sprite portrait = null)
    {
        dialogueActive = true;
        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        if (portraitImage != null)
        {
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.enabled = true;
            }
            else
            {
                portraitImage.enabled = false;
            }
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));

        if (dialogueCanvasGroup != null)
            StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 0f, 1f, 0.3f));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        fullText = text;
        if (dialogueText != null)
            dialogueText.text = "";

        // Hide "Press ..." while typing
        if (continueText != null)
            continueText.alpha = 0;

        if (dialogueText != null)
        {
            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        else
        {
            // fallback: wait the approximate time so timing remains consistent
            yield return new WaitForSeconds(text.Length * typingSpeed);
        }

        isTyping = false;

        // Only fade in if this dialogue is allowed to show the prompt
        if (showContinuePrompt && continueText != null)
        {
            // Set the correct platform text before fading in
            continueText.text = GetPlatformPrompt();
            StartCoroutine(FadeInContinueText());
        }
    }

    IEnumerator FadeInContinueText()
    {
        while (continueText != null && continueText.alpha < 1)
        {
            continueText.alpha += Time.deltaTime * continueFadeSpeed;
            yield return null;
        }
    }

    public void FinishTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        if (dialogueText != null)
            dialogueText.text = fullText;

        // Only show immediately if allowed
        if (showContinuePrompt && continueText != null)
        {
            continueText.text = GetPlatformPrompt();
            continueText.alpha = 1;
        }
    }

    public void HideDialogue()
    {
        dialogueActive = false;

        if (dialogueCanvasGroup != null)
            StartCoroutine(FadeOutAndDisable());
        else if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (portraitImage != null)
            portraitImage.enabled = false;

        // Also hide slideshow image when dialogue is hidden
        HideSlideImage();

        if (continueText != null)
            continueText.alpha = 0; // Hide prompt
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f, 0.3f);
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null)
            yield break;

        float elapsed = 0f;
        cg.alpha = start;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        cg.alpha = end;
    }

    /// <summary>
    /// Displays a sprite in the slideshow Image panel. Pass null to hide.
    /// </summary>
    public void ShowSlideImage(Sprite slide)
    {
        if (slideImage == null) return;

        if (slide != null)
        {
            slideImage.sprite = slide;
            slideImage.enabled = true;
        }
        else
        {
            slideImage.enabled = false;
        }
    }

    /// <summary>
    /// Hides the slideshow Image panel.
    /// </summary>
    public void HideSlideImage()
    {
        if (slideImage != null)
            slideImage.enabled = false;
    }
}
