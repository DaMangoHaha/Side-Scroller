using UnityEngine;

public class Coin : MonoBehaviour
{
    public enum CoinType { Bronze, Silver, Gold }
    public CoinType coinType;

    private int scoreValue;

    void Start()
    {
        switch (coinType)
        {
            case CoinType.Bronze:
                scoreValue = 1;
                break;
            case CoinType.Silver:
                scoreValue = 3;
                break;
            case CoinType.Gold:
                scoreValue = 5;
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerScore score = other.GetComponent<PlayerScore>();
            if (score != null)
            {
                score.AddScore(scoreValue);
            }

            Destroy(gameObject); // remove coin after collection
        }
    }
}

