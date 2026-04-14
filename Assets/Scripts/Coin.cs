using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType { Bronze, Silver, Gold, Platnium, Emerald, Diamond }
    public CoinType coinType;

    [Header("VFX")]
    public GameObject collectVFXPrefab; // drag your particle prefab here

    private int coinValue;   // how many coins to add
    private int scoreValue;  // how many score points to add


    void Start()
    {
        // Set coin + score values
        switch (coinType)
        {
            case CoinType.Bronze:
                coinValue = 1;
                scoreValue = 1;
                break;
            case CoinType.Silver:
                coinValue = 2;
                scoreValue = 3;
                break;
            case CoinType.Gold:
                coinValue = 3;
                scoreValue = 5;
                break;
            case CoinType.Platnium:
                coinValue = 4;
                scoreValue = 10;
                break;
            case CoinType.Emerald:
                coinValue = 5;
                scoreValue = 15;
                break;
            case CoinType.Diamond:
                coinValue = 6;
                scoreValue = 20;
                break;

        }


    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Check ThiefSkill for upgrade bonuses
            ThiefSkill thiefSkill = other.GetComponent<ThiefSkill>();

            int effectiveCoinValue = coinValue;

            // Calculate the final display value including the CoinsManager multiplier
            int displayCoinValue = effectiveCoinValue;
            if (CoinsManager.Instance != null)
            {
                displayCoinValue = Mathf.RoundToInt(effectiveCoinValue * CoinsManager.Instance.GetCoinMultiplier());
            }

            // Show floating "+X Coin" popup at the coin's position (with multiplier applied)
            CoinPopup.Create(transform.position, displayCoinValue);

            // Add coins to counter (multiplier is applied inside AddCoins)
            if (CoinsManager.Instance != null)
                CoinsManager.Instance.AddCoins(effectiveCoinValue);
            SoundManager.Instance.PlaySound2D("Coin");

            // Add to per-level score
            PlayerScore score = other.GetComponent<PlayerScore>();
            if (score != null)
            {
                score.AddScore(scoreValue);

                // Tier 2: bonus +50 score when collected during active skill
                if (thiefSkill != null)
                {
                    int bonusScore = thiefSkill.GetBonusCoinScore();
                    if (bonusScore > 0)
                        score.AddScore(bonusScore);
                }
            }

            // Tier 3: coins collected during Sticky Fingers grant +1 energy
            if (thiefSkill != null && thiefSkill.ShouldGrantEnergy())
            {
                PlayerEnergy energy = other.GetComponent<PlayerEnergy>();
                if (energy != null)
                {
                    energy.RestoreEnergy(thiefSkill.GetEnergyPerCoin());
                }
            }

            // Spawn collection VFX
            if (collectVFXPrefab != null)
            {
                GameObject vfx = Instantiate(collectVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2f); // auto-cleanup after 2 seconds
            }

            // Remove coin
            Destroy(gameObject);
        }
    }
}
