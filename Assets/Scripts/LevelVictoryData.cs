using UnityEngine;

/// <summary>
/// Attach this to any GameObject in a level scene to configure the star thresholds.
/// Stars are awarded based on BOTH score and time survived — the player earns the
/// star tier whose requirements they meet for both criteria.
/// 
/// Example:
///   1 Star  ? score >= 50  AND  time >= 30s
///   2 Stars ? score >= 150 AND  time >= 60s
///   3 Stars ? score >= 300 AND  time >= 120s
/// </summary>
public class LevelVictoryData : MonoBehaviour
{
    public static LevelVictoryData Instance;

    [Header("1 Star Requirements")]
    public int oneStarScore = 50;
    public float oneStarTime = 30f; // seconds

    [Header("2 Star Requirements")]
    public int twoStarScore = 150;
    public float twoStarTime = 60f;

    [Header("3 Star Requirements")]
    public int threeStarScore = 300;
    public float threeStarTime = 120f;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Returns 0-3 stars based on the player's final score and survival time.
    /// The player must meet BOTH the score and time thresholds for a given tier.
    /// </summary>
    public int CalculateStars(int score, float timeSurvived)
    {
        if (score >= threeStarScore && timeSurvived >= threeStarTime)
            return 3;
        if (score >= twoStarScore && timeSurvived >= twoStarTime)
            return 2;
        if (score >= oneStarScore && timeSurvived >= oneStarTime)
            return 1;

        return 0;
    }
}
