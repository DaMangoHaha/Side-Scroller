using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ClearAllData()
    {
        Time.timeScale = 1f;

        // Delete the JSON save file
        SaveSystem.DeleteSave();

        // Reset CoinsManager if it exists
        if (CoinsManager.Instance != null)
            CoinsManager.Instance.SetCoins(0);

        // Refresh UI
        if (CharacterShopManager.Instance != null)
            CharacterShopManager.Instance.RefreshAllButtons();

        // Load main menu
        SceneManager.LoadScene("MainMenu");

        Debug.Log("ALL DATA CLEARED - JSON save file deleted");
    }
}
