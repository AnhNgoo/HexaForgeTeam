using UnityEngine;
using UnityEngine.AI;

public class EnemyLocomotion : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private NavMeshAgent _navMeshAgent;

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
        _navMeshAgent.speed = _enemyBase.enemyData.moveSpeed;
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
}
