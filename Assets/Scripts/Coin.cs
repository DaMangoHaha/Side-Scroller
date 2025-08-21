using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType { Bronze, Silver, Gold }
    public CoinType coinType;

    private int energyValue;
    private int scoreValue;

    void Start()
    {
        switch (coinType)
        {
            case CoinType.Bronze:
                energyValue = 5;
                scoreValue = 1;
                break;
            case CoinType.Silver:
                energyValue = 10;
                scoreValue = 3;
                break;
            case CoinType.Gold:
                energyValue = 20;
                scoreValue = 5;
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEnergy energy = other.GetComponent<PlayerEnergy>();
            if (energy != null)
            {
                energy.currentEnergy = Mathf.Clamp(
                    energy.currentEnergy + energyValue,
                    0, energy.maxEnergy
                );
            }

            PlayerScore score = other.GetComponent<PlayerScore>();
            if (score != null)
            {
                score.AddScore(scoreValue);
            }

            Destroy(gameObject); // remove coin after collection
        }
    }
}
