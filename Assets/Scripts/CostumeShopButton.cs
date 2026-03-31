using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI button for a single costume in the costume shop.
/// Handles purchase (coins or Cupid Coins), equip, and visual state updates.
///
/// Assign one per costume entry in your shop scroll view.
/// </summary>
public class CostumeShopButton : MonoBehaviour
{
    [Header("Costume Reference")]
    [Tooltip("The CostumeData ScriptableObject this button represents")]
    public CostumeData costumeData;

    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI buttonText;
    public Image costumeIcon;
    public TextMeshProUGUI costumeName;
    public TextMeshProUGUI costText;

    [Header("Button Sprites")]
    public Sprite normalSprite;
    public Sprite equippedSprite;
    public Image buttonImage;

    [Header("Currency Mode")]
    [Tooltip("If true, this costume costs Cupid Coins instead of regular coins")]
    public bool useCupidCoins = false;

    // Popup references (for "Not enough coins" popup)
    private GameObject popupPanel;
    private Canvas popupCanvas;

    void Start()
    {
        RefreshUI();
    }

    public void OnButtonPressed()
    {
        if (costumeData == null || CostumeManager.Instance == null) return;

        string id = costumeData.costumeID;

        if (!CostumeManager.Instance.IsCostumeOwned(id))
        {
            // Attempt purchase
            bool success;

            if (useCupidCoins)
            {
                int cupidCoins = (CupidCoinsManager.Instance != null) ? CupidCoinsManager.Instance.GetCoins() : 0;
                if (cupidCoins < costumeData.cupidCoinCost)
                {
                    ShowNotEnoughCoinsPanel("Not enough Cupid Coins!");
                    return;
                }
                success = CostumeManager.Instance.PurchaseCostumeWithCupidCoins(id);
            }
            else
            {
                int coins = (CoinsManager.Instance != null) ? CoinsManager.Instance.GetCoins() : 0;
                if (coins < costumeData.coinCost)
                {
                    ShowNotEnoughCoinsPanel("Not enough coins!");
                    return;
                }
                success = CostumeManager.Instance.PurchaseCostumeWithCoins(id);
            }

            if (!success) return;

            // Auto-equip on purchase
            CostumeManager.Instance.EquipCostume(id);
        }
        else
        {
            // Already owned — check if it's already equipped
            string equippedID = CostumeManager.Instance.GetEquippedCostumeID(costumeData.characterID);

            if (equippedID == id)
            {
                // Already equipped — unequip (revert to default)
                CostumeManager.Instance.UnequipCostume(costumeData.characterID);
            }
            else
            {
                // Equip this costume
                CostumeManager.Instance.EquipCostume(id);
            }
        }

        RefreshUI();

        // Refresh sibling buttons in case another costume was previously equipped
        CostumeShopButton[] siblings = GetComponentInParent<Transform>().GetComponentsInChildren<CostumeShopButton>();
        foreach (var sibling in siblings)
        {
            if (sibling != this)
                sibling.RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (costumeData == null || CostumeManager.Instance == null) return;

        string id = costumeData.costumeID;
        bool owned = CostumeManager.Instance.IsCostumeOwned(id);
        string equippedID = CostumeManager.Instance.GetEquippedCostumeID(costumeData.characterID);
        bool isEquipped = equippedID == id;

        // Update icon
        if (costumeIcon != null && costumeData.shopIcon != null)
            costumeIcon.sprite = costumeData.shopIcon;

        // Update costume name
        if (costumeName != null)
            costumeName.text = costumeData.displayName;

        // Update cost display
        if (costText != null)
        {
            if (owned)
                costText.text = "";
            else if (useCupidCoins)
                costText.text = costumeData.cupidCoinCost + " Cupid Coins";
            else
                costText.text = costumeData.coinCost + " Coins";
        }

        // Update button state
        if (isEquipped)
        {
            if (buttonText != null) buttonText.text = "Equipped";
            if (buttonImage != null && equippedSprite != null) buttonImage.sprite = equippedSprite;
        }
        else if (owned)
        {
            if (buttonText != null) buttonText.text = "Equip";
            if (buttonImage != null && normalSprite != null) buttonImage.sprite = normalSprite;
        }
        else
        {
            if (buttonText != null) buttonText.text = "Purchase";
            if (buttonImage != null && normalSprite != null) buttonImage.sprite = normalSprite;
        }

        if (button != null)
            button.interactable = true;
    }

    // -------------------------------------------------------
    // "Not Enough Coins" Popup
    // -------------------------------------------------------

    private void ShowNotEnoughCoinsPanel(string message)
    {
        if (popupPanel != null) return;

        Canvas canvas = GetOrCreatePopupCanvas();

        // Dark overlay
        popupPanel = new GameObject("NotEnoughCoinsPanel");
        popupPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = popupPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelImage = popupPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.7f);

        // Message text
        GameObject msgGO = new GameObject("MessageText");
        msgGO.transform.SetParent(popupPanel.transform, false);

        RectTransform msgRT = msgGO.AddComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0.5f, 0.5f);
        msgRT.anchorMax = new Vector2(0.5f, 0.5f);
        msgRT.anchoredPosition = new Vector2(0f, 60f);
        msgRT.sizeDelta = new Vector2(600f, 80f);

        TextMeshProUGUI msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
        msgTMP.text = message;
        msgTMP.fontSize = 48;
        msgTMP.alignment = TextAlignmentOptions.Center;
        msgTMP.color = Color.white;

        // OK button
        GameObject btnGO = new GameObject("OKButton");
        btnGO.transform.SetParent(popupPanel.transform, false);

        RectTransform btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(0f, -40f);
        btnRT.sizeDelta = new Vector2(200f, 60f);

        Image btnBG = btnGO.AddComponent<Image>();
        btnBG.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button okBtn = btnGO.AddComponent<Button>();
        okBtn.targetGraphic = btnBG;
        okBtn.onClick.AddListener(DismissPopup);

        // Button label
        GameObject labelGO = new GameObject("Text");
        labelGO.transform.SetParent(btnGO.transform, false);

        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text = "OK";
        labelTMP.fontSize = 28;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color = Color.white;
    }

    private void DismissPopup()
    {
        if (popupPanel != null)
        {
            Destroy(popupPanel);
            popupPanel = null;
        }
    }

    private Canvas GetOrCreatePopupCanvas()
    {
        if (popupCanvas != null) return popupCanvas;

        GameObject existing = GameObject.Find("CostumeShopPopupCanvas");
        if (existing != null)
        {
            popupCanvas = existing.GetComponent<Canvas>();
            if (popupCanvas != null) return popupCanvas;
        }

        GameObject canvasGO = new GameObject("CostumeShopPopupCanvas");
        popupCanvas = canvasGO.AddComponent<Canvas>();
        popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        popupCanvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        return popupCanvas;
    }
}
