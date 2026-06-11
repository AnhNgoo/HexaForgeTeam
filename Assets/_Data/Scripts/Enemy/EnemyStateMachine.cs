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
    private EnemyState_Return returnState;
    #region Getters
    public EnemyState CurrentState => currentState;
    public EnemyState_Idle EnemyIdleState => idleState;
    public EnemyState_Stagger EnemyStaggerState => staggerState;
    public EnemyState_Chase EnemyChaseState => chaseState;
    public EnemyState_Attack EnemyAttackState => attackState;
    public EnemyState_Dead EnemyDeadState => deadState;
    public EnemyState_Patrol EnemyPatrolState => patrolState;
    public EnemyState_Suspicion EnemySuspicionState => suspicionState;
    public EnemyState_Return EnemyReturnState => returnState;
    #endregion

    private bool _isSubscribed;
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
        returnState = new EnemyState_Return(_enemyBase);
        currentState = null;

        Subcribe(); // Đăng ký sự kiện khi khởi tạo để đảm bảo rằng trạng thái sẽ được kích hoạt khi sự kiện vỡ trạng thái xảy ra
    }

    private void OnEnable()
    {
        Subcribe(); // Đăng ký sự kiện khi đối tượng được kích hoạt để đảm bảo rằng trạng thái sẽ được kích hoạt khi sự kiện vỡ trạng thái xảy ra, cần kiểm tra nếu đã đăng ký để tránh đăng ký lại nhiều lần
    }

    private void OnDisable()
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
        if (_isSubscribed || _enemyBase == null || _enemyBase.EventManager == null) return; // Kiểm tra nếu đã đăng ký hoặc _enemyBase hoặc EventManager chưa được gán để tránh lỗi
        _enemyBase.EventManager.OnStagger += ActivateStunState; // Đăng ký sự kiện vỡ trạng thái để kích hoạt trạng thái Stagger
        _enemyBase.EventManager.OnDead += ActivateDeadState; // Đăng ký sự kiện chết để kích hoạt trạng thái Dead, có thể dùng lambda để tránh lỗi khi truyền trực tiếp hàm nếu hàm đó có tham số
        _isSubscribed = true; // Đánh dấu đã đăng ký để tránh đăng ký lại nhiều lần
    }

    private void Unsubscribe()
    {
        //Kiểm tra xem _enemyBase và EventManager đã được gán hay chưa
        if (!_isSubscribed || _enemyBase == null || _enemyBase.EventManager == null) return;
        _enemyBase.EventManager.OnStagger -= ActivateStunState; // Hủy đăng ký sự kiện khi đối tượng bị hủy để tránh lỗi
        _enemyBase.EventManager.OnDead -= ActivateDeadState; // Hủy đăng ký sự kiện chết để tránh lỗi, cần đảm bảo rằng lambda được hủy đúng cách nếu dùng lambda để đăng ký   
        _isSubscribed = false; // Đánh dấu đã hủy đăng ký
    }
    #endregion

    #region State Management
    //Hàm chuyển đổi trạng thái
    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return; //Nếu đang ở trạng thái muốn chuyển đến thì không làm gì để tránh lỗi chuyển trạng thái liên tục khi đã ở trong trạng thái đó

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
        if (_enemyBase.Locomotion.isPatroller)
        {
            ChangeState(patrolState);
        }
        else
            ChangeState(idleState);
    }
    #endregion
}
