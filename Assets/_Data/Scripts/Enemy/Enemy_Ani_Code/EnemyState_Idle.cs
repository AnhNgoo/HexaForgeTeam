using UnityEngine;

public class EnemyState_Idle : EnemyState
{
    public EnemyState_Idle(EnemyBase enemyBase) : base(enemyBase)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // To_Do: Thực hiện các hành động khi vào trạng thái Idle, ví dụ: phát animation Idle
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Idle.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        //Nếu Radar báo có mục tiêu -> Chuyển sang rượt đuổi ngay!
        if (_enemyBase.Detection.CurrentTarget != null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState);
        }
    }
}
