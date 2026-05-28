using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashEffect : LoadComponents
{
    [SerializeField] private PoolType poolType;

    [Header("Hướng nghiêng của các hiệu ứng chém")]
    [SerializeField] private List<float> rotationZValues; // Các góc quay Z khác nhau cho hiệu ứng chém

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }

    private void OnEnable()
    {
        EventManager.Subscribe(GameEvent.OnPlaySlashEffect, Play);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvent.OnPlaySlashEffect, Play);
    }
    private void Play(object data)
    {
        if (data is not int index) return;

        float rotationZ = rotationZValues[index - 1];
        Quaternion rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, rotationZ);
        transform.rotation = rotation; //Nghiêng hiệu ứng theo hướng chém

        ObjectPooling.Instance?.SpawnFromPool(poolType, transform.position, transform.rotation, transform);

    }
}
