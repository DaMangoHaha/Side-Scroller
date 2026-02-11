using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the currently equipped Pixel Augment icon in the level HUD.
/// Attach to an empty GameObject in each level scene and assign 
/// the icon Image and the sprites for each augment in the Inspector.
/// If no augment is equipped, the icon is hidden.
/// </summary>
public class AugmentHUDDisplay : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The Image component that displays the augment icon in-game.")]
    public Image augmentIcon;

    [Header("Augment Sprites")]
    public Sprite coinFragmentSprite;
    public Sprite stabilityPatchSprite;
    public Sprite emergencyUSBSprite;

    void Start()
    {
        RefreshDisplay();
    }

    void OnEnable()
    {
        RefreshDisplay();
    }

    /// <summary>
    /// Updates the HUD icon to show the currently equipped augment.
    /// </summary>
    public void RefreshDisplay()
    {
        if (augmentIcon == null) return;

        string equipped = "";
        if (PixelAugmentManager.Instance != null)
            equipped = PixelAugmentManager.Instance.GetEquippedAugment();

        switch (equipped)
        {
            case "CoinFragment":
                augmentIcon.sprite = coinFragmentSprite;
                augmentIcon.enabled = true;
                break;
            case "StabilityPatch":
                augmentIcon.sprite = stabilityPatchSprite;
                augmentIcon.enabled = true;
                break;
            case "EmergencyUSB":
                augmentIcon.sprite = emergencyUSBSprite;
                augmentIcon.enabled = true;
                break;
            default:
                augmentIcon.enabled = false;
                break;
        }
    }
}
