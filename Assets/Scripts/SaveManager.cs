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
        Debug.Log("CLEARING ALL PLAYER DATA");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Optional: Reset runtime systems immediately
        if (CoinsManager.Instance != null)
            CoinsManager.Instance.SetCoins(0);

        if (CharacterEquipManager.Instance != null)
            CharacterEquipManager.Instance.EquipCharacter("Bits");

        Debug.Log("Data reset complete.");
    }
}
