using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CubitPassive : MonoBehaviour
{
    [Header("Protection Protocol Settings")]
    public float energyThreshold = 20f;           // Trigger when energy is below this percentage (base: 20% -> 30% at Tier 0, modified by upgrades)
    public float protectionDuration = 5f;         // Duration of energy pause
    public float protectionCooldown = 45f;        // Cooldown before can trigger again
    public float damageReduction = 0.75f;         // Player takes 75% damage
    public float damageStorage = 0.25f;           // Store 25% of damage
    public float energyConversionRate = 0.10f;    // Convert 10% of stored damage to energy

    [Header("UI")]
    public Image cubitIcon; // drag the CubitIcon here in Inspector
    private Color inactiveColor;
    private Color activeColor;

    [Header("Cooldown Text")]
    public SkillCooldownUI skillCooldownUI;

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string protectionProtocolDialogue = "I am stronger than I look.";
    public Sprite cubitPortrait;

    [Header("Visual Feedback")]
    [Tooltip("Looping VFX prefab spawned around Cubit while Protection Protocol is active.")]
    public GameObject protectionVFXPrefab;

    // --- Upgrade System ---
    [Header("Upgrade")]
    public int upgradeTier = 0; // 0 = no upgrades, 1-3 = tiers

    // Base values stored so upgrades can derive from them
    private float baseCooldown;
    private float baseThreshold;
    private float baseDuration;

    // Tier 1: cooldown reduction
    private float tier1CooldownReduction = 5f;

    // Tier 2: energy threshold increases from 30% to 40%
    private float tier2EnergyThreshold = 40f;

    // Tier 3: protection duration doubles from 5s to 10s
    private float tier3ProtectionDuration = 10f;

    private PlayerEnergy playerEnergy;
    private bool isProtectionReady = true;
    private bool isProtectionActive = false;
    private float storedDamage = 0f;
    private GameObject activeEffect;

    // Cooldown tracking for UI
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        
        if (playerEnergy == null)
        {
            Debug.LogError("CubitPassive requires PlayerEnergy component!");
            enabled = false;
            return;
        }

        // Store base values before upgrades modify them
        baseCooldown = protectionCooldown;
        baseThreshold = energyThreshold;
        baseDuration = protectionDuration;

        // Load upgrade tier from save data
        SaveData data = SaveSystem.LoadData();
        upgradeTier = data.cubitSkillUpgradeTier;
        ApplyUpgrades();

        if (cubitIcon != null)
        {
            activeColor = cubitIcon.color;
            inactiveColor = cubitIcon.color;
            inactiveColor.a = 0.2f; // faded look
            cubitIcon.color = inactiveColor;
        }
    }

    /// <summary>
    /// Applies upgrade effects based on the current tier.
    /// </summary>
    public void ApplyUpgrades()
    {
        float effectiveCooldown = baseCooldown;
        float effectiveThreshold = baseThreshold;
        float effectiveDuration = baseDuration;

        // Tier 1: Decrease cooldown by 5 seconds
        if (upgradeTier >= 1)
        {
            effectiveCooldown -= tier1CooldownReduction;
            if (effectiveCooldown < 5f)
                effectiveCooldown = 5f; // safety clamp
        }

        // Tier 2: Skill activates at 40% energy instead of 30%
        if (upgradeTier >= 2)
        {
            effectiveThreshold = tier2EnergyThreshold;
        }

        // Tier 3: Protection pauses for 10 seconds instead of 5
        if (upgradeTier >= 3)
        {
            effectiveDuration = tier3ProtectionDuration;
        }

        protectionCooldown = effectiveCooldown;
        energyThreshold = effectiveThreshold;
        protectionDuration = effectiveDuration;
    }

    void Update()
    {
        if (playerEnergy == null) return;

        // Track cooldown timer for UI display
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                isOnCooldown = false;
            }
        }

        // Check if energy drops below threshold and protocol is ready
        float energyPercent = (playerEnergy.currentEnergy / playerEnergy.maxEnergy) * 100f;
        
        if (energyPercent < energyThreshold && isProtectionReady && !isProtectionActive)
        {
            StartCoroutine(ActivateProtectionProtocol());
        }

        UpdateCooldownUI();
    }

    private IEnumerator ActivateProtectionProtocol()
    {
        isProtectionActive = true;
        isProtectionReady = false;
        isOnCooldown = false;
        storedDamage = 0f;

        Debug.Log("Protection Protocol ACTIVATED! Duration: " + protectionDuration + "s, Cooldown: " + protectionCooldown + "s, Threshold: " + energyThreshold + "%");

        // Pause energy depletion
        playerEnergy.PauseDepletion(protectionDuration);

        // Spawn looping VFX around Cubit if prefab assigned
        if (protectionVFXPrefab != null && activeEffect == null)
        {
            activeEffect = Instantiate(protectionVFXPrefab, transform.position, Quaternion.identity, transform);
            activeEffect.transform.localPosition = Vector3.zero;
            Debug.Log("Protection Protocol VFX spawned.");
        }

        // Keep icon at active color during protection
        if (cubitIcon != null)
            cubitIcon.color = activeColor;

        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue(protectionProtocolDialogue, cubitPortrait);
        }

        // Play sound effect if available
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("ProtectionProtocol");

        // Wait for protection duration
        yield return new WaitForSeconds(protectionDuration);

        // Convert stored damage to energy
        if (storedDamage > 0)
        {
            float energyToRestore = storedDamage * energyConversionRate;
            playerEnergy.RestoreEnergy(energyToRestore);
            Debug.Log($"Protection Protocol ended. Converted {storedDamage:F1} stored damage to {energyToRestore:F1} energy!");
        }
        else
        {
            Debug.Log("Protection Protocol ended. No damage was stored.");
        }

        // Clean up effect
        if (activeEffect != null)
            Destroy(activeEffect);

        isProtectionActive = false;

        // Set icon to inactive (faded) state during cooldown
        if (cubitIcon != null)
            cubitIcon.color = inactiveColor;

        // Start cooldown — also track for UI
        isOnCooldown = true;
        cooldownTimer = protectionCooldown;

        yield return new WaitForSeconds(protectionCooldown);

        // Ready again - set to active color
        isProtectionReady = true;
        isOnCooldown = false;
        cooldownTimer = 0f;
        if (cubitIcon != null)
            cubitIcon.color = activeColor;

        Debug.Log("Protection Protocol is ready again!");
    }

    // This method intercepts damage and applies Protection Protocol logic
    public float ProcessDamage(float incomingDamage)
    {
        if (!isProtectionActive)
            return incomingDamage; // No protection active, take full damage

        // Store 25% of the damage
        float damageToStore = incomingDamage * damageStorage;
        storedDamage += damageToStore;

        // Take 75% of the damage
        float damageToTake = incomingDamage * damageReduction;

        Debug.Log($"Protection Protocol: Reduced {incomingDamage:F1} damage to {damageToTake:F1}, stored {damageToStore:F1}");

        return damageToTake;
    }

    public bool IsProtectionActive()
    {
        return isProtectionActive;
    }

    public bool IsProtectionReady()
    {
        return isProtectionReady;
    }

    // --- Upgrade helpers ---

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
        data.cubitSkillUpgradeTier = tier;
        SaveSystem.SaveData(data);
    }

    private void UpdateCooldownUI()
    {
        if (skillCooldownUI == null) return;

        if (isProtectionActive)
        {
            skillCooldownUI.ShowUsing();
        }
        else if (isOnCooldown)
        {
            skillCooldownUI.ShowCooldown(cooldownTimer);
        }
        else if (isProtectionReady)
        {
            skillCooldownUI.ShowReady();
        }
    }
}