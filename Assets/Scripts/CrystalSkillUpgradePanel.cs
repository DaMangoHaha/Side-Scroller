using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Manages the Crystal Skill Upgrade UI panel.
/// Attach to a GameObject with a Button (the Upgrade Icon).
/// When the player clicks the icon, a panel showing three upgrade tiers appears.
/// Each tier costs 300 coins and must be purchased in order.
/// </summary>
public class CrystalSkillUpgradePanel : MonoBehaviour
{
    [Header("References")]
    public CrystalAbility crystalAbility; // assign the player's CrystalAbility component

    [Header("Upgrade Cost")]
    public int upgradeCost = 300;

    // Runtime UI references
    private GameObject panelRoot;
    private Canvas popupCanvas;
    private Button[] tierButtons = new Button[3];
    private TextMeshProUGUI[] tierTexts = new TextMeshProUGUI[3];
    private Image[] tierBackgrounds = new Image[3];

    private bool isPanelOpen = false;

    // Tier descriptions
    private readonly string[] tierDescriptions = new string[]
    {
        "Tier 1: Glaciate needs 1 less Snowflake to activate",
        "Tier 2: Collecting a Snowflake during Glaciate extends it by 1s",
        "Tier 3: 25% chance to spawn a Chill Wind on Glaciate (+25% Max Energy, 50% Slower Depletion, 20% DMG Reduction for 25s)"
    };

    void Start()
    {
        // Hook up the upgrade icon button
        Button iconButton = GetComponent<Button>();
        if (iconButton == null)
            iconButton = gameObject.AddComponent<Button>();
        iconButton.transition = Selectable.Transition.None;
        iconButton.onClick.AddListener(TogglePanel);
    }

    /// <summary>
    /// Toggles the upgrade panel on or off.
    /// </summary>
    public void TogglePanel()
    {
        if (isPanelOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    /// <summary>
    /// Opens the upgrade panel.
    /// </summary>
    public void OpenPanel()
    {
        if (panelRoot != null) return;

        if (PauseManager.Instance != null && PauseManager.IsPaused)
            PauseManager.Instance.DismissPauseMenuOnly();

        Canvas canvas = GetOrCreatePopupCanvas();
        int currentTier = 0;
        if (crystalAbility != null)
            currentTier = crystalAbility.GetUpgradeTier();

        panelRoot = new GameObject("CrystalUpgradePanel");
        panelRoot.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = panelRoot.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelBG = panelRoot.AddComponent<Image>();
        panelBG.color = new Color(0f, 0f, 0f, 0.75f);

        GameObject boxGO = new GameObject("UpgradeBox");
        boxGO.transform.SetParent(panelRoot.transform, false);

        RectTransform boxRT = boxGO.AddComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0.5f, 0.5f);
        boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.anchoredPosition = Vector2.zero;
        boxRT.sizeDelta = new Vector2(700f, 420f);

        Image boxBG = boxGO.AddComponent<Image>();
        boxBG.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(boxGO.transform, false);

        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -30f);
        titleRT.sizeDelta = new Vector2(600f, 50f);

        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "Glaciate Upgrades";
        titleTMP.fontSize = 36;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(0.5f, 0.8f, 1f); // icy blue for Crystal

        Button firstInteractable = null;

        for (int i = 0; i < 3; i++)
        {
            int tierIndex = i;
            int tierNumber = i + 1;

            GameObject btnGO = new GameObject("Tier" + tierNumber + "Button");
            btnGO.transform.SetParent(boxGO.transform, false);

            RectTransform btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 1f);
            btnRT.anchorMax = new Vector2(0.5f, 1f);
            btnRT.anchoredPosition = new Vector2(0f, -90f - (tierIndex * 80f));
            btnRT.sizeDelta = new Vector2(600f, 65f);

            Image btnBG = btnGO.AddComponent<Image>();
            tierBackgrounds[i] = btnBG;

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnBG;
            tierButtons[i] = btn;

            Navigation nav = btn.navigation;
            nav.mode = Navigation.Mode.Automatic;
            btn.navigation = nav;

            GameObject labelGO = new GameObject("Text");
            labelGO.transform.SetParent(btnGO.transform, false);

            RectTransform labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10f, 5f);
            labelRT.offsetMax = new Vector2(-10f, -5f);

            TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.fontSize = 24;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.color = Color.white;
            tierTexts[i] = labelTMP;

            if (currentTier >= tierNumber)
            {
                labelTMP.text = tierDescriptions[tierIndex] + "  [OWNED]";
                btnBG.color = new Color(0.15f, 0.4f, 0.15f, 0.9f); // green-ish
                btn.interactable = false;
            }
            else if (currentTier == tierNumber - 1)
            {
                labelTMP.text = tierDescriptions[tierIndex] + "  [" + upgradeCost + " Coins]";
                btnBG.color = new Color(0.2f, 0.2f, 0.35f, 0.9f); // blue-ish
                btn.interactable = true;
                btn.onClick.AddListener(() => PurchaseTier(tierNumber));
                if (firstInteractable == null) firstInteractable = btn;
            }
            else
            {
                labelTMP.text = tierDescriptions[tierIndex] + "  [LOCKED]";
                btnBG.color = new Color(0.25f, 0.25f, 0.25f, 0.6f); // grey
                btn.interactable = false;
            }

            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.35f, 0.55f, 1f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.25f, 1f);
            colors.selectedColor = new Color(0.35f, 0.35f, 0.55f, 1f);
            colors.disabledColor = btnBG.color;
            btn.colors = colors;
        }

        GameObject closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(boxGO.transform, false);

        RectTransform closeRT = closeBtnGO.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.5f, 0f);
        closeRT.anchorMax = new Vector2(0.5f, 0f);
        closeRT.anchoredPosition = new Vector2(0f, 40f);
        closeRT.sizeDelta = new Vector2(180f, 50f);

        Image closeBG = closeBtnGO.AddComponent<Image>();
        closeBG.color = new Color(0.5f, 0.15f, 0.15f, 0.9f);

        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeBG;
        closeBtn.onClick.AddListener(ClosePanel);

        Navigation closeNav = closeBtn.navigation;
        closeNav.mode = Navigation.Mode.Automatic;
        closeBtn.navigation = closeNav;

        ColorBlock closeColors = closeBtn.colors;
        closeColors.highlightedColor = new Color(0.7f, 0.2f, 0.2f, 1f);
        closeColors.pressedColor = new Color(0.35f, 0.1f, 0.1f, 1f);
        closeColors.selectedColor = new Color(0.7f, 0.2f, 0.2f, 1f);
        closeBtn.colors = closeColors;

        GameObject closeLabelGO = new GameObject("Text");
        closeLabelGO.transform.SetParent(closeBtnGO.transform, false);

        RectTransform closeLabelRT = closeLabelGO.AddComponent<RectTransform>();
        closeLabelRT.anchorMin = Vector2.zero;
        closeLabelRT.anchorMax = Vector2.one;
        closeLabelRT.offsetMin = Vector2.zero;
        closeLabelRT.offsetMax = Vector2.zero;

        TextMeshProUGUI closeLabelTMP = closeLabelGO.AddComponent<TextMeshProUGUI>();
        closeLabelTMP.text = "Close";
        closeLabelTMP.fontSize = 26;
        closeLabelTMP.alignment = TextAlignmentOptions.Center;
        closeLabelTMP.color = Color.white;

        isPanelOpen = true;

        Button toSelect = firstInteractable != null ? firstInteractable : closeBtn;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
    }

    /// <summary>
    /// Closes the upgrade panel.
    /// </summary>
    public void ClosePanel()
    {
        if (panelRoot != null)
        {
            Object.Destroy(panelRoot);
            panelRoot = null;
        }
        isPanelOpen = false;

        if (PauseManager.Instance != null && PauseManager.IsPaused)
            PauseManager.Instance.ResumeGame();
    }

    /// <summary>
    /// Attempts to purchase the specified upgrade tier.
    /// </summary>
    private void PurchaseTier(int tierNumber)
    {
        if (crystalAbility == null) return;

        int currentTier = crystalAbility.GetUpgradeTier();

        // Must purchase in order
        if (tierNumber != currentTier + 1)
        {
            Debug.Log("Must purchase previous tiers first!");
            return;
        }

        // Check CoinsManager exists
        if (CoinsManager.Instance == null)
        {
            Debug.Log("CoinsManager not found!");
            return;
        }

        // Attempt to spend coins (this validates we have enough)
        if (!CoinsManager.Instance.SpendCoins(upgradeCost))
        {
            Debug.Log("Not enough coins for Crystal Skill Upgrade!");
            ShowNotEnoughCoinsMessage();
            return;
        }

        // Apply upgrade
        crystalAbility.SetUpgradeTier(tierNumber);

        Debug.Log("Crystal Skill upgraded to Tier " + tierNumber + "!");

        // Refresh the panel to reflect new state
        ClosePanel();
        OpenPanel();
    }

    /// <summary>
    /// Displays a brief "Not enough coins!" message on the panel.
    /// </summary>
    private void ShowNotEnoughCoinsMessage()
    {
        ClosePanel();
        OpenPanel();

        // Add temporary warning text
        if (panelRoot != null)
        {
            GameObject warningGO = new GameObject("Warning");
            warningGO.transform.SetParent(panelRoot.transform, false);

            RectTransform warnRT = warningGO.AddComponent<RectTransform>();
            warnRT.anchorMin = new Vector2(0.5f, 0f);
            warnRT.anchorMax = new Vector2(0.5f, 0f);
            warnRT.anchoredPosition = new Vector2(0f, 80f);
            warnRT.sizeDelta = new Vector2(500f, 50f);

            TextMeshProUGUI warnTMP = warningGO.AddComponent<TextMeshProUGUI>();
            warnTMP.text = "Not enough coins!";
            warnTMP.fontSize = 32;
            warnTMP.alignment = TextAlignmentOptions.Center;
            warnTMP.color = Color.red;

            // Auto-destroy after 2 seconds
            Object.Destroy(warningGO, 2f);
        }
    }

    private Canvas GetOrCreatePopupCanvas()
    {
        if (popupCanvas != null) return popupCanvas;

        GameObject existing = GameObject.Find("CrystalUpgradeCanvas");
        if (existing != null)
        {
            popupCanvas = existing.GetComponent<Canvas>();
            if (popupCanvas != null) return popupCanvas;
        }

        GameObject canvasGO = new GameObject("CrystalUpgradeCanvas");
        popupCanvas = canvasGO.AddComponent<Canvas>();
        popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        popupCanvas.sortingOrder = 999;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        return popupCanvas;
    }
}
