using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;
    private void Start()
{
    Invoke(nameof(UpdatePowerScore), 1f);
}

    private void Awake()
{
    if (Instance == null)
    {
        Instance = this;

        DontDestroyOnLoad(
            gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}

    public int CalculatePowerScore()
    {
        int score = 0;

        int level =
            SaveLoadManager.Instance
            .SaveData.accountLevel;

        int lifetimeGem =
            SaveLoadManager.Instance
            .SaveData.lifetimeGemEarned;

        int completedAchievement =
            GetCompletedAchievementCount();

        score += level * 100;

        score += completedAchievement * 1000;

        score += lifetimeGem / 10;

        return score;
    }

    private int GetCompletedAchievementCount()
    {
        int count = 0;

        if (AchievementManager.Instance == null)
        {
            return 0;
        }

        List<AchievementData> achievements =
            SaveLoadManager.Instance
            .SaveData.achievements;

        if (achievements == null)
        {
            return 0;
        }

        for (int i = 0;
            i < achievements.Count;
            i++)
        {
            if (achievements[i].isCompleted)
            {
                count++;
            }
        }

        return count;
    }

    public void UpdatePowerScore()
    {
        int powerScore =
            CalculatePowerScore();

        var request =
            new UpdatePlayerStatisticsRequest
            {
                Statistics =
                new List<StatisticUpdate>()
                {
                    new StatisticUpdate()
                    {
                        StatisticName =
                            "PowerScore",

                        Value =
                            powerScore
                    }
                }
            };

        PlayFabClientAPI.UpdatePlayerStatistics(
            request,
            result =>
            {
                Debug.Log(
                    $"PowerScore Updated: {powerScore}");
            },
            error =>
            {
                Debug.LogError(
                    error.GenerateErrorReport());
            });
    }
    public void LoadLeaderboard(
    LeaderboardUI ui)
{
    LoadMyRank(ui);
    PlayFabClientAPI.GetLeaderboard(
        new GetLeaderboardRequest
        {
            StatisticName = "PowerScore",
            StartPosition = 0,
            MaxResultsCount = 20
        },
        result =>
        {
            ui.ClearItems();

            foreach (var player
                in result.Leaderboard)
            {
                string playerName =
                    string.IsNullOrEmpty(
                        player.DisplayName)
                    ? player.PlayFabId
                    : player.DisplayName;

                ui.AddItem(
                    player.Position + 1,
                    playerName,
                    player.StatValue);
            }
        },
        error =>
        {
            Debug.LogError(
                error.GenerateErrorReport());
        });
}
public void LoadMyRank(LeaderboardUI ui)
{
    PlayFabClientAPI.GetLeaderboardAroundPlayer(
        new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "PowerScore",
            MaxResultsCount = 1
        },
        result =>
        {
            if (result.Leaderboard.Count <= 0)
            {
                return;
            }

            var player =
                result.Leaderboard[0];

            ui.SetMyInfo(
                player.Position + 1,
                player.StatValue);
        },
        error =>
        {
            Debug.LogError(
                error.GenerateErrorReport());
        });
}
}