using System;

public sealed class WeaponReplacementMenuData
{
    public WeaponData RewardWeapon { get; }
    public Func<int, bool> OnConfirmed { get; }
    public Action OnCancelled { get; }

    public WeaponReplacementMenuData(WeaponData rewardWeapon, Func<int, bool> onConfirmed, Action onCancelled)
    {
        RewardWeapon = rewardWeapon;
        OnConfirmed = onConfirmed;
        OnCancelled = onCancelled;
    }
}