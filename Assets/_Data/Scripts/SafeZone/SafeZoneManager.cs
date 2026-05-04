using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SafeZoneManager : MonoBehaviour
{
    [SerializeField] private SafeZone safeZonePrefab;

    [Button("Create Safe Zone")]
    public void CreateSafeZone()
    {
        safeZonePrefab = ObjectPooling.Instance?
                .SpawnFromPool(PoolType.SafeZone,
                transform.position,
                Quaternion.identity)?.GetComponent<SafeZone>();

        if (safeZonePrefab == null) return;

        safeZonePrefab.InitSafeZone(transform, 100f);
    }
}
