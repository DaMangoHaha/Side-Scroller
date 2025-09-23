using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Pixels/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterIcon;
    public string skillName;
    [TextArea] public string skillDescription;
    public bool isUnlocked; // default true for Bit, false for Thief
}

