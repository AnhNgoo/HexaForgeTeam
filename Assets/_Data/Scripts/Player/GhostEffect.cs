using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEffect : LoadComponents
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderer = new SkinnedMeshRenderer[0]; //SkinnedMeshRenderer của nhân vật

    protected override void LoadComponent()
    {
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.Length == 0)
            skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (defaultMaterial == null && skinnedMeshRenderer.Length > 0)
            defaultMaterial = skinnedMeshRenderer[0].sharedMaterial;
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void SetGhostEffect(bool isGhost)
    {
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.Length == 0)
            return;

        foreach (var smr in skinnedMeshRenderer)
        {
            if (smr == null)
                continue;

            if (isGhost)
            {
                smr.material = ghostMaterial;
            }
            else
            {
                smr.material = defaultMaterial;
            }
        }
    }
}
