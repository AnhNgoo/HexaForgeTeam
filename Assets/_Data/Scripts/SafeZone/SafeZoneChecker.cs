using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Gắn vào những vật thể muốn bị ảnh hưởng bởi vòng bo
/// </summary>
public class SafeZoneChecker : LoadComponents
{
    [SerializeField] private VolumeProfile outsideSafeZoneVolumeProfile;
    [SerializeField] private bool isInSafeZone = false;
    [SerializeField] private bool isFirstTimeCheckDistanceSafeZone = false;
    [SerializeField] private float damagePerSecondOutsideSafeZone = 10f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float delayTimeWhenLeftSafeZone = 0.5f;
    private IEnumerator causeEffectOutsideSafeZoneCoroutine;

    protected override void LoadComponent()
    {
        if (outsideSafeZoneVolumeProfile == null)
            outsideSafeZoneVolumeProfile = Resources.Load<VolumeProfile>("Volumes/OutsideSafeZone");
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void Update()
    {
        CheckSafeZone();
    }

    private void CheckSafeZone()
    {
        if (SafeZoneManager.Instance?.SafeZone == null) return;
        if (!SafeZoneManager.Instance.IsActiveSafeZone) return;

        var safeZone = SafeZoneManager.Instance;

        if (!isFirstTimeCheckDistanceSafeZone)
        {
            isFirstTimeCheckDistanceSafeZone = true;
            CheckDistanceSafeZoneFirstTime(safeZone);
        }
        //Nếu ở ngoài vòng bo
        if (!safeZone.CheckObjectInSafeZone(transform))
        {
            // Đối tượng này đã rời khỏi vòng bo
            if (isInSafeZone)
            {
                OutsideSafeZone();
            }
        }
        // Nếu ở trong vòng bo
        else
        {
            // Đối tượng này đang ở trong vòng bo
            if (!isInSafeZone)
            {
                InsideSafeZone();
            }
        }
    }

    private void CheckDistanceSafeZoneFirstTime(SafeZoneManager safeZone)
    {
        if (!safeZone.CheckObjectInSafeZone(transform))
        {
            isInSafeZone = true;
        }
        // Nếu ở trong vòng bo
        else
        {
            isInSafeZone = false;
        }
    }
    private void OutsideSafeZone()
    {
        isInSafeZone = false;

        //Nếu là player thì đổi màu màn hình
        if (gameObject.CompareTag("Player"))
            VolumeSwitcher.Instance?.ChangeVolumeProfile(outsideSafeZoneVolumeProfile);

        causeEffectOutsideSafeZoneCoroutine = CauseEffectOutsideSafeZone();
        StartCoroutine(causeEffectOutsideSafeZoneCoroutine);

    }

    private void InsideSafeZone()
    {
        isInSafeZone = true;

        //Nếu là player thì đổi màu màn hình về mặc định
        if (gameObject.CompareTag("Player"))
            VolumeSwitcher.Instance?.ResetToDefaultProfile();

        if (causeEffectOutsideSafeZoneCoroutine != null)
        {
            StopCoroutine(causeEffectOutsideSafeZoneCoroutine);
            causeEffectOutsideSafeZoneCoroutine = null;
        }
    }

    IEnumerator CauseEffectOutsideSafeZone()
    {
        yield return new WaitForSeconds(delayTimeWhenLeftSafeZone);
        while (true)
        {
            if (gameObject.CompareTag("Player"))
                ScreenDamageEffect.Instance?.PlayScreenDamageEffect();

            if (gameObject.TryGetComponent(out ITakeDamage itakeDamage))
            {
                DamageInfo damageInfo = new DamageInfo
                {
                    damageAmount = damagePerSecondOutsideSafeZone,
                    isFromSafeZoneEffect = true
                };
                itakeDamage.TakeDamage(damageInfo);
            }

            yield return new WaitForSeconds(damageInterval);
        }
    }

}

