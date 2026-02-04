using TMPro;
using UnityEngine;

public class CupidCoinsUI : MonoBehaviour
{
    private TextMeshProUGUI cupidCoinsText;

    void Start()
    {
        cupidCoinsText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        int amount = (CupidCoinsManager.Instance != null) ? CupidCoinsManager.Instance.totalCupidCoins : -999;
        cupidCoinsText.text = "Cupid Coins: " + amount;
    }
}