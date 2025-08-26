using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float depletionRate = 5f; // per second

    [Header("UI")]
    public Slider energySlider;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer; // assign your player’s sprite
    public Color hurtColor = Color.red;
    public float flashDuration = 0.2f;

    private Color originalColor;
    private bool isFlashing = false;

    public bool hasBitBuff = false; // Is the skill active?
    public float damageReduction = 0.5f; // 50% damage reduction

    void Start()
    {
        currentEnergy = maxEnergy;

        if (energySlider != null)
            energySlider.maxValue = maxEnergy;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    void Update()
    {
        // Deplete energy over time
        currentEnergy -= depletionRate * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        if (energySlider != null)
            energySlider.value = currentEnergy;

        if (currentEnergy <= 0)
        {
            GameOver();
        }
    }

    public void TakeDamage(float amount)
    {
        if (hasBitBuff)
        {
            amount *= damageReduction; // Reduces damage
            hasBitBuff = false; // Buff is consumed
            Debug.Log("Bit Buff activated! Damage reduced.");
        }
        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy);

        if (!isFlashing)
            StartCoroutine(FlashRed());

        if (currentEnergy <= 0)
            GameOver();
    }

    private System.Collections.IEnumerator FlashRed()
    {
        isFlashing = true;
        spriteRenderer.color = hurtColor;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    void GameOver()
    {
        Debug.Log("Energy depleted! You suck.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

