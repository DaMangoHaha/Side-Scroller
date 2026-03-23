using TMPro;
using UnityEngine;
using System.Collections;

public class CoinsManager : MonoBehaviour
{
    public static CoinsManager Instance;

    public int totalCoins;
    public TextMeshProUGUI coinsText;

    // --- Coin Multiplier ---
    private float currentCoinMultiplier = 1f;
    private Coroutine multiplierCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCoins();
        UpdateUI();
    }

    // --------------------
    // Coin Logic
    // --------------------
    public void AddCoins(int amount)
    {
        // Apply the current multiplier to the coin amount
        int multipliedAmount = Mathf.RoundToInt(amount * currentCoinMultiplier);
        totalCoins += multipliedAmount;
        SaveCoins();
        UpdateUI();
    }

    /// <summary>
    /// Returns the current coin multiplier (1.0 if no buff is active).
    /// </summary>
    public float GetCoinMultiplier()
    {
        return currentCoinMultiplier;
    }

    /// <summary>
    /// Returns true if a coin multiplier buff is currently active.
    /// </summary>
    public bool HasCoinMultiplier()
    {
        return currentCoinMultiplier > 1f;
    }

    /// <summary>
    /// Applies a temporary coin multiplier for the specified duration.
    /// If a multiplier is already active, it will be replaced.
    /// </summary>
    public void ApplyCoinMultiplier(float multiplier, float duration)
    {
        // Stop any existing multiplier coroutine
        if (multiplierCoroutine != null)
        {
            StopCoroutine(multiplierCoroutine);
        }

        multiplierCoroutine = StartCoroutine(CoinMultiplierCoroutine(multiplier, duration));
    }

    private IEnumerator CoinMultiplierCoroutine(float multiplier, float duration)
    {
        currentCoinMultiplier = multiplier;
        Debug.Log($"Coin Multiplier x{multiplier} activated for {duration} seconds!");

        yield return new WaitForSeconds(duration);

        currentCoinMultiplier = 1f;
        multiplierCoroutine = null;
        Debug.Log("Coin Multiplier expired.");
    }

    public int GetCoins()
    {
        return totalCoins;
    }

    // --------------------
    // Save / Load
    // --------------------
    private void SaveCoins()
    {
        SaveData data = SaveSystem.LoadData();
        data.totalCoins = totalCoins;
        SaveSystem.SaveData(data);
    }

    private void LoadCoins()
    {
        SaveData data = SaveSystem.LoadData();
        totalCoins = data.totalCoins;
    }

    public void SetCoins(int amount)
    {
        totalCoins = amount;

        if (coinsText != null)
            coinsText.text = "Coins: " + totalCoins;
    }
    
    private void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = "Coins: " + totalCoins;
    }
}
