using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

public class WeaponInventorySystem : Singleton<WeaponInventorySystem>
{
    [SerializeField] private CharacterWeapon characterWeapon;
    [SerializeField] private List<WeaponData> weaponSlots = new List<WeaponData>() { null, null, null }; // Giới hạn số lượng vũ khí có thể trang bị
    public List<WeaponData> WeaponSlots => weaponSlots;

    [SerializeField] private int maxWeaponSlots = 3;
    [SerializeField] private int currentWeaponIndex = -1; // Index của vũ khí đang được trang bị, -1 nếu không có vũ khí nào được trang bị
    public int CurrentWeaponIndex => currentWeaponIndex;
    public int IndexWeaponSelectedInInventory; // Vũ khí đang được chọn trong menu Inventory, có thể khác với vũ khí đang được trang bị
    [SerializeField] private Vector2 forceDropItem = new Vector2(1f, 2f);
    private bool isSelectingRewardReplacement;

    protected override void Awake()
    {
        base.Awake();
        EventManager.Subscribe(GameEvent.OnSelectItemInInventory, SetWeaponSelectedInInventory);
        EventManager.Subscribe(GameEvent.OnDeselectItemInInventory, UnsetWeaponSelectedInInventory);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnSelectItemInInventory, SetWeaponSelectedInInventory);
        EventManager.Unsubscribe(GameEvent.OnDeselectItemInInventory, UnsetWeaponSelectedInInventory);
    }
    public void Init(CharacterWeapon characterWeapon)
    {
        this.characterWeapon = characterWeapon;
        ClearAllWeaponInventory();
        EventManager.Notify(GameEvent.OnUpdateDisplayWeapon, null);
    }

    private void Update()
    {
        if (isSelectingRewardReplacement)
            return;

        if (InputManager.InputActions.Keyboard.Discard.triggered && (UIManager.Instance.CurrentMenuType == MenuType.InventoryMenu || UIManager.Instance.CurrentMenuType == MenuType.GameSystemMenu))
        {
            if (IndexWeaponSelectedInInventory != -1)
            {
                DiscardWeapon(IndexWeaponSelectedInInventory);
                IndexWeaponSelectedInInventory = -1;
            }
        }
    }

    public void SetRewardReplacementMode(bool active)
    {
        isSelectingRewardReplacement = active;
        IndexWeaponSelectedInInventory = -1;
    }

    [Button("Add Weapon")]
    public void AddWeapon(WeaponData weaponData)
    {
        int emptySlotIndex = GetFirstEmptySlotIndex();

        if (weaponData == null || emptySlotIndex < 0)
        {
            Debug.LogWarning("Weapon inventory đã đầy hoặc reward không hợp lệ.");
            return;
        }


        // Kiểm tra nếu đang dùng vũ khí, thì chỉ thêm vào danh sách mà không trang bị
        if (currentWeaponIndex != -1)
        {
            weaponSlots[emptySlotIndex] = weaponData;
            ItemSLotData itemSLotData = new ItemSLotData()
            {
                itemData = weaponData,
                index = emptySlotIndex
            };
            EventManager.Notify(GameEvent.OnAddWeaponToInventory, itemSLotData);
            Debug.Log($"Added {weaponData.name} to inventory at slot {emptySlotIndex}.");
            return;
        }

        // Nếu chưa có vũ khí nào hoặc đang không dùng vũ khí, thêm vào danh sách và trang bị ngay lập tức
        weaponSlots[emptySlotIndex] = weaponData;
        characterWeapon.EquipWeapon(weaponData);
        currentWeaponIndex = GetIndexOfWeapon(weaponData);

        ItemSLotData _itemSLotData = new ItemSLotData()
        {
            itemData = weaponData,
            index = emptySlotIndex
        };
        EventManager.Notify(GameEvent.OnAddWeaponToInventory, _itemSLotData);
        EventManager.Notify(GameEvent.OnUpdateDisplayWeapon, weaponData);
        Debug.Log($"Added and equipped {weaponData.name} to inventory at slot {emptySlotIndex}.");
    }

    public bool TryAddWeapon(WeaponData weaponData)
    {
        if (weaponData == null || !CheckEmptyWeaponSlots())
            return false;

        AddWeapon(weaponData);
        return true;
    }

    public bool TryReplaceWeapon(int slotIndex, WeaponData rewardWeapon)
    {
        if (rewardWeapon == null || characterWeapon == null || slotIndex < 0 || slotIndex >= weaponSlots.Count)
        {
            return false;
        }

        WeaponData oldWeapon = weaponSlots[slotIndex];

        if (oldWeapon == null)
            return TryAddWeapon(rewardWeapon);

        // Spawn thành công rồi mới xóa weapon cũ.
        if (!TrySpawnWeaponPickup(oldWeapon))
            return false;

        bool wasEquipped = currentWeaponIndex == slotIndex || characterWeapon.CurrentWeapon == oldWeapon;

        if (wasEquipped) characterWeapon.UnequipWeapon();

        weaponSlots[slotIndex] = rewardWeapon;

        EventManager.Notify(GameEvent.OnDiscardItemInInventory, slotIndex);
        EventManager.Notify(GameEvent.OnAddWeaponToInventory, new ItemSLotData
        {
            itemData = rewardWeapon,
            index = slotIndex
        });

        if (wasEquipped)
        {
            characterWeapon.EquipWeapon(rewardWeapon);
            currentWeaponIndex = slotIndex;
            EventManager.Notify(GameEvent.OnUpdateDisplayWeapon, rewardWeapon);
        }

        return true;
    }

    /// <summary>
    /// Thử spawn vũ khí thành công, nếu spawn thất bại thì giữ lại vũ khí cũ
    /// </summary>
    /// <param name="weaponData"></param>
    /// <returns></returns>
    private bool TrySpawnWeaponPickup(WeaponData weaponData)
    {
        if (weaponData == null ||
            weaponData.pickUpItem == PoolType.None ||
            characterWeapon == null ||
            ObjectPooling.Instance == null)
        {
            return false;
        }

        Vector3 position =
            characterWeapon.transform.position + Vector3.up;

        GameObject pickup = ObjectPooling.Instance.SpawnFromPool(
            weaponData.pickUpItem,
            position,
            Quaternion.identity);

        if (pickup == null)
            return false;

        if (!pickup.TryGetComponent(out PickUpWeapon pickupWeapon))
        {
            ObjectPooling.Instance.ReturnToPool(
                weaponData.pickUpItem,
                pickup);

            Debug.LogError(
                $"{weaponData.name}: pickup prefab thiếu PickUpWeapon.");

            return false;
        }

        pickupWeapon.Initialize(weaponData);

        if (pickup.TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            float horizontal = Random.Range(-1f, 1f);
            Vector3 direction =
                characterWeapon.transform.forward +
                characterWeapon.transform.right * horizontal +
                Vector3.up;

            rb.AddForce(
                direction.normalized *
                Random.Range(forceDropItem.x, forceDropItem.y),
                ForceMode.Impulse);
        }

        return true;
    }

    [Button("Discard Weapon")]
    public void DiscardWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Count)
        {
            Debug.LogWarning("Invalid weapon index.");
            return;
        }

        WeaponData weaponToRemove = weaponSlots[index];

        if (weaponToRemove == null)
            return;

        if (!TrySpawnWeaponPickup(weaponToRemove))
        {
            Debug.LogWarning(
                $"Không thể drop {weaponToRemove.name}. Weapon được giữ lại.");

            return;
        }

        // Nếu vũ khí đang được trang bị, hãy bỏ trang bị trước khi xóa
        if (characterWeapon.CurrentWeapon == weaponToRemove)
        {
            currentWeaponIndex = -1;
            characterWeapon.UnequipWeapon();
            EventManager.Notify(GameEvent.OnUpdateDisplayWeapon, null);
        }

        weaponSlots[index] = null; // Đặt slot thành null thay vì xóa khỏi danh sách
        EventManager.Notify(GameEvent.OnDiscardItemInInventory, index);
    }

    private void ClearAllWeaponInventory()
    {
        characterWeapon.UnequipWeapon();
        currentWeaponIndex = -1;
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            weaponSlots[i] = null;
            EventManager.Notify(GameEvent.OnDiscardItemInInventory, i);
        }
    }
    // Đặt vũ khí được chọn trong menu Inventory, có thể khác với vũ khí đang được trang bị
    private void SetWeaponSelectedInInventory(object obj)
    {
        if (obj is not int index || index < 0)
        {
            return;
        }
        IndexWeaponSelectedInInventory = index;
    }

    // Bỏ đặt vũ khí được chọn trong menu Inventory
    private void UnsetWeaponSelectedInInventory(object obj)
    {
        IndexWeaponSelectedInInventory = -1;
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
            EventManager.Notify(GameEvent.OnUpdateDisplayWeapon, null);
            return;
        }

        characterWeapon.UnequipWeapon();

        WeaponData newWeapon = weaponSlots[index];
        characterWeapon.EquipWeapon(newWeapon);
        currentWeaponIndex = index;
        EventManager.Notify(GameEvent.OnUpdateDisplayWeapon, newWeapon);
    }

    public void SpawnPickUpItems(WeaponData weaponData)
    {
        GameObject pickUpItem = ObjectPooling.Instance.SpawnFromPool(weaponData.pickUpItem, characterWeapon.transform.position + Vector3.up * 1f, Quaternion.identity);

        if (pickUpItem != null)
        {
            Rigidbody rb = pickUpItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float x = Random.Range(-1f, 1f);
                Vector3 direction = characterWeapon.transform.forward + characterWeapon.transform.right * x + Vector3.up;

                float forceDropItem = Random.Range(this.forceDropItem.x, this.forceDropItem.y);
                rb.AddForce(direction.normalized * forceDropItem, ForceMode.Impulse);
            }
        }
    }
    #region Get Weapon Info
    // Lấy index của vũ khí trong danh sách weaponSlots, nếu không tìm thấy thì trả về -1
    public int GetIndexOfWeapon(WeaponData weaponData)
    {
        return weaponSlots.IndexOf(weaponData);
    }

    //  Tìm index có slot trống đầu tiên trong danh sách weaponSlots, nếu không tìm thấy thì trả về -1
    public int GetFirstEmptySlotIndex()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] == null)
            {
                return i;
            }
        }
        return -1; // Không tìm thấy slot trống
    }

    public int GetWeaponCount()
    {
        return weaponSlots.Count;
    }

    public WeaponData GetWeaponAtIndex(int index)
    {
        if (index < 0 || index >= weaponSlots.Count)
        {
            Debug.LogWarning("Invalid weapon index.");
            return null;
        }
        return weaponSlots[index];
    }

    #endregion

    #region Check Weapon Slots
    // Kiểm tra còn vũ khí nào trong danh sách weaponSlots hay không, nếu còn thì trả về true, nếu hết thì trả về false
    public bool CheckWeaponInSlots()
    {
        foreach (WeaponData weapon in weaponSlots)
        {
            if (weapon != null)
                return true;
        }
        return false;
    }
    public bool CheckEmptyWeaponSlots()
    {
        return weaponSlots.Exists(weapon => weapon == null);
    }
    #endregion
}

