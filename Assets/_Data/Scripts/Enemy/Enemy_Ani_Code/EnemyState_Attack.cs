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
        if (Time.time < _attackEndTime)
        {
            return; //Nếu thời gian hiện tại vẫn chưa đến thời điểm kết thúc của đòn tấn công, không thực hiện logic tấn công mới để tránh lỗi spam tấn công liên tục
        }
        Transform playerTransform = _enemyBase.Detection.CurrentTarget;

        if (playerTransform == null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyIdleState);
            return;
        }
        //Tính toán khoảng cách đến người chơi để quyết định có tiếp tục tấn công hay không
        float distanceToPlayer = Vector3.Distance(_enemyBase.MyTransform.position, playerTransform.position);

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
