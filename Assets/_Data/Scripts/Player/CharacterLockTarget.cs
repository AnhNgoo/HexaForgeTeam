using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterLockTarget : LoadComponents
{
    [SerializeField] private float lockRadius = 50f; // Bán kính để khóa mục tiêu
    [SerializeField] private float unlockRadius = 60f; // Bán kính để mở khóa mục tiêu, thường lớn hơn lockRadius để tránh việc mục tiêu bị khóa mở liên tục khi di chuyển gần rìa
    [SerializeField] private float maxScreenDistance = 3000f; // Khoảng cách tối đa để hiển thị mục tiêu, 3000 = full screen
    [SerializeField] private LayerMask targetLayer; // Lớp của mục tiêu để kiểm tra va chạm
    [SerializeField] private LayerMask obstacleLayer; // Lớp của chướng ngại vật để kiểm tra va chạm
    [SerializeField] private bool debugMode = false; // Chế độ debug để vẽ gizmos

    [SerializeField] private Transform followTarget;
    [SerializeField] private Transform lookAtTarget;
    public Transform Target => lookAtTarget;

    public bool IsLockingTarget { get; private set; } = false;
    private GameObject lockTargetMarker;
    private EnemyBase currentTargetEnemy;

    protected override void LoadComponent()
    {
        if (followTarget == null)
            followTarget = transform;
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void Update()
    {
        UnLockTarget();
    }

    private void UnLockTarget()
    {
        if (!IsLockingTarget) return;

        if (!IsValidTarget()) // Nếu mục tiêu bị hủy hoặc mất, tự động mở khóa
        {
            ToggleLockTarget();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, lookAtTarget.position);
        if (distanceToTarget > unlockRadius) // Nếu mục tiêu quá xa, tự động mở khóa
        {
            ToggleLockTarget();
        }

    }

    /// <summary>
    /// Kiểm tra xem mục tiêu đang lock còn sống không
    /// </summary>
    private bool IsValidTarget()
    {
        if (lookAtTarget == null || currentTargetEnemy == null || currentTargetEnemy.Health.CurrentHealth <= 0f)
            return false;

        return true;
    }


    /// <summary>
    /// Kiểm tra xem mục tiêu có hợp lệ để lock không, dựa trên các tiêu chí: mục tiêu không null, là EnemyBase và còn sống
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private bool IsValidTarget(Transform target)
    {
        if (target == null)
            return false;

        EnemyBase enemy = GetEnemyBase(target);
        if (enemy == null || enemy.Health.CurrentHealth <= 0f)
            return false;

        return true;
    }

    private EnemyBase GetEnemyBase(Transform target)
    {
        if (target == null)
            return null;

        return target.GetComponentInParent<EnemyBase>();
    }
    /// <summary>
    /// Bật/tắt khoá mục tiêu
    /// </summary>
    public void ToggleLockTarget()
    {
        if (!IsLockingTarget) // Nếu chưa khóa mục tiêu, tìm và khóa mục tiêu mới
        {
            if (lockTargetMarker != null)
                ObjectPooling.Instance?.ReturnToPool(PoolType.LockTargetMarker, lockTargetMarker); // Trả marker cũ về pool nếu có

            lookAtTarget = FindBestTarget();
            if (lookAtTarget == null) // Kiểm tra nếu lookAtTarget không có
                return;
            currentTargetEnemy = GetEnemyBase(lookAtTarget);

            CameraManager.Instance?.SetCamera(CameraType.LockTarget, followTarget, lookAtTarget);

            lockTargetMarker = ObjectPooling.Instance?.SpawnFromPool(PoolType.LockTargetMarker, lookAtTarget.position, Quaternion.identity, lookAtTarget);
            IsLockingTarget = true;
            return;
        }

        if (lockTargetMarker != null)
            ObjectPooling.Instance?.ReturnToPool(PoolType.LockTargetMarker, lockTargetMarker); // Trả marker cũ về pool nếu có
        CameraManager.Instance?.SetCamera(CameraType.Normal, followTarget, followTarget);
        IsLockingTarget = false;
        lookAtTarget = null;
    }

    //Tắt khoá mục tiêu 
    public void ForceUnlockTarget()
    {
        if (!IsLockingTarget) return;

        if (lockTargetMarker != null)
            ObjectPooling.Instance?.ReturnToPool(PoolType.LockTargetMarker, lockTargetMarker); // Trả marker cũ về pool nếu có
        CameraManager.Instance?.SetCamera(CameraType.Normal, followTarget, followTarget);
        IsLockingTarget = false;
        lookAtTarget = null;
    }

    /// <summary>
    /// Thiết lập camera theo chế độ Normal, chế độ này sẽ theo dõi đối tượng mà không khóa mục tiêu nhìn.
    /// </summary>
    public void SetFollowTarget()
    {
        if (CameraManager.Instance == null)
        {
            Debug.LogWarning($"[{nameof(CharacterLockTarget)}] CameraManager instance not found.");
            return;
        }
        CameraManager.Instance.SetCamera(CameraType.Normal, followTarget, followTarget);
    }

    /// <summary>
    /// Tìm mục tiêu tốt nhất để khóa dựa trên các tiêu chí: nằm trong bán kính lockRadius, không bị chướng ngại vật chắn giữa camera và mục tiêu, và gần tâm màn hình nhất.
    /// </summary>
    /// <returns></returns>
    private Transform FindBestTarget()
    {
        // Tìm tất cả các mục tiêu trong bán kính lockRadius
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, lockRadius, targetLayer);

        Transform bestTarget = null;
        float bestScreenDistance = float.MaxValue;

        //Tâm màn hình
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        foreach (Collider targetCollider in targetsInRange)
        {
            Transform target = targetCollider.transform;

            if (!IsValidTarget(target))
                continue;

            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(target.position);

            if (viewportPosition.z < 0) // Mục tiêu ở phía sau camera
                continue;

            //Bỏ qua nếu mục tiêu nằm ngoài màn hình
            if (viewportPosition.x < 0 || viewportPosition.x > 1 ||
                viewportPosition.y < 0 || viewportPosition.y > 1)
                continue;

            //Hướng từ cam đến mục tiêu
            Vector3 dirToTarget = target.position - Camera.main.transform.position;
            //Đổi hướng sang khoảng cách
            float distanceToTarget = dirToTarget.magnitude;

            if (Physics.Raycast(
                Camera.main.transform.position, //Vị trí của camera
                dirToTarget.normalized, //Hướng từ camera đến mục tiêu
                distanceToTarget, //Khoảng cách từ camera đến mục tiêu
                obstacleLayer //Lớp chướng ngại vật để kiểm tra va chạm
            ))
                continue; //Bỏ qua mục tiêu nếu có chướng ngại vật chắn giữa camera và mục tiêu

            //Chuyển vị trí mục tiêu sang xy trên màn hình
            Vector2 targetScreenPos = Camera.main.WorldToScreenPoint(target.position);

            //Tính khoảng cách từ tâm tới vị trí mục tiêu trên mặt phẳng màn hình
            float screenDistance = Vector2.Distance(screenCenter, targetScreenPos);

            //Bỏ qua mục tiêu nếu nó quá xa tâm
            if (screenDistance > maxScreenDistance)
                continue;

            //Nếu mục tiêu gần tâm hơn mục tiêu trước thì cập nhật mục tiêu tốt nhất
            if (screenDistance < bestScreenDistance)
            {
                bestScreenDistance = screenDistance;
                bestTarget = target;
            }
        }

        return bestTarget;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debugMode)
            return;
        // Vẽ bán kính lockRadius để dễ dàng điều chỉnh trong editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lockRadius);
    }
#endif
}
