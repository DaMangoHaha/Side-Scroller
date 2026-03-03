using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float depletionRate = 5f; // per second
    public bool isDepleting = true; // allows potions to pause depletion

    [Header("UI")]
    public Slider energySlider;
    public Image energyFill; // assign Fill Area > Fill image

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer; // assign your player's sprite

    private Color originalColor;
    private Color originalFillColor;

    [Header("Bit Buff")]
    public bool hasBitBuff = false; // Is the skill active?
    public float damageReduction = 0.5f; // 50% damage reduction
    public int bitBuffStacks = 0; // number of active buff stacks

    // Invulnerability Variables
    [Header("Invulnerability Settings")]
    public float invulnerabilityDuration = 2f;
    private bool isInvulnerable = false;

    // Cubit Passive Reference
    private CubitPassive cubitPassive;

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

        // Check if Cubit passive exists
        cubitPassive = GetComponent<CubitPassive>();

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
        // Prevent damage if currently invulnerable
        if (isInvulnerable)
        {
            Debug.Log("Damage ignored — player is invulnerable!");
            return;
        }

        // Check Cubit's Protection Protocol
        if (cubitPassive != null && cubitPassive.IsProtectionActive())
        {
            amount = cubitPassive.ProcessDamage(amount);
        }

        // Check Bit Buff (all stacks consumed on any hit)
        if (hasBitBuff && bitBuffStacks > 0)
        {
            amount *= damageReduction;
            hasBitBuff = false;
            bitBuffStacks = 0;

            BitSkill skill = GetComponent<BitSkill>();
            if (skill != null)
            {
                SoundManager.Instance.PlaySound2D("BitBuffDamage");
                skill.ConsumeBuff();
            }

            Debug.Log("Bit Buff activated! All stacks consumed. Damage reduced.");
        }

        // Show floating damage popup
        CoinPopup.CreateDamage(transform.position, amount);

        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy);
        UpdateUI();

        // Start invulnerability period
        StartCoroutine(TemporaryInvulnerability());

        if (currentEnergy <= 0)
            GameOver();
    }

    public void UpdateUI()
    {
        if (energySlider != null)
            energySlider.value = currentEnergy;
    }

    // Invulnerability Coroutine
    private IEnumerator TemporaryInvulnerability()
    {
        isInvulnerable = true;
        Debug.Log("Player is now invulnerable!");

        float actualDuration = invulnerabilityDuration;

        float elapsed = 0f;
        Color tempColor = spriteRenderer.color;

        while (elapsed < actualDuration)
        {
            // Fade to 50% transparency
            tempColor.a = 0.5f;
            spriteRenderer.color = tempColor;
            yield return new WaitForSeconds(0.1f);

            // Fade back to full opacity
            tempColor.a = 1f;
            spriteRenderer.color = tempColor;
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        // Reset transparency and end invulnerability
        tempColor.a = 1f;
        spriteRenderer.color = tempColor;
        isInvulnerable = false;

        Debug.Log("Invulnerability ended.");
    }

    public void GameOver()
    {
        Debug.Log("Energy depleted! You suck.");

        // Save the current scene name before switching
        PlayerPrefs.SetString("LastLevel", SceneManager.GetActiveScene().name);

        // Load the Game Over scene
        SceneManager.LoadScene("GameOver");
    }

    // Potion Support
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

    public void RestoreEnergy(float amount)
    {
        CoinPopup.CreateEnergy(transform.position, amount);
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        UpdateUI();
    }
}
