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

    // Pixel Augments
    public string equippedAugment = "";  // empty = none equipped

    // --- Serializable ownership lists (replaces Dictionary<string,bool>) ---
    public List<StringBoolPair> ownedCharactersList = new List<StringBoolPair>();
    public List<StringBoolPair> ownedAugmentsList = new List<StringBoolPair>();

    // --- Runtime dictionaries (not serialized, rebuilt from lists) ---
    [NonSerialized] public Dictionary<string, bool> ownedCharacters;
    [NonSerialized] public Dictionary<string, bool> ownedAugments;

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

        // Set default augment ownership
        ownedAugmentsList = new List<StringBoolPair>
        {
            new StringBoolPair("CoinFragment", false),
            new StringBoolPair("StabilityPatch", false),
            new StringBoolPair("EmergencyUSB", false)
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

        ownedAugments = new Dictionary<string, bool>();
        if (ownedAugmentsList != null)
        {
            foreach (var pair in ownedAugmentsList)
                ownedAugments[pair.key] = pair.value;
        }

        // Ensure defaults exist even if the save file is from an older version
        if (!ownedCharacters.ContainsKey("Bits"))
            ownedCharacters["Bits"] = true;

        if (!ownedAugments.ContainsKey("CoinFragment"))
            ownedAugments["CoinFragment"] = false;
        if (!ownedAugments.ContainsKey("StabilityPatch"))
            ownedAugments["StabilityPatch"] = false;
        if (!ownedAugments.ContainsKey("EmergencyUSB"))
            ownedAugments["EmergencyUSB"] = false;
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

        ownedAugmentsList = new List<StringBoolPair>();
        if (ownedAugments != null)
        {
            foreach (var kvp in ownedAugments)
                ownedAugmentsList.Add(new StringBoolPair(kvp.Key, kvp.Value));
        }
    }
}