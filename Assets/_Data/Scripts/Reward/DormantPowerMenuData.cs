using System;
using System.Collections.Generic;

public class DormantPowerMenuData
{
    public IReadOnlyList<BossRewardDataSO> Rewards { get; }
    public Action<BossRewardDataSO> OnSelected { get; }

    public DormantPowerMenuData(
        IReadOnlyList<BossRewardDataSO> rewards,
        Action<BossRewardDataSO> onSelected)
    {
        Rewards = rewards;
        OnSelected = onSelected;
    }
}
