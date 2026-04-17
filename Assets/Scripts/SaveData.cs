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

/// <summary>
/// A single string-string key-value entry for serialization with JsonUtility.
/// Used for equipped costumes (characterID ? costumeID).
/// </summary>
[Serializable]
public class StringStringPair
{
    public string key;
    public string value;

    public StringStringPair() { }

    public StringStringPair(string key, string value)
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
    public float bestTimeLevel5 = 0f;
    public float bestTimeLevel6 = 0f;
    public float bestTimeLevel7 = 0f;

    // Best scores per level (0 = no record)
    public int bestScoreLevel1 = 0;
    public int bestScoreLevel2 = 0;
    public int bestScoreLevel3 = 0;
    public int bestScoreLevel4 = 0;
    public int bestScoreLevel5 = 0;
    public int bestScoreLevel6 = 0;
    public int bestScoreLevel7 = 0;

    // Best star ratings per level (0-3)
    public int bestStarsLevel1 = 0;
    public int bestStarsLevel2 = 0;
    public int bestStarsLevel3 = 0;
    public int bestStarsLevel4 = 0;
    public int bestStarsLevel5 = 0;
    public int bestStarsLevel6 = 0;
    public int bestStarsLevel7 = 0;

    // Whether the player has survived 1 minute in Level 1 to unlock all levels
    public bool levelsUnlocked = false;

    // --- Bit Skill Upgrade Tier (0 = none, 1-3 = purchased tiers) ---
    public int bitSkillUpgradeTier = 0;

    // --- Thief Skill Upgrade Tier (0 = none, 1-3 = purchased tiers) ---
    public int thiefSkillUpgradeTier = 0;

    // --- Ninja Skill Upgrade Tier (0 = none, 1-3 = purchased tiers) ---
    public int ninjaSkillUpgradeTier = 0;

    // --- WizKid Skill Upgrade Tier (0 = none, 1-3 = purchased tiers) ---
    public int wizKidSkillUpgradeTier = 0;

    // --- Crystal Skill Upgrade Tier (0 = none, 1-3 = purchased tiers) ---
    public int crystalSkillUpgradeTier = 0;

    // --- Cubit Skill Upgrade Tier (0 = none, 1-3 = purchased tiers) ---
    public int cubitSkillUpgradeTier = 0;

    // --- Serializable ownership lists (replaces Dictionary<string,bool>) ---
    public List<StringBoolPair> ownedCharactersList = new List<StringBoolPair>();

    // --- Costume ownership (costumeID ? owned) ---
    public List<StringBoolPair> ownedCostumesList = new List<StringBoolPair>();

    // --- Equipped costumes per character (characterID ? costumeID) ---
    public List<StringStringPair> equippedCostumesList = new List<StringStringPair>();

    // --- Runtime dictionaries (not serialized, rebuilt from lists) ---
    [NonSerialized] public Dictionary<string, bool> ownedCharacters;
    [NonSerialized] public Dictionary<string, bool> ownedCostumes;
    [NonSerialized] public Dictionary<string, string> equippedCostumes;

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

        ownedCostumesList = new List<StringBoolPair>();
        equippedCostumesList = new List<StringStringPair>();

        RebuildDictionaries();
    }

    /// <summary>
    /// Rebuilds the runtime dictionaries from the serialized lists.
    /// Must be called after deserialization (JsonUtility does not call constructors on existing objects).
    /// </summary>
    public void RebuildDictionaries()
    {
        // --- Character ownership ---
        ownedCharacters = new Dictionary<string, bool>();
        if (ownedCharactersList != null)
        {
            foreach (var pair in ownedCharactersList)
                ownedCharacters[pair.key] = pair.value;
        }

        // Ensure defaults exist even if the save file is from an older version
        if (!ownedCharacters.ContainsKey("Bits"))
            ownedCharacters["Bits"] = true;

        // --- Costume ownership ---
        ownedCostumes = new Dictionary<string, bool>();
        if (ownedCostumesList != null)
        {
            foreach (var pair in ownedCostumesList)
                ownedCostumes[pair.key] = pair.value;
        }

        // --- Equipped costumes ---
        equippedCostumes = new Dictionary<string, string>();
        if (equippedCostumesList != null)
        {
            foreach (var pair in equippedCostumesList)
                equippedCostumes[pair.key] = pair.value;
        }
    }

    /// <summary>
    /// Syncs the runtime dictionaries back into the serializable lists.
    /// Must be called before saving to disk.
    /// </summary>
    public void SyncListsFromDictionaries()
    {
        // --- Character ownership ---
        ownedCharactersList = new List<StringBoolPair>();
        if (ownedCharacters != null)
        {
            foreach (var kvp in ownedCharacters)
                ownedCharactersList.Add(new StringBoolPair(kvp.Key, kvp.Value));
        }

        // --- Costume ownership ---
        ownedCostumesList = new List<StringBoolPair>();
        if (ownedCostumes != null)
        {
            foreach (var kvp in ownedCostumes)
                ownedCostumesList.Add(new StringBoolPair(kvp.Key, kvp.Value));
        }

        // --- Equipped costumes ---
        equippedCostumesList = new List<StringStringPair>();
        if (equippedCostumes != null)
        {
            foreach (var kvp in equippedCostumes)
                equippedCostumesList.Add(new StringStringPair(kvp.Key, kvp.Value));
        }
    }
}