using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SlashEffect : LoadComponents
{
    [SerializeField] private Transform slashPoint;

    protected override void LoadComponent()
    {
        if (slashPoint == null)
            slashPoint = transform.Find("SlashPoint").transform;
    }

    protected override void LoadComponentRuntime()
    {
    }

    // private void Start()
    // {
    //     EventManager.Instance.Subscribe(GameEvent.OnSlashEffect, _ => Play());
    // }
    private void OnEnable()
    {
        EventManager.Instance?.Subscribe(GameEvent.OnSlashEffect, _ => Play());
    }

    private void OnDisable()
    {
        EventManager.Instance?.Unsubscribe(GameEvent.OnSlashEffect, _ => Play());
    }
    public void Play()
    {
        var effect = ObjectPooling.Instance?.SpawnFromPool(PoolType.KaelSlashEffect, slashPoint.position, slashPoint.rotation);

        if (effect == null)
            return;

        if (effect.TryGetComponent<VisualEffect>(out var vfx))
        {
            vfx.Play();
        }
    }
}
