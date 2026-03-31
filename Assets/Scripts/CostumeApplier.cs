using UnityEngine;

/// <summary>
/// Attach this component to each character's player GameObject (Bits, Thief, Ninja, etc.).
/// On Awake, it reads the equipped costume from CostumeManager and applies the
/// visual overrides (Animator Controller swap).
///
/// The character's default Animator Controller is cached on Awake so you can
/// always revert to the original look.
/// </summary>
public class CostumeApplier : MonoBehaviour
{
    [Header("Character Identity")]
    [Tooltip("Must match the characterID used in CostumeData, e.g. 'Bits', 'Thief'")]
    public string characterID;

    // Cached defaults so we can revert
    private RuntimeAnimatorController defaultAnimatorController;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Cache defaults
        if (animator != null)
            defaultAnimatorController = animator.runtimeAnimatorController;

        // Apply the equipped costume (if any)
        ApplyEquippedCostume();
    }

    /// <summary>
    /// Reads the currently equipped costume from CostumeManager and applies it.
    /// Call this again at runtime if the player changes costumes mid-session.
    /// </summary>
    public void ApplyEquippedCostume()
    {
        if (CostumeManager.Instance == null)
            return;

        CostumeData costume = CostumeManager.Instance.GetEquippedCostume(characterID);

        if (costume != null)
        {
            ApplyCostume(costume);
        }
        else
        {
            RevertToDefault();
        }
    }

    /// <summary>
    /// Applies a specific costume's visuals to this character.
    /// </summary>
    public void ApplyCostume(CostumeData costume)
    {
        if (costume == null) return;

        // Swap Animator Controller if the costume provides one
        if (costume.animatorOverride != null && animator != null)
        {
            animator.runtimeAnimatorController = costume.animatorOverride;
        }

        Debug.Log($"Applied costume '{costume.displayName}' to {characterID}");
    }

    /// <summary>
    /// Reverts this character to their default visuals.
    /// </summary>
    public void RevertToDefault()
    {
        if (animator != null && defaultAnimatorController != null)
        {
            animator.runtimeAnimatorController = defaultAnimatorController;
        }
    }
}
