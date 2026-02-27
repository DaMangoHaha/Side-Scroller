using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single key-value entry for serialization with JsonUtility,
/// which does not support Dictionary.
/// </summary>
[Serializable]
public class StringBoolPair
{
    public string key;
    public bool value;

    public StringBoolPair() { }

    public StringBoolPair(string key, bool value)
    {
        this.key = key;
        this.value = value;
    }
}

[Serializable]
public class SaveData
{
    public int totalCoins = 0;
    public string equippedCharacter = "Bits";
    public string selectedCharacter = "Bits";
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    // Best times per level (in seconds; 0 = no record)
    public float bestTimeLevel1 = 0f;
    public float bestTimeLevel2 = 0f;
    public float bestTimeLevel3 = 0f;
    public float bestTimeLevel4 = 0f;

    // Whether the player has survived 1 minute in Level 1 to unlock all levels
    public bool levelsUnlocked = false;

    // --- Bit Skill Upgrade Tier (0 = none, 1-3 = purchased tiers) ---
    public int bitSkillUpgradeTier = 0;

    // --- Thief Skill Upgrade Tier (0 = none, 1-3 = purchased tiers) ---
    public int thiefSkillUpgradeTier = 0;

    // --- Serializable ownership lists (replaces Dictionary<string,bool>) ---
    public List<StringBoolPair> ownedCharactersList = new List<StringBoolPair>();

    // --- Runtime dictionaries (not serialized, rebuilt from lists) ---
    [NonSerialized] public Dictionary<string, bool> ownedCharacters;

    public SaveData()
    {
        // Set default character ownership
        ownedCharactersList = new List<StringBoolPair>
        {
            new StringBoolPair("Bits", true),
            new StringBoolPair("Thief", false),
            new StringBoolPair("Ninja", false),
            new StringBoolPair("WizKid", false),
            new StringBoolPair("Crystal", false),
            new StringBoolPair("Cubit", false)
        };

        RebuildDictionaries();
    }

    /// <summary>
    /// Rebuilds the runtime dictionaries from the serialized lists.
    /// Must be called after deserialization (JsonUtility does not call constructors on existing objects).
    /// </summary>
    public void RebuildDictionaries()
    {
        ownedCharacters = new Dictionary<string, bool>();
        if (ownedCharactersList != null)
        {
            foreach (var pair in ownedCharactersList)
                ownedCharacters[pair.key] = pair.value;
        }

        // Ensure defaults exist even if the save file is from an older version
        if (!ownedCharacters.ContainsKey("Bits"))
            ownedCharacters["Bits"] = true;
    }

    /// <summary>
    /// Syncs the runtime dictionaries back into the serializable lists.
    /// Must be called before saving to disk.
    /// </summary>
    public void SyncListsFromDictionaries()
    {
        ownedCharactersList = new List<StringBoolPair>();
        if (ownedCharacters != null)
        {
            foreach (var kvp in ownedCharacters)
                ownedCharactersList.Add(new StringBoolPair(kvp.Key, kvp.Value));
        }
    }
}