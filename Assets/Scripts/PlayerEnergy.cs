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

    private Color _originalColor;
    public Color OriginalColor => _originalColor;
    private Color originalFillColor;

    // Active tint override — when set, invulnerability flashes use this color
    // instead of _originalColor so buff tints (e.g. Chill Wind) persist through damage.
    private bool _hasTintOverride = false;
    private Color _tintOverride;

    /// <summary>
    /// Returns the color that should be treated as the current "base" color.
    /// If a tint override is active (e.g. Chill Wind), returns that; otherwise returns the true original.
    /// </summary>
    public Color ActiveBaseColor => _hasTintOverride ? _tintOverride : _originalColor;

    /// <summary>
    /// Sets a temporary tint override. While active, invulnerability flashes
    /// will restore to this color instead of the original sprite color.
    /// </summary>
    public void SetTintOverride(Color tint)
    {
        _hasTintOverride = true;
        _tintOverride = tint;
    }

    /// <summary>
    /// Clears the tint override so invulnerability restores the true original color.
    /// </summary>
    public void ClearTintOverride()
    {
        _hasTintOverride = false;
    }

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

    // --- Chill Wind Buff ---
    [Header("Chill Wind (Crystal Tier 3)")]
    [HideInInspector] public float chillWindDamageReduction = 0f;   // 0 = no reduction, 0.2 = 20% reduction
    [HideInInspector] public float depletionRateMultiplier = 1f;    // 1 = normal, 0.5 = 50% slower
    [HideInInspector] public float maxEnergyMultiplier = 1f;        // 1 = normal, 1.25 = +25%

    // --- Player Buff Manager ---
    [HideInInspector] public float playerBuffDamageReduction = 0f;  // 0 = no reduction, stacks from PlayerBuffManager

    void Start()
    {
        currentEnergy = maxEnergy;

        if (energySlider != null)
            energySlider.maxValue = maxEnergy;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            _originalColor = spriteRenderer.color;

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
            currentEnergy -= depletionRate * depletionRateMultiplier * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy * maxEnergyMultiplier);
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

        // Apply Chill Wind damage reduction (Crystal Tier 3)
        if (chillWindDamageReduction > 0f)
        {
            amount *= (1f - chillWindDamageReduction);
            Debug.Log("Chill Wind reduced damage by " + (chillWindDamageReduction * 100f) + "%!");
        }

        // Apply Player Buff defense reduction (from PlayerBuffManager)
        if (playerBuffDamageReduction > 0f)
        {
            amount *= (1f - playerBuffDamageReduction);
            Debug.Log("Player Buff reduced damage by " + (playerBuffDamageReduction * 100f) + "%!");
        }

        // Show floating damage popup
        CoinPopup.CreateDamage(transform.position, amount);

        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy * maxEnergyMultiplier);
        UpdateUI();

        // Start invulnerability period
        StartCoroutine(TemporaryInvulnerability());

        if (currentEnergy <= 0)
            GameOver();
    }

    /// <summary>
    /// Deals damage that bypasses invulnerability and does NOT trigger i-frames.
    /// Used by status effects like Burning that tick through invincibility.
    /// </summary>
    public void TakeBurnDamage(float amount)
    {
        // Apply Chill Wind damage reduction (Crystal Tier 3) — burn can still be reduced
        if (chillWindDamageReduction > 0f)
        {
            amount *= (1f - chillWindDamageReduction);
        }

        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy * maxEnergyMultiplier);
        UpdateUI();

        if (currentEnergy <= 0)
            GameOver();
    }

    public void UpdateUI()
    {
        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy * maxEnergyMultiplier;
            energySlider.value = currentEnergy;
        }
    }

    // Invulnerability Coroutine
    private IEnumerator TemporaryInvulnerability()
    {
        isInvulnerable = true;
        Debug.Log("Player is now invulnerable!");

        float actualDuration = invulnerabilityDuration;

        float elapsed = 0f;

        while (elapsed < actualDuration)
        {
            // Fade to 50% transparency — use ActiveBaseColor so buff tints
            // (e.g. Chill Wind) are preserved during the flash
            Color c = ActiveBaseColor;
            c.a = 0.5f;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.1f);

            // Fade back to full opacity
            c = ActiveBaseColor;
            c.a = 1f;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        // Reset to the current base color with full opacity
        spriteRenderer.color = ActiveBaseColor;
        isInvulnerable = false;

        Debug.Log("Invulnerability ended.");
    }

    public void GameOver()
    {
        Debug.Log("Energy depleted! Run complete.");

        string levelName = SceneManager.GetActiveScene().name;

        // Save the current scene name before switching
        PlayerPrefs.SetString("LastLevel", levelName);

        // Gather final score
        PlayerScore playerScore = GetComponent<PlayerScore>();
        int finalScore = (playerScore != null) ? playerScore.score : 0;
        PlayerPrefs.SetInt("LastScore", finalScore);

        // Gather survival time
        float timeSurvived = 0f;
        if (LevelTimer.Instance != null)
        {
            timeSurvived = LevelTimer.Instance.GetElapsedTime();
            LevelTimer.Instance.StopTimer();
        }
        PlayerPrefs.SetFloat("LastTimeSurvived", timeSurvived);

        // Calculate stars (requires LevelVictoryData in the scene)
        int stars = 0;
        if (LevelVictoryData.Instance != null)
            stars = LevelVictoryData.Instance.CalculateStars(finalScore, timeSurvived);
        PlayerPrefs.SetInt("LastStars", stars);

        PlayerPrefs.Save();

        // Load the Victory scene instead of Game Over
        SceneManager.LoadScene("Victory");
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
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy * maxEnergyMultiplier);
        UpdateUI();
    }

    /// <summary>
    /// Grants the player invulnerability for the specified duration.
    /// Can be called by external scripts (e.g., WizKid Tier 3).
    /// </summary>
    public void GrantInvulnerability(float duration)
    {
        StartCoroutine(ExternalInvulnerability(duration));
    }

    private IEnumerator ExternalInvulnerability(float duration)
    {
        isInvulnerable = true;
        Debug.Log("Player granted invulnerability for " + duration + " seconds!");

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Use ActiveBaseColor so buff tints are preserved during the flash
            Color c = ActiveBaseColor;
            c.a = 0.5f;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.1f);

            c = ActiveBaseColor;
            c.a = 1f;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.1f);

            elapsed += 0.2f;
        }

        // Reset to the current base color with full opacity
        spriteRenderer.color = ActiveBaseColor;
        isInvulnerable = false;

        Debug.Log("Granted invulnerability ended.");
    }
}
