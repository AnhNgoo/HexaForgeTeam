using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DestroyOverLifetime : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private PoolType poolType;
    public PoolType PoolType => poolType;


    public void OnReturnToPool()
    {

    }

    private void ReturnToPool()
    {
        ObjectPooling.Instance?.ReturnToPool(PoolType, gameObject);
    }

    public void OnSpawnFromPool()
    {
        Invoke(nameof(ReturnToPool), lifetime);
    }

}
