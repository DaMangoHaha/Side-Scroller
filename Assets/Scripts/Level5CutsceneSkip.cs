using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Level5CutsceneSkip : MonoBehaviour
{
    [Header("Cutscene Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public Sprite portrait;
    public bool autoStart = false;

    [Header("Slideshow Images")]
    [Tooltip("One sprite per dialogue line. If an entry is left empty, the previous image stays. Array length should match dialogueLines.")]
    public Sprite[] slideImages;

    [Header("Input (New Input System)")]
    public InputActionReference advanceActionRef; // left click / gamepad confirm
    public InputActionReference skipActionRef;    // right click / gamepad skip
    private InputAction advanceAction;
    private InputAction skipAction;
    private bool createdLocalAdvance = false;
    private bool createdLocalSkip = false;

    [Header("Mobile Touch Settings")]
    [Tooltip("How long (in seconds) the player must hold before the cutscene is skipped.")]
    public float holdToSkipDuration = 1.0f;

    private float touchStartTime = -1f;
    private bool touchHeldSkipped = false;

    private Level5CutsceneOpening level5CutsceneOpening;
    private int currentLineIndex = 0;
    private bool conversationActive = false;

    void Start()
    {
        level5CutsceneOpening = Object.FindFirstObjectByType<Level5CutsceneOpening>();
        if (level5CutsceneOpening == null)
            level5CutsceneOpening = FindFirstObjectByType<Level5CutsceneOpening>();

        if (autoStart)
            StartConversation();
    }

    void OnEnable()
    {
        // Advance action (Left Click)
        if (advanceActionRef != null && advanceActionRef.action != null)
            advanceAction = advanceActionRef.action;
        else
        {
            advanceAction = new InputAction("AdvanceDialogue", InputActionType.Button);
            advanceAction.AddBinding("<Mouse>/leftButton");
            advanceAction.AddBinding("<Gamepad>/buttonSouth");
            createdLocalAdvance = true;
        }

        if (advanceAction != null)
        {
            advanceAction.performed += OnAdvancePerformed;
            advanceAction.Enable();
        }

        // Skip action (Right Click)
        if (skipActionRef != null && skipActionRef.action != null)
            skipAction = skipActionRef.action;
        else
        {
            skipAction = new InputAction("SkipCutscene", InputActionType.Button);
            skipAction.AddBinding("<Mouse>/rightButton");
            skipAction.AddBinding("<Gamepad>/buttonNorth");
            createdLocalSkip = true;
        }

        if (skipAction != null)
        {
            skipAction.performed += OnSkipPerformed;
            skipAction.Enable();
        }
    }

    void OnDisable()
    {
        if (advanceAction != null)
        {
            advanceAction.performed -= OnAdvancePerformed;
            advanceAction.Disable();
        }
        if (skipAction != null)
        {
            skipAction.performed -= OnSkipPerformed;
            skipAction.Disable();
        }

        if (createdLocalAdvance && advanceAction != null)
        {
            advanceAction.Dispose();
            advanceAction = null;
            createdLocalAdvance = false;
        }

        if (createdLocalSkip && skipAction != null)
        {
            skipAction.Dispose();
            skipAction = null;
            createdLocalSkip = false;
        }
    }

    public void StartConversation()
    {
        if (level5CutsceneOpening == null || dialogueLines == null || dialogueLines.Length == 0)
            return;

        // Disable Level5CutsceneOpening's own advance input so this controller has exclusive control
        level5CutsceneOpening.SetAdvanceInputEnabled(false);

        level5CutsceneOpening.SetContinuePromptVisible(true);
        currentLineIndex = 0;
        conversationActive = true;
        level5CutsceneOpening.ShowDialogue(dialogueLines[currentLineIndex], portrait);

        // Show the first slide image (if provided)
        ShowSlideForCurrentLine();
    }

    void Update()
    {
        if (!conversationActive || level5CutsceneOpening == null)
            return;

        // --- Mobile touch input ---
        HandleMobileTouch();

        // Legacy fallback if new Input System actions are not assigned/enabled
        // Use Mouse.current for left/right click fallback
        if ((skipAction == null || !skipAction.enabled) && Mouse.current != null)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                SkipCutscene();
                return;
            }
        }
        if ((advanceAction == null || !advanceAction.enabled) && Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleAdvance();
            }
        }
    }

    private void HandleMobileTouch()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null) return;

        var primaryTouch = touchscreen.primaryTouch;

        // Touch began — record the start time
        if (primaryTouch.press.wasPressedThisFrame)
        {
            touchStartTime = Time.unscaledTime;
            touchHeldSkipped = false;
        }

        // Touch is being held — check if the hold duration has been reached
        if (primaryTouch.press.isPressed && !touchHeldSkipped && touchStartTime > 0f)
        {
            if (Time.unscaledTime - touchStartTime >= holdToSkipDuration)
            {
                touchHeldSkipped = true;
                SkipCutscene();
            }
        }

        // Touch released — if it was a short tap (not a hold-skip), advance the dialogue
        if (primaryTouch.press.wasReleasedThisFrame)
        {
            if (!touchHeldSkipped && touchStartTime > 0f)
            {
                HandleAdvance();
            }

            // Reset touch state
            touchStartTime = -1f;
            touchHeldSkipped = false;
        }
    }

    private void OnAdvancePerformed(InputAction.CallbackContext ctx)
    {
        HandleAdvance();
    }

    private void OnSkipPerformed(InputAction.CallbackContext ctx)
    {
        SkipCutscene();
    }

    private void HandleAdvance()
    {
        if (!conversationActive || level5CutsceneOpening == null)
            return;

        if (!level5CutsceneOpening.IsTyping)
        {
            currentLineIndex++;

            if (currentLineIndex < dialogueLines.Length)
            {
                level5CutsceneOpening.ShowDialogue(dialogueLines[currentLineIndex], portrait);
                ShowSlideForCurrentLine();
            }
            else
            {
                EndConversation();
            }
        }
        else
        {
            level5CutsceneOpening.FinishTyping();
        }
    }

    void EndConversation()
    {
        conversationActive = false;
        if (level5CutsceneOpening != null)
        {
            level5CutsceneOpening.HideDialogue();
            level5CutsceneOpening.HideSlideImage();
            level5CutsceneOpening.SetContinuePromptVisible(false);
            // re-enable Level5CutsceneOpening input now that this controller is done
            level5CutsceneOpening.SetAdvanceInputEnabled(true);
        }

        // Load Level5 immediately when conversation ends
        SceneManager.LoadScene("Level5");
    }

    void SkipCutscene()
    {
        Debug.Log("Level 5 Cutscene skipped.");
        conversationActive = false;

        if (level5CutsceneOpening != null)
        {
            level5CutsceneOpening.FinishTyping();
            level5CutsceneOpening.HideDialogue();
            level5CutsceneOpening.HideSlideImage();
            level5CutsceneOpening.SetContinuePromptVisible(false);
            // re-enable Level5CutsceneOpening input
            level5CutsceneOpening.SetAdvanceInputEnabled(true);
        }

        // Load Level5 immediately when skipping
        SceneManager.LoadScene("Level5");
    }

    /// <summary>
    /// Shows the slide image that corresponds to the current dialogue line.
    /// If the slideImages array is shorter than dialogueLines, the last valid
    /// slide stays visible. If no slides are assigned at all, nothing happens.
    /// </summary>
    private void ShowSlideForCurrentLine()
    {
        if (level5CutsceneOpening == null || slideImages == null || slideImages.Length == 0)
            return;

        // Clamp index so the last image persists for any extra dialogue lines
        int index = Mathf.Min(currentLineIndex, slideImages.Length - 1);

        // Only update if there is a non-null sprite at this index;
        // otherwise keep the previous slide visible
        if (slideImages[index] != null)
            level5CutsceneOpening.ShowSlideImage(slideImages[index]);
    }
}
