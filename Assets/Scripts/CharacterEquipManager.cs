using UnityEngine;

public class CharacterEquipManager : MonoBehaviour
{
    public static CharacterEquipManager Instance;

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

        SaveData data = SaveSystem.LoadData();
        data.equippedCharacter = characterID;
        SaveSystem.SaveData(data);
        
        CharacterShopManager.Instance.RefreshAllButtons();

        Debug.Log(characterID + " equipped!");
    }

    public string GetEquippedCharacter()
    {
        SaveData data = SaveSystem.LoadData();
        return data.equippedCharacter;
    }
}

