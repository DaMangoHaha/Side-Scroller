using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterShopButton : MonoBehaviour
{
    public string characterID;
    public int cost;

    public Button button;
    public TextMeshProUGUI buttonText;

    public Sprite normalSprite;
    public Sprite equippedSprite;
    public Image buttonImage;

    void Start()
    {
        RefreshUI();
        CharacterShopManager.Instance.RefreshAllButtons();
    }

    public void OnButtonPressed()
    {
        if (!CharacterPurchaseManager.Instance.IsCharacterOwned(characterID))
        {
            CharacterPurchaseManager.Instance.PurchaseCharacter(characterID, cost);
        }
        else
        {
            CharacterEquipManager.Instance.EquipCharacter(characterID);
        }
        
        SaveData data = SaveSystem.LoadData();
        data.selectedCharacter = characterID;
        SaveSystem.SaveData(data);

        RefreshUI();
        CharacterShopManager.Instance.RefreshAllButtons();
    }

    public void RefreshUI()
    {
        if (button == null || buttonText == null || buttonImage == null)
            return;

        SaveData data = SaveSystem.LoadData();
        string equipped = data.selectedCharacter;
        bool owned = CharacterPurchaseManager.Instance.IsCharacterOwned(characterID);
        int coins = (CoinsManager.Instance != null) ? CoinsManager.Instance.GetCoins() : 0;

        // Equipped state
        if (equipped == characterID)
        {
            buttonText.text = "Equipped";
            buttonImage.sprite = equippedSprite;
            button.interactable = true;
            return;
        }

        // Not owned -> show purchase text and disable if not enough coins
        if (!owned)
        {
            buttonText.text = $"Purchase";
            buttonImage.sprite = normalSprite;
            button.interactable = coins >= cost;
            return;
        }

        // Owned but not equipped -> can equip
        buttonText.text = "Equip";
        buttonImage.sprite = normalSprite;
        button.interactable = true;
    }

    public void OnMouseEnter()
    {
        // Prefer not to destroy/hide the button GameObject; use interactable state instead.
        // Keep behavior consistent with RefreshUI:
        if (button == null)
            return;

        bool owned = CharacterPurchaseManager.Instance.IsCharacterOwned(characterID);
        int coins = (CoinsManager.Instance != null) ? CoinsManager.Instance.GetCoins() : 0;

        if (!owned)
            button.interactable = coins >= cost;
        else
            button.interactable = true;
    }
}

