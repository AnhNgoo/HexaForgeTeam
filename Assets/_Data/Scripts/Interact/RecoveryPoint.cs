using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoveryPoint : InteractBase
{
    [SerializeField] private int recoveryAmount = 1;
    public override string InteractionName => "Receive recovery bottle";

    private bool isUsed = false;

    protected override void Update()
    {
        if (isUsed) return;
        base.Update();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isUsed) return;
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (isUsed) return;
        base.OnTriggerExit(other);
    }
    protected override void InteractAction()
    {
        isUsed = true;
        InteractionManager.Instance?.UnregisterInteractable(this);
        ObjectPooling.Instance.SpawnFromPool(PoolType.ReceiveRecoveryBottleEffect, transform.position + Vector3.up * 0.21f, Quaternion.identity, transform);
        character?.CharacterRecovery?.AddBottle(recoveryAmount);
        Hide(1f);
    }

    public override void ResetInteraction()
    {
        isUsed = false;
    }

    private async void Hide(float delay)
    {
        await System.Threading.Tasks.Task.Delay((int)(delay * 1000));
        gameObject.SetActive(false);
    }
}
