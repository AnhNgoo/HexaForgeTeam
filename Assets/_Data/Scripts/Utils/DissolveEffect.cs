using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class DissolveEffect : LoadComponents
{
    [SerializeField] private Material dissolveMaterial; //Material dùng để tạo hiệu ứng dissolve
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderer = new SkinnedMeshRenderer[0]; //SkinnedMeshRenderer của nhân vật
    private IEnumerator dissolveCoroutine; // Coroutine để quản lý hiệu ứng dissolve
    private Material cacheDefaultMaterial; // Lưu lại material gốc để reset sau khi dissolve xong
    private Texture cacheDefaultTexture; // Lưu lại texture gốc để reset sau khi dissolve xong

    protected override void LoadComponent()
    {
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.Length == 0)
            skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    [Button("Play Dissolve Effect")]
    public void PlayDissolveEffect(float duration)
    {
        if (skinnedMeshRenderer == null || skinnedMeshRenderer.Length == 0)
            return;

        ResetDefaultMaterial();
        cacheDefaultMaterial = skinnedMeshRenderer[0].material; // Lưu lại material gốc để reset sau khi dissolve xong
        cacheDefaultTexture = cacheDefaultMaterial.GetTexture("_Texture2D"); // Lưu lại texture gốc để reset sau khi dissolve xong

        dissolveMaterial.SetTexture("_Texture2D", cacheDefaultTexture); // Set texture gốc vào dissolve material

        foreach (var smr in skinnedMeshRenderer)
        {
            if (smr == null || !smr.enabled) continue;

            smr.material = dissolveMaterial; // Set dissolve material cho từng SkinnedMeshRenderer
        }

        if (dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine); // Dừng coroutine cũ nếu đang chạy
            dissolveCoroutine = null;
        }

        ObjectPooling.Instance?.SpawnFromPool(PoolType.DissolveEffect, transform.position, transform.rotation);
        dissolveCoroutine = DissolveCoroutine(duration);
        StartCoroutine(dissolveCoroutine); // Bắt đầu coroutine mới
    }

    private IEnumerator DissolveCoroutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float dissolveAmount = Mathf.Clamp01(elapsedTime / duration);
            dissolveMaterial.SetFloat("_DissolveAmount", dissolveAmount);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    [Button("Reset Default Material")]
    public void ResetDefaultMaterial()
    {
        if (cacheDefaultMaterial == null) return; // Nếu chưa có material gốc thì không làm gì cả

        // Reset lại material và texture gốc sau khi dissolve xong
        foreach (var smr in skinnedMeshRenderer)
        {
            if (smr == null || !smr.enabled) continue;

            smr.material = cacheDefaultMaterial; // Reset material gốc
            smr.material.SetTexture("_Texture2D", cacheDefaultTexture); // Reset texture gốc
        }
    }
}
