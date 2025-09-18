using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public CharacterData[] characters;
    public int selectedCharacterIndex = 0;

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

    public void SelectCharacter(int index)
    {
        if (characters[index].isUnlocked)
        {
            selectedCharacterIndex = index;
            Debug.Log("Selected: " + characters[index].characterName);
        }
        else
        {
            Debug.Log("Character locked!");
        }
    }

    public bool PurchaseCharacter(int index)
    {
        if (!characters[index].isUnlocked && CoinsManager.Instance.totalCoins >= characters[index].cost)
        {
            CoinsManager.Instance.totalCoins -= characters[index].cost;
            characters[index].isUnlocked = true;
            return true;
        }
        return false;
    }
}
