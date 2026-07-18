using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ItemDataBase
{
    public WeaponType weaponType;
    public PoolType weapon;
    public PoolType pickUpItem;
}

public enum WeaponType
{
    None = 0,
    Melee = 1,
    Bow = 2,
    MagicWand = 3,
}
