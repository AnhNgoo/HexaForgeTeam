using UnityEngine;

public enum BossRewardType
{
    Weapon,
    MaxHealth,
    Damage,
    Defense,
    Stamina,
    MoveSpeed,
    MaxMP,
    PoisonDamage,
    StaminaRegen,
    MPRegen
}

[CreateAssetMenu(fileName = "BossReward", menuName = "Enemy/Boss Reward/Reward")]
public class BossRewardDataSO : ScriptableObject
{
    [Header("Reward Type")]
    [SerializeField] private BossRewardType rewardType;

    [Header("Buff Display")]
    [SerializeField] private string rewardName;
    [SerializeField] private Sprite rewardIcon;
    [TextArea(3, 8)]
    [SerializeField] private string rewardDescription;
    [SerializeField] private ItemRarity rarity = ItemRarity.Uncommon;

    [Header("Weapon Reward")]
    [SerializeField] private WeaponData weapon;
    public WeaponData Weapon => weapon;

    [Header("Stat Reward")]
    [Min(0f)]
    [SerializeField] private float percentageValue;

    [Header("Localization")]
    [SerializeField] private string rewardNameKey;
    [SerializeField] private string rewardDescriptionKey;

    public BossRewardType RewardType => rewardType;
    public float PercentageValue => percentageValue;

    public bool IsConfigured => rewardType == BossRewardType.Weapon ? weapon != null : percentageValue > 0f;

    public string DisplayName
    {
        get
        {
            string fallback = rewardType == BossRewardType.Weapon && weapon != null ? weapon.itemName : rewardName;
            return LocalizationText.Get(rewardNameKey, fallback);
        }
    }

    public Sprite DisplayIcon => rewardType == BossRewardType.Weapon && weapon != null ? weapon.itemIcon : rewardIcon;

    public string DisplayDescription
    {
        get
        {
            string fallback = rewardType == BossRewardType.Weapon && weapon != null ? weapon.itemDescription : rewardDescription;
            return LocalizationText.Get(rewardDescriptionKey, fallback);
        }
    }

    public ItemRarity DisplayRarity => rewardType == BossRewardType.Weapon && weapon != null ? weapon.rarity : rarity;

    public string TypeLabel => rewardType switch
    {
        BossRewardType.Weapon =>
            LocalizationText.Get("ui.reward.type.weapon", "Weapon"),

        BossRewardType.MaxHealth =>
            LocalizationText.Get("ui.reward.type.max_health", "Max Health"),

        BossRewardType.Damage =>
            LocalizationText.Get("ui.reward.type.damage", "Damage"),

        BossRewardType.Defense =>
            LocalizationText.Get("ui.reward.type.defense", "Defense"),

        BossRewardType.Stamina =>
            LocalizationText.Get("ui.reward.type.stamina", "Stamina"),

        BossRewardType.MoveSpeed =>
            LocalizationText.Get("ui.reward.type.move_speed", "Move Speed"),

        BossRewardType.MaxMP =>
            LocalizationText.Get("ui.reward.type.max_mp", "Max MP"),

        BossRewardType.PoisonDamage =>
            LocalizationText.Get("ui.reward.type.poison_damage", "Poison Damage"),

        BossRewardType.StaminaRegen =>
            LocalizationText.Get("ui.reward.type.stamina_regen", "Stamina Regeneration"),

        BossRewardType.MPRegen =>
            LocalizationText.Get("ui.reward.type.mp_regen", "MP Regeneration"),

        _ => rewardType.ToString()
    };

    public string RarityLabel => DisplayRarity switch
    {
        ItemRarity.Common =>
            LocalizationText.Get("ui.rarity.common", "Common"),

        ItemRarity.Uncommon =>
            LocalizationText.Get("ui.rarity.uncommon", "Uncommon"),

        ItemRarity.Rare =>
            LocalizationText.Get("ui.rarity.rare", "Rare"),

        ItemRarity.Legendary =>
            LocalizationText.Get("ui.rarity.legendary", "Legendary"),

        _ => DisplayRarity.ToString()
    };

    public string UniqueKey => rewardType == BossRewardType.Weapon && weapon != null ? $"Weapon:{weapon.name}" : $"Stat:{rewardType}";

    public bool Grant(CharacterBase character)
    {
        if (!IsConfigured || character == null)
            return false;

        if (rewardType == BossRewardType.Weapon)
        {
            return WeaponInventorySystem.Instance != null && WeaponInventorySystem.Instance.TryAddWeapon(weapon);
        }

        return character.CharacterStat != null && character.CharacterStat.ApplyRunReward(rewardType, percentageValue);
    }
}
