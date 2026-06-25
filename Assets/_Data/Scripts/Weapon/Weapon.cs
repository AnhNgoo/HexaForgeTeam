using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : LoadComponents, IWeapon
{
    [SerializeField] private WeaponData weaponData;
    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {

    }
}
