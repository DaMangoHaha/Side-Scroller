using UnityEngine;
using UnityEngine.InputSystem;

public class OpeningCutsceneTextSkip : MonoBehaviour
{
    [Header("Cutscene Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public Sprite portrait;
    public bool autoStart = false;

    [Header("Input (New Input System)")]
    public InputActionReference advanceActionRef; // left click / gamepad confirm
    public InputActionReference skipActionRef;    // right click / gamepad skip
    private InputAction advanceAction;
    private InputAction skipAction;
    private bool createdLocalAdvance = false;
    private bool createdLocalSkip = false;

    private OpeningCutscene openingCutscene;
    private int currentLineIndex = 0;
    private bool conversationActive = false;

    void Start()
    {
        openingCutscene = Object.FindFirstObjectByType<OpeningCutscene>();
        if (openingCutscene == null)
            openingCutscene = FindFirstObjectByType<OpeningCutscene>();

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
        if (openingCutscene == null || dialogueLines == null || dialogueLines.Length == 0)
            return;

        // Disable OpeningCutscene's own advance input so this controller has exclusive control
        openingCutscene.SetAdvanceInputEnabled(false);

        openingCutscene.SetContinuePromptVisible(true);
        currentLineIndex = 0;
        conversationActive = true;
        openingCutscene.ShowDialogue(dialogueLines[currentLineIndex], portrait);
    }

    void Update()
    {
        if (!conversationActive || openingCutscene == null)
            return;

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
        if (!conversationActive || openingCutscene == null)
            return;

        if (!openingCutscene.IsTyping)
        {
            currentLineIndex++;

            if (currentLineIndex < dialogueLines.Length)
            {
                openingCutscene.ShowDialogue(dialogueLines[currentLineIndex], portrait);
            }
            else
            {
                EndConversation();
            }
        }
        else
        {
            openingCutscene.FinishTyping();
        }
    }

    void EndConversation()
    {
        conversationActive = false;
        if (openingCutscene != null)
        {
            openingCutscene.HideDialogue();
            openingCutscene.SetContinuePromptVisible(false);
            // re-enable OpeningCutscene input now that this controller is done
            openingCutscene.SetAdvanceInputEnabled(true);
        }
    }

    void SkipCutscene()
    {
        Debug.Log("Opening Cutscene skipped.");
        conversationActive = false;

        if (openingCutscene != null)
        {
            openingCutscene.FinishTyping();
            openingCutscene.HideDialogue();
            openingCutscene.SetContinuePromptVisible(false);
            // re-enable OpeningCutscene input
            openingCutscene.SetAdvanceInputEnabled(true);
        }
    }
}
