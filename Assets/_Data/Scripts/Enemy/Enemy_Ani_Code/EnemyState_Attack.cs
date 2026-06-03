using UnityEngine;

public class EnemyState_Attack : EnemyState
{
    private float _attackEndTime; //Biến để theo dõi thời gian kết thúc của đòn tấn công hiện tại, giúp kiểm soát thời gian giữa các đòn tấn công và tránh lỗi spam tấn công liên tục
    public EnemyState_Attack(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        _attackEndTime = 0f; //Khởi tạo thời gian kết thúc của đòn tấn công hiện tại, có thể điều chỉnh giá trị này dựa trên thời gian của animation tấn công hoặc thời gian hồi chiêu của đòn tấn công
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
        Vector3 lookDir = (playerTransform.position - _enemyBase.MyTransform.position).normalized;
        lookDir.y = 0; //Giữ nguyên trục Y để tránh nghiêng lên xuống
        _enemyBase.MyTransform.rotation = Quaternion.Slerp(_enemyBase.MyTransform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f); //Quay về hướng người chơi với tốc độ mượt mà, có thể điều chỉnh tốc độ quay nếu cần thiết

        if (Time.time < _attackEndTime) return; //Nếu đang trong thời gian của đòn tấn công hiện tại thì không thực hiện logic tấn công mới để tránh lỗi spam tấn công liên tục

        //Tính toán khoảng cách đến người chơi để quyết định có tiếp tục tấn công hay không
        float distanceToPlayer = Vector3.Distance(_enemyBase.MyTransform.position, playerTransform.position);

        bool hasClearShot = _enemyBase.Detection.IsTargetVisible(playerTransform); //Kiểm tra xem có đường bắn thẳng đến người chơi hay không để quyết định có thực hiện tấn công tầm xa hay không, tránh trường hợp Enemy vẫn thực hiện tấn công tầm xa mặc dù người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách tấn công

        if (!hasClearShot)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState); //Nếu không có đường bắn thẳng đến người chơi, chuyển sang trạng thái Chase để tiếp tục truy đuổi
            return;
        }

        AttackDataSO chosenAttack = _enemyBase.Combat.ChooseAttack(distanceToPlayer); //Chọn đòn tấn công phù hợp dựa trên khoảng cách đến player
        if (chosenAttack != null)
        {
            _enemyBase.Combat.PerformAttack(chosenAttack); //Thực hiện đòn tấn công đã chọn
            _attackEndTime = Time.time + chosenAttack.attackDuration; //Cập nhật thời gian kết thúc của đòn tấn công hiện tại dựa trên thời gian của đòn tấn công đã chọn, giúp kiểm soát thời gian giữa các đòn tấn công và tránh lỗi spam tấn công liên tục
        }
        else
        {
            float maxRangeInArsenal = 0f;
            foreach (var atk in _enemyBase.Combat.AttackArsenal)
            {
                if (atk.maxAttackRange > maxRangeInArsenal) maxRangeInArsenal = atk.maxAttackRange;
            }

            if (distanceToPlayer > maxRangeInArsenal)
            {
                _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState); //Nếu không có đòn tấn công nào phù hợp và player đã di chuyển ra khỏi phạm vi tấn công, chuyển sang trạng thái Chase để tiếp tục truy đuổi
            }
            // MẶC ĐỊNH: Nếu người chơi đứng im trước mặt mà quái hết chiêu (đang cooldown) Nó sẽ đứng yên tại chỗ ngó (Idle) chờ hồi chiêu
        }
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Combat.ForceCloseHitbox(); //Đảm bảo rằng hitbox sẽ được đóng đúng thời điểm khi rời khỏi trạng thái Attack, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        Debug.Log($"{_enemyBase.gameObject.name} đã rời khỏi trạng thái Attack.");
    }
}
