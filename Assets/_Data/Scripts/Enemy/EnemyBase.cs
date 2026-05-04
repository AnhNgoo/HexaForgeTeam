using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : LoadComponents, IPoolable
{
    public PoolType PoolType => PoolType.Enemy;

    private void CacheReferences()
    {

    }

    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {
    }

    public void OnSpawnFromPool()
    {
        gameObject.SetActive(true);
    }

    public void OnReturnToPool()
    {
        gameObject.SetActive(false);
    }


}
