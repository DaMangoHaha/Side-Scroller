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
    public Image skillIcon;
    private Color activeColor;
    private Color inactiveColor;

    public float flickerSpeed = 6f;   // how fast icon flickers

    [Header("Skill Dialogue")]
    public SkillDialogueUI skillDialogueUI;
    [TextArea]
    public string stickyFingersDialogue = "I'll be taking these...";
    public Sprite thiefPortrait;

    void Start()
    {

        cooldownTimer = cooldownTime;

        if (skillIcon != null)
        {
            activeColor = skillIcon.color;
            inactiveColor = activeColor;
            inactiveColor.a = 0.3f;
            skillIcon.color = inactiveColor;  // starts faded
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

        // Activation
        if (!isOnCooldown && Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(ActivateSkill());
            SoundManager.Instance.PlaySound2D("StickyFingers");
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

    void OnDrawGizmosSelected() //This gizmo shows the coin pull radius
    {
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, coinPullRadius);
    }
}

