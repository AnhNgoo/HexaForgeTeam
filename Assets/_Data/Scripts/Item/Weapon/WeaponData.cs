using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ItemDataBase
{
    public WeaponType weaponType;
    public PoolType weapon;
    public PoolType pickUpItem;
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
    public float damageBonus;
    [Range(0f, 2f)] public float damagePercentage;
    public float poisonDamage;
}
