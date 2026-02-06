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

    void Start()
    {
        cooldownRemaining = cooldownTime;
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
            activateSkillAction.AddBinding("<Gamepad>/leftShoulder");
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
        TryFireElectricBolt();
    }

    // Called by mobile UI button tap on skill icon
    public void ActivateSkillInput(bool pressed)
    {
        if (pressed)
        {
            TryFireElectricBolt();
        }
    }

    private void TryFireElectricBolt()
    {
        if (cooldownRemaining == 0f)
        {
            FireElectricBolt();
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

        // legacy fallback if InputSystem action not available/enabled
        if ((activateSkillAction == null || !activateSkillAction.enabled) && Keyboard.current != null)
        {
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame && cooldownRemaining == 0f)
            {
                TryFireElectricBolt();
            }
        }
    }

    void FireElectricBolt()
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

        Instantiate(electricBoltPrefab, boltSpawnPoint.position, Quaternion.identity);

        // reset cooldown
        cooldownRemaining = cooldownTime;
        if (readyIcon != null)
            readyIcon.color = inactiveColor;
    }
}
