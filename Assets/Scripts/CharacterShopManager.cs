using UnityEngine;

public class CharacterShopManager : MonoBehaviour
{
    public static CharacterShopManager Instance;
    public CharacterShopButton[] characterButtons;

    void Awake()
    {
        Instance = this;
    }

    public void RefreshAllButtons()
    {
        foreach (var button in characterButtons)
            button.RefreshUI();
    }
}
