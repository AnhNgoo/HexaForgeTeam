using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    [SerializeField] private Transform Player; //Lớp của mục tiêu (ví dụ: Player)
    [SerializeField] private LayerMask obstacleLayerMask; //Lớp của chướng ngại vật (ví dụ: Tường)

    private EnemyBase _enemyBase;
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        Debug.Log($"{gameObject.name} - EnemyDetection đã được khởi tạo!");
    }

    private void Update()
    {
        FindTarget();
        CheckLoseTarget();
    }

    private void FindTarget()
    {
        if (currentTarget != null) return; //có mục tiêu rồi thì không tìm nữa
        Debug.Log($"{gameObject.name} đang tìm mục tiêu...");

        Transform potentialTarget = Player; //Trỏ thẳng đến Player, có thể mở rộng sau này để tìm nhiều loại mục tiêu khác nhau (nên gọi thẳng từ PlayerManager)
        //Kiểm tra khoảng cách từ Enemy đến mục tiêu
        float dstToTarget = Vector3.Distance(transform.position, potentialTarget.position);

        //Kiểm tra nếu mục tiêu nằm trong khoảng cách phát hiện
        if (dstToTarget <= _enemyBase.enemyData.detectRange)
        {
            Vector3 directionToTarget = (potentialTarget.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < _enemyBase.enemyData.povAngle / 2f)
            {
                //Xem có chướng ngại vật nào giữa Enemy và mục tiêu không bằng cách raycast
                if (!Physics.Raycast(transform.position, directionToTarget, dstToTarget, obstacleLayerMask))
                {
                    currentTarget = potentialTarget;
                    Debug.Log($"{gameObject.name} đã phát hiện mục tiêu: {currentTarget.name}");
                }
                else Debug.Log($"{gameObject.name} không thể nhìn thấy mục tiêu {potentialTarget.name} do có chướng ngại vật");
            }
        }
    }

    private void CheckLoseTarget()
    {
        if (currentTarget == null) return; //không có mục tiêu thì không cần kiểm tra
        Debug.Log($"{gameObject.name} đang kiểm tra mất mục tiêu: {currentTarget.name}");

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > _enemyBase.enemyData.loseTargetRange)
        {
            Debug.Log($"{gameObject.name} đã mất mục tiêu: {currentTarget.name}");
            currentTarget = null;
        }
    }

    public void ResetDetection()
    {
        currentTarget = null;
    }

    #region Debug Visualization
    private void OnDrawGizmosSelected()
    {
        //Chỉ khi đã khởi tạo EnemyBase và có dữ liệu enemyData mới vẽ gizmos
        if (_enemyBase == null || _enemyBase.enemyData == null) return;

        //1. Vẽ hình cầu phát hiện (Màu vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _enemyBase.enemyData.detectRange);
        //2.vẽ 2 tia giới hạn góc nhìn (Màu xanh)
        Gizmos.color = Color.blue;
        float halfPOV = _enemyBase.enemyData.povAngle / 2f;

        //Tính toán hướng của 2 tia
        Vector3 leftRayDirection = Quaternion.Euler(0, -halfPOV, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, halfPOV, 0) * transform.forward;

        //Vẽ 2 tia giới hạn góc nhìn
        Gizmos.DrawRay(transform.position, leftRayDirection * _enemyBase.enemyData.detectRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * _enemyBase.enemyData.detectRange);

        //Vẽ đường thẳng từ Enemy đến mục tiêu hiện tại (Màu đỏ)
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
    #endregion
}
