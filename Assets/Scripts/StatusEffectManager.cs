using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages temporary status effects (debuffs) applied to the player by enemies.
/// Attach this component to the Player GameObject.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    // ??? Sticky (Green Slime) ???
    [Header("Sticky — Green Slime")]
    public float stickyDuration = 3f;
    public float jumpForceMultiplier = 0.5f; // halves jump force

    // ??? Burning (Red Slime) ???
    [Header("Burning — Red Slime")]
    public float burningDuration = 5f;
    public float burningDamagePerTick = 5f;
    public float burningTickInterval = 1f; // damage every 1 second

    // ??? Soggy (Blue Slime) ???
    [Header("Soggy — Blue Slime")]
    public float soggyDuration = 5f;
    public float inputDelaySeconds = 0.15f; // small delay before actions register

    // ??? Cursed (Magic Bolts) ???
    [Header("Cursed — Magic Bolts")]
    public float cursedDuration = 7f;
    [Tooltip("Chance (0-1) for a magic bolt to inflict Cursed on hit")]
    public float cursedChance = 0.25f;

    // ??? Status Effect Icons ???
    [Header("Status Effect Icons")]
    [Tooltip("UI Image component for the Sticky debuff icon")]
    public Image stickyIcon;
    [Tooltip("Sprite to display for Sticky debuff (optional - uses existing Image sprite if not set)")]
    public Sprite stickySprite;

    [Tooltip("UI Image component for the Burning debuff icon")]
    public Image burningIcon;
    [Tooltip("Sprite to display for Burning debuff (optional - uses existing Image sprite if not set)")]
    public Sprite burningSprite;

    [Tooltip("UI Image component for the Soggy debuff icon")]
    public Image soggyIcon;
    [Tooltip("Sprite to display for Soggy debuff (optional - uses existing Image sprite if not set)")]
    public Sprite soggySprite;

    [Tooltip("UI Image component for the Cursed debuff icon")]
    public Image cursedIcon;
    [Tooltip("Sprite to display for Cursed debuff (optional - uses existing Image sprite if not set)")]
    public Sprite cursedSprite;

    [Header("Status Effect VFX")]
    [Tooltip("VFX prefab spawned as child of player while Sticky is active")]
    public GameObject stickyVFXPrefab;
    [Tooltip("VFX prefab spawned as child of player while Burning is active")]
    public GameObject burningVFXPrefab;
    [Tooltip("VFX prefab spawned as child of player while Soggy is active")]
    public GameObject soggyVFXPrefab;
    [Tooltip("VFX prefab spawned as child of player while Cursed is active")]
    public GameObject cursedVFXPrefab;

    private GameObject activeStickyVFX;
    private GameObject activeBurningVFX;
    private GameObject activeSoggyVFX;
    private GameObject activeCursedVFX;

    [Header("Icon Animation Settings")]
    [Tooltip("Should icons pulse/flash while active?")]
    public bool animateIcons = true;
    [Tooltip("Speed of the pulse animation")]
    public float pulseSpeed = 2f;
    [Tooltip("Minimum alpha during pulse")]
    public float pulseMinAlpha = 0.5f;

    // Cached player components
    private DoubleJump doubleJump;
    private PlayerEnergy playerEnergy;
    private PlayerFreeMove playerFreeMove;
    private PlayerSlide playerSlide;

    // Track active debuffs so we can prevent stacking
    private Coroutine stickyCoroutine;
    private Coroutine burningCoroutine;
    private Coroutine soggyCoroutine;
    private Coroutine cursedCoroutine;

    // Icon animation coroutines
    private Coroutine stickyIconAnimCoroutine;
    private Coroutine burningIconAnimCoroutine;
    private Coroutine soggyIconAnimCoroutine;
    private Coroutine cursedIconAnimCoroutine;

    // Public flags so other scripts can check active debuffs
    [HideInInspector] public bool isSticky = false;
    [HideInInspector] public bool isBurning = false;
    [HideInInspector] public bool isSoggy = false;
    [HideInInspector] public bool isCursed = false;

    void Awake()
    {
        doubleJump = GetComponent<DoubleJump>();
        playerEnergy = GetComponent<PlayerEnergy>();
        playerFreeMove = GetComponent<PlayerFreeMove>();
        playerSlide = GetComponent<PlayerSlide>();

        // Initialize icons - hide them at start
        InitializeIcons();
    }

    /// <summary>
    /// Sets up icons with their sprites and hides them initially.
    /// </summary>
    private void InitializeIcons()
    {
        // Sticky icon setup
        if (stickyIcon != null)
        {
            if (stickySprite != null)
                stickyIcon.sprite = stickySprite;
            stickyIcon.gameObject.SetActive(false);
        }

        // Burning icon setup
        if (burningIcon != null)
        {
            if (burningSprite != null)
                burningIcon.sprite = burningSprite;
            burningIcon.gameObject.SetActive(false);
        }

        // Soggy icon setup
        if (soggyIcon != null)
        {
            if (soggySprite != null)
                soggyIcon.sprite = soggySprite;
            soggyIcon.gameObject.SetActive(false);
        }

        // Cursed icon setup
        if (cursedIcon != null)
        {
            if (cursedSprite != null)
                cursedIcon.sprite = cursedSprite;
            cursedIcon.gameObject.SetActive(false);
        }
    }

    // --------------------------------------------------
    // Icon Management
    // --------------------------------------------------

    /// <summary>
    /// Shows a status effect icon and optionally starts pulsing animation.
    /// </summary>
    private void ShowIcon(Image icon, ref Coroutine animCoroutine)
    {
        if (icon == null) return;

        icon.gameObject.SetActive(true);

        // Reset alpha to full
        Color c = icon.color;
        c.a = 1f;
        icon.color = c;

        // Start pulse animation if enabled
        if (animateIcons)
        {
            if (animCoroutine != null)
                StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(PulseIcon(icon));
        }
    }

    /// <summary>
    /// Hides a status effect icon and stops any animation.
    /// </summary>
    private void HideIcon(Image icon, ref Coroutine animCoroutine)
    {
        if (icon == null) return;

        // Stop animation
        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }

        icon.gameObject.SetActive(false);
    }

    /// <summary>
    /// Pulse animation for active status icons.
    /// </summary>
    private IEnumerator PulseIcon(Image icon)
    {
        if (icon == null) yield break;

        while (true)
        {
            // Pulse from full alpha down to min alpha and back
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0 to 1
            float alpha = Mathf.Lerp(pulseMinAlpha, 1f, t);

            Color c = icon.color;
            c.a = alpha;
            icon.color = c;

            yield return null;
        }
    }

    // --------------------------------------------------
    // Status Effect VFX Helpers
    // --------------------------------------------------

    private void ShowStatusVFX(GameObject prefab, ref GameObject activeInstance)
    {
        if (prefab == null || activeInstance != null) return;
        activeInstance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        activeInstance.transform.localPosition = Vector3.zero;

        // Counteract the parent's world scale so the VFX always renders at its
        // intended size regardless of which character's transform it is parented to.
        Vector3 ps = transform.lossyScale;
        if (ps.x != 0f && ps.y != 0f && ps.z != 0f)
        {
            activeInstance.transform.localScale = new Vector3(
                1f / ps.x,
                1f / ps.y,
                1f / ps.z
            );
        }
    }

    private void HideStatusVFX(ref GameObject activeInstance)
    {
        if (activeInstance != null)
        {
            Destroy(activeInstance);
            activeInstance = null;
        }
    }

    // --------------------------------------------------
    // Sticky
    // --------------------------------------------------

    public void ApplySticky(Vector3 popupPosition)
    {
        CoinPopup.CreateStatusEffect(popupPosition, "Sticky!", new Color(0.2f, 0.9f, 0.2f, 1f)); // green

        // Restart the debuff if already active (refresh duration)
        if (stickyCoroutine != null)
            StopCoroutine(stickyCoroutine);

        stickyCoroutine = StartCoroutine(StickyRoutine());
    }

    private IEnumerator StickyRoutine()
    {
        if (doubleJump == null) yield break;

        isSticky = true;
        ShowIcon(stickyIcon, ref stickyIconAnimCoroutine);
        ShowStatusVFX(stickyVFXPrefab, ref activeStickyVFX);

        float originalJumpForce = doubleJump.jumpForce;
        doubleJump.jumpForce *= jumpForceMultiplier;

        Debug.Log("Sticky! Jump force halved for " + stickyDuration + "s.");

        yield return new WaitForSeconds(stickyDuration);

        doubleJump.jumpForce = originalJumpForce;
        isSticky = false;
        HideIcon(stickyIcon, ref stickyIconAnimCoroutine);
        HideStatusVFX(ref activeStickyVFX);
        stickyCoroutine = null;

        Debug.Log("Sticky effect wore off.");
    }

    // --------------------------------------------------
    // Burning — deals damage over time (bypasses invulnerability)
    // --------------------------------------------------
    public void ApplyBurning(Vector3 popupPosition)
    {
        CoinPopup.CreateStatusEffect(popupPosition, "Burning!", new Color(1f, 0.5f, 0f, 1f)); // orange

        // Restart the debuff if already active (refresh duration)
        if (burningCoroutine != null)
            StopCoroutine(burningCoroutine);

        burningCoroutine = StartCoroutine(BurningRoutine());
    }

    private IEnumerator BurningRoutine()
    {
        if (playerEnergy == null) yield break;

        isBurning = true;
        ShowIcon(burningIcon, ref burningIconAnimCoroutine);
        ShowStatusVFX(burningVFXPrefab, ref activeBurningVFX);

        float elapsed = 0f;

        Debug.Log("Burning! Taking " + burningDamagePerTick + " damage every " + burningTickInterval + "s for " + burningDuration + "s. (Bypasses invulnerability)");

        while (elapsed < burningDuration)
        {
            yield return new WaitForSeconds(burningTickInterval);
            elapsed += burningTickInterval;

            CoinPopup.CreateDamage(transform.position, burningDamagePerTick);
            playerEnergy.TakeBurnDamage(burningDamagePerTick);
        }

        isBurning = false;
        HideIcon(burningIcon, ref burningIconAnimCoroutine);
        HideStatusVFX(ref activeBurningVFX);
        burningCoroutine = null;

        Debug.Log("Burning effect wore off.");
    }

    // --------------------------------------------------
    // Soggy — adds input delay to all actions
    // --------------------------------------------------
    public void ApplySoggy(Vector3 popupPosition)
    {
        CoinPopup.CreateStatusEffect(popupPosition, "Soggy!", new Color(0.2f, 0.5f, 1f, 1f)); // blue

        // Restart the debuff if already active (refresh duration)
        if (soggyCoroutine != null)
            StopCoroutine(soggyCoroutine);

        soggyCoroutine = StartCoroutine(SoggyRoutine());
    }

    private IEnumerator SoggyRoutine()
    {
        isSoggy = true;
        ShowIcon(soggyIcon, ref soggyIconAnimCoroutine);
        ShowStatusVFX(soggyVFXPrefab, ref activeSoggyVFX);

        Debug.Log("Soggy! Input delayed by " + inputDelaySeconds + "s for " + soggyDuration + "s.");

        yield return new WaitForSeconds(soggyDuration);

        isSoggy = false;
        HideIcon(soggyIcon, ref soggyIconAnimCoroutine);
        HideStatusVFX(ref activeSoggyVFX);
        soggyCoroutine = null;

        Debug.Log("Soggy effect wore off.");
    }

    // --------------------------------------------------
    // Cursed — disables/disrupts the player's skill for 7 seconds
    // --------------------------------------------------

    /// <summary>
    /// Attempts to apply the Cursed debuff. Call this from magic bolt scripts.
    /// Cubit is immune to the Cursed debuff.
    /// </summary>
    public void ApplyCursed(Vector3 popupPosition)
    {
        // Cubit is immune to Cursed
        CubitPassive cubitPassive = GetComponent<CubitPassive>();
        if (cubitPassive != null)
        {
            CoinPopup.CreateStatusEffect(popupPosition, "Immune!", new Color(0.5f, 1f, 0.5f, 1f)); // light green
            Debug.Log("Cubit is immune to the Cursed debuff!");
            return;
        }

        CoinPopup.CreateStatusEffect(popupPosition, "Cursed!", new Color(0.6f, 0.2f, 0.9f, 1f)); // purple

        // Restart the debuff if already active (refresh duration)
        if (cursedCoroutine != null)
            StopCoroutine(cursedCoroutine);

        cursedCoroutine = StartCoroutine(CursedRoutine());
    }

    /// <summary>
    /// Rolls a 25% chance and applies Cursed if successful.
    /// Convenience method for magic bolt scripts to call.
    /// </summary>
    public bool TryApplyCursed(Vector3 popupPosition)
    {
        if (Random.value <= cursedChance)
        {
            ApplyCursed(popupPosition);
            return true;
        }
        return false;
    }

    private IEnumerator CursedRoutine()
    {
        isCursed = true;
        ShowIcon(cursedIcon, ref cursedIconAnimCoroutine);
        ShowStatusVFX(cursedVFXPrefab, ref activeCursedVFX);

        // Tint the cursed icon purple
        if (cursedIcon != null)
        {
            cursedIcon.color = new Color(0.6f, 0.2f, 0.9f, 1f);
        }

        // Play/Wire Cursed VFX
        if (cursedVFXPrefab != null && activeCursedVFX == null)
        {
            activeCursedVFX = Instantiate(cursedVFXPrefab, transform);
            var ps = activeCursedVFX.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(activeCursedVFX, ps.main.duration); // Auto-destroy based on particle system duration
        }

        Debug.Log("Cursed! Player's skill is disrupted for " + cursedDuration + "s.");

        // --- Apply character-specific curse effects ---

        // Bits: negate Bit Buff stacks and damage reduction
        BitSkill bitSkill = GetComponent<BitSkill>();
        if (bitSkill != null)
        {
            bitSkill.OnCursed();
        }

        // Thief: cancel active Sticky Fingers, pause cooldown
        ThiefSkill thiefSkill = GetComponent<ThiefSkill>();
        if (thiefSkill != null)
        {
            thiefSkill.OnCursed();
        }

        // Ninja: disable skill activation, pause cooldown
        NinjaSkill_ElectricBolt ninjaSkill = GetComponent<NinjaSkill_ElectricBolt>();
        if (ninjaSkill != null)
        {
            ninjaSkill.OnCursed();
        }

        // Wiz Kid: pause cooldown timer, delay activation until curse ends
        WizKidSkill wizKidSkill = GetComponent<WizKidSkill>();
        if (wizKidSkill != null)
        {
            wizKidSkill.OnCursed();
        }

        // Crystal: pause snowflake spawning, cancel active Glaciate
        CrystalAbility crystalAbility = GetComponent<CrystalAbility>();
        if (crystalAbility != null)
        {
            crystalAbility.OnCursed();
        }

        // Tint the character's skill icon purple while cursed
        TintSkillIconPurple(true);

        yield return new WaitForSeconds(cursedDuration);

        // --- Remove curse effects ---
        isCursed = false;

        if (bitSkill != null)
            bitSkill.OnCurseLifted();

        if (thiefSkill != null)
            thiefSkill.OnCurseLifted();

        if (ninjaSkill != null)
            ninjaSkill.OnCurseLifted();

        if (wizKidSkill != null)
            wizKidSkill.OnCurseLifted();

        if (crystalAbility != null)
            crystalAbility.OnCurseLifted();

        // Restore skill icon tint
        TintSkillIconPurple(false);

        HideIcon(cursedIcon, ref cursedIconAnimCoroutine);
        HideStatusVFX(ref activeCursedVFX);
        cursedCoroutine = null;

        Debug.Log("Cursed effect wore off.");
    }

    /// <summary>
    /// Tints the active character's skill icon purple (when cursed) or restores it.
    /// </summary>
    private void TintSkillIconPurple(bool tint)
    {
        Color purpleTint = new Color(0.6f, 0.2f, 0.9f, 1f);

        // Check each possible skill and tint its icon
        BitSkill bitSkill = GetComponent<BitSkill>();
        if (bitSkill != null && bitSkill.shieldIcon != null)
        {
            bitSkill.shieldIcon.color = tint ? purpleTint : (bitSkill.shieldIcon.color); // BitSkill manages its own colors
            if (!tint) bitSkill.RefreshIconColor();
        }

        ThiefSkill thiefSkill = GetComponent<ThiefSkill>();
        if (thiefSkill != null && thiefSkill.skillIcon != null)
        {
            if (tint)
                thiefSkill.skillIcon.color = purpleTint;
            else
                thiefSkill.RefreshIconColor();
        }

        NinjaSkill_ElectricBolt ninjaSkill = GetComponent<NinjaSkill_ElectricBolt>();
        if (ninjaSkill != null && ninjaSkill.readyIcon != null)
        {
            if (tint)
                ninjaSkill.readyIcon.color = purpleTint;
            else
                ninjaSkill.RefreshIconColor();
        }

        WizKidSkill wizKidSkill = GetComponent<WizKidSkill>();
        if (wizKidSkill != null && wizKidSkill.wizIcon != null)
        {
            if (tint)
                wizKidSkill.wizIcon.color = purpleTint;
            else
                wizKidSkill.RefreshIconColor();
        }

        CrystalAbility crystalAbility = GetComponent<CrystalAbility>();
        if (crystalAbility != null && crystalAbility.skillIcon != null)
        {
            if (tint)
                crystalAbility.skillIcon.color = purpleTint;
            else
                crystalAbility.RefreshIconColor();
        }
    }

    /// <summary>
    /// Returns the current input delay in seconds (0 if no Soggy debuff is active).
    /// Called by DoubleJump, PlayerFreeMove, and PlayerSlide to throttle input.
    /// </summary>
    public float GetInputDelay()
    {
        return isSoggy ? inputDelaySeconds : 0f;
    }

    /// <summary>
    /// Clears all active status effects. Useful when respawning or transitioning scenes.
    /// </summary>
    public void ClearAllEffects()
    {
        // Clear Sticky
        if (stickyCoroutine != null)
        {
            StopCoroutine(stickyCoroutine);
            stickyCoroutine = null;
        }
        if (doubleJump != null && isSticky)
        {
            // Note: This won't restore jump force perfectly if called mid-effect
            // Consider storing original value at Start() if this becomes an issue
        }
        isSticky = false;
        HideIcon(stickyIcon, ref stickyIconAnimCoroutine);
        HideStatusVFX(ref activeStickyVFX); // <- Hide Sticky VFX

        // Clear Burning
        if (burningCoroutine != null)
        {
            StopCoroutine(burningCoroutine);
            burningCoroutine = null;
        }
        isBurning = false;
        HideIcon(burningIcon, ref burningIconAnimCoroutine);
        HideStatusVFX(ref activeBurningVFX); // <- Hide Burning VFX

        // Clear Soggy
        if (soggyCoroutine != null)
        {
            StopCoroutine(soggyCoroutine);
            soggyCoroutine = null;
        }
        isSoggy = false;
        HideIcon(soggyIcon, ref soggyIconAnimCoroutine);
        HideStatusVFX(ref activeSoggyVFX); // <- Hide Soggy VFX

        // Clear Cursed
        if (cursedCoroutine != null)
        {
            StopCoroutine(cursedCoroutine);
            cursedCoroutine = null;
        }
        if (isCursed)
        {
            // Lift curse from all skills
            BitSkill bitSkill = GetComponent<BitSkill>();
            if (bitSkill != null) bitSkill.OnCurseLifted();

            ThiefSkill thiefSkill = GetComponent<ThiefSkill>();
            if (thiefSkill != null) thiefSkill.OnCurseLifted();

            NinjaSkill_ElectricBolt ninjaSkill = GetComponent<NinjaSkill_ElectricBolt>();
            if (ninjaSkill != null) ninjaSkill.OnCurseLifted();

            WizKidSkill wizKidSkill = GetComponent<WizKidSkill>();
            if (wizKidSkill != null) wizKidSkill.OnCurseLifted();

            CrystalAbility crystalAbility = GetComponent<CrystalAbility>();
            if (crystalAbility != null) crystalAbility.OnCurseLifted();

            TintSkillIconPurple(false);
        }
        isCursed = false;
        HideIcon(cursedIcon, ref cursedIconAnimCoroutine);
        HideStatusVFX(ref activeCursedVFX); // <- Hide Cursed VFX

        Debug.Log("All status effects cleared.");
    }

    // --------------------------------------------------
    // VFX Management
    // --------------------------------------------------

    /// <summary>
    /// Hides the active Sticky VFX if present.
    /// </summary>
    private void HideStickyVFX()
    {
        if (activeStickyVFX != null)
        {
            Destroy(activeStickyVFX);
            activeStickyVFX = null;
        }
    }

    /// <summary>
    /// Hides the active Burning VFX if present.
    /// </summary>
    private void HideBurningVFX()
    {
        if (activeBurningVFX != null)
        {
            Destroy(activeBurningVFX);
            activeBurningVFX = null;
        }
    }

    /// <summary>
    /// Hides the active Soggy VFX if present.
    /// </summary>
    private void HideSoggyVFX()
    {
        if (activeSoggyVFX != null)
        {
            Destroy(activeSoggyVFX);
            activeSoggyVFX = null;
        }
    }

    /// <summary>
    /// Hides the active Cursed VFX if present.
    /// </summary>
    private void HideCursedVFX()
    {
        if (activeCursedVFX != null)
        {
            Destroy(activeCursedVFX);
            activeCursedVFX = null;
        }
    }
}