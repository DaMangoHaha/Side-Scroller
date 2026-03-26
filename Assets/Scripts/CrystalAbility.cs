using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrystalAbility : MonoBehaviour
{
    public int snowflakesNeeded = 5;
    private int currentSnowflakes = 0;

    public bool abilityReady = false;
    public float glaciateDuration = 5f;
    public GameObject glaciateEffectPrefab;

    [Header("Skill Icon")]
    public Image skillIcon;
    public Color fadedColor;
    public Color readyColor;
    public float flickerSpeed = 6f; // how fast icon flickers

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string glaciateDialogue = "The cold will take care of you.";
    public Sprite crystalPortrait;

    [Header("Input (New Input System)")]
    public InputActionReference activateAbilityActionRef; // optional: assign from an Input Actions asset
    private InputAction activateAbilityAction;
    private bool createdLocalAction = false;

    private bool abilityActive = false;
    private PlayerEnergy playerEnergy;

    private Button skillButton;

    // --- Upgrade System ---
    [Header("Upgrade")]
    public int upgradeTier = 0; // 0 = no upgrades, 1-3 = tiers

    // Tier 1: reduce snowflakes needed by 1
    public int snowflakeReduction = 1;

    // Tier 2: extend Glaciate by 1s when collecting a snowflake during activation
    public float snowflakeExtension = 1f;

    // Tier 3: 25% chance to spawn Chill Wind on Glaciate activation
    public float chillWindChance = 0.25f;
    private bool chillWindActive = false; // tracks if a Chill Wind is currently active

    // ——— Chill Wind Buff Icon ———
    [Header("Chill Wind Buff Icon")]
    [Tooltip("UI Image component for the Chill Wind buff icon")]
    public Image chillWindIcon;
    [Tooltip("Sprite to display for Chill Wind buff (optional - uses existing Image sprite if not set)")]
    public Sprite chillWindSprite;

    [Header("Chill Wind Icon Animation")]
    [Tooltip("Should the icon pulse/flash while active?")]
    public bool animateChillWindIcon = true;
    [Tooltip("Speed of the pulse animation")]
    public float chillWindPulseSpeed = 2f;
    [Tooltip("Minimum alpha during pulse")]
    public float chillWindPulseMinAlpha = 0.5f;

    // Icon animation coroutine
    private Coroutine chillWindIconAnimCoroutine;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();

        // Load upgrade tier from save data
        SaveData data = SaveSystem.LoadData();
        upgradeTier = data.crystalSkillUpgradeTier;
        ApplyUpgrades();

        if (skillIcon != null)
        {
            readyColor = skillIcon.color;     // normal visible sprite
            fadedColor = skillIcon.color;
            fadedColor.a = 0.2f;              // faded power-down
            skillIcon.color = fadedColor;

            // Make the skill icon tappable on mobile
            skillButton = skillIcon.GetComponent<Button>();
            if (skillButton == null)
                skillButton = skillIcon.gameObject.AddComponent<Button>();
            skillButton.transition = Selectable.Transition.None;
            skillButton.onClick.AddListener(() => ActivateAbilityInput(true));

            Debug.Log("CrystalAbility: Skill Icon assigned and initialized. Active: " + skillIcon.gameObject.activeInHierarchy);
        }
        else
        {
            Debug.LogWarning("CrystalAbility: Skill Icon is NOT assigned in the Inspector!");
        }

        // Initialize Chill Wind icon - hide it at start
        InitializeChillWindIcon();
    }

    /// <summary>
    /// Sets up the Chill Wind icon with its sprite and hides it initially.
    /// </summary>
    private void InitializeChillWindIcon()
    {
        if (chillWindIcon != null)
        {
            if (chillWindSprite != null)
                chillWindIcon.sprite = chillWindSprite;
            chillWindIcon.gameObject.SetActive(false);

            Debug.Log("CrystalAbility: Chill Wind Icon assigned and hidden at start.");
        }
        else
        {
            Debug.LogWarning("CrystalAbility: Chill Wind Icon is NOT assigned in the Inspector!");
        }
    }

    /// <summary>
    /// Applies upgrade effects based on the current tier.
    /// </summary>
    public void ApplyUpgrades()
    {
        // Tier 1: Glaciate needs one less snowflake
        // (Applied dynamically in CollectSnowflake via GetEffectiveSnowflakesNeeded)

        // Tier 2 & 3 logic is handled at activation / collection time
    }

    /// <summary>
    /// Returns the effective number of snowflakes needed, accounting for Tier 1 upgrade.
    /// </summary>
    private int GetEffectiveSnowflakesNeeded()
    {
        int needed = snowflakesNeeded;
        if (upgradeTier >= 1)
        {
            needed -= snowflakeReduction;
            if (needed < 1) needed = 1; // safety clamp
        }
        return needed;
    }

    void OnEnable()
    {
        // prefer an assigned InputActionReference, otherwise create a simple fallback action
        if (activateAbilityActionRef != null && activateAbilityActionRef.action != null)
        {
            activateAbilityAction = activateAbilityActionRef.action;
        }
        else
        {
            activateAbilityAction = new InputAction("ActivateGlaciate", InputActionType.Button);    
            activateAbilityAction.AddBinding("<Keyboard>/leftShift");
            activateAbilityAction.AddBinding("<Gamepad>/buttonEast");
            createdLocalAction = true;
        }

        if (activateAbilityAction != null)
        {
            activateAbilityAction.performed += OnActivatePerformed;
            activateAbilityAction.Enable();
        }
    }

    void OnDisable()
    {
        if (activateAbilityAction != null)
        {
            activateAbilityAction.performed -= OnActivatePerformed;
            activateAbilityAction.Disable();
        }

        if (createdLocalAction && activateAbilityAction != null)
        {
            activateAbilityAction.Dispose();
            activateAbilityAction = null;
            createdLocalAction = false;
        }
    }

    private void OnActivatePerformed(InputAction.CallbackContext ctx)
    {
        TryActivateGlaciate();
    }

    // Called by mobile UI button tap on skill icon
    public void ActivateAbilityInput(bool pressed)
    {
        if (pressed)
        {
            TryActivateGlaciate();
        }
    }

    private void TryActivateGlaciate()
    {
        if (abilityReady && !abilityActive)
        {
            StartCoroutine(ActivateGlaciate());
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound2D("Glaciate");
        }
    }

    void Update()
    {
        // Legacy fallback if the InputAction isn't assigned/enabled (optional)
        if ((activateAbilityAction == null || !activateAbilityAction.enabled) && Keyboard.current != null)
        {
            if (abilityReady && Keyboard.current.leftShiftKey.wasPressedThisFrame && !abilityActive)
            {
                TryActivateGlaciate();
            }
        }
    }

    public void CollectSnowflake()
    {
        // Tier 2: if Glaciate is active, extend duration instead of counting
        if (abilityActive && upgradeTier >= 2)
        {
            glaciateTimeRemaining += snowflakeExtension;
            Debug.Log("Snowflake collected during Glaciate! Duration extended by " + snowflakeExtension + "s.");
            return;
        }

        if (abilityActive) return;

        currentSnowflakes++;

        if (currentSnowflakes >= GetEffectiveSnowflakesNeeded())
        {
            abilityReady = true;

            if (skillIcon != null)
                skillIcon.color = readyColor;
        }
    }

    // Used by the Glaciate coroutine to track remaining time (for Tier 2 extension)
    private float glaciateTimeRemaining = 0f;

    private IEnumerator ActivateGlaciate()
    {
        abilityActive = true;
        abilityReady = false;
        currentSnowflakes = 0;

        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue(glaciateDialogue, crystalPortrait);
        }

        // Start flicker
        if (skillIcon != null)
            StartCoroutine(FlickerIcon());

        // Spawn mist effect
        GameObject effect = Instantiate(glaciateEffectPrefab, transform.position, Quaternion.identity, transform);

        GlaciateArea glaciate = GetComponentInChildren<GlaciateArea>();
        glaciate.EnableRadius(true);

        // Tier 3: chance to spawn Chill Wind
        if (upgradeTier >= 3 && !chillWindActive)
        {
            float roll = Random.value;
            if (roll <= chillWindChance)
            {
                SpawnChillWind();
            }
            else
            {
                Debug.Log("Chill Wind roll failed (" + (roll * 100f).ToString("F0") + "%). No Chill Wind this time.");
            }
        }
        else if (upgradeTier >= 3 && chillWindActive)
        {
            Debug.Log("Chill Wind is already active. Skipping Chill Wind spawn.");
        }

        // Use glaciateTimeRemaining so Tier 2 can extend it
        glaciateTimeRemaining = glaciateDuration;

        while (glaciateTimeRemaining > 0f)
        {
            glaciateTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        glaciate.EnableRadius(false);
        Destroy(effect);

        abilityActive = false;

        // End flicker ? return to faded
        if (skillIcon != null)
            skillIcon.color = fadedColor;
    }

    /// <summary>
    /// Spawns the Chill Wind buff on Crystal.
    /// </summary>
    private void SpawnChillWind()
    {
        chillWindActive = true;
        ShowChillWindIcon();
        Debug.Log("Chill Wind activated! Crystal gains icy buffs for 25 seconds.");

        // Add or get ChillWind component
        ChillWind wind = GetComponent<ChillWind>();
        if (wind == null)
            wind = gameObject.AddComponent<ChillWind>();

        wind.Activate(this);
    }

    /// <summary>
    /// Called by ChillWind when its duration expires.
    /// </summary>
    public void OnChillWindExpired()
    {
        chillWindActive = false;
        HideChillWindIcon();
        Debug.Log("Chill Wind expired.");
    }

    // --------------------------------------------------
    // Chill Wind Icon Management
    // --------------------------------------------------

    /// <summary>
    /// Shows the Chill Wind buff icon and optionally starts pulsing animation.
    /// </summary>
    private void ShowChillWindIcon()
    {
        if (chillWindIcon == null)
        {
            Debug.LogWarning("CrystalAbility: Cannot show Chill Wind Icon - not assigned!");
            return;
        }

        chillWindIcon.gameObject.SetActive(true);
        Debug.Log("CrystalAbility: Chill Wind Icon shown. Active in hierarchy: " + chillWindIcon.gameObject.activeInHierarchy);

        // Reset alpha to full
        Color c = chillWindIcon.color;
        c.a = 1f;
        chillWindIcon.color = c;

        // Start pulse animation if enabled
        if (animateChillWindIcon)
        {
            if (chillWindIconAnimCoroutine != null)
                StopCoroutine(chillWindIconAnimCoroutine);
            chillWindIconAnimCoroutine = StartCoroutine(PulseChillWindIcon());
        }
    }

    /// <summary>
    /// Hides the Chill Wind buff icon and stops any animation.
    /// </summary>
    private void HideChillWindIcon()
    {
        if (chillWindIcon == null) return;

        // Stop animation
        if (chillWindIconAnimCoroutine != null)
        {
            StopCoroutine(chillWindIconAnimCoroutine);
            chillWindIconAnimCoroutine = null;
        }

        chillWindIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// Pulse animation for the active Chill Wind buff icon.
    /// </summary>
    private IEnumerator PulseChillWindIcon()
    {
        if (chillWindIcon == null) yield break;

        while (true)
        {
            // Pulse from full alpha down to min alpha and back
            float t = (Mathf.Sin(Time.time * chillWindPulseSpeed) + 1f) * 0.5f; // 0 to 1
            float alpha = Mathf.Lerp(chillWindPulseMinAlpha, 1f, t);

            Color c = chillWindIcon.color;
            c.a = alpha;
            chillWindIcon.color = c;

            yield return null;
        }
    }

    /// <summary>
    /// Returns the current upgrade tier.
    /// </summary>
    public int GetUpgradeTier()
    {
        return upgradeTier;
    }

    /// <summary>
    /// Sets the upgrade tier and re-applies effects. Also saves to disk.
    /// </summary>
    public void SetUpgradeTier(int tier)
    {
        upgradeTier = tier;
        ApplyUpgrades();

        // Persist
        SaveData data = SaveSystem.LoadData();
        data.crystalSkillUpgradeTier = tier;
        SaveSystem.SaveData(data);
    }

    private IEnumerator FlickerIcon()
    {
        float t = 0;

        while (abilityActive)
        {
            t += Time.deltaTime * flickerSpeed;

            float alpha = Mathf.Abs(Mathf.Sin(t));   // goes 0 ? 1 ? 0 smoothly
            Color c = readyColor;
            c.a = alpha;

            if (skillIcon != null)
                skillIcon.color = c;

            yield return null;
        }
    }
}

