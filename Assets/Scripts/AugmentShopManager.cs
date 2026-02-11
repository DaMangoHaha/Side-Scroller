using UnityEngine;

/// <summary>
/// Manages the Pixel Augment shop UI. Attach to a persistent GameObject
/// and assign all AugmentShopButton references in the Inspector.
/// Mirrors CharacterShopManager.
/// </summary>
public class AugmentShopManager : MonoBehaviour
{
    public static AugmentShopManager Instance;
    public AugmentShopButton[] augmentButtons;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RefreshAllButtons()
    {
        if (augmentButtons == null) return;

        foreach (var button in augmentButtons)
        {
            if (button != null)
                button.RefreshUI();
        }
    }
}
