using System;
using System.Collections.Generic;

[Serializable]
public class AchievementData
{
    public string achievementID;

    public string title;

    public string description;

    public int currentProgress;

    public int targetProgress;

    public int rewardGem;

    public int rewardShard;

    public List<CostData> rewardItems = new List<CostData>();

    public bool isCompleted;

    public bool isClaimed;

    public AchievementData(
        string achievementID,
        string title,
        string description,
        int targetProgress,
        int rewardGem,
        int rewardShard = 0,
        List<CostData> rewardItems = null)
    {
        this.achievementID = achievementID;
        this.title = title;
        this.description = description;
        this.targetProgress = targetProgress;
        this.rewardGem = rewardGem;
        this.rewardShard = rewardShard;
        if (rewardItems != null)
        {
            this.rewardItems = rewardItems;
        }

        currentProgress = 0;
        isCompleted = false;
        isClaimed = false;
    }

    public void AddProgress(
        int amount)
    {
        if (isCompleted)
        {
            return;
        }

        currentProgress += amount;

        if (currentProgress >=
            targetProgress)
        {
            currentProgress =
                targetProgress;

            isCompleted = true;
        }
    }
}