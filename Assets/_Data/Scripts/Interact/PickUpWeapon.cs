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
        if (weaponData != null)
        {
            EquipmentSystem.Instance.AddWeapon(weaponData);
            EventManager.Notify(GameEvent.OnHidePickUpItemPanel);
            InteractionManager.Instance?.UnregisterInteractable(this);
            ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
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
