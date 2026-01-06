using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterShopButton : MonoBehaviour
{
    public string characterID;
    public int cost;

    public Button button;
    public TextMeshProUGUI buttonText;

    void Start()
    {
        Refresh();
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

        Refresh();
    }

    void Refresh()
    {
        bool owned = CharacterPurchaseManager.Instance.IsCharacterOwned(characterID);
        string equipped = CharacterEquipManager.Instance.GetEquippedCharacter();

        if (!owned)
        {
            buttonText.text = cost + " Coins";
            button.interactable = CoinsManager.Instance.GetCoins() >= cost;
        }
        else if (equipped == characterID)
        {
            buttonText.text = "Equipped";
            button.interactable = true;
        }
        else
        {
            buttonText.text = "Equip";
            button.interactable = true;
        }
    }
}

