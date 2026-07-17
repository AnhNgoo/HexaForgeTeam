using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CharacterCinematic : MonoBehaviour
{
    [SerializeField] private float cinematicAnchorDistance = 2f;
    [SerializeField] private PoolType cinematicAnchor = PoolType.CinematicAnchor;

    private GameObject cinematicAnchorObj;
    private GameObject cinematicUIObj;

    public async void PlayCinematicAuto(float duration)
    {
        if (cinematicAnchorObj != null)
            ObjectPooling.Instance.ReturnToPool(cinematicAnchor, cinematicAnchorObj);
        cinematicAnchorObj = ObjectPooling.Instance.SpawnFromPool(cinematicAnchor, transform.position + transform.forward * cinematicAnchorDistance, transform.rotation);

        if (cinematicUIObj != null)
            ObjectPooling.Instance.ReturnToPool(PoolType.CinematicUI, cinematicUIObj);
        cinematicUIObj = ObjectPooling.Instance.SpawnFromPool(PoolType.CinematicUI);

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetCamera(CameraType.CinematicAttack, cinematicAnchorObj.transform, cinematicAnchorObj.transform);
        }

        await UniTask.Delay((int)(duration * 1000));

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetCamera(CameraType.Normal, transform, transform);
        }

        if (cinematicAnchorObj != null)
            ObjectPooling.Instance.ReturnToPool(cinematicAnchor, cinematicAnchorObj);
        if (cinematicUIObj != null)
            ObjectPooling.Instance.ReturnToPool(PoolType.CinematicUI, cinematicUIObj);
    }

    public void PlayCinematic(bool hasAnchor = true)
    {
        if (cinematicAnchorObj != null)
            ObjectPooling.Instance.ReturnToPool(cinematicAnchor, cinematicAnchorObj);

        if (hasAnchor)
        {
            cinematicAnchorObj = ObjectPooling.Instance.SpawnFromPool(cinematicAnchor, transform.position + transform.forward * cinematicAnchorDistance, transform.rotation);
        }

        if (cinematicUIObj != null)
            ObjectPooling.Instance.ReturnToPool(PoolType.CinematicUI, cinematicUIObj);
        cinematicUIObj = ObjectPooling.Instance.SpawnFromPool(PoolType.CinematicUI);

        if (CameraManager.Instance != null)
        {
            if (hasAnchor)
            {
                CameraManager.Instance.SetCamera(CameraType.CinematicAttack, cinematicAnchorObj.transform, cinematicAnchorObj.transform);
                return;
            }

            CameraManager.Instance.SetCamera(CameraType.CinematicAttack, transform, transform);
        }
    }

    public void StopCinematic()
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.SetCamera(CameraType.Normal, transform, transform);
        }
        if (cinematicAnchorObj != null)
            ObjectPooling.Instance.ReturnToPool(cinematicAnchor, cinematicAnchorObj);
        if (cinematicUIObj != null)
            ObjectPooling.Instance.ReturnToPool(PoolType.CinematicUI, cinematicUIObj);
    }
}
