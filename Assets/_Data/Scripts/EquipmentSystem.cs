using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EquipmentSystem : Singleton<EquipmentSystem>
{
    [SerializeField] private CharacterWeapon characterWeapon;
    [SerializeField] private List<WeaponData> weaponSlots = new List<WeaponData>();
    public List<WeaponData> WeaponSlots => weaponSlots;

    [SerializeField] private int currentWeaponIndex = 0;
    public int CurrentWeaponIndex => currentWeaponIndex;

    public void Init(CharacterWeapon characterWeapon)
    {
        this.characterWeapon = characterWeapon;
    }

    [Button("Add Weapon")]
    public void AddWeapon(WeaponData weaponData)
    {
        // Kiểm tra nếu có ít nhất 1 vũ khí rồi thì chỉ thêm vào danh sách vũ khí mà không cần trang bị
        if (weaponSlots.Count > 0)
        {
            weaponSlots.Add(weaponData);
            return;
        }
        weaponSlots.Add(weaponData);
        characterWeapon.EquipWeapon(weaponData);
        currentWeaponIndex = GetIndexOfWeapon(weaponData);
    }

    [Button("Remove Weapon")]
    public void RemoveWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Count)
        {
            Debug.LogWarning("Invalid weapon index.");
            return;
        }

        WeaponData weaponToRemove = weaponSlots[index];

        // Nếu vũ khí đang được trang bị, hãy bỏ trang bị trước khi xóa
        if (characterWeapon.CurrentWeapon == weaponToRemove)
        {
            characterWeapon.UnequipWeapon();
        }

        weaponSlots.RemoveAt(index);

        // Nếu danh sách vũ khí không còn vũ khí nào, hãy đặt CurrentWeaponIndex về -1
        if (weaponSlots.Count == 0)
        {
            currentWeaponIndex = -1;
            return;
        }
        // Cập nhật CurrentWeaponIndex nếu vũ khí bị xóa là vũ khí đang được trang bị
        if (currentWeaponIndex >= weaponSlots.Count)
        {
            currentWeaponIndex = weaponSlots.Count - 1;
        }

    }

    // Đổi vũ khí theo index
    public void ChangeWeapon(int index)
    {
        if (index < -1 || index >= weaponSlots.Count)
        {
            Debug.LogWarning("Invalid weapon index.");
            return;
        }

        if (index == -1) // Nếu index là -1, chuyển thành tay không
        {

            characterWeapon.UnequipWeapon();
            currentWeaponIndex = -1;
            return;
        }

        characterWeapon.UnequipWeapon();

        WeaponData newWeapon = weaponSlots[index];
        characterWeapon.EquipWeapon(newWeapon);
        currentWeaponIndex = index;
    }

    // Lấy index của vũ khí trong danh sách weaponSlots, nếu không tìm thấy thì trả về -1
    public int GetIndexOfWeapon(WeaponData weaponData)
    {
        return weaponSlots.IndexOf(weaponData);
    }

    public int GetWeaponCount()
    {
        return weaponSlots.Count;
    }
}

