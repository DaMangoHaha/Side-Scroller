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

    // Popup references
    private GameObject popupPanel;
    private Canvas popupCanvas;

    void Start()
    {
        RefreshUI();
        CharacterShopManager.Instance.RefreshAllButtons();
        DontDestroyOnLoad(gameObject);
    }

    public void OnButtonPressed()
    {
        if (!CharacterPurchaseManager.Instance.IsCharacterOwned(characterID))
        {
            int coins = (CoinsManager.Instance != null) ? CoinsManager.Instance.GetCoins() : 0;

            if (coins < cost)
            {
                ShowNotEnoughCoinsPanel();
                return;
            }

            // Purchase and auto-equip
            CharacterPurchaseManager.Instance.PurchaseCharacter(characterID, cost);
            CharacterEquipManager.Instance.EquipCharacter(characterID);
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

        // Equipped state
        if (equipped == characterID)
        {
            buttonText.text = "Equipped";
            buttonImage.sprite = equippedSprite;
            button.interactable = true;
            return;
        }

        // Not owned -> show purchase (always interactable)
        if (!owned)
        {
            buttonText.text = $"Purchase";
            buttonImage.sprite = normalSprite;
            button.interactable = true;
            return;
        }

        // Owned but not equipped -> can equip
        buttonText.text = "Equip";
        buttonImage.sprite = normalSprite;
        button.interactable = true;
    }

    public void OnMouseEnter()
    {
        if (button == null)
            return;

        button.interactable = true;
    }

    // -------------------------------------------------------
    // "Not Enough Coins" Popup (built in code like PauseManager)
    // -------------------------------------------------------

    private void ShowNotEnoughCoinsPanel()
    {
        if (popupPanel != null)
            return; // already showing

        // --- Canvas ---
        Canvas canvas = GetOrCreatePopupCanvas();

        // --- Dark overlay panel ---
        popupPanel = new GameObject("NotEnoughCoinsPanel");
        popupPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = popupPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelImage = popupPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.7f);

        // --- Message text ---
        GameObject msgGO = new GameObject("MessageText");
        msgGO.transform.SetParent(popupPanel.transform, false);

        RectTransform msgRT = msgGO.AddComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0.5f, 0.5f);
        msgRT.anchorMax = new Vector2(0.5f, 0.5f);
        msgRT.anchoredPosition = new Vector2(0f, 60f);
        msgRT.sizeDelta = new Vector2(600f, 80f);

        TextMeshProUGUI msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
        msgTMP.text = "Not enough coins!";
        msgTMP.fontSize = 48;
        msgTMP.alignment = TextAlignmentOptions.Center;
        msgTMP.color = Color.white;

        // --- OK button ---
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

        ColorBlock colors = okBtn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.selectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        okBtn.colors = colors;

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

        GameObject existing = GameObject.Find("ShopPopupCanvas");
        if (existing != null)
        {
            popupCanvas = existing.GetComponent<Canvas>();
            if (popupCanvas != null) return popupCanvas;
        }

        GameObject canvasGO = new GameObject("ShopPopupCanvas");
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

