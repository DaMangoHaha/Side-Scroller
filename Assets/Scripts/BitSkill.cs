using UnityEngine;
using UnityEngine.UI;

public class BitSkill : MonoBehaviour
{
    [Header("Buff Settings")]
    public float buffCooldown = 10f;   // base time to fully charge
    public float warningTime = 3f;     // twinkle before buff
    private float timer = 0f;

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string bitBuffDialogue = "Try and hit me!";
    public Sprite bitPortrait;

    private PlayerEnergy playerEnergy;
    private SpriteRenderer spriteRenderer;
    private bool isWarning = false;

    [Header("UI")]
    public Image shieldIcon; // drag the ShieldIcon here in Inspector
    public Image shieldIcon2; // second stack icon (only visible at Tier 3)
    private Color inactiveColor;
    private Color activeColor;

    [Header("Cooldown Text")]
    public SkillCooldownUI skillCooldownUI;

    [Header("Audio")]
    public AudioClip buffActivateSFX;   // SFX when buff activates
    public AudioClip buffConsumeSFX;    // SFX when buff is consumed
    private AudioSource audioSource;

    [Header("VFX")]
    [Tooltip("Particle prefab spawned as a child of Bits when Bit Buff is at max stacks. Assign a looping particle system.")]
    public GameObject bitBuffAuraVFXPrefab;
    private GameObject activeAuraVFX;

    [Tooltip("One-shot shield VFX prefab that plays for a short duration when a stack activates.")]
    public GameObject bitBuffShieldVFXPrefab;

    [Tooltip("How long the shield VFX lasts before being destroyed (seconds).")]
    public float shieldVFXDuration = 2f;

    // --- Cursed Debuff ---
    private bool isCursedPaused = false;

    // --- Buff Timer Pause ---
    // When at max stacks the cooldown timer freezes until stacks are consumed.
    private bool isBuffPaused = false;

    // --- Upgrade System ---
    [Header("Upgrade")]
    public int upgradeTier = 0; // 0 = no upgrades, 1-3 = tiers

    // Tier 1: cooldown reduction
    private float cooldownReduction = 5f;

    // Tier 2: improved damage reduction (0.4 means player takes 40% = 60% reduction)
    private float tier2DamageReduction = 0.4f;

    // Tier 3: stacking
    private int maxStacks = 1;
    private int currentStacks = 0;
    private float tier3DamageReduction = 0.2f; // at 2 stacks, player takes 20% = 80% reduction

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Load upgrade tier from save data
        SaveData data = SaveSystem.LoadData();
        upgradeTier = data.bitSkillUpgradeTier;
        ApplyUpgrades();

        if (shieldIcon != null)
        {
            activeColor = shieldIcon.color;
            inactiveColor = shieldIcon.color;
            inactiveColor.a = 0.2f; // faded look
            shieldIcon.color = inactiveColor;
        }

        // Hide second stack icon unless Tier 3
        if (shieldIcon2 != null)
        {
            if (upgradeTier >= 3)
            {
                shieldIcon2.gameObject.SetActive(true);
                shieldIcon2.color = inactiveColor;
            }
            else
            {
                shieldIcon2.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Applies upgrade effects based on the current tier.
    /// </summary>
    public void ApplyUpgrades()
    {
        float effectiveCooldown = buffCooldown;

        // Tier 1: Decrease cooldown by 5 seconds
        if (upgradeTier >= 1)
        {
            effectiveCooldown -= cooldownReduction;
            if (effectiveCooldown < warningTime + 1f)
                effectiveCooldown = warningTime + 1f; // safety clamp
        }

        // Tier 2: Improve damage reduction from 50% to 60%
        if (upgradeTier >= 2)
        {
            playerEnergy.damageReduction = tier2DamageReduction;
        }
        else
        {
            playerEnergy.damageReduction = 0.5f; // default 50% reduction
        }

        // Tier 3: Enable stacking up to 2
        if (upgradeTier >= 3)
        {
            maxStacks = 2;
        }
        else
        {
            maxStacks = 1;
        }

        buffCooldown = effectiveCooldown;
    }

    void Update()
    {
        // If cursed or at max stacks, pause the cooldown timer entirely
        if (isCursedPaused || isBuffPaused)
        {
            // Still update UI while paused
            UpdateCooldownUI();
            return;
        }

        timer += Time.deltaTime;

        // Start warning twinkle before buff activates
        if (!isWarning && timer >= buffCooldown - warningTime)
        {
            isWarning = true;
            StartCoroutine(TwinkleBlue());
        }

        // Activate buff when timer is up
        if (timer >= buffCooldown)
        {
            ActivateBuff();
            timer = 0f;
            isWarning = false;
        }

        UpdateCooldownUI();
    }

    void ActivateBuff()
    {
        if (currentStacks < maxStacks)
        {
            currentStacks++;
        }

        // Update PlayerEnergy buff state
        playerEnergy.hasBitBuff = true;
        playerEnergy.bitBuffStacks = currentStacks;

        // Set damage reduction based on stack count
        if (upgradeTier >= 3 && currentStacks >= 2)
        {
            playerEnergy.damageReduction = tier3DamageReduction; // 80% reduction (take 20%)
        }
        else if (upgradeTier >= 2)
        {
            playerEnergy.damageReduction = tier2DamageReduction; // 60% reduction (take 40%)
        }
        else
        {
            playerEnergy.damageReduction = 0.5f; // 50% reduction (take 50%)
        }

        // Play one-shot shield VFX on every activation
        ShowShieldVFX();

        // Pause the cooldown timer when at max stacks
        if (currentStacks >= maxStacks)
        {
            isBuffPaused = true;
            ShowAuraVFX();
            Debug.Log("Bit Buff at max stacks — cooldown timer paused.");
        }

        Debug.Log("Bit Buff Ready! Stacks: " + currentStacks + "/" + maxStacks +
                  " | Damage reduction: " + ((1f - playerEnergy.damageReduction) * 100f) + "%");

        // Show shield icons as active
        UpdateShieldIcons();

        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue(bitBuffDialogue, bitPortrait);
        }

        // Play activation sound
        if (audioSource != null && buffActivateSFX != null)
            audioSource.PlayOneShot(buffActivateSFX);
    }

    /// <summary>
    /// Called by PlayerEnergy when the buff is consumed by taking damage.
    /// All stacks are lost at once.
    /// </summary>
    public void ConsumeBuff()
    {
        currentStacks = 0;

        // Resume the cooldown timer now that stacks have been consumed
        isBuffPaused = false;

        // Hide aura VFX
        HideAuraVFX();

        // Reset icons
        UpdateShieldIcons();

        // Play consumption sound
        if (audioSource != null && buffConsumeSFX != null)
            audioSource.PlayOneShot(buffConsumeSFX);

        Debug.Log("Bit Buff consumed! All stacks lost. Cooldown timer resumed.");
    }

    /// <summary>
    /// Called by StatusEffectManager when the Cursed debuff is applied.
    /// If Bits has stacks, negate damage reduction and remove stacks.
    /// If no stacks, pause the cooldown timer.
    /// </summary>
    public void OnCursed()
    {
        if (currentStacks > 0)
        {
            // Negate Bit Buff: remove all stacks and damage reduction
            Debug.Log("Cursed! Bit Buff stacks negated! Lost " + currentStacks + " stack(s).");
            currentStacks = 0;
            playerEnergy.hasBitBuff = false;
            playerEnergy.bitBuffStacks = 0;
            playerEnergy.damageReduction = 1f; // no damage reduction while cursed

            // Also unpause the buff timer since stacks are gone
            isBuffPaused = false;

            // Hide aura VFX
            HideAuraVFX();

            UpdateShieldIcons();

            if (audioSource != null && buffConsumeSFX != null)
                audioSource.PlayOneShot(buffConsumeSFX);
        }

        // Always pause the cooldown timer while cursed
        isCursedPaused = true;
        Debug.Log("Cursed! Bit Buff cooldown timer paused.");
    }

    /// <summary>
    /// Called by StatusEffectManager when the Cursed debuff wears off.
    /// Resumes the cooldown timer and restores damage reduction settings.
    /// </summary>
    public void OnCurseLifted()
    {
        isCursedPaused = false;

        // Restore proper damage reduction based on upgrade tier
        if (upgradeTier >= 2)
            playerEnergy.damageReduction = tier2DamageReduction;
        else
            playerEnergy.damageReduction = 0.5f;

        Debug.Log("Curse lifted! Bit Buff cooldown timer resumed.");
    }

    /// <summary>
    /// Refreshes the skill icon color to its correct state (called when curse tint is removed).
    /// </summary>
    public void RefreshIconColor()
    {
        UpdateShieldIcons();
    }

    /// <summary>
    /// Updates the shield icon visuals based on current stack count.
    /// </summary>
    private void UpdateShieldIcons()
    {
        // First icon
        if (shieldIcon != null)
        {
            shieldIcon.color = currentStacks >= 1 ? activeColor : inactiveColor;
        }

        // Second icon (Tier 3 only)
        if (shieldIcon2 != null && upgradeTier >= 3)
        {
            shieldIcon2.color = currentStacks >= 2 ? activeColor : inactiveColor;
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

        // Show/hide second stack icon
        if (shieldIcon2 != null)
        {
            if (upgradeTier >= 3)
            {
                shieldIcon2.gameObject.SetActive(true);
                shieldIcon2.color = inactiveColor;
            }
            else
            {
                shieldIcon2.gameObject.SetActive(false);
            }
        }

        // Persist
        SaveData data = SaveSystem.LoadData();
        data.bitSkillUpgradeTier = tier;
        SaveSystem.SaveData(data);
    }

    private System.Collections.IEnumerator TwinkleBlue()
    {
        // Use the saved original color from PlayerEnergy so we never
        // snapshot a color corrupted by an in-progress invulnerability flash.
        Color original = playerEnergy != null
            ? playerEnergy.OriginalColor
            : spriteRenderer.color;

        Color twinkle = Color.blue;

        float flashInterval = 0.3f;
        float elapsed = 0f;

        while (elapsed < warningTime)
        {
            // Set blue tint but preserve the current alpha
            // (invulnerability may be flashing it to 0.5)
            Color c = twinkle;
            c.a = spriteRenderer.color.a;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(flashInterval);

            // Restore original RGB but preserve the current alpha
            c = original;
            c.a = spriteRenderer.color.a;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(flashInterval);

            elapsed += flashInterval * 2;
        }

        // Final restore: original RGB, preserve current alpha
        Color finalColor = original;
        finalColor.a = spriteRenderer.color.a;
        spriteRenderer.color = finalColor;
    }

    private void UpdateCooldownUI()
    {
        if (skillCooldownUI == null) return;

        if (currentStacks >= maxStacks)
        {
            // Buff is fully charged / active
            skillCooldownUI.ShowReady();
        }
        else
        {
            // Show time remaining until next buff
            float remaining = buffCooldown - timer;
            if (remaining < 0f) remaining = 0f;
            skillCooldownUI.ShowCooldown(remaining);
        }
    }

    // ------------------------------------------------------------------
    //  AURA VFX HELPERS
    // ------------------------------------------------------------------

    /// <summary>
    /// Spawns the aura VFX as a child of Bits (follows the player automatically).
    /// Only spawns if the prefab is assigned and no aura is already active.
    /// </summary>
    private void ShowAuraVFX()
    {
        if (bitBuffAuraVFXPrefab == null || activeAuraVFX != null) return;

        activeAuraVFX = Instantiate(bitBuffAuraVFXPrefab, transform.position, Quaternion.identity, transform);
        activeAuraVFX.transform.localPosition = Vector3.zero;

        Debug.Log("Bit Buff aura VFX activated.");
    }

    /// <summary>
    /// Destroys the active aura VFX instance.
    /// </summary>
    private void HideAuraVFX()
    {
        if (activeAuraVFX != null)
        {
            Destroy(activeAuraVFX);
            activeAuraVFX = null;
            Debug.Log("Bit Buff aura VFX deactivated.");
        }
    }

    // ------------------------------------------------------------------
    //  SHIELD VFX HELPERS
    // ------------------------------------------------------------------

    /// <summary>
    /// Plays the one-shot shield VFX at the player's position. This is an
    /// ephemeral effect that is destroyed after `shieldVFXDuration` seconds.
    /// </summary>
    private void ShowShieldVFX()
    {
        if (bitBuffShieldVFXPrefab == null) return;

        GameObject vfx = Instantiate(bitBuffShieldVFXPrefab, transform.position, Quaternion.identity);
        vfx.transform.SetParent(transform); // keep it following the player
        vfx.transform.localPosition = Vector3.zero;

        Destroy(vfx, shieldVFXDuration);
    }
}
