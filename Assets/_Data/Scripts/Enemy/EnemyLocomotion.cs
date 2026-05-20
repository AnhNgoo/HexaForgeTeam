using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyLocomotion : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [Header("Patrol Settings")]
    public bool isPatroller; //Công tắc để tắt bật đi tuần
    public Transform[] wayPoints; //Mảng chứa các điểm tuần tra
    public int currentWaypointIndex; //Chỉ số điểm tuần tra hiện tại

    private void OnValidate()
    {
        if (_navMeshAgent == null)
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            if (_navMeshAgent == null)
            {
                Debug.LogError("NavMeshAgent component is missing on " + gameObject.name);
            }
        }
    }

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        Debug.Log($"{gameObject.name} - EnemyLocomotion đã được khởi tạo!");
        if (isPatroller && (wayPoints == null || wayPoints.Length == 0))
        {
            Debug.LogWarning($"{gameObject.name} được đặt là patroller nhưng không có điểm tuần tra nào được gán!");
            SetSpeed(_enemyBase.Data.patrolSpeed); // Đặt tốc độ di chuyển ban đầu là tốc độ tuần tra, có thể thay đổi sau này khi vào trạng thái khác
        }
        else if (isPatroller)
        {
            SetSpeed(_enemyBase.Data.patrolSpeed); // Đặt tốc độ di chuyển ban đầu là tốc độ tuần tra, có thể thay đổi sau này khi vào trạng thái khác
        }
        else
        {
            SetSpeed(_enemyBase.Data.moveSpeed); // Đặt tốc độ di chuyển ban đầu là tốc độ di chuyển bình thường
        }

    }

    public void SetSpeed(float speed)
    {
        _navMeshAgent.speed = speed;
    }

    //Hàm di chuyển đến vị trí mục tiêu
    public void MoveToTarget(Vector3 targetPosition)
    {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(targetPosition);
    }

    //Hàm dừng di chuyển
    public void StopMoving()
    {
        _navMeshAgent.isStopped = true;
    }

    //Hàm kiểm tra lấy điểm mốc tiếp theo 
    public Vector3 GetNextWaypoint()
    {
        // Nếu không có điểm tuần tra nào, trả về vị trí hiện tại
        if (wayPoints == null || wayPoints.Length == 0) return transform.position;

        //Lấy Transform của điểm tuần tra hiện tại để sử dụng trong việc di chuyển và kiểm tra khoảng cách, có thể dùng để hiển thị debug hoặc các mục đích khác nếu cần thiết
        Transform targetTransform = wayPoints[currentWaypointIndex];

        if (targetTransform != null)
        {
            // Ép nó nhích qua điểm tiếp theo để lần sau không bị kẹt lại ở ô lỗi này nữa
            currentWaypointIndex = (currentWaypointIndex + 1) % wayPoints.Length; // Tăng index, nếu vượt quá số lượng điểm tuần tra thì quay lại điểm đầu tiên

            return targetTransform.position; // Tạm thời đứng im tại chỗ
        }

        // Nếu điểm tuần tra hiện tại bị null, log lỗi và tiếp tục với điểm tuần tra tiếp theo
        Vector3 target = targetTransform.position;
        //Tăng index, nếu vượt quá số lượng điểm tuần tra thì quay lại điểm đầu tiên
        currentWaypointIndex = (currentWaypointIndex + 1) % wayPoints.Length;
        return target;

    }
}
