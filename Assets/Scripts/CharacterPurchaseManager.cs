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
        return PlayerPrefs.GetInt("CHAR_OWNED_" + characterID, characterID == "Bits" ? 1 : 0) == 1;
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
        PlayerPrefs.SetInt("CHAR_OWNED_" + characterID, 1);
        PlayerPrefs.Save();

        Debug.Log(characterID + " purchased!");
    }
}

