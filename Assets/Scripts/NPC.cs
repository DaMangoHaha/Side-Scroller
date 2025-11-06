using UnityEngine;

public class NPC : MonoBehaviour
{
    public string npcName = "Unnamed NPC";
    [TextArea] public string dialogue = "Hello there!";

    public void Interact()
    {
        Debug.Log($"{npcName}: {dialogue}");
        // You can later open a dialogue box UI here instead of Debug.Log
    }
}

