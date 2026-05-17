using System.Collections;
using System.Collections.Generic;
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
    private EnemyState_Attack attackState; //Có thể thêm sau nếu cần thiết
    #region Getters
    public EnemyState CurrentState => currentState;
    public EnemyState_Idle EnemyIdleState => idleState;
    public EnemyState_Stagger EnemyStaggerState => staggerState;
    public EnemyState_Chase EnemyChaseState => chaseState;
    public EnemyState_Attack EnemyAttackState => attackState; //Có thể thêm sau nếu cần thiết
    #endregion

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        Debug.Log($"{gameObject.name} - EnemyStateMachine đã được khởi tạo!");
        idleState = new EnemyState_Idle(_enemyBase);
        staggerState = new EnemyState_Stagger(_enemyBase);
        chaseState = new EnemyState_Chase(_enemyBase);
        attackState = new EnemyState_Attack(_enemyBase);
        ChangeState(idleState);
        Subcribe(); // Đăng ký sự kiện khi khởi tạo để đảm bảo rằng trạng thái sẽ được kích hoạt khi sự kiện vỡ trạng thái xảy ra
    }

    public void OnDestroy()
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
        _enemyBase.EventManager.OnStagger += ActivateStun; // Đăng ký sự kiện vỡ trạng thái để kích hoạt trạng thái Stagger
    }

    private void Unsubscribe()
    {
        _enemyBase.EventManager.OnStagger -= ActivateStun; // Hủy đăng ký sự kiện khi đối tượng bị hủy để tránh lỗi
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
    private void ActivateStun()
    {
        ChangeState(staggerState);
    }
    //Hàm Reset trạng thái về Idle (có thể gọi sau khi kết thúc trạng thái Stagger)
    public void ResetToDefaultState()
    {
        ChangeState(idleState);
    }
    #endregion
}
