using System.Collections;
using UnityEngine;

/// <summary>
/// Manages temporary status effects (debuffs) applied to the player by enemies.
/// Attach this component to the Player GameObject.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    // ——— Sticky (Green Slime) ———
    [Header("Sticky — Green Slime")]
    public float stickyDuration = 3f;
    public float jumpForceMultiplier = 0.5f; // halves jump force

    // ——— Burning (Red Slime) ———
    [Header("Burning — Red Slime")]
    public float burningDuration = 5f;
    public float burningDamagePerTick = 5f;
    public float burningTickInterval = 1f; // damage every 1 second

    // ——— Soggy (Blue Slime) ———
    [Header("Soggy — Blue Slime")]
    public float soggyDuration = 5f;
    public float inputDelaySeconds = 0.15f; // small delay before actions register

    // Cached player components
    private DoubleJump doubleJump;
    private PlayerEnergy playerEnergy;
    private PlayerFreeMove playerFreeMove;
    private PlayerSlide playerSlide;

    // Track active debuffs so we can prevent stacking
    private Coroutine stickyCoroutine;
    private Coroutine burningCoroutine;
    private Coroutine soggyCoroutine;

    // Public flags so other scripts can check active debuffs
    [HideInInspector] public bool isSticky = false;
    [HideInInspector] public bool isBurning = false;
    [HideInInspector] public bool isSoggy = false;

    void Awake()
    {
        doubleJump = GetComponent<DoubleJump>();
        playerEnergy = GetComponent<PlayerEnergy>();
        playerFreeMove = GetComponent<PlayerFreeMove>();
        playerSlide = GetComponent<PlayerSlide>();
    }

    // --------------------------------------------------
    // Sticky — halves jump force for a few seconds
    // --------------------------------------------------
    public void ApplySticky(Vector3 popupPosition)
    {
        CoinPopup.CreateStatusEffect(popupPosition, "Sticky!", new Color(0.2f, 0.9f, 0.2f, 1f)); // green

        // Restart the debuff if already active (refresh duration)
        if (stickyCoroutine != null)
            StopCoroutine(stickyCoroutine);

        stickyCoroutine = StartCoroutine(StickyRoutine());
    }

    private IEnumerator StickyRoutine()
    {
        if (doubleJump == null) yield break;

        isSticky = true;
        float originalJumpForce = doubleJump.jumpForce;
        doubleJump.jumpForce *= jumpForceMultiplier;

        Debug.Log("Sticky! Jump force halved for " + stickyDuration + "s.");

        yield return new WaitForSeconds(stickyDuration);

        // Restore original jump force
        doubleJump.jumpForce = originalJumpForce;
        isSticky = false;
        stickyCoroutine = null;

        Debug.Log("Sticky effect wore off.");
    }

    // --------------------------------------------------
    // Burning — deals damage over time (bypasses invulnerability)
    // --------------------------------------------------
    public void ApplyBurning(Vector3 popupPosition)
    {
        CoinPopup.CreateStatusEffect(popupPosition, "Burning!", new Color(1f, 0.5f, 0f, 1f)); // orange

        // Restart the debuff if already active (refresh duration)
        if (burningCoroutine != null)
            StopCoroutine(burningCoroutine);

        burningCoroutine = StartCoroutine(BurningRoutine());
    }

    private IEnumerator BurningRoutine()
    {
        if (playerEnergy == null) yield break;

        isBurning = true;
        float elapsed = 0f;

        Debug.Log("Burning! Taking " + burningDamagePerTick + " damage every " + burningTickInterval + "s for " + burningDuration + "s. (Bypasses invulnerability)");

        while (elapsed < burningDuration)
        {
            yield return new WaitForSeconds(burningTickInterval);
            elapsed += burningTickInterval;

            // Show a small burn tick popup each time
            CoinPopup.CreateDamage(transform.position, burningDamagePerTick);

            // Use TakeBurnDamage to bypass invulnerability and skip triggering i-frames
            playerEnergy.TakeBurnDamage(burningDamagePerTick);
        }

        isBurning = false;
        burningCoroutine = null;

        Debug.Log("Burning effect wore off.");
    }

    // --------------------------------------------------
    // Soggy — adds input delay to all actions
    // --------------------------------------------------
    public void ApplySoggy(Vector3 popupPosition)
    {
        CoinPopup.CreateStatusEffect(popupPosition, "Soggy!", new Color(0.2f, 0.5f, 1f, 1f)); // blue

        // Restart the debuff if already active (refresh duration)
        if (soggyCoroutine != null)
            StopCoroutine(soggyCoroutine);

        soggyCoroutine = StartCoroutine(SoggyRoutine());
    }

    private IEnumerator SoggyRoutine()
    {
        isSoggy = true;

        Debug.Log("Soggy! Input delayed by " + inputDelaySeconds + "s for " + soggyDuration + "s.");

        yield return new WaitForSeconds(soggyDuration);

        isSoggy = false;
        soggyCoroutine = null;

        Debug.Log("Soggy effect wore off.");
    }

    /// <summary>
    /// Returns the current input delay in seconds (0 if no Soggy debuff is active).
    /// Called by DoubleJump, PlayerFreeMove, and PlayerSlide to throttle input.
    /// </summary>
    public float GetInputDelay()
    {
        return isSoggy ? inputDelaySeconds : 0f;
    }
}