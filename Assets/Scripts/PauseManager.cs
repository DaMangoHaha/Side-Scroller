using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages pausing and resuming the game. Works across PC (Escape),
/// Gamepad (Start button), and Mobile (on-screen pause button).
/// Attach this to a GameObject in each level scene, or make it persistent.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("Pause Menu UI (assign in Inspector or auto-created)")]
    [Tooltip("Drag your Pause Menu panel here. If left empty, a basic one is created at runtime.")]
    public GameObject pauseMenuPanel;

    [Header("Optional: Mobile Pause Button")]
    [Tooltip("Drag your on-screen pause button here. If left empty, one is created at runtime.")]
    public Button pauseButton;

    [Header("Settings")]
    [Tooltip("Scene name to load when the player chooses Main Menu.")]
    public string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// True when the game is currently paused.
    /// </summary>
    public static bool IsPaused { get; private set; }

    // Input action for pause (Escape / Gamepad Start)
    private InputAction pauseAction;

    // References to auto-created UI (so we can clean up)
    private Canvas autoCanvas;
    private GameObject autoPanel;
    private GameObject autoPauseButton;

    void Awake()
    {
        // Simple singleton — allow one per scene (non-persistent)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        // Create a pause input action that listens to Escape and Gamepad Start
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Gamepad>/start");
        pauseAction.performed += OnPauseInput;
        pauseAction.Enable();

        // Build UI if nothing was assigned in the Inspector
        if (pauseMenuPanel == null)
            CreatePauseMenuUI();

        if (pauseButton == null)
            CreateMobilePauseButton();

        // Make sure the pause menu starts hidden and game is running
        pauseMenuPanel.SetActive(false);
        IsPaused = false;
    }

    void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPauseInput;
            pauseAction.Disable();
            pauseAction.Dispose();
            pauseAction = null;
        }

        // Ensure time is restored if this object is disabled/destroyed
        Time.timeScale = 1f;
        IsPaused = false;
    }

    void OnDestroy()
    {
        // Safety: always restore time scale
        if (IsPaused)
        {
            Time.timeScale = 1f;
            IsPaused = false;
        }
    }

    

    private void OnPauseInput(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    

    /// <summary>
    /// Toggles between paused and unpaused states.
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    /// <summary>
    /// Pauses the game: freezes time, shows the pause menu.
    /// </summary>
    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Resumes the game: restores time, hides the pause menu.
    /// </summary>
    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    /// <summary>
    /// Returns to the main menu scene, restoring time scale first.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Quits the application (no effect in the Editor).
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        Debug.Log("Quit from pause menu.");
        Application.Quit();
    }

   

    /// <summary>
    /// Called by the on-screen pause button (mobile / touch).
    /// </summary>
    public void OnPauseButtonPressed()
    {
        TogglePause();
    }

   
    // These helpers build a minimal but functional pause UI entirely in
    // code so the feature works even before you design a polished prefab.
    

    private Canvas GetOrCreateCanvas()
    {
        if (autoCanvas != null) return autoCanvas;

        // Look for an existing "PauseCanvas" in the scene
        GameObject existing = GameObject.Find("PauseCanvas");
        if (existing != null)
        {
            autoCanvas = existing.GetComponent<Canvas>();
            if (autoCanvas != null) return autoCanvas;
        }

        // Create a new Screen-Space Overlay canvas
        GameObject canvasGO = new GameObject("PauseCanvas");
        autoCanvas = canvasGO.AddComponent<Canvas>();
        autoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        autoCanvas.sortingOrder = 999; // Render on top of everything
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        return autoCanvas;
    }

    private void CreatePauseMenuUI()
    {
        Canvas canvas = GetOrCreateCanvas();

        //  Dark semi-transparent overlay / panel 
        autoPanel = new GameObject("PauseMenuPanel");
        autoPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRT = autoPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelImage = autoPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.7f); // dark overlay

        //  Title
        GameObject titleGO = CreateTextElement(autoPanel.transform, "PAUSED", 64, TextAlignmentOptions.Center);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.anchoredPosition = new Vector2(0f, 140f);
        titleRT.sizeDelta = new Vector2(500f, 80f);

        //  Resume Button 
        CreateMenuButton(autoPanel.transform, "ResumeButton", "Resume", new Vector2(0f, 40f), ResumeGame);

        // Main Menu Button 
        CreateMenuButton(autoPanel.transform, "MainMenuButton", "Main Menu", new Vector2(0f, -40f), GoToMainMenu);

        //  Quit Button 
        CreateMenuButton(autoPanel.transform, "QuitButton", "Quit", new Vector2(0f, -120f), QuitGame);

        pauseMenuPanel = autoPanel;
    }

    private void CreateMobilePauseButton()
    {
        Canvas canvas = GetOrCreateCanvas();

        // Small pause icon in the top-right corner
        autoPauseButton = new GameObject("MobilePauseButton");
        autoPauseButton.transform.SetParent(canvas.transform, false);

        RectTransform btnRT = autoPauseButton.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1f, 1f); // top-right
        btnRT.anchorMax = new Vector2(1f, 1f);
        btnRT.pivot = new Vector2(1f, 1f);
        btnRT.anchoredPosition = new Vector2(-20f, -20f);
        btnRT.sizeDelta = new Vector2(80f, 80f);

        Image btnImage = autoPauseButton.AddComponent<Image>();
        btnImage.color = new Color(1f, 1f, 1f, 0.5f); // semi-transparent white

        Button btn = autoPauseButton.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(OnPauseButtonPressed);

        // "| |" text as a simple pause icon
        GameObject iconText = CreateTextElement(autoPauseButton.transform, "| |", 36, TextAlignmentOptions.Center);
        RectTransform iconRT = iconText.GetComponent<RectTransform>();
        iconRT.anchorMin = Vector2.zero;
        iconRT.anchorMax = Vector2.one;
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        iconText.GetComponent<TextMeshProUGUI>().color = Color.black;

        pauseButton = btn;
    }

    //UI Helper Methods

    private void CreateMenuButton(Transform parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction callback)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300f, 60f);

        Image bg = btnGO.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bg;

        // Hover / press color tint
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.selectedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        btn.colors = colors;

        // Navigation mode set to Automatic so gamepad can navigate between buttons
        Navigation nav = btn.navigation;
        nav.mode = Navigation.Mode.Automatic;
        btn.navigation = nav;

        btn.onClick.AddListener(callback);

        // Label
        GameObject textGO = CreateTextElement(btnGO.transform, label, 28, TextAlignmentOptions.Center);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    private GameObject CreateTextElement(Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        return textGO;
    }
}
