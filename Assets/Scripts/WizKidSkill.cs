using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WizKidSkill : MonoBehaviour
{
    [Header("Settings")]
    public float cooldown = 30f;
    public float tickInterval = 0.5f;

    [Header("Healing Amounts")]
    public float smallHeal = 1.5f;
    public float mediumHeal = 3f;
    public float largeHeal = 6f;

    [Header("Durations")]
    public float smallDuration = 3f;
    public float mediumDuration = 5f;
    public float largeDuration = 7f;

    [Header("UI")]
    public Image wizIcon;
    private Color inactiveColor;
    private Color activeColor;

    [Header("Cooldown Text")]
    public SkillCooldownUI skillCooldownUI;

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string sproutingSorceryDialogue = "My spellbook taught me this one!";
    public Sprite wizKidPortrait;

    [Header("Effects")]
    public GameObject confettiPrefab;
    public GameObject auraPrefab;

    // Runtime reference to the spawned aura instance
    private GameObject auraInstance;

    // --- Upgrade System ---
    [Header("Upgrade")]
    public int upgradeTier = 0; // 0 = no upgrades, 1-3 = tiers

    // Tier 1: cooldown reduction
    private float tier1CooldownReduction = 3f;

    // Tier 2: energy bonus & energy loss chance
    private float tier2EnergyBonus = 0.10f;       // +10% healing per tick
    private float tier2EnergyLossPercent = 0.15f;  // lose 15% of current energy

    // Tier 3: expanded effect pool
    private int tier3CoinReward = 50;
    private float tier3InvulnerabilityDuration = 5f;

    // Base cooldown stored so upgrades can derive from it
    private float baseCooldown;

    private PlayerEnergy playerEnergy;
    private float timer = 0f;

    // --- Cursed Debuff ---
    private bool isCursedPaused = false;
    private bool wasReadyWhenCursed = false;

    // Skill active tracking
    private bool isSkillActive = false;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        baseCooldown = cooldown;

        // Load upgrade tier from save data
        SaveData data = SaveSystem.LoadData();
        upgradeTier = data.wizKidSkillUpgradeTier;
        ApplyUpgrades();

        timer = cooldown;
        if (wizIcon != null)
        {
            activeColor = wizIcon.color;
            inactiveColor = wizIcon.color;
            inactiveColor.a = 0.2f; // faded look
            wizIcon.color = inactiveColor;
        }
    }

    void Update()
    {
        // If cursed, pause the timer entirely
        if (isCursedPaused)
        {
            UpdateCooldownUI();
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartCoroutine(ActivateSproutingSorcery());
            timer = cooldown;
        }

        UpdateCooldownUI();
    }

    /// <summary>
    /// Applies upgrade effects based on the current tier.
    /// </summary>
    public void ApplyUpgrades()
    {
        float effectiveCooldown = baseCooldown;

        // Tier 1: Decrease cooldown by 3 seconds
        if (upgradeTier >= 1)
        {
            effectiveCooldown -= tier1CooldownReduction;
            if (effectiveCooldown < 5f)
                effectiveCooldown = 5f; // safety clamp
        }

        cooldown = effectiveCooldown;
    }

    private IEnumerator ActivateSproutingSorcery()
    {
        isSkillActive = true;

        // Light up icon when skill activates
        if (wizIcon != null)
            wizIcon.color = activeColor;

        // Spawn aura VFX on the player
        if (auraPrefab != null)
        {
            auraInstance = Instantiate(auraPrefab, transform.position, Quaternion.identity, transform);
            // Counteract the player's scale so the VFX always appears at its intended size
            Vector3 prefabScale = auraPrefab.transform.localScale;
            Vector3 parentScale = transform.lossyScale;
            auraInstance.transform.localScale = new Vector3(
                prefabScale.x / parentScale.x,
                prefabScale.y / parentScale.y,
                prefabScale.z / parentScale.z
            );
        }

        // Determine the chosen effect using weighted random selection.
        // At every tier the three heals remain the most likely outcomes.
        // Tier 0-1: Small 33% | Medium 33% | Large 34%
        // Tier 2:   Small 27% | Medium 27% | Large 26% | Energy Loss 20%
        // Tier 3:   Small 20% | Medium 20% | Large 20% | Energy Loss 15% | Coins 12.5% | Invuln 12.5%
        int choice;

        if (upgradeTier >= 3)
        {
            float roll = Random.value; // 0.0 to 1.0
            if (roll < 0.20f)
                choice = 0; // Small heal
            else if (roll < 0.40f)
                choice = 1; // Medium heal
            else if (roll < 0.60f)
                choice = 2; // Large heal
            else if (roll < 0.75f)
                choice = 3; // Energy loss
            else if (roll < 0.875f)
                choice = 4; // Coins
            else
                choice = 5; // Invulnerability
        }
        else if (upgradeTier >= 2)
        {
            float roll = Random.value;
            if (roll < 0.27f)
                choice = 0;
            else if (roll < 0.54f)
                choice = 1;
            else if (roll < 0.80f)
                choice = 2;
            else
                choice = 3;
        }
        else
        {
            choice = Random.Range(0, 3);
        }

        // Handle based on chosen effect
        switch (choice)
        {
            case 0: // Small heal (default)
                yield return StartCoroutine(HealOverTime(smallDuration, smallHeal, "SmallBurst"));
                break;

            case 1: // Medium heal (default)
                yield return StartCoroutine(HealOverTime(mediumDuration, mediumHeal, "MediumBurst"));
                break;

            case 2: // Large heal (default)
                yield return StartCoroutine(HealOverTime(largeDuration, largeHeal, "LargeBurst"));
                break;

            case 3: // Tier 2+: Lose 15% of current energy
                ApplyEnergyLoss();
                break;

            case 4: // Tier 3: Grant 50 coins
                GrantCoins();
                break;

            case 5: // Tier 3: Grant invulnerability for 5 seconds
                GrantInvulnerability();
                break;
        }

        // Dim icon after effect completes
        if (wizIcon != null)
            wizIcon.color = inactiveColor;

        // Destroy aura VFX
        if (auraInstance != null)
        {
            Destroy(auraInstance);
            auraInstance = null;
        }

        isSkillActive = false;
    }

    /// <summary>
    /// Heals the player over time with confetti. Applies Tier 2 bonus if applicable.
    /// </summary>
    private IEnumerator HealOverTime(float duration, float healAmount, string soundName)
    {
        SoundManager.Instance.PlaySound2D(soundName);

        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue(sproutingSorceryDialogue, wizKidPortrait);
        }

        // Tier 2+: boost healing by 10%
        float effectiveHeal = healAmount;
        if (upgradeTier >= 2)
        {
            effectiveHeal = healAmount * (1f + tier2EnergyBonus);
        }

        // Spawn the confetti
        StartCoroutine(SpawnConfetti(duration));

        float timePassed = 0f;

        while (timePassed < duration)
        {
            playerEnergy.RestoreEnergy(effectiveHeal);
            timePassed += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
    }

    /// <summary>
    /// Tier 2+: Wiz Kid loses 15% of his current energy.
    /// </summary>
    private void ApplyEnergyLoss()
    {
        float energyLoss = playerEnergy.maxEnergy * tier2EnergyLossPercent;

        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue("Ouch... that wasn't the right spell!", wizKidPortrait);
        }

        SoundManager.Instance.PlaySound2D("SmallBurst");
        playerEnergy.TakeDamage(energyLoss);
        Debug.Log("Sprouting Sorcery backfired! Lost " + energyLoss + " energy (" + (tier2EnergyLossPercent * 100f) + "% of total).");
    }

    /// <summary>
    /// Tier 3: Grants the player 50 coins.
    /// </summary>
    private void GrantCoins()
    {
        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue("Look what I conjured!", wizKidPortrait);
        }

        SoundManager.Instance.PlaySound2D("MediumBurst");

        if (CoinsManager.Instance != null)
        {
            CoinsManager.Instance.AddCoins(tier3CoinReward);
        }

        Debug.Log("Sprouting Sorcery granted " + tier3CoinReward + " coins!");
    }

    /// <summary>
    /// Tier 3: Grants the player invulnerability for 5 seconds.
    /// </summary>
    private void GrantInvulnerability()
    {
        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue("Nothing can touch me now!", wizKidPortrait);
        }

        SoundManager.Instance.PlaySound2D("LargeBurst");

        playerEnergy.GrantInvulnerability(tier3InvulnerabilityDuration);

        // Spawn confetti during invulnerability for visual flair
        StartCoroutine(SpawnConfetti(tier3InvulnerabilityDuration));

        Debug.Log("Sprouting Sorcery granted invulnerability for " + tier3InvulnerabilityDuration + " seconds!");
    }

    private IEnumerator SpawnConfetti(float duration)
    {
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            if (confettiPrefab != null)
            {
                // spawn around the player within a small radius
                Vector3 offset = Random.insideUnitCircle * 1f;
                Instantiate(confettiPrefab, transform.position + offset, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.2f); // rapid burst effect
        }
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
        data.wizKidSkillUpgradeTier = tier;
        SaveSystem.SaveData(data);
    }

    /// <summary>
    /// Called by StatusEffectManager when the Cursed debuff is applied.
    /// If on cooldown, pauses the cooldown timer.
    /// If ready to activate (timer about to fire), delays it until curse ends.
    /// </summary>
    public void OnCursed()
    {
        isCursedPaused = true;

        // Check if the skill was about to fire (timer very close to 0 or already at 0)
        if (timer <= 0f)
        {
            wasReadyWhenCursed = true;
            Debug.Log("Cursed! Sprouting Sorcery was ready — activation delayed until curse ends.");
        }
        else
        {
            wasReadyWhenCursed = false;
            Debug.Log("Cursed! Sprouting Sorcery cooldown timer paused.");
        }
    }

    /// <summary>
    /// Called by StatusEffectManager when the Cursed debuff wears off.
    /// Resumes the cooldown timer. If the skill was ready when cursed, activate it now.
    /// </summary>
    public void OnCurseLifted()
    {
        isCursedPaused = false;

        if (wasReadyWhenCursed)
        {
            // Fire the skill immediately since it was ready before the curse
            timer = 0f; // will trigger on next Update
            Debug.Log("Curse lifted! Sprouting Sorcery activating now!");
        }
        else
        {
            Debug.Log("Curse lifted! Sprouting Sorcery cooldown timer resumed.");
        }

        wasReadyWhenCursed = false;
    }

    /// <summary>
    /// Refreshes the skill icon color to its correct state (called when curse tint is removed).
    /// </summary>
    public void RefreshIconColor()
    {
        if (wizIcon != null)
        {
            wizIcon.color = inactiveColor;
        }
    }

    /// <summary>
    /// Updates the cooldown UI element based on the skill state.
    /// </summary>
    private void UpdateCooldownUI()
    {
        if (skillCooldownUI == null) return;

        if (isSkillActive)
        {
            skillCooldownUI.ShowUsing();
        }
        else if (timer <= 0f)
        {
            skillCooldownUI.ShowReady();
        }
        else
        {
            skillCooldownUI.ShowCooldown(timer);
        }
    }
}
