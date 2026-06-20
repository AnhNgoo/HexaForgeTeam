using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyLocomotion : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [Header("Patrol Settings")]
    public bool isPatroller; //Công tắc để tắt bật đi tuần

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
        if (isPatroller)
        {
            SetSpeed(_enemyBase.Data.patrolSpeed); //Đặt tốc độ di chuyển khi tuần tra, có thể điều chỉnh trong EnemyData để tạo ra sự đa dạng về hành vi di chuyển của các loại Enemy khác nhau
        }
        else
        {
            SetSpeed(_enemyBase.Data.moveSpeed); //Đặt tốc độ di chuyển bình thường, có thể điều chỉnh trong EnemyData để tạo ra sự đa dạng về hành vi di chuyển của các loại Enemy khác nhau
        }
    }

    public void SetSpeed(float speed)
    {
        _navMeshAgent.speed = speed;
    }

    //Hàm di chuyển đến vị trí mục tiêu
    public void MoveToTarget(Vector3 targetPosition, float stoppingDistance = 0f)
    {
        if (_navMeshAgent == null ||
            !_navMeshAgent.enabled ||
            !_navMeshAgent.isOnNavMesh)
            return;

        _navMeshAgent.stoppingDistance = Mathf.Max(0f, stoppingDistance);
        _navMeshAgent.isStopped = false;
        _navMeshAgent.updateRotation = true;
        _navMeshAgent.SetDestination(targetPosition);
    }

    //Hàm đặt tốc độ xoay mặt tự động của NavMeshAgent    
    public void SetAngularSpeed(float speed)
    {
        if (_navMeshAgent != null && _navMeshAgent.enabled)
        {
            _navMeshAgent.angularSpeed = speed;
        }
    }

    //Hàm dừng di chuyển
    public void StopMoving()
    {
        if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = true;
        }
    }

    public void SetAgentActive(bool isActive)
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = isActive;
        }
    }

    //Dịch chuyển an toàn cho NavMeshAgent
    public void WarpTo(Vector3 position)
    {
        if (_navMeshAgent.isActiveAndEnabled)
        {
            _navMeshAgent.Warp(position);
        }
        else
        {
            transform.position = position; //Nếu NavMeshAgent không hoạt động, di chuyển trực tiếp bằng cách đặt vị trí của transform, có thể dùng để đảm bảo rằng Enemy vẫn có thể được di chuyển đến vị trí mong muốn ngay cả khi NavMeshAgent gặp sự cố hoặc bị tắt
        }
    }

    //Lấy một điểm ngẫu nhiên trên NavMesh xung quanh vị trí gốc
    public Vector3 GetRandomRoamPosition(Vector3 origin, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius; //Tạo một vector ngẫu nhiên trong một hình cầu có bán kính là radius
        randomDirection += origin; //Dịch chuyển vector ngẫu nhiên đến xung quanh vị trí gốc

        NavMeshHit hit;
        //Quét bán kính tìm điểm hợp lệ trên mặt đất
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            return hit.position; //Trả về vị trí hợp lệ trên NavMesh nếu tìm thấy
        }
        return origin; //Nếu không tìm thấy điểm hợp lệ nào, trả về vị trí gốc để tránh việc Enemy bị mắc kẹt hoặc di chuyển đến vị trí không hợp lệ
    }
}
