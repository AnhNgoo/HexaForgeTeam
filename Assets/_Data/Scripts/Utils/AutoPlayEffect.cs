using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlayEffect : LoadComponents
{
    [SerializeField] private ParticleSystem effect;
    protected override void LoadComponent()
    {
        if (effect == null)
            effect = GetComponentInChildren<ParticleSystem>();
    }

    protected override void LoadComponentRuntime()
    {
    }

    private void OnEnable()
    {
        effect.Play();
    }

    private void OnDisable()
    {
        effect.Stop();
    }
}