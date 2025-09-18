using UnityEngine;
using TMPro;

public class CoinsManager : MonoBehaviour
{
    public static CoinsManager Instance;

    public int totalCoins = 0;               // coin count (resets each level)
    public TextMeshProUGUI coinsText;        // drag your CoinsText (TMP) here

    void Awake()
    {
        // Simple singleton (no persistence between scenes)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        Debug.Log("Coins: " + totalCoins);

        if (coinsText != null)
            coinsText.text = "Coins: " + totalCoins;
    }
}
