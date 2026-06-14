using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementCardUI :
    MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text progressText;

    [SerializeField]
    private TMP_Text rewardText;

    [SerializeField]
    private Button claimButton;

    private AchievementData
        achievementData;

    public void Setup(
        AchievementData data)
    {
        achievementData = data;

        RefreshUI();

        if (claimButton != null)
        {
            claimButton.onClick
                .RemoveAllListeners();

            claimButton.onClick
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

        if (titleText != null)
        {
            titleText.text =
                achievementData.title;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                achievementData.description;
        }

        if (progressText != null)
        {
            progressText.text =
                $"{achievementData.currentProgress}" +
                $" / " +
                $"{achievementData.targetProgress}";
        }

        if (rewardText != null)
        {
            rewardText.text =
                $"{achievementData.rewardGem} Gems";
        }

        if (claimButton != null)
        {
            claimButton.interactable =
                achievementData.isCompleted &&
                !achievementData.isClaimed;

            TMP_Text buttonText =
                claimButton
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
    }
}