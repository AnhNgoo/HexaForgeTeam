using System.Collections.Generic;
using UnityEngine;

public class DormantPowerInteractable : InteractBase
{
    [SerializeField] private BossRewardTableSO rewardTable;

    private List<BossRewardDataSO> rolledRewards = new();
    private bool isClaimed;

    public override string InteractionName => "Nhận Dormant Power";

    public void Initialize(BossRewardTableSO table)
    {
        rewardTable = table;
        isClaimed = false;
        rolledRewards.Clear();

        if (rewardTable != null)
            rolledRewards = rewardTable.RollRewards();
    }

    protected override void InteractAction()
    {
        if (isClaimed || rolledRewards.Count == 0)
            return;

        UIManager.Instance?.ChangeMenu(MenuType.DormantPowerMenu, new DormantPowerMenuData(rolledRewards, ClaimReward));
    }

    private void ClaimReward(BossRewardDataSO reward)
    {
        if (isClaimed || reward == null || character == null)
            return;

        if (!reward.Grant(character))
        {
            Debug.LogWarning(
                $"Không thể nhận reward {reward.name}.");
            return;
        }

        isClaimed = true;
        InteractionManager.Instance?.UnregisterInteractable(this);
        EventManager.Notify(GameEvent.OnHidePickUpItemPanel);
        UIManager.Instance?.ChangeMenu(MenuType.GameplayMenu);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        InteractionManager.Instance?.UnregisterInteractable(this);
    }

    public override void ResetInteraction()
    {
        // Dormant Power được Instantiate rồi Destroy, không cần reset pool.
    }
}
