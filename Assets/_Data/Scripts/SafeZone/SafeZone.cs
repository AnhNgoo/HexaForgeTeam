using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SafeZone : MonoBehaviour
{
    [SerializeField] private Transform centerPoint;
    [SerializeField] private float heightCenterPoint = 0f;
    [SerializeField] private float currentRadius = 0f;
    [SerializeField] private float scaleY = 1000f;
    private bool isShrinking = false;
    private bool isStopShrinkSafeZone = false;

    /// <summary>
    /// Khởi tạo vòng bo
    /// </summary>
    [Button("Init Safe Zone")]
    public void InitSafeZone(Transform centerPoint, float currentRadius)
    {
        StopShrinkingSafeZone();

        this.centerPoint.position = new Vector3(centerPoint.position.x, heightCenterPoint, centerPoint.position.z);
        transform.position = this.centerPoint.position;

        SetRadius(currentRadius);
    }

    private void SetRadius(float radius)
    {
        currentRadius = radius;
        transform.localScale = new Vector3(currentRadius, scaleY, currentRadius);
    }

    [Button("Shrink Safe Zone")]
    public async void ShrinkSafeZone(float newRadius, float shrinkDuration)
    {
        if (isShrinking) return;
        isShrinking = true;
        isStopShrinkSafeZone = false;

        float elapsedTime = 0f;
        float initialRadius = currentRadius;

        while (elapsedTime < shrinkDuration && !isStopShrinkSafeZone)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;
            float radius = Mathf.Lerp(initialRadius, newRadius, t);
            SetRadius(radius);
            await UniTask.Yield();
        }

        if (isStopShrinkSafeZone) return;

        SetRadius(newRadius); // Đảm bảo vòng bo đạt đến kích thước mới sau khi hoàn thành
        isShrinking = false;
    }

    [Button("Stop Shrinking")]
    public void StopShrinkingSafeZone()
    {
        isShrinking = false;
        isStopShrinkSafeZone = true;
    }
}