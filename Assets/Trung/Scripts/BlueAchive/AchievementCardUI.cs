using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementCardUI :
    LoadComponents
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text TitleText;

    [SerializeField]
    private TMP_Text DescriptionText;

    [SerializeField]
    private TMP_Text ProgressText;

    [SerializeField]
    private TMP_Text RewardText;
    [SerializeField]
private Slider progressSlider;

    [SerializeField]
    private Button ClaimButton;

    private AchievementData
        achievementData;

    protected override void LoadComponent()
    {
        if (TitleText == null)
        {
            TitleText =
                transform.Find("TitleText")
                ?.GetComponent<TMP_Text>();
        }

        if (DescriptionText == null)
        {
            DescriptionText =
                transform.Find("DescriptionText")
                ?.GetComponent<TMP_Text>();
        }

        if (ProgressText == null)
        {
            ProgressText =
                transform.Find("ProgressText")
                ?.GetComponent<TMP_Text>();
        }

        if (RewardText == null)
        {
            RewardText =
                transform.Find("RewardText")
                ?.GetComponent<TMP_Text>();
        }
        if (progressSlider == null)
{
    progressSlider =
        transform.Find("Progress")
        ?.GetComponent<Slider>();
}

        if (ClaimButton == null)
        {
            ClaimButton =
                transform.Find("ClaimButton")
                ?.GetComponent<Button>();
        }
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void Setup(
        AchievementData data)
    {
        achievementData = data;

        RefreshUI();

        if (ClaimButton != null)
        {
            ClaimButton.onClick
                .RemoveAllListeners();

            ClaimButton.onClick
                .AddListener(
                    ClaimReward);
        }
    }

    public void RefreshUI()
    {
        if (achievementData == null)
        {
            return;
        }

        if (TitleText != null)
        {
            TitleText.SetTextSafe(
                achievementData.title);
        }

        if (DescriptionText != null)
        {
            DescriptionText.SetTextSafe(
                achievementData.description);
        }

        if (ProgressText != null)
        {
            ProgressText.SetTextSafe(
                $"{achievementData.currentProgress}" +
                $" / " +
                $"{achievementData.targetProgress}");
        }
        if (progressSlider != null)
{
    progressSlider.maxValue =
        achievementData.targetProgress;

    progressSlider.value =
        Mathf.Clamp(
            achievementData.currentProgress,
            0,
            achievementData.targetProgress);
}

        if (RewardText != null)
        {
            RewardText.SetTextSafe(
                $"{achievementData.rewardGem} Gems");
        }

        if (ClaimButton != null)
        {
            ClaimButton.interactable =
                achievementData.isCompleted &&
                !achievementData.isClaimed;

            TMP_Text buttonText =
                ClaimButton
                .GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                if (achievementData.isClaimed)
                {
                    buttonText.SetTextSafe(
                        "Claimed");
                }
                else if
                (
                    achievementData.isCompleted
                )
                {
                    buttonText.SetTextSafe(
                        "Claim");
                }
                else
                {
                    buttonText.SetTextSafe(
                        "Locked");
                }
            }
        }
    }

    private void ClaimReward()
    {
        if (achievementData == null)
        {
            return;
        }

        if (!achievementData.isCompleted)
        {
            return;
        }

        if (achievementData.isClaimed)
        {
            return;
        }

        achievementData.isClaimed =
            true;

        GemManager.Instance
            .AddGem(
                achievementData.rewardGem);

        AchievementManager.Instance
            .SaveAchievement();

        AchievementManager.Instance
            .RefreshUI();
        AchievementManager.Instance
    .CheckUltimateRuneReward();
    }
}