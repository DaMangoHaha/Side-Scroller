using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Victory scene UI. Reads the player's run results from PlayerPrefs
/// (set by PlayerEnergy when energy runs out) and displays score, time survived,
/// and victory stars.
///
/// Setup in the "Victory" scene:
///   1. Create a Canvas with TextMeshProUGUI elements and assign them in the Inspector.
///   2. Create 3 Image objects for the stars and assign them to starImages[].
///   3. Assign filled/empty star sprites.
///   4. Create Continue, Retry, and Main Menu buttons wired to the public methods.
/// </summary>
public class VictoryScreen : MonoBehaviour
{
    [Header("UI Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI starsText; // optional fallback if you want "Stars: 2/3"

    [Header("Star Images (left to right)")]
    public Image[] starImages; // assign exactly 3

    [Header("Star Sprites")]
    public Sprite filledStar;
    public Sprite emptyStar;

    [Header("Audio")]
    public string victoryMusic = "Victory"; // played via MusicManager

    private string lastLevel;
    private int finalScore;
    private float timeSurvived;
    private int starsEarned;

    void Start()
    {
        // Read data saved by PlayerEnergy
        lastLevel = PlayerPrefs.GetString("LastLevel", "Level1");
        finalScore = PlayerPrefs.GetInt("LastScore", 0);
        timeSurvived = PlayerPrefs.GetFloat("LastTimeSurvived", 0f);
        starsEarned = PlayerPrefs.GetInt("LastStars", 0);

        // Display score
        if (scoreText != null)
            scoreText.text = "Score: " + finalScore;

        // Display time survived
        if (timeText != null)
            timeText.text = "Time Survived: " + LevelTimer.FormatTime(timeSurvived);

        // Display stars as text (optional)
        if (starsText != null)
            starsText.text = starsEarned + " / 3 Stars";

        // Display star images
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;

                if (i < starsEarned)
                    starImages[i].sprite = filledStar;
                else
                    starImages[i].sprite = emptyStar;

                starImages[i].enabled = true;
            }
        }

        // Save best score and best stars for this level
        SaveBestResults();

        // Play victory music
        if (MusicManager.Instance != null && !string.IsNullOrEmpty(victoryMusic))
            MusicManager.Instance.PlayMusic(victoryMusic);
    }

    /// <summary>
    /// Persists the best score and best star count for the level.
    /// </summary>
    private void SaveBestResults()
    {
        SaveData data = SaveSystem.LoadData();

        // Best score
        int previousBestScore = GetBestScore(data, lastLevel);
        if (finalScore > previousBestScore)
            SetBestScore(data, lastLevel, finalScore);

        // Best stars
        int previousBestStars = GetBestStars(data, lastLevel);
        if (starsEarned > previousBestStars)
            SetBestStars(data, lastLevel, starsEarned);

        SaveSystem.SaveData(data);
    }

    // --- Button Callbacks ---

    /// <summary>
    /// Retry the same level.
    /// </summary>
    public void OnRetryPressed()
    {
        string level = PlayerPrefs.GetString("LastLevel", "Level1");

        // Derive the music track name from the scene name (e.g. "Level1" -> "Level 1")
        string musicTrack = GetMusicTrackForLevel(level);

        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene(level, () =>
            {
                if (MusicManager.Instance != null && musicTrack != null)
                    MusicManager.Instance.PlayMusic(musicTrack);
            });
        else
            SceneManager.LoadScene(level);
    }
    

    /// <summary>
    /// Return to the Main Menu.
    /// </summary>
    public void OnMainMenuPressed()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("MainMenu", () => MusicManager.Instance.PlayMusic("Main Menu"));
        else
            SceneManager.LoadScene("MainMenu");
    }

    // --- SaveData Helpers ---

    private static int GetBestScore(SaveData data, string levelName)
    {
        return levelName switch
        {
            "Level1" => data.bestScoreLevel1,
            "Level2" => data.bestScoreLevel2,
            "Level3" => data.bestScoreLevel3,
            "Level4" => data.bestScoreLevel4,
            "Level5" => data.bestScoreLevel5,
            "Level6" => data.bestScoreLevel6,
            "Level7" => data.bestScoreLevel7,
            _ => 0
        };
    }

    private static void SetBestScore(SaveData data, string levelName, int score)
    {
        switch (levelName)
        {
            case "Level1": data.bestScoreLevel1 = score; break;
            case "Level2": data.bestScoreLevel2 = score; break;
            case "Level3": data.bestScoreLevel3 = score; break;
            case "Level4": data.bestScoreLevel4 = score; break;
            case "Level5": data.bestScoreLevel5 = score; break;
            case "Level6": data.bestScoreLevel6 = score; break;
            case "Level7": data.bestScoreLevel7 = score; break;
        }
    }

    private static int GetBestStars(SaveData data, string levelName)
    {
        return levelName switch
        {
            "Level1" => data.bestStarsLevel1,
            "Level2" => data.bestStarsLevel2,
            "Level3" => data.bestStarsLevel3,
            "Level4" => data.bestStarsLevel4,
            "Level5" => data.bestStarsLevel5,
            "Level6" => data.bestStarsLevel6,
            "Level7" => data.bestStarsLevel7,
            _ => 0
        };
    }

    private static void SetBestStars(SaveData data, string levelName, int stars)
    {
        switch (levelName)
        {
            case "Level1": data.bestStarsLevel1 = stars; break;
            case "Level2": data.bestStarsLevel2 = stars; break;
            case "Level3": data.bestStarsLevel3 = stars; break;
            case "Level4": data.bestStarsLevel4 = stars; break;
            case "Level5": data.bestStarsLevel5 = stars; break;
            case "Level6": data.bestStarsLevel6 = stars; break;
            case "Level7": data.bestStarsLevel7 = stars; break;
        }
    }

    /// <summary>
    /// Maps a level scene name to its music track name.
    /// Follows the same naming convention used in LevelSelect.
    /// </summary>
    private static string GetMusicTrackForLevel(string levelName)
    {
        return levelName switch
        {
            "Level1" => "Level 1",
            "Level2" => "Level 2",
            "Level3" => "Level 3",
            "Level4" => "Level 4",
            "Level5" => "Level 5",
            "Level6" => "Level 6",
            "Level7" => "Level 7",
            _ => null
        };
    }
}
