using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpWeapon : InteractBase, IPoolable
{
    [SerializeField] private float lifeTime = 60f; // Thời gian sống của vũ khí sau khi rơi ra khỏi rương
    [SerializeField] private WeaponData weaponData;
    private WeaponData runtimeWeaponData;
    private WeaponData CurrentWeaponData => runtimeWeaponData != null ? runtimeWeaponData : weaponData;

    [Header("Set Pool Type trùng tên với tên prefab")]
    [SerializeField] private PoolType poolType;
    public PoolType PoolType => poolType;
    public override string InteractionName => "Pick Up Weapon";
    private IEnumerator lifeTimeCoroutine;

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
        EventManager.Subscribe(GameEvent.OnPlayerDeath, ReturnToPoolOnCharacterDeath);
        EventManager.Subscribe(GameEvent.OnLoadingComplete, ReturnToPoolOnCharacterDeath);
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
            lifeTimeCoroutine = null;
        }
        lifeTimeCoroutine = LifeTimeCoroutine();
        StartCoroutine(lifeTimeCoroutine);
    }

    public void OnReturnToPool()
    {
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
            lifeTimeCoroutine = null;
        }

        EventManager.Unsubscribe(GameEvent.OnPlayerDeath, ReturnToPoolOnCharacterDeath);
        EventManager.Unsubscribe(GameEvent.OnLoadingComplete, ReturnToPoolOnCharacterDeath);
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

    // trả về pool khi nhân vật về khi loading xong hoặc khi nhân vật chết
    private void ReturnToPoolOnCharacterDeath(object data = null)
    {
        InteractionManager.Instance?.UnregisterInteractable(this);
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
    }

    private IEnumerator LifeTimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        InteractionManager.Instance?.UnregisterInteractable(this);
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
    }
}