using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpGoldFalling : InteractBase, IPoolable
{
    [SerializeField] private int goldAmount;
    public override string InteractionName => "Pick Up Gold";

    public PoolType PoolType => PoolType.GoldFalling;

    public void Init(int goldAmount)
    {
        this.goldAmount = goldAmount;
    }

    public void OnReturnToPool()
    {

    }

    public void OnSpawnFromPool()
    {

    }

    public override void ResetInteraction()
    {

    }

    protected override void InteractAction()
    {
        GoldManager.Instance.AddGold(goldAmount);
        InteractionManager.Instance?.UnregisterInteractable(this);
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject);
        ObjectPooling.Instance.SpawnFromPool(PoolType.PickedUpItemEffect, transform.position + new Vector3(0, -0.5f, 0), Quaternion.identity);
    }
}
