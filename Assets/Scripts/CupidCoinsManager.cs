using TMPro;
using UnityEngine;

public class CupidCoinsManager : MonoBehaviour
{
    public static CupidCoinsManager Instance;

    public int totalCupidCoins;
    public TextMeshProUGUI cupidCoinsText;

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
    // Cupid Coin Logic
    // --------------------
    public void AddCoins(int amount)
    {
        totalCupidCoins += amount;
        SaveCoins();
        UpdateUI();
    }

    public int GetCoins()
    {
        return totalCupidCoins;
    }

    public void SetCoins(int amount)
    {
        totalCupidCoins = amount;
        SaveCoins();
        UpdateUI();
    }

    // --------------------
    // Save / Load
    // --------------------
    private void SaveCoins()
    {
        PlayerPrefs.SetInt(SaveKeys.CupidCoins, totalCupidCoins);
        PlayerPrefs.Save();
    }

    private void LoadCoins()
    {
        totalCupidCoins = PlayerPrefs.GetInt(SaveKeys.CupidCoins, 0);
    }

    private void UpdateUI()
    {
        if (cupidCoinsText != null)
            cupidCoinsText.text = "Cupid Coins: " + totalCupidCoins;
    }
}