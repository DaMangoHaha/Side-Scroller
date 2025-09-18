using UnityEngine;
using TMPro;

public class GlobalCoinsUI : MonoBehaviour
{
    public TextMeshProUGUI coinsText;

    void Update()
    {
        if (CoinsManager.Instance != null)
        {
            coinsText.text = "Coins: " + CoinsManager.Instance.totalCoins;
        }
    }
}

