using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string characterName;
    public Sprite characterIcon;
    public int cost;
    public string skillName;
    [TextArea] public string skillDescription;
    public bool isUnlocked;
}
