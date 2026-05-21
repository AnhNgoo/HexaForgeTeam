using UnityEngine;

public class EnemyState_Attack : EnemyState
{
    public EnemyState_Attack(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Attack.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        Transform playerTransform = _enemyBase.Detection.CurrentTarget;

        if (playerTransform == null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyIdleState);
            return;
        }

        //Tính toán khoảng cách đến người chơi để quyết định có tiếp tục tấn công hay không
        float distanceToPlayer = Vector3.Distance(_enemyBase.MyTransform.position, playerTransform.position);
        if (distanceToPlayer > 2f)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState);
            return;
        }
        else
        {
            if (_enemyBase.Combat.CanAttack())
            {
                _enemyBase.Combat.PerformAttack();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Combat.ForceCloseHitbox(); //Đảm bảo rằng hitbox sẽ được đóng đúng thời điểm khi rời khỏi trạng thái Attack, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        Debug.Log($"{_enemyBase.gameObject.name} đã rời khỏi trạng thái Attack.");
    }
}
