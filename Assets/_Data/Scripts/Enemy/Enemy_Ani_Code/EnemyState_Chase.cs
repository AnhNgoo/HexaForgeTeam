using UnityEngine;

public class EnemyState_Chase : EnemyState
{
    public EnemyState_Chase(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash); // Phát animation chạy khi vào trạng thái này
        _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.moveSpeed); // Đặt tốc độ di chuyển khi truy đuổi
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Chase.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        Transform playerTransform = _enemyBase.Detection.CurrentTarget;

        if (playerTransform == null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemySuspicionState);
            return;
        }

        //Tính toán khoảng cách đến người chơi để quyết định có tiếp tục truy đuổi hay không
        float distanceToPlayer = Vector3.Distance(_enemyBase.MyTransform.position, playerTransform.position);
        //Tính khoảng cách xem nên chuyển sang trạng thái tấn công hay tiếp tục truy đuổi
        if (distanceToPlayer > 2f)
        {
            _enemyBase.Locomotion.MoveToTarget(playerTransform.position);
        }
        else
        {
            _enemyBase.Locomotion.StopMoving();
            //Chuyển sang trạng thái tấn công nếu đã đủ gần
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyAttackState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Locomotion.StopMoving();
        Debug.Log($"{_enemyBase.gameObject.name} đã rời khỏi trạng thái Chase.");
    }
}
