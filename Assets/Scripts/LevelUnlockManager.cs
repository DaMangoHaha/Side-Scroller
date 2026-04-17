using UnityEngine;

public class LevelUnlockManager : MonoBehaviour
{
    public static LevelUnlockManager Instance;

    // 0 = locked, 1 = unlocked
    public bool level1Unlocked = true;
    public bool level2Unlocked = false;
    public bool level3Unlocked = false;
    public bool level4Unlocked = false;
    public bool level5Unlocked = false;
    public bool level6Unlocked = false;
    public bool level7Unlocked = false;

    // The minimum time (in seconds) a player must survive in Level 1 to unlock other levels
    public float requiredLevel1Time = 60f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadUnlockState();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads the unlock state from saved data.
    /// If the player has previously met the 1-minute Level 1 requirement, all levels stay unlocked.
    /// </summary>
    private void LoadUnlockState()
    {
        SaveData data = SaveSystem.LoadData();

        if (data.levelsUnlocked)
        {
            level1Unlocked = true;
            level2Unlocked = true;
            level3Unlocked = true;
            level4Unlocked = true;
            level5Unlocked = true;
            level6Unlocked = true;
            level7Unlocked = true;
        }
        else
        {
            // First-time player: only Level 1 is available
            level1Unlocked = true;
            level2Unlocked = false;
            level3Unlocked = false;
            level4Unlocked = false;
            level5Unlocked = false;
            level6Unlocked = false;
            level7Unlocked = false;
        }
    }

    public void UnlockLevel(int levelNumber)
    {
        if (levelNumber == 1) level1Unlocked = true;
        if (levelNumber == 2) level2Unlocked = true;
        if (levelNumber == 3) level3Unlocked = true;
        if (levelNumber == 4) level4Unlocked = true;
        if (levelNumber == 5) level5Unlocked = true;
        if (levelNumber == 6) level6Unlocked = true;
        if (levelNumber == 7) level7Unlocked = true;
        // Later, extend this with more levels
    }

    /// <summary>
    /// Called when the player has survived the required time in Level 1.
    /// Unlocks all levels and persists the state.
    /// </summary>
    public void UnlockAllLevels()
    {
        level1Unlocked = true;
        level2Unlocked = true;
        level3Unlocked = true;
        level4Unlocked = true;
        level5Unlocked = true;
        level6Unlocked = true;
        level7Unlocked = true;

        SaveData data = SaveSystem.LoadData();
        data.levelsUnlocked = true;
        SaveSystem.SaveData(data);

        Debug.Log("All levels unlocked! Player survived " + requiredLevel1Time + " seconds in Level 1.");
    }

    /// <summary>
    /// Checks whether the Level 1 time threshold has been met and unlocks levels if so.
    /// </summary>
    public void CheckAndUnlockFromLevel1(float elapsedTime)
    {
        SaveData data = SaveSystem.LoadData();
        if (!data.levelsUnlocked && elapsedTime >= requiredLevel1Time)
        {
            UnlockAllLevels();
        }
    }
}

