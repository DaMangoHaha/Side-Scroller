using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CoinsManager : MonoBehaviour
{
    public static CoinsManager Instance;

    public int totalCoins;

    private const string COINS_KEY = "TotalCoins";

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
    }

    // --------------------
    // Coin Logic
    // --------------------
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        SaveCoins();
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
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);
        PlayerPrefs.Save();
    }

    private void LoadCoins()
    {
        totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }
}
