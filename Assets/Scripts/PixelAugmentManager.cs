using UnityEngine;

/// <summary>
/// Singleton that tracks which Pixel Augment is currently equipped.
/// Provides static helpers so any gameplay script can query active buffs.
/// </summary>
public class PixelAugmentManager : MonoBehaviour
{
    public static PixelAugmentManager Instance;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // -------------------------
    // OWNERSHIP
    // -------------------------
    public bool IsAugmentOwned(string augmentID)
    {
        SaveData data = SaveSystem.LoadData();

        if (data.ownedAugments.ContainsKey(augmentID))
            return data.ownedAugments[augmentID];

        return false;
    }

    public void PurchaseAugment(string augmentID, int cost)
    {
        if (IsAugmentOwned(augmentID))
            return;

        if (CoinsManager.Instance == null || CoinsManager.Instance.GetCoins() < cost)
        {
            Debug.Log("Not enough coins to purchase augment!");
            return;
        }

        CoinsManager.Instance.AddCoins(-cost);

        SaveData data = SaveSystem.LoadData();
        data.ownedAugments[augmentID] = true;
        SaveSystem.SaveData(data);

        Debug.Log(augmentID + " augment purchased!");
    }

    // -------------------------
    // EQUIPPING
    // -------------------------
    public void EquipAugment(string augmentID)
    {
        if (!IsAugmentOwned(augmentID))
        {
            Debug.Log("Augment not owned!");
            return;
        }

        SaveData data = SaveSystem.LoadData();
        data.equippedAugment = augmentID;
        SaveSystem.SaveData(data);

        Debug.Log(augmentID + " augment equipped!");
    }

    public void UnequipAugment()
    {
        SaveData data = SaveSystem.LoadData();
        data.equippedAugment = "";
        SaveSystem.SaveData(data);

        Debug.Log("Augment unequipped.");
    }

    public string GetEquippedAugment()
    {
        SaveData data = SaveSystem.LoadData();
        return data.equippedAugment;
    }

    // -------------------------
    // BUFF QUERIES
    // -------------------------

    /// <summary>
    /// Coin Fragment: Returns true if the Coin Fragment augment is equipped.
    /// Coin value is increased by 10% (rounded up).
    /// </summary>
    public bool IsCoinFragmentActive()
    {
        return GetEquippedAugment() == "Coin Fragment";
    }

    /// <summary>
    /// Stability Patch: Returns true if the Stability Patch augment is equipped.
    /// Invincibility duration increased by 20% when taking damage.
    /// </summary>
    public bool IsStabilityPatchActive()
    {
        return GetEquippedAugment() == "Stability Patch";
    }

    /// <summary>
    /// Emergency USB: Returns true if the Emergency USB augment is equipped.
    /// Obstacles/Enemies deal 15% less damage.
    /// </summary>
    public bool IsEmergencyUSBActive()
    {
        return GetEquippedAugment() == "Emergency USB";
    }

    /// <summary>
    /// Applies Coin Fragment bonus: increases coin value by 10%, rounded up.
    /// </summary>
    public int ApplyCoinFragmentBonus(int baseCoinValue)
    {
        if (!IsCoinFragmentActive()) return baseCoinValue;
        return Mathf.CeilToInt(baseCoinValue * 1.10f);
    }

    /// <summary>
    /// Applies Stability Patch bonus: increases invulnerability duration by 20%.
    /// </summary>
    public float ApplyStabilityPatchBonus(float baseDuration)
    {
        if (!IsStabilityPatchActive()) return baseDuration;
        return baseDuration * 1.20f;
    }

    /// <summary>
    /// Applies Emergency USB bonus: reduces incoming damage by 15%.
    /// </summary>
    public float ApplyEmergencyUSBReduction(float baseDamage)
    {
        if (!IsEmergencyUSBActive()) return baseDamage;
        return baseDamage * 0.85f;
    }
}
