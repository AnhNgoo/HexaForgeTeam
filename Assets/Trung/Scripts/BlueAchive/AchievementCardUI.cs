using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AchievementCardUI : LoadComponents
{
    [Header("UI")]
    [SerializeField] private TMP_Text TitleText;
    [SerializeField] private TMP_Text DescriptionText;
    [SerializeField] private TMP_Text ProgressText;
    [SerializeField] private CostDisplayUI rewardDisplayUI;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button ClaimButton;

    private AchievementData achievementData;
    private Tween sliderTween;

    protected override void LoadComponent()
    {
        if (TitleText == null) TitleText = transform.Find("TitleText")?.GetComponent<TMP_Text>();
        if (DescriptionText == null) DescriptionText = transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
        if (ProgressText == null) ProgressText = transform.Find("ProgressText")?.GetComponent<TMP_Text>();
        if (rewardDisplayUI == null) rewardDisplayUI = GetComponentInChildren<CostDisplayUI>();
        if (progressSlider == null) progressSlider = transform.Find("Progress")?.GetComponent<Slider>();

        if (progressSlider != null) progressSlider.interactable = false;

        if (ClaimButton == null) ClaimButton = transform.Find("ClaimButton")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime() { }

    public void Setup(AchievementData data)
    {
        achievementData = data;
        RefreshUI();

        if (ClaimButton != null)
        {
            ClaimButton.onClick.RemoveAllListeners();
            ClaimButton.onClick.AddListener(ClaimReward);
        }
    }

    public void RefreshUI()
    {
        if (achievementData == null) return;

        if (TitleText != null) TitleText.SetTextSafe(achievementData.title);
        if (DescriptionText != null) DescriptionText.SetTextSafe(achievementData.description);

        if (ProgressText != null)
        {
            ProgressText.SetTextSafe($"{achievementData.currentProgress} / {achievementData.targetProgress}");
        }

        // DOTween cho Slider Tiến Trình
        if (progressSlider != null)
        {
            progressSlider.maxValue = achievementData.targetProgress;
            float targetVal = Mathf.Clamp(achievementData.currentProgress, 0, achievementData.targetProgress);

            if (sliderTween != null) sliderTween.Kill();
            sliderTween = progressSlider.DOValue(targetVal, 0.5f).SetEase(Ease.OutQuad);
        }

        if (rewardDisplayUI != null)
        {
            List<CostData> rewards = new List<CostData>();

            if (achievementData.achievementID == "MASTER_ACHIEVEMENT")
            {
                rewards.Add(new CostData("ORIGIN_RUNE", 1));
            }
            else
            {
                if (achievementData.rewardGem > 0) rewards.Add(new CostData("GEM", achievementData.rewardGem));
                if (achievementData.rewardShard > 0) rewards.Add(new CostData("RUNE_SHARD", achievementData.rewardShard));
                if (achievementData.rewardItems != null) rewards.AddRange(achievementData.rewardItems);
            }

            rewardDisplayUI.SetupCost(rewards);
        }

        if (ClaimButton != null)
        {
            bool canClaim = achievementData.isCompleted && !achievementData.isClaimed;
            ClaimButton.interactable = canClaim;

            TMP_Text buttonText = ClaimButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                if (achievementData.isClaimed) buttonText.SetTextSafe("Claimed");
                else if (achievementData.isCompleted) buttonText.SetTextSafe("Claim");
                else buttonText.SetTextSafe("Locked");
            }
        }
    }

    private void ClaimReward()
    {
        if (achievementData == null || !achievementData.isCompleted || achievementData.isClaimed) return;

        // Hiệu ứng nảy nút Claim bằng DOTween
        if (ClaimButton != null)
        {
            ClaimButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 8, 0.5f);
        }

        achievementData.isClaimed = true;

        if (achievementData.rewardGem > 0 && GemManager.Instance != null)
        {
            GemManager.Instance.AddGem(achievementData.rewardGem);
        }

        if (achievementData.rewardShard > 0 && RuneShardManager.Instance != null)
        {
            RuneShardManager.Instance.AddShards(achievementData.rewardShard);
        }

        if (achievementData.rewardItems != null && InventoryItemManager.Instance != null)
        {
            for (int i = 0; i < achievementData.rewardItems.Count; i++)
            {
                CostData item = achievementData.rewardItems[i];
                if (item != null && !string.IsNullOrEmpty(item.itemID) && item.amount > 0)
                {
                    string itemName = item.itemID == "GACHA_TICKET_01" ? "Gacha Ticket" :
                                       item.itemID == "FUSION_CHARM_01" ? "Protection Charm" :
                                       item.itemID == "REROLL_SCROLL_01" ? "Reroll Scroll" : "Special Item";
                    InventoryItemManager.Instance.AddItem(item.itemID, itemName, item.amount);
                }
            }
        }

        AchievementManager.Instance.SaveAchievement();
        AchievementManager.Instance.RefreshUI();
        AchievementManager.Instance.CheckUltimateRuneReward();
    }

    private void OnDisable()
    {
        if (sliderTween != null) sliderTween.Kill();
    }
}