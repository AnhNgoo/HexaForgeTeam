using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 1)]
public class WeaponData : ScriptableObject
{
    public WeaponType weaponType;
    public PoolType weapon;
}

public enum WeaponType
{
    None = 0,
    Melee = 1,
    Bow = 2,
    MagicWand = 3,
}
