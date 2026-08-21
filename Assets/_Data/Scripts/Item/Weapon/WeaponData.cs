using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ItemDataBase
{
    public WeaponType weaponType;
    public PoolType weapon;
    public PoolType pickUpItem;

    [Min(0f)]
    [Tooltip("Hệ số damage khi vũ khí này đang được trang bị.")]
    public float damageMultiplier = 1f;
    public WeaponStats weaponStats;
}

public enum WeaponType
{
    None = 0,
    Melee = 1,
    Bow = 2,
    MagicWand = 3
}

[System.Serializable]
public class WeaponStats
{
    public float damageBonus; //Damage cộng thêm
    [Range(0f, 2f)] public float damagePercentage; // damage theo phần trăm (ví dụ: 0.2f = 20% damage cộng thêm)
    public float poisonDamage;
}
