using UnityEngine;

public enum BossRewardType
{
    Weapon,
    MaxHealth,
    Damage,
    Defense,
    Stamina
}

[CreateAssetMenu(fileName = "BossReward", menuName = "Enemy/Boss Reward/Reward")]
public class BossRewardDataSO : ScriptableObject
{
    [Header("Reward Type")]
    [SerializeField] private BossRewardType rewardType;

    [Header("Buff Display")]
    [SerializeField] private string rewardName;
    [SerializeField] private Sprite rewardIcon;
    [TextArea(3, 8)][SerializeField] private string rewardDescription;
    [SerializeField] private ItemRarity rarity = ItemRarity.Uncommon;

    [Header("Weapon Reward")]
    [SerializeField] private WeaponData weapon;

    [Header("Stat Reward")]
    [Min(0f)]
    [SerializeField] private float percentageValue;

    public BossRewardType RewardType => rewardType;
    public float PercentageValue => percentageValue;

    public string DisplayName => rewardType == BossRewardType.Weapon && weapon != null ? weapon.itemName : rewardName;

    public Sprite DisplayIcon => rewardType == BossRewardType.Weapon && weapon != null ? weapon.itemIcon : rewardIcon;

    public string DisplayDescription => rewardType == BossRewardType.Weapon && weapon != null ? weapon.itemDescription : rewardDescription;

    public ItemRarity DisplayRarity => rewardType == BossRewardType.Weapon && weapon != null ? weapon.rarity : rarity;

    public string TypeLabel => rewardType switch
    {
        BossRewardType.Weapon => "Vũ khí",
        BossRewardType.MaxHealth => "Sinh lực tối đa",
        BossRewardType.Damage => "Sát thương",
        BossRewardType.Defense => "Phòng thủ",
        BossRewardType.Stamina => "Thể lực tối đa",
        _ => rewardType.ToString()
    };

    // Dùng để ngăn ba thẻ cùng roll một loại buff/vũ khí.
    public string UniqueKey => rewardType == BossRewardType.Weapon && weapon != null ? $"Weapon:{weapon.name}" : $"Stat:{rewardType}";

    public bool Grant(CharacterBase character)
    {
        if (character == null)
            return false;

        if (rewardType == BossRewardType.Weapon)
        {
            if (weapon == null || WeaponInventorySystem.Instance == null)
                return false;

            return WeaponInventorySystem.Instance.TryAddOrReplaceWeapon(weapon);
        }

        return character.CharacterStat != null && character.CharacterStat.ApplyRunReward(rewardType, percentageValue);
    }
}
