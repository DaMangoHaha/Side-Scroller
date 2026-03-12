using System.Collections;
using UnityEngine;

/// <summary>
/// In-level scripted intro sequence for Level 5.
///
/// Story: Bits is tied up in The Wiz's dungeon. After a few seconds Wiz Kid
/// walks in from off-screen, stops behind Bits, and uses magic (a particle
/// system prefab) to untie him. Once the untying animation finishes the
/// particles are destroyed and the player regains control.
///
/// Setup in the Level 5 scene:
/// 1. Place Bits at his starting position. His Animator should have:
///    - A "TiedIdle" state (default / entry state for the intro).
///    - An "Untying" state triggered by a bool parameter called "Untying".
///    - After "Untying" finishes, transition to the normal idle state.
///      Use the bool parameter "Untying" = false as the exit condition,
///      OR use an AnimatorEvent at the end of the clip — this script
///      waits for the "Untying" state to finish based on normalizedTime.
/// 2. Place Wiz Kid off-screen (e.g. to the right). His Animator can stay
///    in a walk cycle; this script will stop him at the target position.
/// 3. Assign fields in the Inspector.
/// 4. Attach this script to any GameObject in the scene (e.g. an empty
///    "Level5Intro" object).
/// </summary>
public class Level5DungeonIntro : MonoBehaviour
{
    [Header("Character References")]
    [Tooltip("The player character (Bits). Must have PlayerFreeMove and Animator.")]
    public GameObject bitsObject;

    [Tooltip("Wiz Kid NPC who walks in to free Bits. Must have an Animator.")]
    public GameObject wizKidObject;

    [Header("Wiz Kid Walk Settings")]
    [Tooltip("The world position where Wiz Kid stops (behind Bits).")]
    public Transform wizKidTargetPosition;

    [Tooltip("Speed at which Wiz Kid walks toward the target position.")]
    public float wizKidWalkSpeed = 3f;

    [Header("Magic Particle Effect")]
    [Tooltip("Particle system prefab to spawn around Bits when Wiz Kid casts magic.")]
    public GameObject magicParticlePrefab;

    [Tooltip("Local offset from Bits' position for the particle effect.")]
    public Vector3 particleOffset = Vector3.zero;

    [Header("Timing")]
    [Tooltip("Seconds Bits stays in TiedIdle before Wiz Kid starts walking in.")]
    public float tiedIdleWaitTime = 3f;

    [Tooltip("Optional extra pause after Wiz Kid arrives and before the particles / untying start.")]
    public float preUntieDelay = 0.5f;

    [Tooltip("Optional extra pause after the untying animation before the player regains control.")]
    public float postUntieDelay = 0.5f;

    // Cached components
    private PlayerFreeMove playerFreeMove;
    private Animator bitsAnimator;
    private Animator wizKidAnimator;

    // Runtime particle instance
    private GameObject particleInstance;

    void Start()
    {
        // Cache components
        if (bitsObject != null)
        {
            playerFreeMove = bitsObject.GetComponent<PlayerFreeMove>();
            bitsAnimator = bitsObject.GetComponent<Animator>();
        }

        if (wizKidObject != null)
        {
            wizKidAnimator = wizKidObject.GetComponent<Animator>();
        }

        // Disable player control immediately
        if (playerFreeMove != null)
            playerFreeMove.enabled = false;

        // Kick off the intro sequence
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // ---------------------------------------------------------------
        // STEP 1: Bits plays "TiedIdle" — player has no control
        // ---------------------------------------------------------------
        if (bitsAnimator != null)
            bitsAnimator.Play("TiedIdle");

        yield return new WaitForSeconds(tiedIdleWaitTime);

        // ---------------------------------------------------------------
        // STEP 2: Wiz Kid walks into frame and stops behind Bits
        // ---------------------------------------------------------------
        if (wizKidObject != null && wizKidTargetPosition != null)
        {
            // Make sure Wiz Kid faces the correct direction (toward Bits)
            FaceTarget(wizKidObject.transform, wizKidTargetPosition.position);

            // Start walk animation if available
            if (wizKidAnimator != null)
                wizKidAnimator.SetBool("isWalking", true);

            // Move Wiz Kid toward the target position
            while (Vector2.Distance(wizKidObject.transform.position, wizKidTargetPosition.position) > 0.05f)
            {
                wizKidObject.transform.position = Vector2.MoveTowards(
                    wizKidObject.transform.position,
                    wizKidTargetPosition.position,
                    wizKidWalkSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Snap to exact position
            wizKidObject.transform.position = wizKidTargetPosition.position;

            // Stop walk animation
            if (wizKidAnimator != null)
                wizKidAnimator.SetBool("isWalking", false);
        }

        // Short pause before the magic starts
        if (preUntieDelay > 0f)
            yield return new WaitForSeconds(preUntieDelay);

        // ---------------------------------------------------------------
        // STEP 2b: Spawn magic particle effect around Bits
        // ---------------------------------------------------------------
        if (magicParticlePrefab != null && bitsObject != null)
        {
            Vector3 spawnPos = bitsObject.transform.position + particleOffset;
            particleInstance = Instantiate(magicParticlePrefab, spawnPos, Quaternion.identity, bitsObject.transform);
        }

        // ---------------------------------------------------------------
        // STEP 3: Bits plays "Untying" animation while particles stay
        // ---------------------------------------------------------------
        if (bitsAnimator != null)
        {
            bitsAnimator.SetBool("Untying", true);

            // Wait one frame so the Animator transitions into the Untying state
            yield return null;

            // Wait until the Untying animation finishes (normalizedTime >= 1)
            // We check for the "Untying" state on layer 0.
            bool waiting = true;
            while (waiting)
            {
                AnimatorStateInfo stateInfo = bitsAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Untying") && stateInfo.normalizedTime >= 1f)
                {
                    waiting = false;
                }
                yield return null;
            }

            bitsAnimator.SetBool("Untying", false);
        }

        // ---------------------------------------------------------------
        // STEP 4: Destroy particles, give the player control
        // ---------------------------------------------------------------
        if (particleInstance != null)
            Destroy(particleInstance);

        // Transition Bits into his normal Idle state so the Animator
        // doesn't fall back to TiedIdle when PlayerFreeMove re-enables.
        if (bitsAnimator != null)
            bitsAnimator.Play("Idle");

        // Optional short pause so the transition feels smooth
        if (postUntieDelay > 0f)
            yield return new WaitForSeconds(postUntieDelay);

        // Re-enable player movement
        if (playerFreeMove != null)
            playerFreeMove.enabled = true;

        Debug.Log("Level5DungeonIntro: Intro complete — player has control.");
    }

    /// <summary>
    /// Flips the character's localScale.x so it faces toward <paramref name="targetPos"/>.
    /// Uses the same flip convention as PlayerFreeMove (negative scale.x = facing left).
    /// </summary>
    private void FaceTarget(Transform character, Vector3 targetPos)
    {
        if (character == null) return;

        float direction = targetPos.x - character.position.x;
        Vector3 scale = character.localScale;

        // Positive scale.x = facing right; negative = facing left
        if (direction < 0f && scale.x > 0f)
            scale.x *= -1f;
        else if (direction > 0f && scale.x < 0f)
            scale.x *= -1f;

        character.localScale = scale;
    }
}
