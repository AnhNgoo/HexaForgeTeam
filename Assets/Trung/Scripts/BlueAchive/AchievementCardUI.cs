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
            TitleText.text =
                achievementData.title;
        }

        if (DescriptionText != null)
        {
            DescriptionText.text =
                achievementData.description;
        }

        if (ProgressText != null)
        {
            ProgressText.text =
                $"{achievementData.currentProgress}" +
                $" / " +
                $"{achievementData.targetProgress}";
        }

        if (RewardText != null)
        {
            RewardText.text =
                $"{achievementData.rewardGem} Gems";
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
                    buttonText.text =
                        "Claimed";
                }
                else if
                (
                    achievementData.isCompleted
                )
                {
                    buttonText.text =
                        "Claim";
                }
                else
                {
                    buttonText.text =
                        "Locked";
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