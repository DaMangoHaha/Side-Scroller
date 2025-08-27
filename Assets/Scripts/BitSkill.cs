using UnityEngine;
using UnityEngine.UI;

public class BitSkill : MonoBehaviour
{
    public float buffCooldown = 10f;   // time to fully charge
    public float warningTime = 3f;     // twinkle before buff
    private float timer = 0f;

    private PlayerEnergy playerEnergy;
    private SpriteRenderer spriteRenderer;
    private bool isWarning = false;

    [Header("UI")]
    public Image shieldIcon; // drag the ShieldIcon here in Inspector
    private Color inactiveColor;
    private Color activeColor;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (shieldIcon != null)
        {
            activeColor = shieldIcon.color;
            inactiveColor = shieldIcon.color;
            inactiveColor.a = 0.2f; // faded look
            shieldIcon.color = inactiveColor;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Start warning twinkle 3s before buff
        if (!isWarning && timer >= buffCooldown - warningTime)
        {
            isWarning = true;
            StartCoroutine(TwinkleYellow());
        }

        // Activate buff
        if (timer >= buffCooldown)
        {
            ActivateBuff();
            timer = 0f;
            isWarning = false;
        }
    }

    void ActivateBuff()
    {
        playerEnergy.hasBitBuff = true;
        Debug.Log("Bit Buff Ready! Next spike damage is halved.");

        // Show shield icon as active
        if (shieldIcon != null)
            shieldIcon.color = activeColor;
    }

    public void ConsumeBuff()
    {
        // Called by PlayerEnergy when the buff is used
        if (shieldIcon != null)
            shieldIcon.color = inactiveColor;
    }

    private System.Collections.IEnumerator TwinkleYellow()
    {
        Color original = spriteRenderer.color;
        Color twinkle = Color.yellow;

        float flashInterval = 0.3f;
        float elapsed = 0f;

        while (elapsed < warningTime)
        {
            spriteRenderer.color = twinkle;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = original;
            yield return new WaitForSeconds(flashInterval);

            elapsed += flashInterval * 2;
        }

        spriteRenderer.color = original;
    }
}


