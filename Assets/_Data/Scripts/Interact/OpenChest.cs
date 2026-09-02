using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ItemRarityRate
{
    public ItemRarity itemRarity;
    public float dropRate;
}
public class OpenChest : InteractBase
{
    [SerializeField] private Animator chestAnim;
    [SerializeField] private Vector2 forceDropItem = new Vector2(3f, 5f);

    [Header("List of Pick Up Items")]
    [SerializeField] private List<ItemRarityRate> itemRarities = new List<ItemRarityRate>();
    [SerializeField] private List<WeaponData> pickUpItems = new List<WeaponData>();
    public override string InteractionName => "Open Chest";
    private bool isOpened = false;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (chestAnim == null)
        {
            chestAnim = modelItem.GetComponent<Animator>();
        }
    }

    protected override void Update()
    {
        if (isOpened) return;
        base.Update();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isOpened) return;
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (isOpened) return;
        base.OnTriggerExit(other);
    }

    protected override void InteractAction()
    {
        isOpened = true;
        chestAnim.CrossFade("OpenChest", 0.1f);
        InteractionManager.Instance?.UnregisterInteractable(this);
        SpawnPickUpItems();
    }

    [Button("Get All Weapon Data", ButtonSizes.Medium)]
    private void GetAllWeaponData()
    {
        pickUpItems.Clear();

#if UNITY_EDITOR
        string[] weaponGuids = AssetDatabase.FindAssets("t:WeaponData");
        foreach (string weaponGuid in weaponGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(weaponGuid);
            WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);

            if (weaponData != null)
            {
                pickUpItems.Add(weaponData);
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"Loaded {pickUpItems.Count} WeaponData into {name}.", this);
#else
        Debug.LogWarning("Get All Weapon Data chỉ hoạt động trong Unity Editor.", this);
#endif
    }

    public async void SpawnPickUpItems()
    {
        await UniTask.Delay(1500);

        WeaponData selectedWeapon = GetRandomWeaponData();
        if (selectedWeapon == null || selectedWeapon.pickUpItem == PoolType.None)
        {
            Debug.LogWarning($"{name} không có WeaponData hợp lệ để drop.", this);
            return;
        }

        GameObject pickUpItem = ObjectPooling.Instance.SpawnFromPool(
            selectedWeapon.pickUpItem,
            transform.position + Vector3.up * 1f,
            Quaternion.identity);

        if (pickUpItem != null)
        {
            pickUpItem.GetComponent<PickUpWeapon>()?.Initialize(selectedWeapon);

            Rigidbody rb = pickUpItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float x = Random.Range(-1f, 1f);
                Vector3 direction = transform.forward + transform.right * x + Vector3.up;

                float forceDropItem = Random.Range(this.forceDropItem.x, this.forceDropItem.y);
                rb.AddForce(direction.normalized * forceDropItem, ForceMode.Impulse);
            }
        }
    }

    private WeaponData GetRandomWeaponData()
    {
        if (!TryGetAllowedWeaponType(out WeaponType allowedWeaponType))
        {
            return null;
        }

        List<ItemRarityRate> validRarityRates = new List<ItemRarityRate>();
        float totalDropRate = 0f;

        foreach (ItemRarityRate rarityRate in itemRarities)
        {
            if (rarityRate == null || rarityRate.dropRate <= 0f)
            {
                continue;
            }

            bool hasMatchingWeapon = pickUpItems.Exists(weaponData =>
                weaponData != null &&
                weaponData.pickUpItem != PoolType.None &&
                weaponData.weaponType == allowedWeaponType &&
                weaponData.rarity == rarityRate.itemRarity);

            if (hasMatchingWeapon)
            {
                validRarityRates.Add(rarityRate);
                totalDropRate += rarityRate.dropRate;
            }
        }

        if (totalDropRate <= 0f)
        {
            return null;
        }

        float roll = Random.value * totalDropRate;
        ItemRarity selectedRarity = ItemRarity.Common;
        foreach (ItemRarityRate rarityRate in validRarityRates)
        {
            roll -= rarityRate.dropRate;
            if (roll <= 0f)
            {
                selectedRarity = rarityRate.itemRarity;
                break;
            }
        }

        List<WeaponData> matchingWeapons = pickUpItems.FindAll(weaponData =>
            weaponData != null &&
            weaponData.pickUpItem != PoolType.None &&
            weaponData.weaponType == allowedWeaponType &&
            weaponData.rarity == selectedRarity);

        return matchingWeapons.Count == 0
            ? null
            : matchingWeapons[Random.Range(0, matchingWeapons.Count)];
    }

    private bool TryGetAllowedWeaponType(out WeaponType allowedWeaponType)
    {
        allowedWeaponType = WeaponType.None;

        CharacterBase currentCharacter = PlayerManager.Instance?.CurrentCharacterBase;
        CharacterTypes characterType = currentCharacter?.CharacterData?.characterTypes ?? CharacterTypes.None;

        switch (characterType)
        {
            case CharacterTypes.PhysicalMelee:
                allowedWeaponType = WeaponType.Melee;
                return true;
            case CharacterTypes.Magical:
                allowedWeaponType = WeaponType.MagicWand;
                return true;
            default:
                Debug.LogWarning($"{name} không xác định được loại nhân vật để chọn vũ khí.", this);
                return false;
        }
    }

    public override void ResetInteraction()
    {
        isOpened = false;
        chestAnim.CrossFade("CloseChest", 0.1f);
    }
}
