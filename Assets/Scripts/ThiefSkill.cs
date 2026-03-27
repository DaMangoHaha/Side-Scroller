using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ThiefSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public float cooldownTime = 30f;
    public float activeDuration = 5f;
    public float coinPullRadius = 5f;
    public float coinPullSpeed = 6f;

    private bool isOnCooldown = true;
    private bool isActive = false;
    private float cooldownTimer;

    [Header("UI")]
    public Image skillIcon;
    private Color activeColor;
    private Color inactiveColor;

    public float flickerSpeed = 6f;   // how fast icon flickers

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string stickyFingersDialogue = "I'll be taking these...";
    public Sprite thiefPortrait;

    [Header("Input (New Input System)")]
    public InputActionReference activateSkillActionRef; // optional: assign from Input Actions asset
    private InputAction activateSkillAction;
    private bool createdLocalAction = false;

    private Button skillButton;

    // --- Upgrade System ---
    [Header("Upgrade")]
    public int upgradeTier = 0; // 0 = no upgrades, 1-3 = tiers

    // Tier 1: cooldown reduction
    private float cooldownReduction = 5f;

    // Tier 2: bonus score per coin during skill + radius multiplier
    private int tier2BonusScore = 50;
    private float tier2RadiusMultiplier = 1.5f; // 0.5x extra = 1.5x total

    // Tier 3: coins collected during skill grant +3 energy
    private float tier3EnergyPerCoin = 3f;

    // Base radius stored so upgrades can scale from it
    private float baseCoinPullRadius;

    /// <summary>Returns true while Sticky Fingers is active.</summary>
    public bool IsSkillActive => isActive;

    void Start()
    {
        cooldownTimer = cooldownTime;

        // Load upgrade tier from save data
        SaveData data = SaveSystem.LoadData();
        upgradeTier = data.thiefSkillUpgradeTier;
        baseCoinPullRadius = coinPullRadius;
        ApplyUpgrades();

        if (skillIcon != null)
        {
            activeColor = skillIcon.color;
            inactiveColor = activeColor;
            inactiveColor.a = 0.3f;
            skillIcon.color = inactiveColor;  // starts faded

            // Make the skill icon tappable on mobile
            skillButton = skillIcon.GetComponent<Button>();
            if (skillButton == null)
                skillButton = skillIcon.gameObject.AddComponent<Button>();
            skillButton.transition = Selectable.Transition.None;
            skillButton.onClick.AddListener(() => ActivateSkillInput(true));
        }
    }

    void OnEnable()
    {
        // Prefer an assigned InputActionReference, otherwise create a simple fallback action
        if (activateSkillActionRef != null && activateSkillActionRef.action != null)
        {
            activateSkillAction = activateSkillActionRef.action;
        }
        else
        {
            activateSkillAction = new InputAction("ActivateThiefSkill", InputActionType.Button);
            activateSkillAction.AddBinding("<Keyboard>/Shift");
            activateSkillAction.AddBinding("<Gamepad>/buttonEast");
            createdLocalAction = true;
        }

        if (activateSkillAction != null)
        {
            activateSkillAction.performed += OnActivatePerformed;
            activateSkillAction.Enable();
        }
    }

    void OnDisable()
    {
        if (activateSkillAction != null)
        {
            activateSkillAction.performed -= OnActivatePerformed;
            activateSkillAction.Disable();
        }

        if (createdLocalAction && activateSkillAction != null)
        {
            activateSkillAction.Dispose();
            activateSkillAction = null;
            createdLocalAction = false;
        }
    }

    private void OnActivatePerformed(InputAction.CallbackContext ctx)
    {
        TryActivateSkill();
    }

    // Called by mobile UI button tap on skill icon
    public void ActivateSkillInput(bool pressed)
    {
        if (pressed)
        {
            TryActivateSkill();
        }
    }

    private void TryActivateSkill()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(ActivateSkill());
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound2D("StickyFingers");
        }
    }

    void Update()
    {
        // Cooldown counting
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0)
            {
                isOnCooldown = false;
                if (skillIcon != null)
                    skillIcon.color = activeColor;
            }
        }

        // legacy fallback: if for some reason the new input isn't set up, allow keyboard input once
        if (activateSkillAction == null || !activateSkillAction.enabled)
        {
            if (!isOnCooldown && Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                TryActivateSkill();
            }
        }

        // Active coin pulling effect
        if (isActive)
            AttractNearbyCoins();
    }

    private IEnumerator ActivateSkill()
    {
        isActive = true;
        isOnCooldown = true;
        cooldownTimer = cooldownTime;

        // Icon fades at activation
        if (skillIcon != null)
            skillIcon.color = inactiveColor;

        Debug.Log("Sticky Fingers Activated!");

        // Start flicker coroutine
        if (skillIcon != null)
            StartCoroutine(FlickerIcon());

        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue(stickyFingersDialogue, thiefPortrait);
        }

        yield return new WaitForSeconds(activeDuration);

        // Skill ends
        isActive = false;
        Debug.Log("Sticky Fingers ended.");

        // Return icon to faded look
        if (skillIcon != null)
            skillIcon.color = inactiveColor;
    }

    private IEnumerator FlickerIcon()
    {
        float t = 0f;

        while (isActive)
        {
            t += Time.deltaTime * flickerSpeed;

            float alpha = Mathf.Abs(Mathf.Sin(t)); // pulsing 0→1→0 loop

            Color c = activeColor;
            c.a = alpha;

            if (skillIcon != null)
                skillIcon.color = c;

            yield return null;
        }
    }

    private void AttractNearbyCoins()
    {
        Collider2D[] nearbyCoins = Physics2D.OverlapCircleAll(transform.position, coinPullRadius);

        foreach (var col in nearbyCoins)
        {
            if (col.CompareTag("Coin"))
            {
                col.transform.position = Vector3.MoveTowards(
                    col.transform.position,
                    transform.position,
                    coinPullSpeed * Time.deltaTime
                );
            }
        }
    }

    // Called from Coin.cs
    public void ReduceCooldown(float amount)
    {
        if (isOnCooldown && cooldownTimer > 0)
        {
            cooldownTimer -= amount;
            if (cooldownTimer < 0)
                cooldownTimer = 0;
        }
    }

    // --- Upgrade helpers ---

    /// <summary>
    /// Applies upgrade effects based on the current tier.
    /// </summary>
    public void ApplyUpgrades()
    {
        // Tier 1: Decrease cooldown by 5 seconds
        if (upgradeTier >= 1)
        {
            float effective = cooldownTime - cooldownReduction;
            if (effective < activeDuration + 1f)
                effective = activeDuration + 1f; // safety clamp
            cooldownTime = effective;
        }

        // Tier 2: Increase pull radius by 0.5x the normal size
        if (upgradeTier >= 2)
        {
            coinPullRadius = baseCoinPullRadius * tier2RadiusMultiplier;
        }
        else
        {
            coinPullRadius = baseCoinPullRadius;
        }
    }

    /// <summary>
    /// Returns the bonus score to add when a coin is collected during the active skill.
    /// Returns 0 if Tier 2 is not unlocked or skill is not active.
    /// </summary>
    public int GetBonusCoinScore()
    {
        if (isActive && upgradeTier >= 2)
            return tier2BonusScore;
        return 0;
    }

    /// <summary>
    /// Returns true if a coin collected during the active skill should grant energy.
    /// Returns false if Tier 3 is not unlocked or skill is not active.
    /// </summary>
    public bool ShouldGrantEnergy()
    {
        if (isActive && upgradeTier >= 3)
            return true;
        return false;
    }

    /// <summary>
    /// Returns the amount of energy to grant per coin collected during the active skill.
    /// </summary>
    public float GetEnergyPerCoin()
    {
        return tier3EnergyPerCoin;
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

        // Reset radius to base before reapplying
        coinPullRadius = baseCoinPullRadius;
        // Re-derive cooldown from original base (30s) before reduction
        cooldownTime = 30f;

        ApplyUpgrades();

        // Persist
        SaveData data = SaveSystem.LoadData();
        data.thiefSkillUpgradeTier = tier;
        SaveSystem.SaveData(data);
    }

    void OnDrawGizmosSelected() //This gizmo shows the coin pull radius
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, coinPullRadius);
    }
}

