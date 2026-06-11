using UnityEngine;

public class EnemyState_Attack : EnemyState
{
    private float _attackEndTime; //Biến để theo dõi thời gian kết thúc của đòn tấn công hiện tại, giúp kiểm soát thời gian giữa các đòn tấn công và tránh lỗi spam tấn công liên tục
    private bool _isWaitingCooldown; //Biến để theo dõi xem Enemy có đang trong quá trình chờ đợi hồi chiêu của đòn tấn công hay không, giúp kiểm soát logic tấn công và tránh lỗi spam tấn công liên tục
    private float _nextStrafeTime; //Biến để theo dõi thời gian tiếp theo mà Enemy có thể thực hiện động tác di chuyển tấn công (strafe) để tạo sự đa dạng trong cách tấn công và tránh lỗi spam động tác di chuyển tấn công liên tục
    private Vector3 _strafeTargetPos; //Biến để lưu trữ vị trí mục tiêu cho động tác di chuyển tấn công (strafe), giúp Enemy có thể di chuyển xung quanh người chơi một cách linh hoạt hơn thay vì chỉ đứng yên tại chỗ khi tấn công, tạo sự đa dạng trong cách tấn công và tránh lỗi spam động tác di chuyển tấn công liên tục
    public EnemyState_Attack(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        _attackEndTime = 0f; //Khởi tạo thời gian kết thúc của đòn tấn công hiện tại, có thể điều chỉnh giá trị này dựa trên thời gian của animation tấn công hoặc thời gian hồi chiêu của đòn tấn công
        _isWaitingCooldown = false; //Khởi tạo trạng thái chờ đợi hồi chiêu của đòn tấn công, có thể điều chỉnh giá trị này dựa trên thời gian của animation tấn công hoặc thời gian hồi chiêu của đòn tấn công
        _nextStrafeTime = 0f; //Khởi tạo thời gian tiếp theo mà Enemy có thể thực hiện động tác di chuyển tấn công (strafe), có thể điều chỉnh giá trị này dựa trên thời gian của animation tấn công hoặc thời gian hồi chiêu của đòn tấn công
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Attack.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        float distanceToOrigin = Vector3.Distance(_enemyBase.MyTransform.position, _enemyBase.SpawnOrigin);
        if (distanceToOrigin > _enemyBase.CurrentLeash + 5f) //Nếu đuổi xa quá rồi ở đó và nghi nghờ
        {
            _enemyBase.Detection.ForceLoseTarget(); //Đặt lại trạng thái phát hiện để xóa mục tiêu hiện tại và các thông tin liên quan, tránh lỗi Enemy vẫn tiếp tục tấn công mặc dù người chơi đã chạy ra khỏi phạm vi tấn công nhưng vẫn còn trong khoảng cách leash
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemySuspicionState); //Nếu đã đi quá xa so với vị trí xuất hiện ban đầu (vượt quá khoảng cách leash cộng thêm một khoảng đệm nhỏ để tránh lỗi chuyển trạng thái liên tục khi đang ở gần ranh giới), chuyển sang trạng thái Suspicion để bắt đầu nghi ngờ và tìm kiếm mục tiêu, tránh trường hợp Enemy vẫn tiếp tục tấn công mặc dù người chơi đã chạy ra khỏi phạm vi tấn công nhưng vẫn còn trong khoảng cách leash
            return;
        }
        Transform playerTransform = _enemyBase.Detection.CurrentTarget;

        if (playerTransform == null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyIdleState);
            return;
        }

        if (!_enemyBase.Detection.IsPointInLeash(playerTransform.position))
        {
            _enemyBase.Detection.ForceLoseTarget();
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemySuspicionState);
            return;
        }

        //Luôn xoay về hướng người chơi khi tấn công để tạo hiệu ứng tương tác và tăng tính chân thực của Enemy, có thể điều chỉnh lại logic quay nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể đứng yên khi tấn công mà không quay về hướng người chơi)
        Vector3 lookDir = (playerTransform.position - _enemyBase.MyTransform.position).normalized;
        lookDir.y = 0; //Giữ nguyên trục Y để tránh nghiêng lên xuống
        _enemyBase.MyTransform.rotation = Quaternion.Slerp(_enemyBase.MyTransform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f); //Quay về hướng người chơi với tốc độ mượt mà, có thể điều chỉnh tốc độ quay nếu cần thiết

        if (Time.time < _attackEndTime) return; //Nếu đang trong thời gian của đòn tấn công hiện tại thì không thực hiện logic tấn công mới để tránh lỗi spam tấn công liên tục

        //Tính toán khoảng cách đến người chơi để quyết định có tiếp tục tấn công hay không
        float distanceToPlayer = Vector3.Distance(_enemyBase.MyTransform.position, playerTransform.position);

        AttackDataSO chosenAttack = _enemyBase.Combat.ChooseAttack(distanceToPlayer); //Chọn đòn tấn công phù hợp dựa trên khoảng cách đến player
        if (chosenAttack != null)
        {
            bool isCloseEnoughForMelee = distanceToPlayer <= 0.5f;
            bool needsClearShot = chosenAttack.attackType == AttackType.Ranged;

            if (needsClearShot && !_enemyBase.Detection.IsTargetVisible(playerTransform))
            {
                if (_enemyBase.Detection.IsPointInLeash(playerTransform.position))
                    _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState);
                else
                {
                    _enemyBase.Detection.ForceLoseTarget();
                    _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemySuspicionState);
                }

                return;
            }

            _isWaitingCooldown = false; //Đặt lại trạng thái chờ đợi hồi chiêu khi đã chọn được đòn tấn công mới, có thể điều chỉnh lại logic này nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể không cần chờ đợi hồi chiêu và có thể tấn công liên tục)
            _enemyBase.Combat.PerformAttack(chosenAttack); //Thực hiện đòn tấn công đã chọn, có thể điều chỉnh lại logic này nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể có hiệu ứng đặc biệt khi thực hiện đòn tấn công)
            _attackEndTime = Time.time + chosenAttack.attackDuration; //Cập nhật thời gian kết thúc của đòn tấn công hiện tại dựa trên thời gian của đòn tấn công đã chọn, có thể điều chỉnh lại logic này nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể có thời gian tấn công dài hơn hoặc ngắn hơn)
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
                if (_enemyBase.Detection.IsPointInLeash(playerTransform.position))
                    _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState); //Nếu không có đòn tấn công nào phù hợp và player đã di chuyển ra khỏi phạm vi tấn công, chuyển sang trạng thái Chase để tiếp tục truy đuổi
            }
            else
            {
                if (!_isWaitingCooldown) //Nếu chưa bắt đầu chờ đợi hồi chiêu thì bắt đầu chờ đợi, nếu đã bắt đầu chờ đợi rồi thì vẫn tiếp tục đứng yên chờ hồi chiêu mà không cần thiết phải đặt lại thời gian chờ đợi
                {
                    _isWaitingCooldown = true; //Bắt đầu quá trình chờ đợi hồi chiêu của đòn tấn công, có thể điều chỉnh lại logic này nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể không cần chờ đợi hồi chiêu và có thể tấn công liên tục)
                    _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed); //Đi bộ tại chỗ khi chờ hồi chiêu, có thể điều chỉnh lại tốc độ này nếu muốn Enemy di chuyển nhẹ nhàng tại chỗ khi chờ hồi chiêu thay vì đứng yên một chỗ
                    CalculateStrafePoint(playerTransform.position); //Tính toán điểm di chuyển tấn công (strafe) xung quanh người chơi để tạo sự đa dạng trong cách tấn công và tránh lỗi spam động tác di chuyển tấn công liên tục
                }
            }
        }
    }

    private void CalculateStrafePoint(Vector3 playerPos)
    {
        Vector3 dirToPlayer = (_enemyBase.MyTransform.position - playerPos).normalized;

        //Chọn ngẫu nhiên hướng strafe trái hoặc phải để tạo sự đa dạng trong cách tấn công
        Vector3 strafeDirection = (Random.value > 0.5f) ? Vector3.Cross(dirToPlayer, Vector3.up) : Vector3.Cross(dirToPlayer, Vector3.down);

        //Tính toán điểm strafe tiềm năng cách người chơi một khoảng nhất định, có thể điều chỉnh khoảng cách này nếu muốn Enemy di chuyển gần hơn hoặc xa hơn khi thực hiện động tác di chuyển tấn công (strafe) 
        Vector3 potentialPoint = _enemyBase.MyTransform.position + strafeDirection * 3f;

        //Điều chỉnh điểm strafe tiềm năng để đảm bảo rằng nó nằm trong khu vực có thể di chuyển được và không bị chặn bởi địa hình hoặc vật cản, giúp Enemy có thể di chuyển xung quanh người chơi một cách linh hoạt hơn thay vì chỉ đứng yên tại chỗ khi tấn công, tạo sự đa dạng trong cách tấn công và tránh lỗi spam động tác di chuyển tấn công liên tục
        _strafeTargetPos = _enemyBase.Locomotion.GetRandomRoamPosition(potentialPoint, 1f);
        //Di chuyển đến điểm strafe đã tính toán nếu đã đến thời gian có thể thực hiện động tác di chuyển tấn công (strafe), giúp Enemy có thể di chuyển xung quanh người chơi một cách linh hoạt hơn thay vì chỉ đứng yên tại chỗ khi tấn công, tạo sự đa dạng trong cách tấn công và tránh lỗi spam động tác di chuyển tấn công liên tục
        _nextStrafeTime = Time.time + Random.Range(1.5f, 3f);
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Combat.ForceCloseHitbox(); //Đảm bảo rằng hitbox sẽ được đóng đúng thời điểm khi rời khỏi trạng thái Attack, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        Debug.Log($"{_enemyBase.gameObject.name} đã rời khỏi trạng thái Attack.");
    }
}
