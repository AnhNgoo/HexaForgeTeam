using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public enum LeaderboardTab
{
    Power,       // Lực chiến (Account Level + Gems)
    Achievement, // Thành tựu (Completed Achievements)
    Hunt,        // Trảm quái (Total Kills)
    Run          // Lượt chạy hầm ngục (Total Runs)
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private LeaderboardTab currentTab = LeaderboardTab.Power;
    private bool isUpdatingStats = false;
    private float lastUpdateTime = -999f;
    private const float UPDATE_COOLDOWN = 3f;

    private void Awake()
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

    private void Start()
    {
        Invoke(nameof(UpdateAllStatistics), 1f);
    }

    public LeaderboardTab GetCurrentTab() => currentTab;

    public void SetCurrentTab(LeaderboardTab tab)
    {
        currentTab = tab;
    }

    private string GetStatisticNameByTab(LeaderboardTab tab)
    {
        switch (tab)
        {
            case LeaderboardTab.Power:
                return "PowerScore";
            case LeaderboardTab.Achievement:
                return "AchievementScore";
            case LeaderboardTab.Hunt:
                return "HuntScore";
            case LeaderboardTab.Run:
                return "RunScore";
            default:
                return "PowerScore";
        }
    }

    #region Score Calculation & Sync PlayFab

    public int CalculatePowerScore()
    {
        if (SaveLoadManager.Instance == null || SaveLoadManager.Instance.SaveData == null) return 0;

        int level = SaveLoadManager.Instance.SaveData.accountLevel;
        int lifetimeGem = SaveLoadManager.Instance.SaveData.lifetimeGemEarned;

        return (level * 100) + (lifetimeGem / 10);
    }

    public int GetCompletedAchievementCount()
    {
        if (AchievementManager.Instance == null || SaveLoadManager.Instance == null || SaveLoadManager.Instance.SaveData == null) return 0;

        List<AchievementData> achievements = SaveLoadManager.Instance.SaveData.achievements;
        if (achievements == null) return 0;

        int count = 0;
        for (int i = 0; i < achievements.Count; i++)
        {
            if (achievements[i] != null && achievements[i].isCompleted)
            {
                count++;
            }
        }
        return count;
    }

    public void UpdateAllStatistics()
    {
        if (Time.time - lastUpdateTime < UPDATE_COOLDOWN)
        {
            Debug.LogWarning("[PlayFab] Update API called too fast. Ignored to prevent 409 Conflict.");
            return;
        }

        if (isUpdatingStats) return;

        if (SaveLoadManager.Instance == null || SaveLoadManager.Instance.SaveData == null) return;

        GameSaveData data = SaveLoadManager.Instance.SaveData;

        int powerScore = CalculatePowerScore();
        int achievScore = GetCompletedAchievementCount();
        int huntScore = data.totalKills; 
        int runScore = data.totalRuns;   

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>()
            {
                new StatisticUpdate { StatisticName = "PowerScore", Value = powerScore },
                new StatisticUpdate { StatisticName = "AchievementScore", Value = achievScore },
                new StatisticUpdate { StatisticName = "HuntScore", Value = huntScore },
                new StatisticUpdate { StatisticName = "RunScore", Value = runScore }
            }
        };

        isUpdatingStats = true;
        lastUpdateTime = Time.time;

        PlayFabClientAPI.UpdatePlayerStatistics(
            request,
            result =>
            {
                isUpdatingStats = false;
                Debug.Log("<color=#00FFCC>[PlayFab Sync] Synced Statistics Successfully!</color>");
            },
            error =>
            {
                isUpdatingStats = false;

                if (error.HttpCode == 409)
                {
                    Debug.LogWarning("[PlayFab] 409 Conflict detected. Retrying silently later...");
                }
                else
                {
                    Debug.LogError(error.GenerateErrorReport());
                }
            }
        );
    }
    public void UpdatePowerScore()
    {
        UpdateAllStatistics();
    }
    #endregion

    #region Detail String Generators

    public string GetMyDetailString()
    {
        if (SaveLoadManager.Instance == null || SaveLoadManager.Instance.SaveData == null) return "";

        var data = SaveLoadManager.Instance.SaveData;

        switch (currentTab)
        {
            case LeaderboardTab.Power:
                string gemStr = data.lifetimeGemEarned >= 1000 ? $"{data.lifetimeGemEarned / 1000f:F1}k" : $"{data.lifetimeGemEarned}";
                return $"Lv.{data.accountLevel} | {gemStr} Gems";

            case LeaderboardTab.Achievement:
                int totalAchiev = data.achievements != null ? data.achievements.Count : 30;
                return $"Completed {GetCompletedAchievementCount()}/{totalAchiev}";

            case LeaderboardTab.Hunt:
                return $"Lv.{data.accountLevel} Hunter";

            case LeaderboardTab.Run:
                return $"Challenger";

            default:
                return "";
        }
    }

    public string GetEstimateDetailString(int score)
    {
        switch (currentTab)
        {
            case LeaderboardTab.Power:
                int estLevel = Mathf.Clamp(score / 200 + 1, 1, 30);
                int estGems = (score % 200) * 10;
                string gemStr = estGems >= 1000 ? $"{estGems / 1000f:F1}k" : $"{estGems}";
                return $"Lv.{estLevel} | {gemStr} Gems";

            case LeaderboardTab.Achievement:
                return $"Completed {score} Badges";

            case LeaderboardTab.Hunt:
                return $"Slayer Rank";

            case LeaderboardTab.Run:
                return $"Dungeon Runner";

            default:
                return "";
        }
    }

    #endregion

    #region PlayFab Leaderboard Fetching

    public void LoadLeaderboard(LeaderboardUI ui)
    {
        string statName = GetStatisticNameByTab(currentTab);

        LoadMyRank(ui);
        PlayFabClientAPI.GetLeaderboard(
            new GetLeaderboardRequest
            {
                StatisticName = statName,
                StartPosition = 0,
                MaxResultsCount = 20
            },
            result =>
            {
                ui.ClearItems();

                string myPlayFabId = PlayFabSettings.staticPlayer != null ? PlayFabSettings.staticPlayer.PlayFabId : "";

                foreach (var player in result.Leaderboard)
                {
                    bool isMe = (!string.IsNullOrEmpty(myPlayFabId) && player.PlayFabId == myPlayFabId);

                    string playerName = string.IsNullOrEmpty(player.DisplayName)
                        ? player.PlayFabId
                        : player.DisplayName;

                    string detailStr = "";
                    int displayScore = player.StatValue;

                    if (isMe)
                    {
                        detailStr = GetMyDetailString();
                        
                        // Lấy điểm chuẩn mới nhất từ SaveData cục bộ phòng trường hợp Server chưa kịp update
                        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
                        {
                            var data = SaveLoadManager.Instance.SaveData;
                            switch (currentTab)
                            {
                                case LeaderboardTab.Power: displayScore = CalculatePowerScore(); break;
                                case LeaderboardTab.Achievement: displayScore = GetCompletedAchievementCount(); break;
                                case LeaderboardTab.Hunt: displayScore = data.totalKills; break;
                                case LeaderboardTab.Run: displayScore = data.totalRuns; break;
                            }
                        }
                    }
                    else
                    {
                        detailStr = GetEstimateDetailString(player.StatValue);
                    }

                    // Truyền flag isMe sang UI để vẽ màu Vàng Highlight!
                    ui.AddItem(
                        player.Position + 1,
                        playerName,
                        displayScore,
                        detailStr,
                        isMe);
                }
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
    }

    public void LoadMyRank(LeaderboardUI ui)
    {
        string statName = GetStatisticNameByTab(currentTab);

        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = statName,
                MaxResultsCount = 1
            },
            result =>
            {
                if (result.Leaderboard.Count <= 0) return;

                var player = result.Leaderboard[0];
                string myDetail = GetMyDetailString();

                ui.SetMyInfo(
                    player.Position + 1,
                    player.StatValue,
                    myDetail);
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
    }

    #endregion
}