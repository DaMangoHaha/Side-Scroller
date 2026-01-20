using TMPro;
using UnityEngine;

public class CoinsUI : MonoBehaviour
{
    private TextMeshProUGUI coinsText;

    void Start()
    {
        coinsText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        int amount = (CoinsManager.Instance != null) ? CoinsManager.Instance.totalCoins : -999;
        coinsText.text = "Coins: " + amount;
    }
}

