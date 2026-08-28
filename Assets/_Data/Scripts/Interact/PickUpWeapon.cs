using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWeapon : InteractBase, IPoolable
{
    [SerializeField] private WeaponData weaponData;
    private WeaponData runtimeWeaponData;
    private WeaponData CurrentWeaponData => runtimeWeaponData != null ? runtimeWeaponData : weaponData;
    [Header("Set Pool Type trùng tên với tên prefab")]
    [SerializeField] private PoolType poolType;
    public PoolType PoolType => poolType;
    public override string InteractionName => "Pick Up Weapon";
    public void Initialize(WeaponData data)
    {
        runtimeWeaponData = data;
    }

    protected override void InteractAction()
    {
        WeaponInventorySystem inventory = WeaponInventorySystem.Instance;

        WeaponData data = CurrentWeaponData;

        if (inventory == null || data == null || ObjectPooling.Instance == null)
        {
            return;
        }

        if (!inventory.CheckEmptyWeaponSlots())
        {
            NotifyUI notify = ObjectPooling.Instance.SpawnFromPool(PoolType.NotifyUI)?.GetComponent<NotifyUI>();

            notify?.SetDescription("The weapon inventory is full.");
            return;
        }
        Vector3 effectPosition = transform.position + Vector3.down * 0.5f;
        inventory.AddWeapon(data);
        EventManager.Notify(GameEvent.OnHidePickUpItemPanel);
        InteractionManager.Instance?.UnregisterInteractable(this);
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
        ObjectPooling.Instance.SpawnFromPool(PoolType.PickedUpItemEffect, effectPosition, Quaternion.identity);
    }

    public void OnSpawnFromPool()
    {
        ResetInteraction();
    }

    public void OnReturnToPool()
    {
        ResetInteraction();
        runtimeWeaponData = null;
    }

    public override void ResetInteraction()
    {
        InteractionManager.Instance?.UnregisterInteractable(this);
        HideHighlight();

        playerInRange = false;
        character = null;
    }
}
