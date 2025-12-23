using UnityEngine;
using UnityEngine.UI;

public class CharacterSelected : MonoBehaviour
{
    public string characterName;             // e.g. "Bits", "Thief", etc.
    public Sprite normalSprite;              // black sprite
    public Sprite selectedSprite;            // green sprite

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        UpdateVisualState();
    }

    public void OnButtonClicked()
    {
        PlayerPrefs.SetString("SelectedCharacter", characterName);
        PlayerPrefs.Save();

        // Refresh ALL buttons in the scene
        CharacterSelected[] allButtons = Object.FindObjectsByType<CharacterSelected>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
            btn.UpdateVisualState();
    }

    public void UpdateVisualState()
    {
        string selected = PlayerPrefs.GetString("SelectedCharacter", "Bits");

        if (selected == characterName)
            buttonImage.sprite = selectedSprite;
        else
            buttonImage.sprite = normalSprite;
    }
}
