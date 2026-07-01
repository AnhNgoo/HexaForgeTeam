using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CharacterWeapon : MonoBehaviour
{
    [SerializeField] protected WeaponData currentWeapon;
    public WeaponData CurrentWeapon => currentWeapon;
    [SerializeField] protected Transform weaponHoldPoint;
    public Transform WeaponHoldPoint => weaponHoldPoint;
    [SerializeField] protected WeaponData weaponStored;


    protected GameObject currentWeaponObject;

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
    public void EquipWeapon(WeaponData newWeaponData, float sizeWeapon = 1f)
    {
        if (newWeaponData == null)
        {
            Debug.LogWarning("No weapon to equip. Please assign a weapon to currentWeapon.");
            return;
        }

        if (weaponHoldPoint == null)
        {
            Debug.LogWarning("Weapon hold point is not assigned. Please assign a transform to weaponHoldPoint.");
            return;
        }

        currentWeaponObject = ObjectPooling.Instance.SpawnFromPool(newWeaponData.weapon, weaponHoldPoint.position, weaponHoldPoint.rotation, weaponHoldPoint);

        currentWeapon = newWeaponData;
    }

    [Button("Unequip Weapon")]
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            Debug.Log($"Unequipping weapon khoa");
            ObjectPooling.Instance.ReturnToPool(currentWeapon.weapon, currentWeaponObject);
            currentWeapon = null;
        }
    }

    // Cất vũ khí
    public void StoreWeapon()
    {
        if (currentWeapon != null)
        {
            weaponStored = currentWeapon;
            currentWeaponObject.gameObject.SetActive(false);
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

    public void ChangeWeapon()
    {
        int nextWeaponIndex = EquipmentSystem.Instance.CurrentWeaponIndex + 1;
        if (nextWeaponIndex >= EquipmentSystem.Instance.WeaponSlots.Count)
        {
            nextWeaponIndex = -1; // Quay lại vũ khí đầu tiên nếu vượt quá danh sách
        }
        EquipmentSystem.Instance.ChangeWeapon(nextWeaponIndex);
    }
}
