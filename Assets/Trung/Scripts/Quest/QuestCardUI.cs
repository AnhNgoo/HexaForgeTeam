using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestCardUI : MonoBehaviour
{
    [Header("UI Texts")]
    [SerializeField] private TMP_Text txtTitle;
    [SerializeField] private TMP_Text txtDescription;
    [SerializeField] private TMP_Text txtProgress;
    [SerializeField] private TMP_Text txtStatus;

    [Header("Components")]
    [SerializeField] private Slider sliderProgress;
    [SerializeField] private CostDisplayUI rewardDisplay;
    [SerializeField] private GameObject statusTagRoot;

    private QuestData questData;
    private Tween sliderTween;

    public void Setup(QuestData data)
    {
        questData = data;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (questData == null) return;

        QuestSO questSO = QuestManager.Instance != null ? QuestManager.Instance.GetQuestSO(questData.questID) : null;

        string title = questSO != null ? questSO.questTitle : questData.title;
        string desc = questSO != null ? questSO.questDescription : questData.description;
        int target = questSO != null ? questSO.targetProgress : questData.targetProgress;

        if (txtTitle != null) txtTitle.SetTextSafe(title);
        if (txtDescription != null) txtDescription.SetTextSafe(desc);
        if (txtProgress != null) txtProgress.SetTextSafe($"{questData.currentProgress} / {target}");

        if (sliderProgress != null)
        {
            sliderProgress.maxValue = target;
            float targetVal = Mathf.Clamp(questData.currentProgress, 0, target);

            if (sliderTween != null) sliderTween.Kill();
            sliderTween = sliderProgress.DOValue(targetVal, 0.4f).SetEase(Ease.OutQuad);
        }

        if (rewardDisplay != null)
        {
            List<CostData> rewards = new List<CostData>();
            int gem = questSO != null ? questSO.rewardGem : questData.rewardGem;
            int shard = questSO != null ? questSO.rewardShard : questData.rewardShard;
            int exp = questSO != null ? questSO.rewardExp : 0;
            List<CostData> items = questSO != null ? questSO.rewardItems : questData.rewardItems;

            if (gem > 0) rewards.Add(new CostData("GEM", gem));
            if (shard > 0) rewards.Add(new CostData("RUNE_SHARD", shard));
            if (exp > 0) rewards.Add(new CostData("EXP", exp));
            if (items != null) rewards.AddRange(items);

            rewardDisplay.SetupCost(rewards);
        }

        if (txtStatus != null)
        {
            switch (questData.state)
            {
                case QuestState.NotStarted:
                    txtStatus.SetTextSafe("<color=#AAAAAA>Not Started</color>");
                    break;
                case QuestState.InProgress:
                    txtStatus.SetTextSafe("<color=#FFCC00>In Progress</color>");
                    break;
                case QuestState.CanClaim:
                    txtStatus.SetTextSafe("<color=#00FFCC>Ready to Claim (Talk to NPC)</color>");
                    break;
                case QuestState.Completed:
                    txtStatus.SetTextSafe("<color=#55FF55>Completed</color>");
                    break;
            }
        }
    }

    private void OnDisable()
    {
        if (sliderTween != null) sliderTween.Kill();
    }
}