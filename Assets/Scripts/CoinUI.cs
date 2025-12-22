using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinsText;

    void Start()
    {
        UpdateCoins();
    }

    void Update()
    {
        UpdateCoins();
    }

    void UpdateCoins()
    {
        if (CoinsManager.Instance != null && coinsText != null)
        {
            coinsText.text = CoinsManager.Instance.GetCoins().ToString();
        }
    }
}
