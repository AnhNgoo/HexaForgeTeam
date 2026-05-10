using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class AvoidObstacleForCamera : LoadComponents
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineFramingTransposer transposer;
    [Header("Chọn lớp layer cho các vật cản mà camera cần tránh, bỏ qua layer player")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Settings")]
    public float originalCameraDistance = 5f;
    public float minCameraDistance = 0.5f;
    public float maxCameraDistance = 5f;
    public float shrinkSpeed = 20f;
    public float expandSpeed = 20f;
    [FormerlySerializedAs("hitPadding")] public float obstacleClearance = 0.1f;
    public float debugRayLength = 8f;

    private float currentCameraDistance;

    protected override void LoadComponent()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        if (transposer == null && virtualCamera != null)
        {
            transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void Start()
    {
        if (transposer != null)
        {
            transposer.m_CameraDistance = originalCameraDistance;
            currentCameraDistance = originalCameraDistance;
        }
    }

    private void LateUpdate()
    {
        if (transposer == null || virtualCamera == null || transposer.FollowTarget == null) return;

        AdjustCameraDistanceBasedOnObstacles();
        transposer.m_CameraDistance = currentCameraDistance;
    }

    private void AdjustCameraDistanceBasedOnObstacles()
    {
        Transform followTarget = transposer.FollowTarget;
        Vector3 cameraPos = virtualCamera.transform.position;
        Vector3 forward = virtualCamera.transform.forward;
        Vector3 targetPosition = followTarget.position;

        Vector3 directionToTarget = (targetPosition - cameraPos).normalized;
        float distanceToTarget = Vector3.Distance(cameraPos, targetPosition);

        bool hitForward = Physics.Raycast(
            cameraPos,
            directionToTarget,
            out RaycastHit forwardHit,
            distanceToTarget,
            obstacleLayer
        );

        bool hitFromTarget = Physics.Raycast(
            targetPosition,
            -directionToTarget,
            out RaycastHit targetHit,
            distanceToTarget,
            obstacleLayer
        );

        Debug.DrawLine(
            cameraPos,
            cameraPos + directionToTarget * Mathf.Min(distanceToTarget, debugRayLength),
            hitForward ? Color.red : Color.green
        );

        if (hitForward || hitFromTarget)
        {
            float desiredDistance = hitFromTarget
                ? targetHit.distance - obstacleClearance
                : distanceToTarget - forwardHit.distance - obstacleClearance;
            desiredDistance = Mathf.Clamp(desiredDistance, minCameraDistance, maxCameraDistance);
            currentCameraDistance = Mathf.MoveTowards(currentCameraDistance, desiredDistance, shrinkSpeed * Time.deltaTime);
        }
        else
        {
            float expandCheckDistance = Mathf.Max(0f, originalCameraDistance - currentCameraDistance);
            bool hitBackward = expandCheckDistance > 0f && Physics.Raycast(
                cameraPos,
                -forward,
                out RaycastHit backwardHit,
                expandCheckDistance,
                obstacleLayer
            );

            Debug.DrawLine(
                cameraPos,
                cameraPos + (-forward) * Mathf.Min(expandCheckDistance, debugRayLength),
                hitBackward ? Color.yellow : Color.cyan
            );

            if (!hitBackward)
            {
                currentCameraDistance = Mathf.MoveTowards(currentCameraDistance, originalCameraDistance, expandSpeed * Time.deltaTime);
            }
        }

        currentCameraDistance = Mathf.Clamp(currentCameraDistance, minCameraDistance, maxCameraDistance);
    }
}