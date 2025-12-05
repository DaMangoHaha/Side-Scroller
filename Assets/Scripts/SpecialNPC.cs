using UnityEngine;

public class SpecialNPC : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public Sprite portrait;

    private DialogueUI dialogueUI;
    private int currentLineIndex = 0;
    private bool conversationActive = false;

    [Header("Post-Dialogue Event")]
    public bool triggersEvent = true;
    public string eventMessage = "QuestUnlocked";

    void Start()
    {
        dialogueUI = Object.FindFirstObjectByType<DialogueUI>();
    }

    public void StartSpecialConversation()
    {
        if (dialogueUI == null || dialogueLines.Length == 0)
            return;

        dialogueUI.SetContinuePromptVisible(true);   // enable "Press Space"
        currentLineIndex = 0;
        conversationActive = true;
        dialogueUI.ShowDialogue(dialogueLines[currentLineIndex], portrait);
    }


    void Update()
    {
        // Only allow advancing when the conversation is active
        if (conversationActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (!dialogueUI.IsTyping)
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
            else
            {
                // Instantly finish current line if still typing
                dialogueUI.FinishTyping();
            }
        }
    }

    void EndConversation()
    {
        conversationActive = false;
        dialogueUI.HideDialogue();
        dialogueUI.SetContinuePromptVisible(false);   // disable again

        if (triggersEvent)
        {
            GameEvents.slimeExterminationActive = true;
            Debug.Log($"Special event triggered: {eventMessage}");
            // Quest/cutscene logic here
        }
    }
}
