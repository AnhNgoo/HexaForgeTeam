using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CharacterWeapon : MonoBehaviour
{
    [SerializeField] protected WeaponBase currentWeapon;
    public WeaponBase CurrentWeapon => currentWeapon;
    [SerializeField] protected Transform weaponHoldPoint;
    public Transform WeaponHoldPoint => weaponHoldPoint;

    [Button("Equip Weapon")]
    public void EquipWeapon(WeaponBase newWeapon, Transform weaponTransform = null, float sizeWeapon = 1f)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("No weapon to equip. Please assign a weapon to currentWeapon.");
            return;
        }
        if (weaponTransform == null)
            weaponTransform = weaponHoldPoint;

        currentWeapon = newWeapon;
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.transform.SetParent(weaponTransform);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.transform.localScale = Vector3.one * sizeWeapon;
    }

    [Button("Unequip Weapon")]
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.transform.SetParent(null);
            currentWeapon = null;
        }
    }
}
