using System.Collections;
using UnityEngine;

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

    private CrystalAbility crystalAbility;
    private PlayerEnergy playerEnergy;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isActive = false;

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

        Debug.Log("Chill Wind EXPIRED. Buffs removed.");

        isActive = false;

        // Notify CrystalAbility that Chill Wind has expired
        if (crystalAbility != null)
            crystalAbility.OnChillWindExpired();

        // Clean up the component
        Destroy(this);
    }
}
