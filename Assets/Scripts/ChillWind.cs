using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Chill Wind buff for Crystal (Tier 3 upgrade).
/// When activated, grants the following buffs for 25 seconds:
/// - Crystal's max energy is extended by 25%
/// - Crystal's energy depletes 50% slower
/// - Any damage taken is reduced by 20%
/// - Crystal is tinted darker to indicate the buff is active.
/// </summary>
public class ChillWind : MonoBehaviour
{
    [Header("Chill Wind Settings")]
    public float duration = 25f;
    public float maxEnergyMultiplier = 1.25f;     // +25% max energy
    public float depletionRateMultiplier = 0.5f;   // 50% slower depletion
    public float damageReduction = 0.2f;           // 20% damage reduction
    public Color chillTint = new Color(0.5f, 0.6f, 0.8f, 1f); // darker icy tint

    // ——— Chill Wind Buff Icon ———
    [Header("Buff Icon")]
    [Tooltip("UI Image component for the Chill Wind buff icon")]
    public Image chillWindIcon;
    [Tooltip("Sprite to display for Chill Wind buff (optional - uses existing Image sprite if not set)")]
    public Sprite chillWindSprite;

    [Header("Icon Animation Settings")]
    [Tooltip("Should the icon pulse/flash while active?")]
    public bool animateIcon = true;
    [Tooltip("Speed of the pulse animation")]
    public float pulseSpeed = 2f;
    [Tooltip("Minimum alpha during pulse")]
    public float pulseMinAlpha = 0.5f;

    private CrystalAbility crystalAbility;
    private PlayerEnergy playerEnergy;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isActive = false;

    // Icon animation coroutine
    private Coroutine iconAnimCoroutine;

    /// <summary>
    /// Activates the Chill Wind buff.
    /// </summary>
    public void Activate(CrystalAbility ability)
    {
        if (isActive) return;

        crystalAbility = ability;
        playerEnergy = GetComponent<PlayerEnergy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (playerEnergy == null)
        {
            Debug.LogWarning("ChillWind: No PlayerEnergy found on Crystal!");
            return;
        }

        isActive = true;
        StartCoroutine(ChillWindCoroutine());
    }

    private IEnumerator ChillWindCoroutine()
    {
        // Store original values
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Apply buffs
        playerEnergy.maxEnergyMultiplier = maxEnergyMultiplier;
        playerEnergy.depletionRateMultiplier = depletionRateMultiplier;
        playerEnergy.chillWindDamageReduction = damageReduction;
        playerEnergy.UpdateUI();

        // Apply dark tint
        if (spriteRenderer != null)
            spriteRenderer.color = chillTint;

        // Show buff icon
        ShowIcon();

        Debug.Log("Chill Wind ACTIVE: +25% max energy, 50% slower depletion, 20% damage reduction for " + duration + "s.");

        yield return new WaitForSeconds(duration);

        // Remove buffs
        playerEnergy.maxEnergyMultiplier = 1f;
        playerEnergy.depletionRateMultiplier = 1f;
        playerEnergy.chillWindDamageReduction = 0f;

        // Clamp current energy to normal max in case it exceeds
        if (playerEnergy.currentEnergy > playerEnergy.maxEnergy)
            playerEnergy.currentEnergy = playerEnergy.maxEnergy;

        playerEnergy.UpdateUI();

        // Restore original color
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        // Hide buff icon
        HideIcon();

        Debug.Log("Chill Wind EXPIRED. Buffs removed.");

        isActive = false;

        // Notify CrystalAbility that Chill Wind has expired
        if (crystalAbility != null)
            crystalAbility.OnChillWindExpired();

        // Clean up the component
        Destroy(this);
    }

    // --------------------------------------------------
    // Icon Management
    // --------------------------------------------------

    /// <summary>
    /// Shows the Chill Wind buff icon and optionally starts pulsing animation.
    /// </summary>
    private void ShowIcon()
    {
        if (chillWindIcon == null) return;

        // Set sprite if provided
        if (chillWindSprite != null)
            chillWindIcon.sprite = chillWindSprite;

        chillWindIcon.gameObject.SetActive(true);

        // Reset alpha to full
        Color c = chillWindIcon.color;
        c.a = 1f;
        chillWindIcon.color = c;

        // Start pulse animation if enabled
        if (animateIcon)
        {
            if (iconAnimCoroutine != null)
                StopCoroutine(iconAnimCoroutine);
            iconAnimCoroutine = StartCoroutine(PulseIcon());
        }
    }

    /// <summary>
    /// Hides the Chill Wind buff icon and stops any animation.
    /// </summary>
    private void HideIcon()
    {
        if (chillWindIcon == null) return;

        // Stop animation
        if (iconAnimCoroutine != null)
        {
            StopCoroutine(iconAnimCoroutine);
            iconAnimCoroutine = null;
        }

        chillWindIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// Pulse animation for the active buff icon.
    /// </summary>
    private IEnumerator PulseIcon()
    {
        if (chillWindIcon == null) yield break;

        while (true)
        {
            // Pulse from full alpha down to min alpha and back
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0 to 1
            float alpha = Mathf.Lerp(pulseMinAlpha, 1f, t);

            Color c = chillWindIcon.color;
            c.a = alpha;
            chillWindIcon.color = c;

            yield return null;
        }
    }
}
