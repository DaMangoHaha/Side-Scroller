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

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (CoinsManager.Instance != null)
            CoinsManager.Instance.SetCoins(0);

        if (CupidCoinsManager.Instance != null)
            CupidCoinsManager.Instance.SetCoins(0);

        CharacterShopManager.Instance.RefreshAllButtons();

        Debug.Log("ALL DATA CLEARED");
    }

}
