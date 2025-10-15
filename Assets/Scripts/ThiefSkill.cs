using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    public Image skillIcon;               // Assign icon in Inspector
    private Color activeColor;
    private Color inactiveColor;

    private AudioSource audioSource;
    public AudioClip activateSFX;         // Optional activation sound

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        cooldownTimer = cooldownTime; // starts on cooldown
        if (skillIcon != null)
        {
            activeColor = skillIcon.color;
            inactiveColor = skillIcon.color;
            inactiveColor.a = 0.3f;
            skillIcon.color = inactiveColor;
        }
    }

    void Update()
    {
        // Cooldown timer
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

        // Activation input
        if (!isOnCooldown && Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(ActivateSkill());
        }

        // Active effect: pull coins
        if (isActive)
        {
            AttractNearbyCoins();
        }
    }

    private IEnumerator ActivateSkill()
    {
        isActive = true;
        isOnCooldown = true;
        cooldownTimer = cooldownTime;

        if (skillIcon != null)
            skillIcon.color = inactiveColor;

        if (activateSFX != null && audioSource != null)
            audioSource.PlayOneShot(activateSFX);

        Debug.Log("Sticky Fingers Activated!");

        yield return new WaitForSeconds(activeDuration);

        isActive = false;
        Debug.Log("Sticky Fingers ended.");
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

    // Called by Coin.cs when Thief collects a coin
    public void ReduceCooldown(float amount)
    {
        if (isOnCooldown && cooldownTimer > 0)
        {
            cooldownTimer -= amount;
            if (cooldownTimer < 0)
                cooldownTimer = 0;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, coinPullRadius);
    }
}

