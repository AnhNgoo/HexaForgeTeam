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
        _enemyBase.Locomotion.SetAngularSpeed(_enemyBase.Data.attackTurnSpeed);
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Attack.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (_enemyBase.MinibossBehaviour != null && _enemyBase.MinibossBehaviour.IsActionLocked)
        {
            _enemyBase.Locomotion.StopMoving();
            return;
        }

        if (_enemyBase.Combat.IsPerformingAttack)
            return;

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

        if (_enemyBase.MinibossBehaviour != null && _enemyBase.MinibossBehaviour.UpdateSpecialMovement(playerTransform))
        {
            return;
        }

        //Luôn xoay về hướng người chơi khi tấn công để tạo hiệu ứng tương tác và tăng tính chân thực của Enemy, có thể điều chỉnh lại logic quay nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể đứng yên khi tấn công mà không quay về hướng người chơi)
        Vector3 lookDirection = playerTransform.position - _enemyBase.MyTransform.position;

        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);

        _enemyBase.MyTransform.rotation = Quaternion.RotateTowards(_enemyBase.MyTransform.rotation, targetRotation, _enemyBase.Data.attackTurnSpeed * Time.deltaTime);

        if (Time.time < _attackEndTime) return; //Nếu đang trong thời gian của đòn tấn công hiện tại thì không thực hiện logic tấn công mới để tránh lỗi spam tấn công liên tục

        //Tính toán khoảng cách đến người chơi để quyết định có tiếp tục tấn công hay không
        float distanceToPlayer = Vector3.Distance(_enemyBase.MyTransform.position, playerTransform.position);

        //Kiểm tra nếu Enemy có thể block và nên block thay vì tiếp tục tấn công, giúp Enemy có thể phòng thủ khi cần thiết thay vì chỉ tập trung vào tấn công, tạo sự đa dạng trong hành vi của Enemy và tránh lỗi spam tấn công liên tục khi người chơi đang ở gần
        if (_enemyBase.Guard != null && _enemyBase.Guard.ShouldEnterGuard(distanceToPlayer))
        {
            _enemyBase.StateMachine.ChangeState(
                _enemyBase.StateMachine.EnemyBlockState
            );
            return;
        }

        float facingAngle = Vector3.Angle(_enemyBase.MyTransform.forward, lookDirection.normalized);

        if (facingAngle > _enemyBase.Data.attackFacingAngle) //Nếu Enemy đang không đối mặt với người chơi trong phạm vi góc cho phép để tấn công, giúp Enemy có thể điều chỉnh hướng tấn công một cách hợp lý thay vì chỉ tập trung vào tấn công khi đang lệch hướng, tạo sự đa dạng trong hành vi của Enemy và tránh lỗi spam tấn công liên tục khi người chơi đang ở gần
        {
            _enemyBase.Locomotion.StopMoving();
            return;
        }

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

            _isWaitingCooldown = false;
            _enemyBase.Locomotion.StopMoving();
            float effectiveDuration = _enemyBase.Combat.PerformAttack(chosenAttack); //Thực hiện đòn tấn công đã chọn và nhận về thời gian hiệu lực của đòn tấn công, giúp kiểm soát thời gian giữa các đòn tấn công và tránh lỗi spam tấn công liên tục
            _attackEndTime = Time.time + effectiveDuration; //Cập nhật thời gian kết thúc của đòn tấn công hiện tại dựa trên thời gian hiệu lực của đòn tấn công, giúp kiểm soát thời gian giữa các đòn tấn công và tránh lỗi spam tấn công liên tục
        }
        else
        {
            float closeCombatRange = GetClosestMeleeRange();

            if (closeCombatRange < 0f)
            {
                float preferredRange = GetPreferredRangedDistance();

                BeginCooldownMovement();

                float minimumRangedDistance = GetMinimumRangedDistance();

                if (distanceToPlayer < minimumRangedDistance)
                {
                    BeginCooldownMovement();

                    if (Time.time >= _nextStrafeTime)
                    {
                        _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.moveSpeed);

                        Vector3 awayFromPlayer =
                            _enemyBase.MyTransform.position - playerTransform.position;

                        awayFromPlayer.y = 0f;

                        if (awayFromPlayer.sqrMagnitude <= 0.01f)
                            awayFromPlayer = -_enemyBase.MyTransform.forward;

                        Vector3 retreatPoint =
                            _enemyBase.MyTransform.position +
                            awayFromPlayer.normalized * (minimumRangedDistance - distanceToPlayer + 2f);

                        retreatPoint = _enemyBase.Detection.ClampPointToLeash(retreatPoint);
                        _strafeTargetPos = _enemyBase.Locomotion.GetRandomRoamPosition(retreatPoint, 1.5f);

                        _nextStrafeTime = Time.time + Random.Range(0.8f, 1.2f);
                    }

                    _enemyBase.Locomotion.MoveToTarget(_strafeTargetPos, 0.2f);
                    return;
                }

                if (distanceToPlayer > preferredRange + 0.5f)
                {
                    _enemyBase.Locomotion.SetSpeed(
                        _enemyBase.Data.moveSpeed
                    );

                    _enemyBase.Locomotion.MoveToTarget(
                        playerTransform.position,
                        preferredRange
                    );
                }
                else
                {
                    if (Time.time >= _nextStrafeTime)
                    {
                        _enemyBase.Locomotion.SetSpeed(
                            _enemyBase.Data.patrolSpeed
                        );

                        CalculateStrafePoint(playerTransform.position);
                    }

                    _enemyBase.Locomotion.MoveToTarget(_strafeTargetPos);
                }

                return;
            }

            if (distanceToPlayer > closeCombatRange)
            {
                BeginCooldownMovement();

                float approachDistance =
                    Mathf.Max(0.8f, closeCombatRange * 0.75f);

                _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.moveSpeed);
                _enemyBase.Locomotion.MoveToTarget(
                    playerTransform.position,
                    approachDistance
                );
                return;
            }

            BeginCooldownMovement();

            if (Time.time >= _nextStrafeTime)
            {
                _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed);
                CalculateStrafePoint(playerTransform.position);
            }

            _enemyBase.Locomotion.MoveToTarget(_strafeTargetPos);
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

    private float GetClosestMeleeRange()
    {
        float result = float.MaxValue;

        foreach (AttackDataSO attack in _enemyBase.Combat.AttackArsenal)
        {
            if (attack == null || attack.attackType != AttackType.Melee)
                continue;

            result = Mathf.Min(result, attack.maxAttackRange);
        }

        return result == float.MaxValue ? -1f : result;
    }

    private void BeginCooldownMovement()
    {
        if (_isWaitingCooldown) return;

        _isWaitingCooldown = true;

        _enemyBase.Locomotion.SetAngularSpeed(_enemyBase.Data.recoveryTurnSpeed);

        int movementHash = _enemyBase.AnimatorController.HasAnimationState(_enemyBase.AnimatorController.WalkHash)
            ? _enemyBase.AnimatorController.WalkHash
            : _enemyBase.AnimatorController.ChaseHash;

        _enemyBase.AnimatorController.PlayAnimation(movementHash);
    }

    private float GetPreferredRangedDistance()
    {
        float sharedMin = 0f;
        float sharedMax = float.MaxValue;
        bool foundRanged = false;

        foreach (AttackDataSO attack in _enemyBase.Combat.AttackArsenal)
        {
            if (attack == null || attack.attackType != AttackType.Ranged)
                continue;

            foundRanged = true;
            sharedMin = Mathf.Max(sharedMin, attack.minAttackRange);
            sharedMax = Mathf.Min(sharedMax, attack.maxAttackRange);
        }

        if (!foundRanged)
            return 1.5f;

        if (sharedMax < sharedMin)
            return sharedMin;

        return (sharedMin + sharedMax) * 0.5f;
    }

    private float GetMinimumRangedDistance()
    {
        float result = float.MaxValue;

        foreach (AttackDataSO attack in _enemyBase.Combat.AttackArsenal)
        {
            if (attack == null || attack.attackType != AttackType.Ranged)
                continue;

            result = Mathf.Min(result, attack.minAttackRange);
        }

        return result == float.MaxValue ? 0f : result;
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Locomotion.SetAngularSpeed(_enemyBase.Data.chaseAngularSpeed);
        _enemyBase.Combat.ForceCloseHitbox(); //Đảm bảo rằng hitbox sẽ được đóng đúng thời điểm khi rời khỏi trạng thái Attack, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        Debug.Log($"{_enemyBase.gameObject.name} đã rời khỏi trạng thái Attack.");
    }
}
