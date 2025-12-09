using UnityEngine;

public class CharacterSelectButton : MonoBehaviour
{
    public string characterName;

    public void SelectCharacter()
    {
        CharacterManager.Instance.SetCharacter(characterName);
        Debug.Log("Selected Character: " + characterName);
    }
}

