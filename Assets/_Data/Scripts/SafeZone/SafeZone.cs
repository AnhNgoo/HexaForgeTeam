using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SafeZone : MonoBehaviour
{
    [SerializeField] private Vector3 currentCenterPoint;
    public Vector3 CurrentCenterPoint => currentCenterPoint;
    [SerializeField] private float currentRadius = 0f;
    public float CurrentRadius => currentRadius;
    [SerializeField] private float heightCenterPoint = 0f;
    [SerializeField] private float scaleY = 1000f;
    public bool IsShrinking { get; private set; } = false;
    private bool isStopShrinkSafeZone = false;

    /// <summary>
    /// Khởi tạo vòng bo
    /// </summary>
    [Button("Init Safe Zone")]
    public void InitSafeZone(Vector3 startCenterPoint, float currentRadius)
    {
        StopShrinkingSafeZone();
        SetRadiusAndCenter(currentRadius, startCenterPoint);
    }

    private void SetRadiusAndCenter(float radius, Vector3 centerPoint)
    {
        currentRadius = radius;
        transform.localScale = new Vector3(currentRadius, scaleY, currentRadius);

        this.currentCenterPoint = new Vector3(centerPoint.x, heightCenterPoint, centerPoint.z);
        transform.position = this.currentCenterPoint;
    }

    [Button("Shrink Safe Zone")]
    public async void ShrinkSafeZone(Vector3 targetCenterPoint, float newRadius, float shrinkDuration)
    {
        if (IsShrinking) return;
        IsShrinking = true;
        isStopShrinkSafeZone = false;

        float elapsedTime = 0f;
        float initialRadius = currentRadius;
        Vector3 initialStartCenterPoint = currentCenterPoint;

        while (elapsedTime < shrinkDuration && !isStopShrinkSafeZone)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration;
            float radius = Mathf.Lerp(initialRadius, newRadius, t);
            Vector3 centerPoint = Vector3.Lerp(initialStartCenterPoint, targetCenterPoint, t);

            SetRadiusAndCenter(radius, centerPoint);
            await UniTask.Yield();
        }

        if (isStopShrinkSafeZone) return;

        SetRadiusAndCenter(newRadius, targetCenterPoint); // Đảm bảo vòng bo đạt đến kích thước mới sau khi hoàn thành
        IsShrinking = false;
    }

    [Button("Stop Shrinking")]
    public void StopShrinkingSafeZone()
    {
        IsShrinking = false;
        isStopShrinkSafeZone = true;
    }
}