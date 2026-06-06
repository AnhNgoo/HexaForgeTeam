using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyState_Idle : EnemyState
{
    public EnemyState_Idle(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        _enemyBase.Locomotion.StopMoving(); // Đảm bảo rằng Enemy sẽ dừng lại khi vào trạng thái Idle
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash); // Phát animation Idle khi vào trạng thái này
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

    public override void Exit()
    {
        base.Exit();
    }
}
