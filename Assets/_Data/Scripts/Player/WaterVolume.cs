using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class WaterVolume : MonoBehaviour
{
    [SerializeField] private float surfaceLevel;

    public float SurfaceLevel => surfaceLevel;

    private void Awake()
    {
        EnsureTriggerCollider();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureTriggerCollider();
    }
#endif

    private void EnsureTriggerCollider()
    {
        Collider waterCollider = GetComponent<Collider>();
        if (waterCollider != null)
            waterCollider.isTrigger = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Collider waterCollider = GetComponent<Collider>();
        if (waterCollider == null)
            return;

        Color previousColor = Gizmos.color;
        Color surfaceColor = new Color(0.1f, 0.7f, 1f, 0.8f);
        Gizmos.color = surfaceColor;

        Bounds bounds = waterCollider.bounds;
        Vector3 center = new Vector3(bounds.center.x, surfaceLevel, bounds.center.z);
        Vector3 frontLeft = new Vector3(bounds.min.x, surfaceLevel, bounds.min.z);
        Vector3 frontRight = new Vector3(bounds.max.x, surfaceLevel, bounds.min.z);
        Vector3 backRight = new Vector3(bounds.max.x, surfaceLevel, bounds.max.z);
        Vector3 backLeft = new Vector3(bounds.min.x, surfaceLevel, bounds.max.z);

        Gizmos.DrawLine(frontLeft, frontRight);
        Gizmos.DrawLine(frontRight, backRight);
        Gizmos.DrawLine(backRight, backLeft);
        Gizmos.DrawLine(backLeft, frontLeft);
        Gizmos.DrawLine(frontLeft, backRight);
        Gizmos.DrawLine(frontRight, backLeft);

        Vector3 markerBottom = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        Gizmos.DrawLine(markerBottom, center);
        Gizmos.DrawSphere(center, 0.15f);

#if UNITY_EDITOR
        Handles.color = surfaceColor;
        Handles.Label(
            center + Vector3.up * 0.25f,
            $"Water Surface Y: {surfaceLevel:F2}");
#endif

        Gizmos.color = previousColor;
    }
#endif
}
