using System;

[Serializable]
public class AchievementData
{
    public string achievementID;

    public string title;

    public string description;

    public int currentProgress;

    public int targetProgress;

    public int rewardGem;

    public bool isCompleted;

    public bool isClaimed;

    public AchievementData(
        string achievementID,
        string title,
        string description,
        int targetProgress,
        int rewardGem)
    {
        this.achievementID =
            achievementID;

        this.title =
            title;

        this.description =
            description;

        this.targetProgress =
            targetProgress;

        this.rewardGem =
            rewardGem;

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