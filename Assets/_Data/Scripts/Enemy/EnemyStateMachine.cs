using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    [Header("Enemy State Machine")]
    private EnemyBase _enemyBase;
    private EnemyState currentState;
    [Header("State")]
    private EnemyState_Idle idleState;
    private EnemyState_Stagger staggerState;
    private EnemyState_Chase chaseState;
    private EnemyState_Attack attackState;
    private EnemyState_Dead deadState;
    private EnemyState_Patrol patrolState;
    private EnemyState_Suspicion suspicionState;
    #region Getters
    public EnemyState CurrentState => currentState;
    public EnemyState_Idle EnemyIdleState => idleState;
    public EnemyState_Stagger EnemyStaggerState => staggerState;
    public EnemyState_Chase EnemyChaseState => chaseState;
    public EnemyState_Attack EnemyAttackState => attackState;
    public EnemyState_Dead EnemyDeadState => deadState;
    public EnemyState_Patrol EnemyPatrolState => patrolState;
    public EnemyState_Suspicion EnemySuspicionState => suspicionState;
    #endregion

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        Debug.Log($"{gameObject.name} - EnemyStateMachine đã được khởi tạo!");
        idleState = new EnemyState_Idle(_enemyBase);
        staggerState = new EnemyState_Stagger(_enemyBase);
        chaseState = new EnemyState_Chase(_enemyBase);
        attackState = new EnemyState_Attack(_enemyBase);
        deadState = new EnemyState_Dead(_enemyBase);
        patrolState = new EnemyState_Patrol(_enemyBase);
        suspicionState = new EnemyState_Suspicion(_enemyBase);
        ResetToDefaultState(); // Đặt trạng thái mặc định khi khởi tạo, có thể là Idle hoặc Patrol tùy thuộc vào thiết kế của Enemy
        Subcribe(); // Đăng ký sự kiện khi khởi tạo để đảm bảo rằng trạng thái sẽ được kích hoạt khi sự kiện vỡ trạng thái xảy ra
    }

    public void OnDisable()
    {
        Unsubscribe(); // Hủy đăng ký sự kiện khi đối tượng bị hủy để tránh lỗi
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateLogic();
        }
    }

    #region Event Handlers
    private void Subcribe()
    {
        _enemyBase.EventManager.OnStagger += ActivateStunState; // Đăng ký sự kiện vỡ trạng thái để kích hoạt trạng thái Stagger
        _enemyBase.EventManager.OnDead += ActivateDeadState; // Đăng ký sự kiện chết để kích hoạt trạng thái Dead, có thể dùng lambda để tránh lỗi khi truyền trực tiếp hàm nếu hàm đó có tham số
    }

    private void Unsubscribe()
    {
        _enemyBase.EventManager.OnStagger -= ActivateStunState; // Hủy đăng ký sự kiện khi đối tượng bị hủy để tránh lỗi
        _enemyBase.EventManager.OnDead -= ActivateDeadState; // Hủy đăng ký sự kiện chết để tránh lỗi, cần đảm bảo rằng lambda được hủy đúng cách nếu dùng lambda để đăng ký
    }
    #endregion

    #region State Management
    //Hàm chuyển đổi trạng thái
    public void ChangeState(EnemyState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;

        if (currentState != null)
        {
            currentState.Enter();
        }
    }
    //Hàm Kích hoạt trạng thái Stagger
    private void ActivateStunState()
    {
        ChangeState(staggerState);
    }
    //Hàm Kích hoạt trạng thái Dead (có thể gọi khi Enemy chết)
    public void ActivateDeadState()
    {
        ChangeState(deadState);
    }
    //Hàm Reset trạng thái về Idle (có thể gọi sau khi kết thúc trạng thái Stagger)
    public void ResetToDefaultState()
    {
        if (_enemyBase.Locomotion.isPatroller && _enemyBase.Locomotion.wayPoints.Length > 0)
        {
            ChangeState(patrolState);
        }
        else
            ChangeState(idleState);
    }
    #endregion
}
