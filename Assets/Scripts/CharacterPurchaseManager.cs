using UnityEngine;

public class CharacterPurchaseManager : MonoBehaviour
{
    public static CharacterPurchaseManager Instance;

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

    // -------------------------
    // OWNERSHIP
    // -------------------------
    public bool IsCharacterOwned(string characterID)
    {
        SaveData data = SaveSystem.LoadData();
        
        if (data.ownedCharacters.ContainsKey(characterID))
            return data.ownedCharacters[characterID];
        
        // Default: Bits is owned, others are not
        return characterID == "Bits";
    }

    public void PurchaseCharacter(string characterID, int cost)
    {
        if (IsCharacterOwned(characterID))
            return;

        if (CoinsManager.Instance.GetCoins() < cost)
        {
            Debug.Log("Not enough coins!");
            return;
        }

        CoinsManager.Instance.AddCoins(-cost);
        
        SaveData data = SaveSystem.LoadData();
        data.ownedCharacters[characterID] = true;
        SaveSystem.SaveData(data);

        Debug.Log(characterID + " purchased!");
    }
}

