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
    [SerializeField] protected WeaponBase weaponStored;

    public void Init(Transform weaponHoldPoint)
    {
        if (weaponHoldPoint == null)
        {
            Debug.LogWarning("Weapon hold point is not assigned. Please assign a transform to weaponHoldPoint.");
            return;
        }
        this.weaponHoldPoint = weaponHoldPoint;
    }

    [Button("Equip Weapon")]
    public void EquipWeapon(WeaponBase newWeapon, float sizeWeapon = 1f)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("No weapon to equip. Please assign a weapon to currentWeapon.");
            return;
        }

        if (weaponHoldPoint == null)
        {
            Debug.LogWarning("Weapon hold point is not assigned. Please assign a transform to weaponHoldPoint.");
            return;
        }

        currentWeapon = newWeapon;
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.transform.SetParent(weaponHoldPoint);
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

    // Cất vũ khí
    public void StoreWeapon()
    {
        if (currentWeapon != null)
        {
            weaponStored = currentWeapon;
            currentWeapon.gameObject.SetActive(false);
            currentWeapon = null;
        }
    }

    // Lấy lại vũ khí đã cất
    public void RetrieveWeapon()
    {
        if (weaponStored != null)
        {
            EquipWeapon(weaponStored);
            weaponStored = null;
        }
    }
}
