using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrystalAbility : MonoBehaviour
{
    public int snowflakesNeeded = 5;
    private int currentSnowflakes = 0;

    public bool abilityReady = false;
    public float glaciateDuration = 5f;
    public GameObject glaciateEffectPrefab;

    [Header("Skill Icon")]
    public Image skillIcon;
    public Color fadedColor;
    public Color readyColor;
    public float flickerSpeed = 6f; // how fast icon flickers

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string glaciateDialogue = "The cold will take care of you.";
    public Sprite crystalPortrait;

    [Header("Input (New Input System)")]
    public InputActionReference activateAbilityActionRef; // optional: assign from an Input Actions asset
    private InputAction activateAbilityAction;
    private bool createdLocalAction = false;

    private bool abilityActive = false;
    private PlayerEnergy playerEnergy;

    private Button skillButton;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();

        if (skillIcon != null)
        {
            readyColor = skillIcon.color;     // normal visible sprite
            fadedColor = skillIcon.color;
            fadedColor.a = 0.2f;              // faded power-down
            skillIcon.color = fadedColor;

            // Make the skill icon tappable on mobile
            skillButton = skillIcon.GetComponent<Button>();
            if (skillButton == null)
                skillButton = skillIcon.gameObject.AddComponent<Button>();
            skillButton.transition = Selectable.Transition.None;
            skillButton.onClick.AddListener(() => ActivateAbilityInput(true));
        }
    }

    void OnEnable()
    {
        // prefer an assigned InputActionReference, otherwise create a simple fallback action
        if (activateAbilityActionRef != null && activateAbilityActionRef.action != null)
        {
            activateAbilityAction = activateAbilityActionRef.action;
        }
        else
        {
            activateAbilityAction = new InputAction("ActivateGlaciate", InputActionType.Button);    
            activateAbilityAction.AddBinding("<Keyboard>/leftShift");
            activateAbilityAction.AddBinding("<Gamepad>/buttonEast");
            createdLocalAction = true;
        }

        if (activateAbilityAction != null)
        {
            activateAbilityAction.performed += OnActivatePerformed;
            activateAbilityAction.Enable();
        }
    }

    void OnDisable()
    {
        if (activateAbilityAction != null)
        {
            activateAbilityAction.performed -= OnActivatePerformed;
            activateAbilityAction.Disable();
        }

        if (createdLocalAction && activateAbilityAction != null)
        {
            activateAbilityAction.Dispose();
            activateAbilityAction = null;
            createdLocalAction = false;
        }
    }

    private void OnActivatePerformed(InputAction.CallbackContext ctx)
    {
        TryActivateGlaciate();
    }

    // Called by mobile UI button tap on skill icon
    public void ActivateAbilityInput(bool pressed)
    {
        if (pressed)
        {
            TryActivateGlaciate();
        }
    }

    private void TryActivateGlaciate()
    {
        if (abilityReady && !abilityActive)
        {
            StartCoroutine(ActivateGlaciate());
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound2D("Glaciate");
        }
    }

    void Update()
    {
        // Legacy fallback if the InputAction isn't assigned/enabled (optional)
        if ((activateAbilityAction == null || !activateAbilityAction.enabled) && Keyboard.current != null)
        {
            if (abilityReady && Keyboard.current.leftShiftKey.wasPressedThisFrame && !abilityActive)
            {
                TryActivateGlaciate();
            }
        }
    }

    public void CollectSnowflake()
    {
        if (abilityActive) return;

        currentSnowflakes++;

        if (currentSnowflakes >= snowflakesNeeded)
        {
            abilityReady = true;

            if (skillIcon != null)
                skillIcon.color = readyColor;
        }
    }


    private IEnumerator ActivateGlaciate()
    {
        abilityActive = true;
        abilityReady = false;
        currentSnowflakes = 0;

        if (skillDialogueUI != null)
        {
            skillDialogueUI.ShowSkillDialogue(glaciateDialogue, crystalPortrait);
        }

        // Start flicker
        if (skillIcon != null)
            StartCoroutine(FlickerIcon());

        // Spawn mist effect
        GameObject effect = Instantiate(glaciateEffectPrefab, transform.position, Quaternion.identity, transform);

        GlaciateArea glaciate = GetComponentInChildren<GlaciateArea>();
        glaciate.EnableRadius(true);

        yield return new WaitForSeconds(glaciateDuration);

        glaciate.EnableRadius(false);
        Destroy(effect);

        abilityActive = false;

        // End flicker ? return to faded
        if (skillIcon != null)
            skillIcon.color = fadedColor;
    }

    private IEnumerator FlickerIcon()
    {
        float t = 0;

        while (abilityActive)
        {
            t += Time.deltaTime * flickerSpeed;

            float alpha = Mathf.Abs(Mathf.Sin(t));   // goes 0 ? 1 ? 0 smoothly
            Color c = readyColor;
            c.a = alpha;

            if (skillIcon != null)
                skillIcon.color = c;

            yield return null;
        }
    }
}

