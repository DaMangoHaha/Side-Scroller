using UnityEngine;

public class CoinsManager : MonoBehaviour
{
    public static CoinsManager Instance;

    public int totalCoins = 0; // persists across scenes

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // survive scene loads
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
    }
}



