using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Clear Data Confirmation")]
    [Tooltip("Optional: Assign a custom panel. If left empty, one will be generated automatically.")]
    public GameObject clearDataPanel;
    public float holdDuration = 3f;

    private float clearDataHoldTime = 0f;
    private bool panelShown = false;

    // Buttons that were interactable before the delete panel opened; restored on close.
    private System.Collections.Generic.List<Button> disabledForPanel = new System.Collections.Generic.List<Button>();

    private void Start()
    {
        LoadVolume();
        MusicManager.Instance.PlayMusic("Main Menu");

        // Generate the panel if not assigned
        if (clearDataPanel == null)
        {
            GenerateClearDataPanel();
        }

        // Ensure the panel is hidden at start
        if (clearDataPanel != null)
        {
            clearDataPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Generates a confirmation panel with Yes/No buttons at runtime.
    /// </summary>
    private void GenerateClearDataPanel()
    {
        // Find a scene-local Canvas (skip DontDestroyOnLoad canvases like FadeCanvas/LoadingScreenCanvas)
        Canvas canvas = null;
        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.gameObject.scene.IsValid() && c.gameObject.scene == gameObject.scene)
            {
                canvas = c;
                break;
            }
        }

        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create the panel
        clearDataPanel = new GameObject("ClearDataPanel");
        clearDataPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = clearDataPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Semi-transparent dark background
        Image panelImage = clearDataPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);

        // Create dialog box container
        GameObject dialogBox = new GameObject("DialogBox");
        dialogBox.transform.SetParent(clearDataPanel.transform, false);

        RectTransform dialogRect = dialogBox.AddComponent<RectTransform>();
        dialogRect.sizeDelta = new Vector2(500f, 250f);
        dialogRect.anchoredPosition = Vector2.zero;

        Image dialogImage = dialogBox.AddComponent<Image>();
        dialogImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Create title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(dialogBox.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.6f);
        titleRect.anchorMax = new Vector2(1f, 0.95f);
        titleRect.offsetMin = new Vector2(20f, 0f);
        titleRect.offsetMax = new Vector2(-20f, 0f);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Delete Save Data?";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Create message text
        GameObject messageObj = new GameObject("MessageText");
        messageObj.transform.SetParent(dialogBox.transform, false);

        RectTransform messageRect = messageObj.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0f, 0.35f);
        messageRect.anchorMax = new Vector2(1f, 0.6f);
        messageRect.offsetMin = new Vector2(20f, 0f);
        messageRect.offsetMax = new Vector2(-20f, 0f);

        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "This action cannot be undone!";
        messageText.fontSize = 24;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.color = new Color(1f, 0.6f, 0.6f);

        // Create button container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(dialogBox.transform, false);

        RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
        buttonContainerRect.anchorMin = new Vector2(0f, 0.05f);
        buttonContainerRect.anchorMax = new Vector2(1f, 0.35f);
        buttonContainerRect.offsetMin = Vector2.zero;
        buttonContainerRect.offsetMax = Vector2.zero;

        HorizontalLayoutGroup layoutGroup = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 40f;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;

        // Create Yes button
        CreatePanelButton(buttonContainer.transform, "YesButton", "Yes", new Color(0.8f, 0.2f, 0.2f), OnConfirmClearData);

        // Create No button
        CreatePanelButton(buttonContainer.transform, "NoButton", "No", new Color(0.3f, 0.3f, 0.3f), OnCancelClearData);
    }

    /// <summary>
    /// Helper to create a styled button for the panel.
    /// </summary>
    private void CreatePanelButton(Transform parent, string name, string label, Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(140f, 50f);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = bgColor;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(onClick);

        // Enable gamepad navigation
        Navigation nav = button.navigation;
        nav.mode = Navigation.Mode.Automatic;
        button.navigation = nav;

        // Button highlight / selected colors
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(
            Mathf.Clamp01(bgColor.r + 0.2f),
            Mathf.Clamp01(bgColor.g + 0.2f),
            Mathf.Clamp01(bgColor.b + 0.2f), 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = label;
        buttonText.fontSize = 28;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
    }

    private void Update()
    {
        // Only process if panel is not already shown
        if (!panelShown)
        {
            bool isHolding = CheckKeyboardHold() || CheckGamepadHold() || CheckTouchHold();

            if (isHolding)
            {
                clearDataHoldTime += Time.unscaledDeltaTime;

                if (clearDataHoldTime >= holdDuration)
                {
                    ShowClearDataPanel();
                }
            }
            else
            {
                // Reset timer when input is released
                clearDataHoldTime = 0f;
            }
        }
    }

    /// <summary>
    /// Checks if backspace is being held on keyboard.
    /// </summary>
    private bool CheckKeyboardHold()
    {
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard.backspaceKey.isPressed;
    }

    /// <summary>
    /// Checks if both left and right bumpers are being held on gamepad.
    /// </summary>
    private bool CheckGamepadHold()
    {
        var gamepad = Gamepad.current;
        return gamepad != null && 
               gamepad.leftShoulder.isPressed && 
               gamepad.rightShoulder.isPressed;
    }

    /// <summary>
    /// Checks if the player is touching the screen (not over a UI button).
    /// </summary>
    private bool CheckTouchHold()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null)
            return false;

        // Check if there's at least one active touch
        foreach (var touch in touchscreen.touches)
        {
            if (touch.press.isPressed)
            {
                // Make sure the touch is not over a UI element (button)
                if (!IsTouchOverUI(touch.position.ReadValue()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if the given screen position is over a UI element.
    /// </summary>
    private bool IsTouchOverUI(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    private void ShowClearDataPanel()
    {
        if (clearDataPanel != null)
        {
            clearDataPanel.SetActive(true);
            panelShown = true;
            clearDataHoldTime = 0f;

            // Disable every interactable button that is NOT inside the panel so
            // gamepad navigation cannot drift out to the main-menu buttons.
            disabledForPanel.Clear();
            foreach (Button b in FindObjectsOfType<Button>())
            {
                if (b.interactable && !IsChildOf(b.transform, clearDataPanel.transform))
                {
                    b.interactable = false;
                    disabledForPanel.Add(b);
                }
            }

            // Auto-select the No button (safe default) for gamepad navigation.
            if (EventSystem.current != null)
            {
                Button toSelect = null;
                foreach (Button b in clearDataPanel.GetComponentsInChildren<Button>(false))
                {
                    if (b.name == "NoButton") { toSelect = b; break; }
                }
                if (toSelect == null)
                {
                    Button[] panelBtns = clearDataPanel.GetComponentsInChildren<Button>(false);
                    if (panelBtns.Length > 0) toSelect = panelBtns[0];
                }
                if (toSelect != null)
                    EventSystem.current.SetSelectedGameObject(toSelect.gameObject);
            }
        }
    }

    /// <summary>
    /// Called by the "Yes" button on the clear data confirmation panel.
    /// </summary>
    public void OnConfirmClearData()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ClearAllData();
        }

        HideClearDataPanel();

        // Reload the MainMenu scene to reflect cleared data
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Called by the "No" button on the clear data confirmation panel.
    /// </summary>
    public void OnCancelClearData()
    {
        HideClearDataPanel();
    }

    private void HideClearDataPanel()
    {
        if (clearDataPanel != null)
        {
            clearDataPanel.SetActive(false);
        }
        panelShown = false;

        // Restore all buttons that were disabled when the panel opened.
        foreach (Button b in disabledForPanel)
        {
            if (b != null)
                b.interactable = true;
        }
        disabledForPanel.Clear();

        // Re-select the first restored main-menu button so the gamepad has a
        // valid starting point again (the panel button is now inactive/gone).
        if (EventSystem.current != null)
        {
            // Clear the stale selection first so Unity doesn't try to keep
            // highlighting the now-inactive panel button.
            EventSystem.current.SetSelectedGameObject(null);

            // Find the first interactable button in the scene that is NOT inside
            // the (now hidden) clear-data panel and select it.
            foreach (Button b in FindObjectsOfType<Button>())
            {
                if (b.interactable && !IsChildOf(b.transform, clearDataPanel.transform))
                {
                    EventSystem.current.SetSelectedGameObject(b.gameObject);
                    break;
                }
            }
        }
    }

    /// <summary>Returns true if <paramref name="child"/> is the same as or a descendant of <paramref name="parent"/>.</summary>
    private static bool IsChildOf(Transform child, Transform parent)
    {
        Transform t = child;
        while (t != null)
        {
            if (t == parent) return true;
            t = t.parent;
        }
        return false;
    }

    // Called by Play Button
    public void PlayGame()
    {
        SceneTransition.Instance.LoadScene("LevelSelect");
    }

    // Called by Settings Button
    public void OpenSettings()
    {
        SceneTransition.Instance.LoadScene("Settings");
    }

    // Called by Shop Button
    public void OpenCredits()
    {
        SceneTransition.Instance.LoadScene("Credits");
    }

    // Called by Quit Button (if added later)
    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume()
    {
        float defaultVolume = 0f; // 0 dB = full volume in most AudioMixer setups
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", defaultVolume);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
    }
}

