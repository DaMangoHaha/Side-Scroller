using UnityEngine;

public class BitSkill : MonoBehaviour
{
    public float buffCooldown = 10f;   // time to fully charge
    public float warningTime = 3f;     // twinkle before buff
    private float timer = 0f;

    private PlayerEnergy playerEnergy;
    private SpriteRenderer spriteRenderer;
    private bool isWarning = false;

    void Start()
    {
        playerEnergy = GetComponent<PlayerEnergy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Start warning 3s before buff activates
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
    }

    private System.Collections.IEnumerator TwinkleYellow()
    {
        Color original = spriteRenderer.color;
        Color twinkle = Color.yellow;

        float flashInterval = 0.3f;
        float twinkleTime = warningTime;
        float elapsed = 0f;

        while (elapsed < twinkleTime)
        {
            spriteRenderer.color = twinkle;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = original;
            yield return new WaitForSeconds(flashInterval);

            elapsed += flashInterval * 2;
        }

        // restore normal color
        spriteRenderer.color = original;
    }
}

