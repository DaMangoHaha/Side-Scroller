using UnityEngine;
using TMPro;

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
        Debug.Log("CoinsUI updating: " + amount);
    }
}

