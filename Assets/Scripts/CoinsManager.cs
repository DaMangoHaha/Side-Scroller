using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinsManager : MonoBehaviour
{
    public static CoinsManager Instance;

    public int totalCoins;
    public TextMeshProUGUI coinsText;

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
        totalCoins += amount;
        SaveCoins();
        UpdateUI();
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
