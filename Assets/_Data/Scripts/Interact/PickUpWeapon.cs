using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWeapon : InteractBase, IPoolable
{
    [SerializeField] private WeaponData weaponData;
    [Header("Set Pool Type trùng tên với tên prefab")]
    [SerializeField] private PoolType poolType;
    public PoolType PoolType => poolType;
    public override string InteractionName => "Pick Up Weapon";

    protected override void InteractAction()
    {
        if (WeaponInventorySystem.Instance.CheckEmptyWeaponSlots() == false)
        {
            NotifyUI notifyUI = ObjectPooling.Instance.SpawnFromPool(PoolType.NotifyUI).GetComponent<NotifyUI>();
            notifyUI.SetDescription("The weapon slots are full.");
            return;
        }
        if (weaponData != null)
        {
            WeaponInventorySystem.Instance.AddWeapon(weaponData);
            EventManager.Notify(GameEvent.OnHidePickUpItemPanel);
            InteractionManager.Instance?.UnregisterInteractable(this);
            ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
            ObjectPooling.Instance.SpawnFromPool(PoolType.PickedUpItemEffect, transform.position + new Vector3(0, -0.5f, 0), Quaternion.identity);
        }
    }

    public void OnSpawnFromPool()
    {

    }

    public void OnReturnToPool()
    {

    }

    public override void ResetInteraction()
    {

    }
}
