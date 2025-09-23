using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PixelsUI : MonoBehaviour
{
    [Header("UI References")]
    public Image characterIcon;
    public TextMeshProUGUI characterNameText;
    public Button skillDescButton;
    public Button selectButton;
    public TextMeshProUGUI selectButtonText;

    [Header("Skill Panel")]
    public GameObject skillPanel;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;

    private int currentIndex = 0; // start with Bit

    void Start()
    {
        ShowCharacter(currentIndex);

        skillDescButton.onClick.AddListener(ShowSkillDescription);
        selectButton.onClick.AddListener(OnSelectButton);
    }

    void ShowCharacter(int index)
    {
        var data = CharacterManager.Instance.characters[index];
        characterIcon.sprite = data.characterIcon;
        characterNameText.text = data.characterName;

        currentIndex = index;

        // Button state
        if (data.isUnlocked)
        {
            if (CharacterManager.Instance.selectedCharacterIndex == index)
            {
                selectButton.interactable = false;
                selectButtonText.text = "Equipped";
            }
            else
            {
                selectButton.interactable = true;
                selectButtonText.text = "Select";
            }
        }
        else
        {
            selectButton.interactable = false;
            selectButtonText.text = "Locked";
        }
    }

    void ShowSkillDescription()
    {
        var data = CharacterManager.Instance.characters[currentIndex];
        skillPanel.SetActive(true);
        skillNameText.text = data.skillName;
        skillDescriptionText.text = data.skillDescription;
    }

    void OnSelectButton()
    {
        CharacterManager.Instance.SelectCharacter(currentIndex);
        ShowCharacter(currentIndex); // refresh button states
    }
}
