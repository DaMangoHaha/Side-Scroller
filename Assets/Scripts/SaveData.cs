using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int totalCoins = 0;
    public string equippedCharacter = "Bits";
    public string selectedCharacter = "Bits";
    public Dictionary<string, bool> ownedCharacters = new Dictionary<string, bool>();
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public SaveData()
    {
        // Set default ownership (Bits is owned by default)
        ownedCharacters["Bits"] = true;
        ownedCharacters["Thief"] = false;
        ownedCharacters["Ninja"] = false;
        ownedCharacters["WizKid"] = false;
        ownedCharacters["Crystal"] = false;
        ownedCharacters["Cubit"] = false;
    }
}