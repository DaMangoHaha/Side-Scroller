using UnityEngine;

public class CharacterEquipManager : MonoBehaviour
{
    public static CharacterEquipManager Instance;

    private const string EQUIPPED_KEY = "EquippedCharacter";

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

    public void EquipCharacter(string characterID)
    {
        if (!CharacterPurchaseManager.Instance.IsCharacterOwned(characterID))
        {
            Debug.Log("Character not owned!");
            return;
        }

        PlayerPrefs.SetString(EQUIPPED_KEY, characterID);
        PlayerPrefs.Save();
        CharacterShopManager.Instance.RefreshAllButtons();

        Debug.Log(characterID + " equipped!");
    }

    public string GetEquippedCharacter()
    {
        return PlayerPrefs.GetString(EQUIPPED_KEY, "Bits");
    }
}

