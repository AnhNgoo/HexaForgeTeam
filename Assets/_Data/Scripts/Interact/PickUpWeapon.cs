using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWeapon : InteractBase
{
    [SerializeField] private WeaponData weaponData;
    protected override void InteractAction()
    {
        if (weaponData != null)
        {
            EquipmentSystem.Instance.AddWeapon(weaponData);
            Destroy(gameObject); //NOTE - lần sau sẽ dùng object pooling để tái sử dụng vũ khí thay vì hủy nó
        }
    }
}
