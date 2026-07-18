using UnityEngine;

public class EnemyState_Chase : EnemyState
{
    public EnemyState_Chase(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash); // Phát animation chạy khi vào trạng thái này
        _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.moveSpeed); // Đặt tốc độ di chuyển khi truy đuổi
        _enemyBase.Locomotion.SetAngularSpeed(_enemyBase.Data.chaseAngularSpeed);
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Chase.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (_enemyBase.MinibossBehaviour != null && _enemyBase.MinibossBehaviour.IsActionLocked)
        {
            _enemyBase.Locomotion.StopMoving();
            return;
        }

        //Kiểm tra dây xích
        float distanceToOrigin = Vector3.Distance(_enemyBase.MyTransform.position, _enemyBase.SpawnOrigin);
        if (distanceToOrigin > _enemyBase.CurrentLeash)
        {
            Debug.Log("color=red><b>Enemy đã vượt quá khoảng cách dây xích, tự động quay về vị trí spawn để tránh bị lạc quá xa và không thể tương tác với player.</b></color>");
            _enemyBase.Detection.ForceLoseTarget(); //Ép mất mục tiêu khi vượt quá khoảng cách dây xích để tránh lỗi Enemy vẫn tiếp tục truy đuổi mặc dù đã đi quá xa so với vị trí xuất hiện ban đầu nhưng vẫn còn trong khoảng cách leash
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemySuspicionState); //Chuyển sang trạng thái Return để quay về vị trí xuất hiện ban đầu, tránh lỗi Enemy vẫn tiếp tục truy đuổi mặc dù đã đi quá xa so với vị trí xuất hiện ban đầu nhưng vẫn còn trong khoảng cách leash
            return;

        }

        Transform playerTransform = _enemyBase.Detection.CurrentTarget;

        if (playerTransform == null)
        {
            return;
        }

        if (!_enemyBase.Detection.IsPointInLeash(playerTransform.position))
        {
            _enemyBase.Detection.ForceLoseTarget();
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemySuspicionState);
            return;
        }

        float maxRangeInArsenal = 0f;
        foreach (var atk in _enemyBase.Combat.AttackArsenal)
        {
            if (atk.maxAttackRange > maxRangeInArsenal) maxRangeInArsenal = atk.maxAttackRange;
        }
        //Tính toán khoảng cách đến người chơi để quyết định có tiếp tục truy đuổi hay không
        float distanceToPlayer = Vector3.Distance(_enemyBase.MyTransform.position, playerTransform.position);
        //Tính khoảng cách xem nên chuyển sang trạng thái tấn công hay tiếp tục truy đuổi
        if (distanceToPlayer > maxRangeInArsenal)
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
