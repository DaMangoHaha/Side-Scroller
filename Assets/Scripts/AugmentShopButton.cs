using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual buy/equip button for a Pixel Augment in the shop.
/// Mirrors CharacterShopButton — assign augmentID, cost, and UI references in Inspector.
/// </summary>
public class AugmentShopButton : MonoBehaviour
{
    [Header("Augment Identity")]
    [Tooltip("Must match the key in SaveData.ownedAugments (e.g. Coin Fragment, Stability Patch, Emergency USB)")]
    public string augmentID;
    public int cost;

    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI buttonText;
    public Sprite normalSprite;
    public Sprite equippedSprite;
    public Image buttonImage;

    private bool needsRefresh = false;
    private Coroutine feedbackCoroutine;

    void Awake()
    {
        RefreshUI();

        if (AugmentShopManager.Instance != null)
            AugmentShopManager.Instance.RefreshAllButtons();
    }

    void Update()
    {
        // Deferred refresh: if Awake ran before PixelAugmentManager was ready,
        // keep trying until it becomes available.
        if (needsRefresh && PixelAugmentManager.Instance != null)
        {
            needsRefresh = false;
            RefreshUI();

            if (AugmentShopManager.Instance != null)
                AugmentShopManager.Instance.RefreshAllButtons();
        }
    }

    public void OnButtonPressed()
    {
        if (PixelAugmentManager.Instance == null) return;

        if (!PixelAugmentManager.Instance.IsAugmentOwned(augmentID))
        {
            // Attempt purchase — check coins first
            int coins = (CoinsManager.Instance != null) ? CoinsManager.Instance.GetCoins() : 0;
            if (coins < cost)
            {
                // Not enough coins — show brief feedback without disabling the button
                if (feedbackCoroutine != null)
                    StopCoroutine(feedbackCoroutine);
                feedbackCoroutine = StartCoroutine(ShowInsufficientCoinsFeedback());
                return;
            }

            PixelAugmentManager.Instance.PurchaseAugment(augmentID, cost);
        }
        else
        {
            string equipped = PixelAugmentManager.Instance.GetEquippedAugment();

            if (equipped == augmentID)
            {
                // Already equipped — unequip
                PixelAugmentManager.Instance.UnequipAugment();
            }
            else
            {
                // Equip this one
                PixelAugmentManager.Instance.EquipAugment(augmentID);
            }
        }

        RefreshUI();

        if (AugmentShopManager.Instance != null)
            AugmentShopManager.Instance.RefreshAllButtons();
    }

    private IEnumerator ShowInsufficientCoinsFeedback()
    {
        if (buttonText != null)
            buttonText.text = "Not enough coins!";

        yield return new WaitForSeconds(1.5f);

        // Restore normal label
        RefreshUI();
        feedbackCoroutine = null;
    }

    public void RefreshUI()
    {
        if (button == null || buttonText == null || buttonImage == null) return;

        if (PixelAugmentManager.Instance == null)
        {
            // Manager not ready yet — mark for deferred refresh
            needsRefresh = true;
            return;
        }

        needsRefresh = false;

        string equipped = PixelAugmentManager.Instance.GetEquippedAugment();
        bool owned = PixelAugmentManager.Instance.IsAugmentOwned(augmentID);

        // Equipped state
        if (equipped == augmentID)
        {
            buttonText.text = "Equipped";
            buttonImage.sprite = equippedSprite;
            button.interactable = true;
            return;
        }

        // Not owned — show purchase (always interactable)
        if (!owned)
        {
            buttonText.text = "Purchase";
            buttonImage.sprite = normalSprite;
            button.interactable = true;
            return;
        }

        // Owned but not equipped — can equip
        buttonText.text = "Equip";
        buttonImage.sprite = normalSprite;
        button.interactable = true;
    }
}
