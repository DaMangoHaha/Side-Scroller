using UnityEngine;

public class NPC : MonoBehaviour
{
    [TextArea(2, 4)]
    public string dialogueText;
    public Sprite portrait; // optional

    public void StartConversation()
    {
        DialogueUI ui = Object.FindFirstObjectByType<DialogueUI>();
        if (ui != null)
        {
            ui.ShowDialogue(dialogueText, portrait);
        }
    }
}

