using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Scrollbook UI on the Main Menu.
///
/// Setup in the "MainMenu" scene:
///   1. Attach this script to an empty GameObject (e.g. "ScrollbookManager").
///   2. Assign the ScrollbookDatabase ScriptableObject in the Inspector.
///   3. Create a small scroll-icon Button in the bottom-left of the title screen
///      and assign it to <see cref="scrollIconButton"/>.
///   4. Create a full-screen Panel called "ScrollInfoPanel" with:
///        - 4 category buttons (Characters, Obstacles, Spritz, The Founders)
///        - A content area with a ScrollRect containing:
///            * A title TMP text
///            * An icon Image
///            * A body TMP text
///        - A list panel (entryListPanel) that holds dynamically-spawned entry buttons
///        - A Back button that calls <see cref="OnBackPressed"/>
///   5. Wire everything in the Inspector and you're done.
///
/// Flow:
///   Scroll Icon ? ScrollInfoPanel (category buttons visible)
///     ? Pick a category ? entry list appears
///       ? Pick an entry ? content area shows the lore
///       ? Back ? returns to entry list or category view
/// </summary>
public class ScrollbookUI : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector References
    // -------------------------------------------------------

    [Header("Database")]
    [Tooltip("Assign the ScrollbookDatabase ScriptableObject here.")]
    public ScrollbookDatabase database;

    [Header("Scroll Icon (Title Screen)")]
    [Tooltip("The small scroll button on the title screen that opens the Scrollbook.")]
    public Button scrollIconButton;

    [Header("Panels")]
    [Tooltip("The full-screen Scroll Info Panel that contains everything.")]
    public GameObject scrollInfoPanel;

    [Tooltip("Panel that holds the category buttons (Characters, Obstacles, Spritz, The Founders).")]
    public GameObject categoryPanel;

    [Tooltip("Panel that holds dynamically-spawned entry buttons for the selected category.")]
    public GameObject entryListPanel;

    [Tooltip("Panel that shows the selected entry's content (title, icon, description).")]
    public GameObject contentPanel;

    [Header("Category Buttons")]
    public Button charactersButton;
    public Button obstaclesButton;
    public Button spritzButton;
    public Button foundersButton;

    [Header("Content Display")]
    [Tooltip("TMP text for the entry title inside the content panel.")]
    public TextMeshProUGUI contentTitle;

    [Tooltip("Image for the entry icon inside the content panel.")]
    public Image contentIcon;

    [Tooltip("TMP text for the entry description inside the content panel.")]
    public TextMeshProUGUI contentDescription;

    [Header("Entry Button Prefab")]
    [Tooltip("A UI Button prefab with a TextMeshProUGUI child. " +
             "If left empty, simple buttons are created at runtime.")]
    public GameObject entryButtonPrefab;

    [Header("Back Button")]
    [Tooltip("A button that navigates backwards through the Scrollbook.")]
    public Button backButton;

    // -------------------------------------------------------
    // Internal State
    // -------------------------------------------------------

    private enum ViewState { Closed, Categories, EntryList, Content }
    private ViewState currentState = ViewState.Closed;
    private ScrollbookEntry[] currentEntries;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    void Start()
    {
        // Make sure panels start hidden
        if (scrollInfoPanel != null)
            scrollInfoPanel.SetActive(false);

        // Wire the scroll icon
        if (scrollIconButton != null)
            scrollIconButton.onClick.AddListener(OpenScrollbook);

        // Wire category buttons
        if (charactersButton != null)
            charactersButton.onClick.AddListener(() => ShowCategory(database.characters));
        if (obstaclesButton != null)
            obstaclesButton.onClick.AddListener(() => ShowCategory(database.obstacles));
        if (spritzButton != null)
            spritzButton.onClick.AddListener(() => ShowCategory(database.spritzHistory));
        if (foundersButton != null)
            foundersButton.onClick.AddListener(() => ShowCategory(database.founders));

        // Wire back button
        if (backButton != null)
            backButton.onClick.AddListener(OnBackPressed);
    }

    // -------------------------------------------------------
    // Public Methods (can also be wired to buttons in Inspector)
    // -------------------------------------------------------

    /// <summary>
    /// Opens the Scrollbook and shows the category selection.
    /// </summary>
    public void OpenScrollbook()
    {
        if (scrollInfoPanel != null)
            scrollInfoPanel.SetActive(true);

        ShowCategoryView();
    }

    /// <summary>
    /// Closes the Scrollbook entirely and returns to the title screen.
    /// </summary>
    public void CloseScrollbook()
    {
        currentState = ViewState.Closed;

        if (scrollInfoPanel != null)
            scrollInfoPanel.SetActive(false);
    }

    /// <summary>
    /// Navigates backward: Content ? Entry List ? Categories ? Close.
    /// </summary>
    public void OnBackPressed()
    {
        switch (currentState)
        {
            case ViewState.Content:
                ShowEntryList(currentEntries);
                break;
            case ViewState.EntryList:
                ShowCategoryView();
                break;
            case ViewState.Categories:
                CloseScrollbook();
                break;
            default:
                CloseScrollbook();
                break;
        }
    }

    // -------------------------------------------------------
    // View Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Displays the four category buttons.
    /// </summary>
    private void ShowCategoryView()
    {
        currentState = ViewState.Categories;

        SetPanelActive(categoryPanel, true);
        SetPanelActive(entryListPanel, false);
        SetPanelActive(contentPanel, false);
    }

    /// <summary>
    /// Called when a category button is pressed.
    /// Populates the entry list for that category.
    /// </summary>
    private void ShowCategory(ScrollbookEntry[] entries)
    {
        currentEntries = entries;
        ShowEntryList(entries);
    }

    /// <summary>
    /// Populates the entry list panel with a button for each entry.
    /// </summary>
    private void ShowEntryList(ScrollbookEntry[] entries)
    {
        currentState = ViewState.EntryList;

        SetPanelActive(categoryPanel, false);
        SetPanelActive(entryListPanel, true);
        SetPanelActive(contentPanel, false);

        // Clear previous buttons
        if (entryListPanel != null)
        {
            foreach (Transform child in entryListPanel.transform)
                Destroy(child.gameObject);
        }

        // Spawn a button for each entry
        if (entries == null) return;

        for (int i = 0; i < entries.Length; i++)
        {
            ScrollbookEntry entry = entries[i];
            GameObject btnGO;

            if (entryButtonPrefab != null)
            {
                btnGO = Instantiate(entryButtonPrefab, entryListPanel.transform);
            }
            else
            {
                btnGO = CreateDefaultEntryButton(entryListPanel.transform);
            }

            // Set the button label
            TextMeshProUGUI label = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = entry.title;

            // Wire click
            Button btn = btnGO.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => ShowEntry(entry));
        }
    }

    /// <summary>
    /// Displays a single lore entry in the content panel.
    /// </summary>
    private void ShowEntry(ScrollbookEntry entry)
    {
        currentState = ViewState.Content;

        SetPanelActive(categoryPanel, false);
        SetPanelActive(entryListPanel, false);
        SetPanelActive(contentPanel, true);

        if (contentTitle != null)
            contentTitle.text = entry.title;

        if (contentDescription != null)
            contentDescription.text = entry.description;

        if (contentIcon != null)
        {
            if (entry.icon != null)
            {
                contentIcon.sprite = entry.icon;
                contentIcon.enabled = true;
            }
            else
            {
                contentIcon.enabled = false;
            }
        }
    }

    // -------------------------------------------------------
    // Utility
    // -------------------------------------------------------

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    /// <summary>
    /// Creates a simple runtime button when no prefab is assigned.
    /// </summary>
    private GameObject CreateDefaultEntryButton(Transform parent)
    {
        GameObject btnGO = new GameObject("EntryButton");
        btnGO.transform.SetParent(parent, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600f, 60f);

        Image bg = btnGO.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bg;

        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        colors.pressedColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        colors.selectedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        btn.colors = colors;

        // Label
        GameObject labelGO = new GameObject("Text");
        labelGO.transform.SetParent(btnGO.transform, false);

        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(10f, 0f);
        labelRT.offsetMax = new Vector2(-10f, 0f);

        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;

        return btnGO;
    }
}
