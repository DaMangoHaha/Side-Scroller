using UnityEngine;
using UnityEngine.InputSystem;

public class SpecialNPC : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public Sprite portrait;

    private DialogueUI dialogueUI;
    private int currentLineIndex = 0;
    private bool conversationActive = false;

    // Active SpecialNPC to allow external advancement (e.g., virtual interact button)
    public static SpecialNPC Active;

    [Header("Post-Dialogue Event")]
    public bool triggersEvent = true;
    public string eventMessage = "QuestUnlocked";

    [Header("Input (New Input System)")]
    public InputActionReference advanceActionRef; // optional: assign "AdvanceDialogue" action
    private InputAction advanceAction;
    private bool createdLocalAction = false;

    void Start()
    {
        dialogueUI = Object.FindFirstObjectByType<DialogueUI>();
    }

    void OnEnable()
    {
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

    public void StartSpecialConversation()
    {
        if (dialogueUI == null || dialogueLines.Length == 0)
            return;

        dialogueUI.SetContinuePromptVisible(true);   // enable "Press Space"
        currentLineIndex = 0;
        conversationActive = true;
        Active = this; // mark this instance as the active conversation
        dialogueUI.ShowDialogue(dialogueLines[currentLineIndex], portrait);
    }

    public void OnAdvancePerformed(InputAction.CallbackContext ctx)
    {
        AdvanceFromExternalPress();
    }

    // Allow external input (e.g., player UI button) to advance or finish typing
    public void AdvanceFromExternalPress()
    {
        if (!conversationActive || dialogueUI == null)
            return;

        if (!dialogueUI.IsTyping)
        {
            AdvanceOrEnd();
        }
        else
        {
            // Instantly finish current line if still typing
            dialogueUI.FinishTyping();
        }
    }

    // Returns true if an active conversation handled advancement
    public static bool AdvanceIfActive()
    {
        if (Active != null && Active.conversationActive)
        {
            Active.AdvanceFromExternalPress();
            return true;
        }
        return false;
    }

    void Update()
    {
        // Legacy fallback if new Input System action is not assigned/enabled
        if ((advanceAction == null || !advanceAction.enabled) && Keyboard.current != null)
        {
            if (conversationActive && Keyboard.current.spaceKey.wasPressedThisFrame && dialogueUI != null)
            {
                if (!dialogueUI.IsTyping)
                    AdvanceOrEnd();
                else
                    dialogueUI.FinishTyping();
            }
        }
    }

    private void AdvanceOrEnd()
    {
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            // Show the next line
            dialogueUI.ShowDialogue(dialogueLines[currentLineIndex], portrait);
        }
        else
        {
            // End of conversation
            EndConversation();
        }
    }

    void EndConversation()
    {
        conversationActive = false;
        if (Active == this)
        {
            Active = null; // clear active when this conversation ends
        }
        if (dialogueUI != null)
        {
            dialogueUI.HideDialogue();
            dialogueUI.SetContinuePromptVisible(false);   // disable again
        }

        if (triggersEvent)
        {
            GameEvents.slimeExterminationActive = true;
            Debug.Log($"Special event triggered: {eventMessage}");
            // Quest/cutscene logic here
        }
    }
}
