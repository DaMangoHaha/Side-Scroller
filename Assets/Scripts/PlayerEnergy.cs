using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float depletionRate = 5f; // per second
    private bool isDepleting = true; // allows potions to pause depletion

    [Header("UI")]
    public Slider energySlider;
    public Image energyFill; // assign Fill Area > Fill image

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer; // assign your player’s sprite
    public Color hurtColor = Color.red;
    public float flashDuration = 0.2f;

    private Color originalColor;
    private Color originalFillColor;
    private bool isFlashing = false;

    [Header("Bit Buff")]
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

        if (energyFill != null)
            originalFillColor = energyFill.color;

        UpdateUI();
    }

    void Update()
    {
        if (isDepleting)
        {
            currentEnergy -= depletionRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            UpdateUI();

            if (currentEnergy <= 0)
                GameOver();
        }
    }

    public void TakeDamage(float amount)
    {
        if (hasBitBuff)
        {
            amount *= damageReduction;
            hasBitBuff = false;

            BitSkill skill = GetComponent<BitSkill>();
            if (skill != null)
                skill.ConsumeBuff();

            Debug.Log("Bit Buff activated! Damage reduced.");
        }

        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy);
        UpdateUI();

        if (!isFlashing)
            StartCoroutine(FlashRed());

        if (currentEnergy <= 0)
            GameOver();
    }

    public void UpdateUI()
    {
        if (energySlider != null)
            energySlider.value = currentEnergy;
    }


    private IEnumerator FlashRed()
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

    // --- Potion Support ---
    public void PauseDepletion(float duration)
    {
        StartCoroutine(PauseEnergyCoroutine(duration));
    }

    private IEnumerator PauseEnergyCoroutine(float duration)
    {
        isDepleting = false;

        if (energyFill != null)
            energyFill.color = Color.green;

        Debug.Log("Energy depletion paused for " + duration + " seconds!");
        yield return new WaitForSeconds(duration);

        isDepleting = true;

        if (energyFill != null)
            energyFill.color = originalFillColor;

        Debug.Log("Energy depletion resumed.");
    }
}




