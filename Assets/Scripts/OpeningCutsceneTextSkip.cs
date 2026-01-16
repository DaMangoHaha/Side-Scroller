using UnityEngine;

public class OpeningCutsceneTextSkip : MonoBehaviour
{
    [Header("Cutscene Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public Sprite portrait;
    public bool autoStart = false;

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

    public void StartConversation()
    {
        if (openingCutscene == null || dialogueLines == null || dialogueLines.Length == 0)
            return;

        openingCutscene.SetContinuePromptVisible(true);
        currentLineIndex = 0;
        conversationActive = true;
        openingCutscene.ShowDialogue(dialogueLines[currentLineIndex], portrait);
    }

    void Update()
    {
        if (!conversationActive || openingCutscene == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
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
    }

    void EndConversation()
    {
        conversationActive = false;
        if (openingCutscene != null)
        {
            openingCutscene.HideDialogue();
            openingCutscene.SetContinuePromptVisible(false);
        }
    }
}
