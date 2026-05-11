using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Selene's "Spiritually Charming" skill.
/// Every 45 seconds: grants invulnerability, drops a pink crystal that destroys all on-screen obstacles.
/// After destroying a cumulative threshold of obstacles, the "Charm!" buff activates for 10 seconds —
/// colliding obstacles have a 30% chance to be charmed, reversing direction and destroying the next obstacle they touch.
/// </summary>
public class SeleneSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float cooldown = 45f;
    public float invulnerabilityDuration = 3f;     // base; Tier 2 raises to 5f

    [Header("Crystal Drop Settings")]
    public float crystalFallSpeed = 15f;
    [Tooltip("Optional pink crystal prefab to spawn. If null, a placeholder is created at runtime.")]
    public GameObject fallingCrystalPrefab;
    [Tooltip("Optional pink shield VFX spawned around Selene when her skill fires.")]
    public GameObject shieldVFXPrefab;

    [Header("Charm Buff Settings")]
    public float charmDuration = 10f;
    public float charmChance = 0.30f;
    public int obstaclesNeededForCharm = 20;       // Tier 3 reduces to 15

    // ── Charm Buff Icon ──
    [Header("Charm Buff Icon")]
    [Tooltip("UI Image component for the Charm buff icon")]
    public Image charmBuffIcon;
    [Tooltip("Sprite to display for the Charm buff (optional - uses existing Image sprite if not set)")]
    public Sprite charmBuffSprite;

    [Header("Charm Icon Animation")]
    [Tooltip("Should the icon pulse/flash while active?")]
    public bool animateCharmBuffIcon = true;
    [Tooltip("Speed of the pulse animation")]
    public float charmBuffPulseSpeed = 2f;
    [Tooltip("Minimum alpha during pulse")]
    public float charmBuffPulseMinAlpha = 0.5f;

    // Icon animation coroutine
    private Coroutine charmBuffIconAnimCoroutine;

    [Header("UI")]
    public Image skillIcon;
    private Color inactiveColor;
    private Color activeColor;

    [Header("Cooldown Text")]
    public SkillCooldownUI skillCooldownUI;

    [Header("Obstacle Count UI")]
    [Tooltip("A second SkillCooldownUI placed to the right of the cooldown text. Shows obstacle progress toward the Charm! buff.")]
    public SkillCooldownUI obstacleCountUI;

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string skillDialogue = "Crystals are so enchanting, right?";
    public Sprite selenePortrait;

    // --- Upgrade System ---
    [Header("Upgrade")]
    public int upgradeTier = 0; // 0 = no upgrades, 1-3 = tiers

    // Tier 1: restore +2 energy per obstacle destroyed
    private float tier1EnergyPerObstacle = 2f;

    // Tier 2: longer invulnerability
    private float tier2InvulnerabilityDuration = 5f;

    // Tier 3: fewer obstacles needed to trigger Charm
    private int tier3ObstaclesNeeded = 10;

    private float baseCooldown;
    private float timer = 0f;
    private bool isSkillActive = false;

    // Charm buff state
    private bool isCharmActive = false;
    private int totalObstaclesDestroyed = 0;

    private PlayerEnergy playerEnergy;

    // --- Cursed Debuff ---
    private bool isCursedPaused = false;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        baseCooldown = cooldown;

        // Load upgrade tier from save data
        SaveData data = SaveSystem.LoadData();
        upgradeTier = data.seleneSkillUpgradeTier;
        ApplyUpgrades();

        timer = cooldown;

        if (skillIcon != null)
        {
            activeColor = skillIcon.color;
            inactiveColor = skillIcon.color;
            inactiveColor.a = 0.2f;
            skillIcon.color = inactiveColor;
        }

        // Initialize Charm buff icon — hide it at start
        InitializeCharmBuffIcon();
    }

    void Update()
    {
        if (isCursedPaused)
        {
            UpdateCooldownUI();
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartCoroutine(ActivateSpirituallyCharming());
            timer = cooldown;
        }

        UpdateCooldownUI();
    }

    /// <summary>
    /// Applies upgrade effects based on the current tier.
    /// Most effects are resolved dynamically at activation time.
    /// </summary>
    public void ApplyUpgrades()
    {
        // All tier effects evaluated at runtime via helper methods.
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private int GetEffectiveObstaclesNeeded()
    {
        return upgradeTier >= 3 ? tier3ObstaclesNeeded : obstaclesNeededForCharm;
    }

    private float GetEffectiveInvulnerabilityDuration()
    {
        return upgradeTier >= 2 ? tier2InvulnerabilityDuration : invulnerabilityDuration;
    }

    // -----------------------------------------------------------------------
    //  Skill Activation
    // -----------------------------------------------------------------------

    private IEnumerator ActivateSpirituallyCharming()
    {
        isSkillActive = true;

        if (skillIcon != null)
            skillIcon.color = activeColor;

        if (skillDialogueUI != null)
            skillDialogueUI.ShowSkillDialogue(skillDialogue, selenePortrait);

        // 1. Grant invulnerability (pink shield)
        float invulDuration = GetEffectiveInvulnerabilityDuration();
        playerEnergy.GrantInvulnerability(invulDuration);

        // 2. Spawn shield VFX parented to Selene
        GameObject shieldInstance = null;
        if (shieldVFXPrefab != null)
        {
            shieldInstance = Instantiate(shieldVFXPrefab, transform.position, Quaternion.identity, transform);
            shieldInstance.transform.localPosition = Vector3.zero;
            Destroy(shieldInstance, invulDuration);
        }

        // 3. Drop the pink crystal and explode on landing
        yield return StartCoroutine(DropCrystal());

        if (skillIcon != null)
            skillIcon.color = inactiveColor;

        isSkillActive = false;
    }

    private IEnumerator DropCrystal()
    {
        // Spawn at top-center of the screen
        Camera cam = Camera.main;
        Vector3 spawnPos;
        float groundY;

        if (cam != null)
        {
            spawnPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 1.25f, cam.nearClipPlane));
            spawnPos.z = 0f;
            Vector3 bottom = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, cam.nearClipPlane));
            groundY = bottom.y + 0.5f;
        }
        else
        {
            spawnPos = new Vector3(0f, 10f, 0f);
            groundY = -4f;
        }

        GameObject crystal;
        if (fallingCrystalPrefab != null)
        {
            crystal = Instantiate(fallingCrystalPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Placeholder pink crystal
            crystal = new GameObject("SeleneFallingCrystal");
            crystal.transform.position = spawnPos;
            SpriteRenderer sr = crystal.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.4f, 0.85f);
            sr.sortingOrder = 10;
        }

        // Fall
        while (crystal != null && crystal.transform.position.y > groundY)
        {
            crystal.transform.position += Vector3.down * crystalFallSpeed * Time.deltaTime;
            yield return null;
        }

        if (crystal != null)
        {
            ExplodeDestroyObstacles();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound2D("LargeBurst");

            Destroy(crystal);
        }
    }

    /// <summary>
    /// Destroys all Spike and Alien obstacles currently active in the scene.
    /// Awards energy per obstacle at Tier 1, and tracks toward the Charm buff threshold.
    /// </summary>
    private void ExplodeDestroyObstacles()
    {
        int count = 0;

        foreach (var spike in FindObjectsOfType<Spike>())
        {
            Destroy(spike.gameObject);
            count++;
        }

        foreach (var alien in FindObjectsOfType<Alien>())
        {
            Destroy(alien.gameObject);
            count++;
        }

        foreach (var slime in FindObjectsOfType<SlimeBase>())
        {
            Destroy(slime.gameObject);
            count++;
        }

        foreach (var bolt in FindObjectsOfType<MagicBolt>())
        {
            Destroy(bolt.gameObject);
            count++;
        }

        foreach (var twin in FindObjectsOfType<TwinMagicBolt>())
        {
            Destroy(twin.gameObject);
            count++;
        }

        // Tier 1: restore energy for each obstacle destroyed
        if (upgradeTier >= 1 && playerEnergy != null && count > 0)
        {
            playerEnergy.RestoreEnergy(count * tier1EnergyPerObstacle);
            Debug.Log($"Selene Tier 1: restored {count * tier1EnergyPerObstacle} energy from {count} obstacles.");
        }

        totalObstaclesDestroyed += count;
        Debug.Log($"Selene's crystal destroyed {count} obstacles. Total: {totalObstaclesDestroyed}/{GetEffectiveObstaclesNeeded()}");

        if (!isCharmActive && totalObstaclesDestroyed >= GetEffectiveObstaclesNeeded())
        {
            StartCoroutine(ActivateCharmBuff());
        }
    }

    private IEnumerator ActivateCharmBuff()
    {
        isCharmActive = true;
        totalObstaclesDestroyed = 0; // reset for next threshold

        Debug.Log("Selene: Charm! buff activated!");
        ShowCharmBuffIcon();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound2D("MediumBurst");

        yield return new WaitForSeconds(charmDuration);

        isCharmActive = false;
        HideCharmBuffIcon();
        Debug.Log("Selene: Charm! buff expired.");
    }

    // -----------------------------------------------------------------------
    //  Charm Buff Icon Management
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sets up the Charm buff icon with its sprite and hides it initially.
    /// </summary>
    private void InitializeCharmBuffIcon()
    {
        if (charmBuffIcon != null)
        {
            if (charmBuffSprite != null)
                charmBuffIcon.sprite = charmBuffSprite;
            charmBuffIcon.gameObject.SetActive(false);

            Debug.Log("SeleneSkill: Charm Buff Icon assigned and hidden at start.");
        }
        else
        {
            Debug.LogWarning("SeleneSkill: Charm Buff Icon is NOT assigned in the Inspector!");
        }
    }

    /// <summary>
    /// Shows the Charm buff icon and optionally starts the pulse animation.
    /// </summary>
    private void ShowCharmBuffIcon()
    {
        if (charmBuffIcon == null)
        {
            Debug.LogWarning("SeleneSkill: Cannot show Charm Buff Icon — not assigned!");
            return;
        }

        charmBuffIcon.gameObject.SetActive(true);
        Debug.Log("SeleneSkill: Charm Buff Icon shown. Active in hierarchy: " + charmBuffIcon.gameObject.activeInHierarchy);

        // Reset alpha to full
        Color c = charmBuffIcon.color;
        c.a = 1f;
        charmBuffIcon.color = c;

        // Start pulse animation if enabled
        if (animateCharmBuffIcon)
        {
            if (charmBuffIconAnimCoroutine != null)
                StopCoroutine(charmBuffIconAnimCoroutine);
            charmBuffIconAnimCoroutine = StartCoroutine(PulseCharmBuffIcon());
        }
    }

    /// <summary>
    /// Hides the Charm buff icon and stops any animation.
    /// </summary>
    private void HideCharmBuffIcon()
    {
        if (charmBuffIcon == null) return;

        if (charmBuffIconAnimCoroutine != null)
        {
            StopCoroutine(charmBuffIconAnimCoroutine);
            charmBuffIconAnimCoroutine = null;
        }

        charmBuffIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// Pulse animation for the active Charm buff icon.
    /// </summary>
    private IEnumerator PulseCharmBuffIcon()
    {
        if (charmBuffIcon == null) yield break;

        while (true)
        {
            // Pulse from full alpha down to min alpha and back
            float t = (Mathf.Sin(Time.time * charmBuffPulseSpeed) + 1f) * 0.5f; // 0 to 1
            float alpha = Mathf.Lerp(charmBuffPulseMinAlpha, 1f, t);

            Color c = charmBuffIcon.color;
            c.a = alpha;
            charmBuffIcon.color = c;

            yield return null;
        }
    }

    // -----------------------------------------------------------------------
    //  Charm Integration (called from Spike / Alien collision code)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called when an obstacle collides with the player.
    /// If the Charm buff is active, rolls a 30% chance to charm the obstacle instead of dealing damage.
    /// Returns true if the obstacle was charmed (caller should skip damage entirely).
    /// </summary>
    public bool TryCharmObstacle(GameObject obstacleObject)
    {
        if (!isCharmActive) return false;

        if (Random.value < charmChance)
        {
            CharmedObstacle charmed = obstacleObject.GetComponent<CharmedObstacle>();
            if (charmed == null)
                charmed = obstacleObject.AddComponent<CharmedObstacle>();

            charmed.Activate();
            Debug.Log("Selene: Obstacle charmed!");
            return true;
        }

        return false;
    }

    public bool IsCharmActive() => isCharmActive;

    // -----------------------------------------------------------------------
    //  Cursed Debuff
    // -----------------------------------------------------------------------

    public void OnCursed()
    {
        isCursedPaused = true;
        Debug.Log("Cursed! Selene's skill cooldown paused.");
    }

    public void OnCurseLifted()
    {
        isCursedPaused = false;
        Debug.Log("Curse lifted! Selene's skill resumed.");
    }

    public void RefreshIconColor()
    {
        if (skillIcon != null)
            skillIcon.color = inactiveColor;
    }

    // -----------------------------------------------------------------------
    //  Upgrade Helpers
    // -----------------------------------------------------------------------

    public int GetUpgradeTier() => upgradeTier;

    public void SetUpgradeTier(int tier)
    {
        upgradeTier = tier;
        ApplyUpgrades();

        SaveData data = SaveSystem.LoadData();
        data.seleneSkillUpgradeTier = tier;
        SaveSystem.SaveData(data);
    }

    // -----------------------------------------------------------------------
    //  Cooldown UI
    // -----------------------------------------------------------------------

    private void UpdateCooldownUI()
    {
        if (skillCooldownUI == null) return;

        if (isSkillActive)
            skillCooldownUI.ShowUsing();
        else if (timer <= 0f)
            skillCooldownUI.ShowReady();
        else
            skillCooldownUI.ShowCooldown(timer);

        UpdateObstacleCountUI();
    }

    /// <summary>
    /// Keeps the obstacle-count text up to date every frame.
    /// Shows "X / N ✦" while building toward Charm, and "Charm!" while the buff is active.
    /// Hidden entirely while the skill animation is playing.
    /// </summary>
    private void UpdateObstacleCountUI()
    {
        if (obstacleCountUI == null) return;

        if (isSkillActive)
        {
            // Hide the counter while the crystal is falling so only "Using Skill..." shows
            obstacleCountUI.ShowCustom("");
        }
        else if (isCharmActive)
        {
            obstacleCountUI.ShowCustom("Charm!");
        }
        else
        {
            int needed = GetEffectiveObstaclesNeeded();
            obstacleCountUI.ShowCustom($"{totalObstaclesDestroyed} / {needed}");
        }
    }
}
