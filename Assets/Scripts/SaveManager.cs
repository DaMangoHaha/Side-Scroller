using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager that handles clearing all saved player data.
/// Persists across scene loads via <see cref="DontDestroyOnLoad"/>.
/// </summary>
public class SaveManager : MonoBehaviour
{
    /// <summary>
    /// Global singleton instance, accessible from any script.
    /// </summary>
    public static SaveManager Instance;

    void Awake()
    {
        // Ensure only one SaveManager exists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive between scene loads
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    /// <summary>
    /// Resets all player progress: PlayerPrefs, the JSON save file,
    /// currency totals, and shop button states.
    /// </summary>
    public void ClearAllData()
    {
        // Ensure the game is unpaused so systems can reset properly
        Time.timeScale = 1f;

        // Wipe all key-value pairs stored in Unity's PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Delete the JSON save file that stores best times, etc.
        SaveSystem.DeleteSave();

        // Reset in-game currency counters to zero
        if (CoinsManager.Instance != null)
            CoinsManager.Instance.SetCoins(0);

        if (CupidCoinsManager.Instance != null)
            CupidCoinsManager.Instance.SetCoins(0);

        // Refresh shop UIs so purchased items no longer appear as owned
        CharacterShopManager.Instance.RefreshAllButtons();

        if (AugmentShopManager.Instance != null)
            AugmentShopManager.Instance.RefreshAllButtons();

        Debug.Log("ALL DATA CLEARED");
    }
}
