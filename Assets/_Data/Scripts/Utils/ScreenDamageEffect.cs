using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenDamageEffect : Singleton<ScreenDamageEffect>
{
    [SerializeField] private Volume volume;
    [SerializeField] private Vignette vignette;
    [SerializeField] private float maxVignetteIntensity = 0.3f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (volume == null)
            volume = GetComponent<Volume>();
    }

    public void PlayScreenDamageEffect()
    {
        StartCoroutine(damageEffectCoroutine());
    }

    IEnumerator damageEffectCoroutine()
    {
        vignette = GetVignette();
        if (vignette == null)
        {
            Debug.LogError("Vignette effect not found in the volume profile.");
        }

        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;
            float currentIntensity = Mathf.Lerp(0f, maxVignetteIntensity, t);
            vignette.intensity.Override(currentIntensity);
            yield return null;
        }

        vignette.intensity.Override(maxVignetteIntensity);

        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;
            float currentIntensity = Mathf.Lerp(maxVignetteIntensity, 0f, t);
            vignette.intensity.Override(currentIntensity);
            yield return null;
        }

        vignette.intensity.Override(0f);
    }

    private Vignette GetVignette()
    {
        if (vignette != null)
            vignette = null;

        return volume.profile.TryGet(out vignette) ? vignette : null;
    }
}
