using UnityEngine;

public class EnemyState_Patrol : EnemyState
{
    private Vector3 currentDestination;
    public EnemyState_Patrol(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"{_enemyBase.gameObject.name} - Entering Patrol State");

        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash); // Phát animation đi bộ khi vào trạng thái này (tạm thời dùng animation Chase)

        _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed); // Đặt tốc độ di chuyển khi đi tuần tra
        currentDestination = _enemyBase.Locomotion.GetNextWaypoint();
        _enemyBase.Locomotion.MoveToTarget(currentDestination);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        //Thấy người chơi thì chuyển sang trạng thái Chase
        if (_enemyBase.Detection.CurrentTarget != null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState);
            return;
        }

        //Kiểm tra xem đã đến điểm tuần tra chưa, nếu đến rồi thì lấy điểm tuần tra tiếp theo
        float distanceToWaypoint = Vector3.Distance(new Vector3(_enemyBase.MyTransform.position.x, 0, _enemyBase.MyTransform.position.z), new Vector3(currentDestination.x, 0, currentDestination.z));

        if (distanceToWaypoint < 0.8f) // Ngưỡng khoảng cách để coi như đã đến điểm tuần tra
        {
            currentDestination = _enemyBase.Locomotion.GetNextWaypoint();
            _enemyBase.Locomotion.MoveToTarget(currentDestination);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
