using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum SafeZoneShape
{
    Circle,
    Ellipse
}
public class SafeZone : MonoBehaviour
{
    [SerializeField] private Vector3 currentCenterPoint;
    public Vector3 CurrentCenterPoint => currentCenterPoint;
    [SerializeField] private SafeZoneShape shape = SafeZoneShape.Circle;

    [SerializeField, Min(0.1f)] private float circleRadius = 100f;

    [SerializeField] private Vector2 ellipseRadii = new Vector2(145f, 62.3f);

    [SerializeField] private Vector2 currentRadii;

    public Vector2 CurrentRadii => currentRadii;
    public float CurrentRadius => Mathf.Min(currentRadii.x, currentRadii.y);
    [SerializeField] private float heightCenterPoint = 0f;
    [SerializeField] private float scaleY = 1000f;
    public bool IsShrinking { get; private set; } = false;
    private bool isStopShrinkSafeZone = false;

    private void OnDisable()
    {
        StopShrinkingSafeZone();
    }

    private void OnDestroy()
    {
        StopShrinkingSafeZone();
    }

    /// <summary>
    /// Khởi tạo vòng bo
    /// </summary>
    [Button("Init Safe Zone")]
    public void InitSafeZone(Vector3 startCenterPoint, float radius)
    {
        StopShrinkingSafeZone();

        Vector2 radii;

        if (shape == SafeZoneShape.Ellipse)
        {
            radii = ellipseRadii;
        }
        else
        {
            circleRadius = radius;
            radii = Vector2.one * circleRadius;
        }

        SetRadiiAndCenter(radii, startCenterPoint);
    }

    private void SetRadiiAndCenter(Vector2 radii, Vector3 centerPoint)
    {
        currentRadii = new Vector2(Mathf.Max(0.1f, radii.x), Mathf.Max(0.1f, radii.y));

        transform.localScale = new Vector3(currentRadii.x, scaleY, currentRadii.y);

        currentCenterPoint = new Vector3(centerPoint.x, heightCenterPoint, centerPoint.z);

        transform.position = currentCenterPoint;
    }

    [Button("Shrink Safe Zone")]
    public async void ShrinkSafeZone(Vector3 targetCenterPoint, float newRadius, float shrinkDuration)
    {
        if (IsShrinking)
            return;

        IsShrinking = true;
        isStopShrinkSafeZone = false;

        float elapsedTime = 0f;

        Vector2 initialRadii = currentRadii;
        Vector2 targetRadii = Vector2.one * newRadius;

        Vector3 initialCenterPoint = currentCenterPoint;

        while (elapsedTime < shrinkDuration && !isStopShrinkSafeZone)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / Mathf.Max(0.01f, shrinkDuration));

            Vector2 radii = Vector2.Lerp(initialRadii, targetRadii, t);

            Vector3 centerPoint = Vector3.Lerp(initialCenterPoint, targetCenterPoint, t);

            SetRadiiAndCenter(radii, centerPoint);

            await UniTask.Yield();
        }

        if (isStopShrinkSafeZone)
            return;

        SetRadiiAndCenter(targetRadii, targetCenterPoint);

        IsShrinking = false;
    }

    [Button("Stop Shrinking")]
    public void StopShrinkingSafeZone()
    {
        IsShrinking = false;
        isStopShrinkSafeZone = true;
    }

    public bool Contains(Vector3 worldPosition, float distanceFromEdge = 0f)
    {
        Vector2 radii = currentRadii - Vector2.one * distanceFromEdge;

        if (radii.x <= 0f || radii.y <= 0f) return false;

        Vector3 delta = worldPosition - currentCenterPoint;

        float x = delta.x / radii.x;
        float z = delta.z / radii.y;

        return x * x + z * z <= 1f;
    }
}