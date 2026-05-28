using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;
    private Vector3 lastKnownTargetPosition; //Vị trí cuối cùng của mục tiêu (có thể dùng để di chuyển đến đó khi mất mục tiêu)
    public Vector3 LastKnownTargetPosition => lastKnownTargetPosition;
    [SerializeField] private Transform Player; //Lớp của mục tiêu (ví dụ: Player)
    [SerializeField] private LayerMask obstacleLayerMask; //Lớp của chướng ngại vật (ví dụ: Tường)
    [Header("Pack AI Settings")]
    [SerializeField] private float alertRadius = 8f; //Bán kính mà Enemy sẽ cảnh báo đồng bọn khi phát hiện mục tiêu, có thể dùng để tìm các Enemy khác trong bán kính này và truyền thông tin về mục tiêu cho chúng
    [SerializeField] private LayerMask enemyLayerMask; //Lớp của các Enemy khác để tìm kiếm đồng bọn trong bán kính cảnh báo, có thể dùng để tìm các Enemy khác trong bán kính này và truyền thông tin về mục tiêu cho chúng

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
        if (currentTarget != null)
        {
            lastKnownTargetPosition = currentTarget.position;
            return; //có mục tiêu rồi thì không tìm nữa
        }
        //Liên tục tìm mục tiêu mới nếu chưa có mục tiêu hiện tại, có thể điều chỉnh tần suất gọi hàm này nếu cần để tối ưu hiệu suất (ví dụ: chỉ tìm mục tiêu mỗi 0.5 giây thay vì mỗi frame)

        Transform potentialTarget = Player; //Trỏ thẳng đến Player, có thể mở rộng sau này để tìm nhiều loại mục tiêu khác nhau (nên gọi thẳng từ PlayerManager)
        //Kiểm tra khoảng cách từ Enemy đến mục tiêu
        float dstToTarget = Vector3.Distance(transform.position, potentialTarget.position);

        //Kiểm tra nếu mục tiêu nằm trong khoảng cách phát hiện
        if (dstToTarget <= _enemyBase.Data.detectRange)
        {
            //Xem mục tiêu đang đứng hướng nào
            Vector3 directionToTarget = (potentialTarget.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < _enemyBase.Data.povAngle / 2f)
            {
                //Xem có chướng ngại vật nào giữa Enemy và mục tiêu không bằng cách raycast
                // HƯỚNG 1: Player đứng trước mặt (Trong góc FOV)
                if (!Physics.Raycast(transform.position, directionToTarget, dstToTarget, obstacleLayerMask))
                {
                    currentTarget = potentialTarget;
                    Debug.Log($"{gameObject.name} đã phát hiện mục tiêu: {currentTarget.name}");
                    AlertNearbyAllies(currentTarget); //Gọi hàm cảnh báo đồng bọn khi phát hiện mục tiêu, có thể mở rộng sau này để truyền thông tin về mục tiêu cho các Enemy khác trong bán kính cảnh báo
                }
                // HƯỚNG 2: Player đứng sau lưng (Ngoài góc FOV nhưng vẫn trong bán kính phát hiện) nhưng lọt vào tầm nghe thính giác (50% tầm nhìn)
                else if (dstToTarget <= _enemyBase.Data.detectRange * 0.5f)
                {
                    if (!Physics.Raycast(transform.position, directionToTarget, dstToTarget, obstacleLayerMask))
                    {
                        lastKnownTargetPosition = potentialTarget.position; //Cập nhật vị trí cuối cùng của mục tiêu để có thể di chuyển đến đó khi mất mục tiêu
                        Debug.Log($"{gameObject.name} đã nghe thấy mục tiêu: {potentialTarget.name}");
                        _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemySuspicionState); //Chuyển sang trạng thái nghi ngờ khi nghe thấy mục tiêu, có thể mở rộng sau này để có trạng thái riêng cho việc nghe thấy mục tiêu và di chuyển đến vị trí cuối cùng của mục tiêu
                    }
                }
            }
        }
    }

    public void AlertNearbyAllies(Transform targetPlayer)
    {
        Collider[] nearbyAllies = Physics.OverlapSphere(transform.position, alertRadius, enemyLayerMask); //Tìm tất cả các Enemy khác trong bán kính cảnh báo
        foreach (Collider col in nearbyAllies)
        {
            if (col.gameObject == gameObject) continue; //Bỏ qua chính mình

            EnemyBase ally = col.GetComponent<EnemyBase>();
            if (ally != null && ally.Detection.CurrentTarget == null) //Chỉ cảnh báo đồng bọn nếu chúng chưa có mục tiêu hiện tại để tránh ghi đè mục tiêu của chúng
            {
                ally.Detection.ForceDetectTarget(targetPlayer); //Ép đồng bọn phát hiện mục tiêu, có thể mở rộng sau này để truyền thông tin về mục tiêu cho các Enemy khác trong bán kính cảnh báo thay vì chỉ ép chúng phát hiện mục tiêu (ví dụ: truyền vị trí cuối cùng của mục tiêu hoặc trạng thái hiện tại của mục tiêu) để đồng bọn có thể phản ứng phù hợp hơn thay vì chỉ đơn giản là phát hiện mục tiêu như nhau với cùng một trạng thái.
            }
        }
    }

    public void ForceDetectTarget(Transform target)
    {
        currentTarget = target; //Ép đặt mục tiêu cho Enemy, có thể dùng trong trường hợp cần thiết như khi bị đồng bọn cảnh báo hoặc khi bị tấn công từ phía sau mà chưa kịp phát hiện mục tiêu
        _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState); //Chuyển sang trạng thái Chase khi bị ép phát hiện mục tiêu, có thể mở rộng sau này để chuyển sang trạng thái khác nếu cần thiết (ví dụ: trạng thái phòng thủ nếu bị tấn công từ phía sau)
    }

    private void CheckLoseTarget()
    {
        if (currentTarget == null) return; //không có mục tiêu thì không cần kiểm tra
        //Kiểm tra khoảng cách từ Enemy đến mục tiêu để quyết định có mất mục tiêu hay không

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > _enemyBase.Data.loseTargetRange)
        {
            //Mất mục tiêu nếu nó di chuyển ra khỏi khoảng cách mất mục tiêu, có thể mở rộng thêm điều kiện mất mục tiêu nếu cần (ví dụ: mất mục tiêu nếu có chướng ngại vật chắn giữa Enemy và mục tiêu)
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
        if (_enemyBase == null || _enemyBase.Data == null) return;

        //1. Vẽ hình cầu phát hiện (Màu vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _enemyBase.Data.detectRange);
        //2.vẽ 2 tia giới hạn góc nhìn (Màu xanh)
        Gizmos.color = Color.blue;
        float halfPOV = _enemyBase.Data.povAngle / 2f;

        //Tính toán hướng của 2 tia
        Vector3 leftRayDirection = Quaternion.Euler(0, -halfPOV, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, halfPOV, 0) * transform.forward;

        //Vẽ 2 tia giới hạn góc nhìn
        Gizmos.DrawRay(transform.position, leftRayDirection * _enemyBase.Data.detectRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * _enemyBase.Data.detectRange);

        //Vẽ đường thẳng từ Enemy đến mục tiêu hiện tại (Màu đỏ)
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
    #endregion
}
