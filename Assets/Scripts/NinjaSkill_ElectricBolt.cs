using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class NinjaSkill_ElectricBolt : MonoBehaviour
{
    [Header("Skill Settings")]
    public float cooldownTime = 30f;
    private float cooldownRemaining = 0f;

    [Header("Bolt Settings")]
    public GameObject electricBoltPrefab; // assign in Inspector
    public Transform boltSpawnPoint;      // where Bolt appears

    [Header("UI")]
    public Image readyIcon;          // like Bits/Thief indicators
    private Color inactiveColor;
    private Color activeColor;

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string electricBoltDialogue = "Too slow.";
    public Sprite ninjaPortrait;

    [Header("Input (New Input System)")]
    public InputActionReference activateSkillActionRef; // optional: assign action from an Input Actions asset
    private InputAction activateSkillAction;
    private bool createdLocalAction = false;

    private Button skillButton;

    // --- Upgrade System ---
    [Header("Upgrade")]
    public int upgradeTier = 0; // 0 = no upgrades, 1-3 = tiers

    // Tier 1: cooldown reduction on kill
    private float tier1CooldownReduction = 3f;

    // Tier 2: hold to fire large bolt
    [Header("Tier 2 - Large Bolt")]
    public GameObject largeBoltPrefab; // assign a larger bolt prefab in Inspector (optional, will scale normal if null)
    public float holdTimeRequired = 2f;
    private float holdTimer = 0f;
    private bool isHolding = false;

    // Tier 3: energy grant on use
    private float tier3EnergyPercent = 0.10f; // 10% of total energy

    // Base cooldown stored so upgrades can derive from it
    private float baseCooldownTime;

    // Reference to PlayerEnergy (for Tier 3)
    private PlayerEnergy playerEnergy;

    void Start()
    {
        cooldownRemaining = cooldownTime;
        baseCooldownTime = cooldownTime;

        // Load upgrade tier from save data
        SaveData data = SaveSystem.LoadData();
        upgradeTier = data.ninjaSkillUpgradeTier;
        ApplyUpgrades();

        playerEnergy = GetComponent<PlayerEnergy>();

        if (readyIcon != null)
        {
            activeColor = readyIcon.color;
            inactiveColor = readyIcon.color;
            inactiveColor.a = 0.2f; // faded look
            readyIcon.color = inactiveColor;

            // Make the skill icon tappable on mobile
            skillButton = readyIcon.GetComponent<Button>();
            if (skillButton == null)
                skillButton = readyIcon.gameObject.AddComponent<Button>();
            skillButton.transition = Selectable.Transition.None;
            skillButton.onClick.AddListener(() => ActivateSkillInput(true));
        }
    }

    void OnEnable()
    {
        // prefer the assigned InputActionReference, otherwise create a simple fallback action
        if (activateSkillActionRef != null && activateSkillActionRef.action != null)
        {
            activateSkillAction = activateSkillActionRef.action;
        }
        else
        {
            activateSkillAction = new InputAction("ActivateElectricBolt", InputActionType.Button);
            activateSkillAction.AddBinding("<Keyboard>/leftShift");
            activateSkillAction.AddBinding("<Gamepad>/buttonEast");
            createdLocalAction = true;
        }

        if (activateSkillAction != null)
        {
            activateSkillAction.started += OnActivateStarted;
            activateSkillAction.canceled += OnActivateCanceled;
            activateSkillAction.performed += OnActivatePerformed;
            activateSkillAction.Enable();
        }
    }

    void OnDisable()
    {
        if (activateSkillAction != null)
        {
            activateSkillAction.started -= OnActivateStarted;
            activateSkillAction.canceled -= OnActivateCanceled;
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
        // For non-Tier-2, fire immediately on performed (press)
        if (upgradeTier < 2)
        {
            TryFireElectricBolt(false);
        }
    }

    private void OnActivateStarted(InputAction.CallbackContext ctx)
    {
        // Tier 2+: begin tracking hold time
        if (upgradeTier >= 2 && cooldownRemaining <= 0f)
        {
            isHolding = true;
            holdTimer = 0f;
        }
    }

    private void OnActivateCanceled(InputAction.CallbackContext ctx)
    {
        if (upgradeTier >= 2)
        {
            if (isHolding)
            {
                if (holdTimer >= holdTimeRequired)
                {
                    // Held long enough - fire large bolt
                    TryFireElectricBolt(true);
                }
                else
                {
                    // Quick press/release - fire normal bolt
                    TryFireElectricBolt(false);
                }
            }
            isHolding = false;
            holdTimer = 0f;
        }
    }

    // Called by mobile UI button tap on skill icon
    public void ActivateSkillInput(bool pressed)
    {
        if (pressed)
        {
            TryFireElectricBolt(false);
        }
    }

    private void TryFireElectricBolt(bool isLargeBolt)
    {
        if (cooldownRemaining <= 0f)
        {
            FireElectricBolt(isLargeBolt);
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound2D("ElectricBolt");
        }
    }

    void Update()
    {
        // Update cooldown
        if (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining <= 0)
            {
                cooldownRemaining = 0;
                if (readyIcon != null)
                    readyIcon.color = activeColor;
            }
        }

        // Track hold time for Tier 2
        if (isHolding && upgradeTier >= 2)
        {
            holdTimer += Time.deltaTime;
        }

        // legacy fallback if InputSystem action not available/enabled
        if ((activateSkillAction == null || !activateSkillAction.enabled) && Keyboard.current != null)
        {
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame && cooldownRemaining <= 0f)
            {
                TryFireElectricBolt(false);
            }
        }
    }

    void FireElectricBolt(bool isLargeBolt)
    {
        if (electricBoltPrefab == null || boltSpawnPoint == null)
        {
            Debug.LogError("Electric bolt or spawn point missing!");
            return;
        }
        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue(electricBoltDialogue, ninjaPortrait);
        }

        GameObject boltObj;

        if (isLargeBolt && upgradeTier >= 2)
        {
            // Tier 2 large bolt: use dedicated prefab if set, otherwise scale up the normal one
            GameObject prefab = largeBoltPrefab != null ? largeBoltPrefab : electricBoltPrefab;
            boltObj = Instantiate(prefab, boltSpawnPoint.position, Quaternion.identity);

            ElectricBolt bolt = boltObj.GetComponent<ElectricBolt>();
            if (bolt != null)
            {
                bolt.isLargeBolt = true;
                bolt.speed = 5f; // travels slowly
            }

            // Scale up visually if using the normal prefab
            if (largeBoltPrefab == null)
            {
                boltObj.transform.localScale *= 2f;
            }
        }
        else
        {
            boltObj = Instantiate(electricBoltPrefab, boltSpawnPoint.position, Quaternion.identity);
        }

        // Pass a reference to this skill so the bolt can call back (Tier 1 cooldown reduction)
        ElectricBolt boltScript = boltObj.GetComponent<ElectricBolt>();
        if (boltScript != null)
        {
            boltScript.ownerSkill = this;
        }

        // Tier 3: Grant Ninja +10% of her total energy on use
        if (upgradeTier >= 3 && playerEnergy != null)
        {
            float energyGrant = playerEnergy.maxEnergy * tier3EnergyPercent;
            playerEnergy.RestoreEnergy(energyGrant);
            Debug.Log("Tier 3: Ninja gained " + energyGrant + " energy from Electric Bolt!");
        }

        // reset cooldown
        cooldownRemaining = cooldownTime;
        if (readyIcon != null)
            readyIcon.color = inactiveColor;
    }

    // --- Upgrade helpers ---

    /// <summary>
    /// Applies upgrade effects based on the current tier.
    /// </summary>
    public void ApplyUpgrades()
    {
        // Currently tier effects are handled at runtime in FireElectricBolt / ElectricBolt
        // No persistent stat changes needed beyond what the tier field controls
    }

    /// <summary>
    /// Called by ElectricBolt when it destroys an enemy/obstacle (Tier 1).
    /// Reduces the current cooldown by 3 seconds.
    /// </summary>
    public void OnBoltKill()
    {
        if (upgradeTier >= 1 && cooldownRemaining > 0)
        {
            cooldownRemaining -= tier1CooldownReduction;
            if (cooldownRemaining < 0f)
                cooldownRemaining = 0f;

            Debug.Log("Tier 1: Electric Bolt cooldown reduced by " + tier1CooldownReduction + "s! Remaining: " + cooldownRemaining);

            // If cooldown hit zero, mark as ready
            if (cooldownRemaining <= 0f && readyIcon != null)
                readyIcon.color = activeColor;
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
        data.ninjaSkillUpgradeTier = tier;
        SaveSystem.SaveData(data);
    }
}
