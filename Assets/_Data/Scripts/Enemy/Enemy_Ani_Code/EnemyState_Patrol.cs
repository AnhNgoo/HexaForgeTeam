using UnityEngine;

public class EnemyState_Patrol : EnemyState
{
    private Vector3 _currentTargetPos;
    private bool _isWaitting;
    private float _waitTimer;
    public EnemyState_Patrol(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        _isWaitting = false;

        _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed); //Đặt tốc độ di chuyển khi tuần tra, có thể điều chỉnh trong EnemyData để tạo ra sự đa dạng về hành vi di chuyển của các loại Enemy khác nhau

        FindNewRoamPoint();
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Patrol.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        //Nếu thấy người chơi lập tức đuổi theo
        if (_enemyBase.Detection.CurrentTarget != null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState);
            return;
        }

        //Logic đi tuần: Khi đến điểm tuần tra, đợi một khoảng thời gian rồi tìm điểm tuần tra mới
        if (_isWaitting)
        {
            //Đứng yên nghỉ ngơi tại chỗ
            if (Time.time >= _waitTimer)
            {
                _isWaitting = false;
                FindNewRoamPoint(); //Tìm điểm tuần tra mới sau khi đã đợi đủ thời gian
            }
        }
        else
        {
            // Bỏ qua trục Y khi tính khoảng cách để tránh lỗi chênh lệch chiều cao mặt đất
            Vector3 myPos = new Vector3(_enemyBase.MyTransform.position.x, 0, _enemyBase.MyTransform.position.z);
            Vector3 targetPos = new Vector3(_currentTargetPos.x, 0, _currentTargetPos.z);

            float distanceToDestination = Vector3.Distance(myPos, targetPos);
            if (distanceToDestination < 0.5f)
            {
                _isWaitting = true;
                _waitTimer = Time.time + Random.Range(3f, 6f); //Đợi một khoảng thời gian ngẫu nhiên từ 3 đến 6 giây trước khi tìm điểm tuần tra mới, giúp tạo ra hành vi đi tuần tự nhiên hơn

                _enemyBase.Locomotion.StopMoving();
                _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash); //Có thể dùng animation đứng yên để làm cho hành vi đi tuần trông tự nhiên hơn, hoặc có thể tạo một animation riêng cho hành vi đứng yên khi đi tuần nếu muốn tạo sự khác biệt rõ ràng giữa hai trạng thái này
            }
        }
    }

    private void FindNewRoamPoint()
    {
        //Nhờ locomotion tìm một điểm an toàn trên NavMesh quanh cái Spawner
        _currentTargetPos = _enemyBase.Locomotion.GetRandomRoamPosition(_enemyBase.SpawnOrigin, _enemyBase.Data.roamRadius);

        _enemyBase.Locomotion.SetAngularSpeed(120f); // Bật tốc độ xoay mặt tự động của NavMesh lên 120 độ/s để Enemy có thể tự động quay về hướng di chuyển khi đi tuần, giúp hành vi đi tuần trông tự nhiên hơn và tránh lỗi Enemy di chuyển đến điểm tuần tra mới nhưng vẫn quay mặt về hướng cũ thay vì hướng về điểm tuần tra mới
        _enemyBase.Locomotion.MoveToTarget(_currentTargetPos);
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash); //Có thể dùng animation chạy để làm cho hành vi đi tuần trông tự nhiên hơn, hoặc có thể tạo một animation riêng cho hành vi đi tuần nếu muốn tạo sự khác biệt rõ ràng giữa hai trạng thái này
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Locomotion.StopMoving();
    }
}
