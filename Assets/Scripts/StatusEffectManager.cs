using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages temporary status effects (debuffs) applied to the player by enemies.
/// Attach this component to the Player GameObject.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    // ——— Sticky (Green Slime) ———
    [Header("Sticky — Green Slime")]
    public float stickyDuration = 3f;
    public float jumpForceMultiplier = 0.5f; // halves jump force

    // ——— Burning (Red Slime) ———
    [Header("Burning — Red Slime")]
    public float burningDuration = 5f;
    public float burningDamagePerTick = 5f;
    public float burningTickInterval = 1f; // damage every 1 second

    // ——— Soggy (Blue Slime) ———
    [Header("Soggy — Blue Slime")]
    public float soggyDuration = 5f;
    public float inputDelaySeconds = 0.15f; // small delay before actions register

    // ——— Status Effect Icons ———
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

    // Icon animation coroutines
    private Coroutine stickyIconAnimCoroutine;
    private Coroutine burningIconAnimCoroutine;
    private Coroutine soggyIconAnimCoroutine;

    // Public flags so other scripts can check active debuffs
    [HideInInspector] public bool isSticky = false;
    [HideInInspector] public bool isBurning = false;
    [HideInInspector] public bool isSoggy = false;

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
    // Sticky — halves jump force for a few seconds
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

        float originalJumpForce = doubleJump.jumpForce;
        doubleJump.jumpForce *= jumpForceMultiplier;

        Debug.Log("Sticky! Jump force halved for " + stickyDuration + "s.");

        yield return new WaitForSeconds(stickyDuration);

        // Restore original jump force
        doubleJump.jumpForce = originalJumpForce;
        isSticky = false;
        HideIcon(stickyIcon, ref stickyIconAnimCoroutine);
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

        float elapsed = 0f;

        Debug.Log("Burning! Taking " + burningDamagePerTick + " damage every " + burningTickInterval + "s for " + burningDuration + "s. (Bypasses invulnerability)");

        while (elapsed < burningDuration)
        {
            yield return new WaitForSeconds(burningTickInterval);
            elapsed += burningTickInterval;

            // Show a small burn tick popup each time
            CoinPopup.CreateDamage(transform.position, burningDamagePerTick);

            // Use TakeBurnDamage to bypass invulnerability and skip triggering i-frames
            playerEnergy.TakeBurnDamage(burningDamagePerTick);
        }

        isBurning = false;
        HideIcon(burningIcon, ref burningIconAnimCoroutine);
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

        Debug.Log("Soggy! Input delayed by " + inputDelaySeconds + "s for " + soggyDuration + "s.");

        yield return new WaitForSeconds(soggyDuration);

        isSoggy = false;
        HideIcon(soggyIcon, ref soggyIconAnimCoroutine);
        soggyCoroutine = null;

        Debug.Log("Soggy effect wore off.");
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

        // Clear Burning
        if (burningCoroutine != null)
        {
            StopCoroutine(burningCoroutine);
            burningCoroutine = null;
        }
        isBurning = false;
        HideIcon(burningIcon, ref burningIconAnimCoroutine);

        // Clear Soggy
        if (soggyCoroutine != null)
        {
            StopCoroutine(soggyCoroutine);
            soggyCoroutine = null;
        }
        isSoggy = false;
        HideIcon(soggyIcon, ref soggyIconAnimCoroutine);

        Debug.Log("All status effects cleared.");
    }
}