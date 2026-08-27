using System;
using System.Collections.Generic;

public class DormantPowerMenuData
{
    public IReadOnlyList<BossRewardDataSO> Rewards { get; }
    public Func<BossRewardDataSO, bool> OnSelected { get; }
    public Func<BossRewardDataSO, bool> OnWeaponInventoryFull { get; }

    public DormantPowerMenuData(IReadOnlyList<BossRewardDataSO> rewards, Func<BossRewardDataSO, bool> onSelected, Func<BossRewardDataSO, bool> onWeaponInventoryFull)
    {
        Rewards = rewards;
        OnSelected = onSelected;
        OnWeaponInventoryFull = onWeaponInventoryFull;
    }
}