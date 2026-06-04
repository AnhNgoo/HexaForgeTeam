using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Linq;

public class TrailSlashEffect : LoadComponents
{
    [SerializeField] private List<TrailRenderer> slashEffects = new List<TrailRenderer>();

    protected override void LoadComponent()
    {
        if (slashEffects.Count > 0) return;
        slashEffects = GetComponentsInChildren<TrailRenderer>(true).ToList();
    }

    protected override void LoadComponentRuntime()
    {
    }

    private void OnEnable()
    {
        EventManager.Subscribe(GameEvent.OnEnableTrailSlashEffect, _ => Play());
        EventManager.Subscribe(GameEvent.OnDisableTrailSlashEffect, _ => Stop());
        ResetTrailSlashEffect();
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvent.OnEnableTrailSlashEffect, _ => Play());
        EventManager.Unsubscribe(GameEvent.OnDisableTrailSlashEffect, _ => Stop());
    }
    public void Play()
    {
        foreach (var slashEffect in slashEffects)
        {
            slashEffect.Clear();
            slashEffect.emitting = true;
        }
    }

    public void Stop()
    {
        ResetTrailSlashEffect();
    }

    private void ResetTrailSlashEffect()
    {
        foreach (var slashEffect in slashEffects)
        {
            slashEffect.Clear();
            slashEffect.emitting = false;
        }
    }
}
