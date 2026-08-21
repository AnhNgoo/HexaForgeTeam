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
}

public enum WeaponType
{
    None = 0,
    Melee = 1,
    Bow = 2,
    MagicWand = 3
}
