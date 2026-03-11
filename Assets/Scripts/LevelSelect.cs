using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelect : MonoBehaviour
{
    [Header("Level Buttons")]
    [Tooltip("Optional: assign level buttons to dim their appearance when locked.")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;
    public Button level5Button;

    [Header("Locked Level Panel")]
    [Tooltip("Optional: assign an existing panel in the scene. If left empty, one will be created at runtime.")]
    public GameObject lockedLevelPanel;

    [Tooltip("Message shown when a locked level is clicked.")]
    [TextArea(2, 4)]
    public string lockedMessage = "This level is locked!\nYou must survive Level 1 for at least 1 minute to unlock the other levels.";

    private GameObject runtimePanel;

    void Start()
    {
        // Hide the assigned panel at start (if any)
        if (lockedLevelPanel != null)
            lockedLevelPanel.SetActive(false);

        UpdateButtonVisuals();
    }

    public void PlayLevel1()
    {
        SceneTransition.Instance.LoadScene("Level1", () => MusicManager.Instance.PlayMusic("Level 1"));
    }

    public void PlayLevel2()
    {
        if (!IsLevelUnlocked(2)) { ShowLockedPanel(); return; }
        SceneTransition.Instance.LoadScene("Level2", () => MusicManager.Instance.PlayMusic("Level 2"));
    }

    public void PlayLevel3()
    {
        if (!IsLevelUnlocked(3)) { ShowLockedPanel(); return; }
        SceneTransition.Instance.LoadScene("Level3", () => MusicManager.Instance.PlayMusic("Level 3"));
    }

    public void PlayLevel4()
    {
        if (!IsLevelUnlocked(4)) { ShowLockedPanel(); return; }
        SceneTransition.Instance.LoadScene("Level4", () => MusicManager.Instance.PlayMusic("Level 4"));
    }

    public void PlayLevel5()
    {
        if (!IsLevelUnlocked(5)) { ShowLockedPanel(); return; }
        SceneTransition.Instance.LoadScene("Level5", () => MusicManager.Instance.PlayMusic("Level 5"));
    }

    private bool IsLevelUnlocked(int levelNumber)
    {
        if (LevelUnlockManager.Instance == null) return true;

        switch (levelNumber)
        {
            case 1: return LevelUnlockManager.Instance.level1Unlocked;
            case 2: return LevelUnlockManager.Instance.level2Unlocked;
            case 3: return LevelUnlockManager.Instance.level3Unlocked;
            case 4: return LevelUnlockManager.Instance.level4Unlocked;
            default: return false;
        }
    }

    // -------------------------------------------------------
    // Button Visuals
    // -------------------------------------------------------

    /// <summary>
    /// Dims locked buttons visually but keeps them interactable so the player can click them
    /// and see the locked message.
    /// </summary>
    private void UpdateButtonVisuals()
    {
        SetButtonLockedVisual(level1Button, IsLevelUnlocked(1));
        SetButtonLockedVisual(level2Button, IsLevelUnlocked(2));
        SetButtonLockedVisual(level3Button, IsLevelUnlocked(3));
        SetButtonLockedVisual(level4Button, IsLevelUnlocked(4));
        SetButtonLockedVisual(level5Button, IsLevelUnlocked(5));
    }

    private void SetButtonLockedVisual(Button button, bool unlocked)
    {
        if (button == null) return;

        // Keep the button interactable so the player can click it and see the message
        button.interactable = true;

        // Dim the button colors when locked to give a visual hint
        if (!unlocked)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            colors.highlightedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            colors.pressedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.selectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            button.colors = colors;
        }
    }

    // -------------------------------------------------------
    // Locked Level Panel
    // -------------------------------------------------------

    private void ShowLockedPanel()
    {
        // Use designer-assigned panel if available
        if (lockedLevelPanel != null)
        {
            lockedLevelPanel.SetActive(true);
            return;
        }

        // Build one at runtime
        if (runtimePanel != null) return;

        Canvas canvas = GetOrCreatePopupCanvas();

        // Dark overlay
        runtimePanel = new GameObject("LockedLevelPanel");
        runtimePanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRT = runtimePanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelBG = runtimePanel.AddComponent<Image>();
        panelBG.color = new Color(0f, 0f, 0f, 0.75f);

        // Message box
        GameObject boxGO = new GameObject("MessageBox");
        boxGO.transform.SetParent(runtimePanel.transform, false);

        RectTransform boxRT = boxGO.AddComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0.5f, 0.5f);
        boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.anchoredPosition = Vector2.zero;
        boxRT.sizeDelta = new Vector2(700f, 260f);

        Image boxBG = boxGO.AddComponent<Image>();
        boxBG.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);

        // Message text
        GameObject msgGO = new GameObject("MessageText");
        msgGO.transform.SetParent(boxGO.transform, false);

        RectTransform msgRT = msgGO.AddComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0.5f, 0.5f);
        msgRT.anchorMax = new Vector2(0.5f, 0.5f);
        msgRT.anchoredPosition = new Vector2(0f, 30f);
        msgRT.sizeDelta = new Vector2(620f, 120f);

        TextMeshProUGUI msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
        msgTMP.text = lockedMessage;
        msgTMP.fontSize = 32;
        msgTMP.alignment = TextAlignmentOptions.Center;
        msgTMP.color = Color.white;
        msgTMP.enableWordWrapping = true;

        // OK button
        GameObject btnGO = new GameObject("OKButton");
        btnGO.transform.SetParent(boxGO.transform, false);

        RectTransform btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(0f, -80f);
        btnRT.sizeDelta = new Vector2(180f, 50f);

        Image btnBG = btnGO.AddComponent<Image>();
        btnBG.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        Button okBtn = btnGO.AddComponent<Button>();
        okBtn.targetGraphic = btnBG;

        ColorBlock btnColors = okBtn.colors;
        btnColors.highlightedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        btnColors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        btnColors.selectedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        okBtn.colors = btnColors;

        okBtn.onClick.AddListener(DismissLockedPanel);

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

    public void DismissLockedPanel()
    {
        if (lockedLevelPanel != null)
        {
            lockedLevelPanel.SetActive(false);
            return;
        }

        if (runtimePanel != null)
        {
            Destroy(runtimePanel);
            runtimePanel = null;
        }
    }

    private Canvas popupCanvas;

    private Canvas GetOrCreatePopupCanvas()
    {
        if (popupCanvas != null) return popupCanvas;

        GameObject existing = GameObject.Find("LevelSelectPopupCanvas");
        if (existing != null)
        {
            popupCanvas = existing.GetComponent<Canvas>();
            if (popupCanvas != null) return popupCanvas;
        }

        GameObject canvasGO = new GameObject("LevelSelectPopupCanvas");
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

