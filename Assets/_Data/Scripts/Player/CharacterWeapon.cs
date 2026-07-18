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

    public bool HasWeapon => currentWeapon != null; // Kiểm tra xem nhân vật có vũ khí hay không, nếu không có thì dùng combo tay không
    protected GameObject currentWeaponObject;
    protected CharacterBase character;

    public void Init(CharacterBase character, Transform weaponHoldPoint)
    {
        this.character = character;
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
}
