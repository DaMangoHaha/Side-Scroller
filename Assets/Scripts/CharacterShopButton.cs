using TMPro;
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
        PlayerPrefs.SetString("SelectedCharacter", characterID);
        PlayerPrefs.Save();

        RefreshUI();
        CharacterShopManager.Instance.RefreshAllButtons();
    }




    public void RefreshUI()
    {
        string equipped = PlayerPrefs.GetString("SelectedCharacter", "Bits");

        if (equipped == characterID)
        {
            buttonText.text = "Equipped";
            buttonImage.sprite = equippedSprite;
        }
        else
        {
            buttonText.text = "Equip";
            buttonImage.sprite = normalSprite;
        }
    }

}

