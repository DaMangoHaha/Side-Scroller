using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton manager that handles costume ownership, equipping, and persistence.
/// Place on a GameObject that persists across scenes (DontDestroyOnLoad).
/// 
/// Assign all available CostumeData assets to the <see cref="allCostumes"/> array in the Inspector.
/// </summary>
public class CostumeManager : MonoBehaviour
{
    public static CostumeManager Instance;

    [Header("All Available Costumes")]
    [Tooltip("Drag every CostumeData ScriptableObject here")]
    public CostumeData[] allCostumes;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===========================
    //  OWNERSHIP
    // ===========================

    /// <summary>
    /// Returns true if the player owns the given costume.
    /// Default/free costumes are always owned.
    /// </summary>
    public bool IsCostumeOwned(string costumeID)
    {
        // Check if it's a free default costume
        CostumeData data = GetCostumeData(costumeID);
        if (data != null && data.isFreeDefault)
            return true;

        SaveData save = SaveSystem.LoadData();
        if (save.ownedCostumes != null && save.ownedCostumes.ContainsKey(costumeID))
            return save.ownedCostumes[costumeID];

        return false;
    }

    /// <summary>
    /// Attempts to purchase a costume using regular coins.
    /// Returns true on success.
    /// </summary>
    public bool PurchaseCostumeWithCoins(string costumeID)
    {
        if (IsCostumeOwned(costumeID))
            return true; // already owned

        CostumeData data = GetCostumeData(costumeID);
        if (data == null) return false;

        if (CoinsManager.Instance == null || CoinsManager.Instance.GetCoins() < data.coinCost)
        {
            Debug.Log("Not enough coins to purchase costume: " + costumeID);
            return false;
        }

        CoinsManager.Instance.AddCoins(-data.coinCost);
        SetCostumeOwned(costumeID, true);
        Debug.Log("Purchased costume: " + costumeID);
        return true;
    }

    /// <summary>
    /// Attempts to purchase a costume using Cupid Coins.
    /// Returns true on success.
    /// </summary>
    public bool PurchaseCostumeWithCupidCoins(string costumeID)
    {
        if (IsCostumeOwned(costumeID))
            return true;

        CostumeData data = GetCostumeData(costumeID);
        if (data == null) return false;

        if (CupidCoinsManager.Instance == null || CupidCoinsManager.Instance.GetCoins() < data.cupidCoinCost)
        {
            Debug.Log("Not enough Cupid Coins to purchase costume: " + costumeID);
            return false;
        }

        CupidCoinsManager.Instance.AddCoins(-data.cupidCoinCost);
        SetCostumeOwned(costumeID, true);
        Debug.Log("Purchased costume (Cupid Coins): " + costumeID);
        return true;
    }

    private void SetCostumeOwned(string costumeID, bool owned)
    {
        SaveData save = SaveSystem.LoadData();
        if (save.ownedCostumes == null)
            save.ownedCostumes = new Dictionary<string, bool>();
        save.ownedCostumes[costumeID] = owned;
        SaveSystem.SaveData(save);
    }

    // ===========================
    //  EQUIPPING
    // ===========================

    /// <summary>
    /// Equips a costume for its associated character. The costume must be owned.
    /// </summary>
    public bool EquipCostume(string costumeID)
    {
        if (!IsCostumeOwned(costumeID))
        {
            Debug.Log("Cannot equip costume — not owned: " + costumeID);
            return false;
        }

        CostumeData data = GetCostumeData(costumeID);
        if (data == null) return false;

        SaveData save = SaveSystem.LoadData();
        if (save.equippedCostumes == null)
            save.equippedCostumes = new Dictionary<string, string>();

        save.equippedCostumes[data.characterID] = costumeID;
        SaveSystem.SaveData(save);

        Debug.Log("Equipped costume " + costumeID + " on " + data.characterID);
        return true;
    }

    /// <summary>
    /// Removes any equipped costume for the given character, reverting to the default look.
    /// </summary>
    public void UnequipCostume(string characterID)
    {
        SaveData save = SaveSystem.LoadData();
        if (save.equippedCostumes != null && save.equippedCostumes.ContainsKey(characterID))
        {
            save.equippedCostumes.Remove(characterID);
            SaveSystem.SaveData(save);
        }
    }

    /// <summary>
    /// Returns the costume ID currently equipped on the given character,
    /// or null/empty if using the default skin.
    /// </summary>
    public string GetEquippedCostumeID(string characterID)
    {
        SaveData save = SaveSystem.LoadData();
        if (save.equippedCostumes != null && save.equippedCostumes.ContainsKey(characterID))
            return save.equippedCostumes[characterID];

        return null;
    }

    /// <summary>
    /// Returns the CostumeData for the currently equipped costume on a character,
    /// or null if using the default skin.
    /// </summary>
    public CostumeData GetEquippedCostume(string characterID)
    {
        string costumeID = GetEquippedCostumeID(characterID);
        if (string.IsNullOrEmpty(costumeID))
            return null;

        return GetCostumeData(costumeID);
    }

    // ===========================
    //  QUERIES
    // ===========================

    /// <summary>
    /// Returns the CostumeData ScriptableObject for a given costume ID.
    /// </summary>
    public CostumeData GetCostumeData(string costumeID)
    {
        if (allCostumes == null) return null;

        foreach (var costume in allCostumes)
        {
            if (costume != null && costume.costumeID == costumeID)
                return costume;
        }

        return null;
    }

    /// <summary>
    /// Returns all costumes that belong to a specific character.
    /// </summary>
    public List<CostumeData> GetCostumesForCharacter(string characterID)
    {
        List<CostumeData> result = new List<CostumeData>();
        if (allCostumes == null) return result;

        foreach (var costume in allCostumes)
        {
            if (costume != null && costume.characterID == characterID)
                result.Add(costume);
        }

        return result;
    }

    /// <summary>
    /// Returns all costumes matching a specific theme tag (e.g. "Valentines").
    /// </summary>
    public List<CostumeData> GetCostumesByTheme(string themeTag)
    {
        List<CostumeData> result = new List<CostumeData>();
        if (allCostumes == null) return result;

        foreach (var costume in allCostumes)
        {
            if (costume != null && costume.themeTag == themeTag)
                result.Add(costume);
        }

        return result;
    }
}
