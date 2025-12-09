using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    // The currently selected character name
    public string selectedCharacter = "Bits"; // default

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved preference (if exists)
            selectedCharacter = PlayerPrefs.GetString("SelectedCharacter", "Bits");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCharacter(string characterName)
    {
        selectedCharacter = characterName;
        PlayerPrefs.SetString("SelectedCharacter", characterName);
        PlayerPrefs.Save();
    }
}
