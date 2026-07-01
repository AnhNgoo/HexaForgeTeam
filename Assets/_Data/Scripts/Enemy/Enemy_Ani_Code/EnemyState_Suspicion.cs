using UnityEngine;
public class EnemyState_Suspicion : EnemyState
{
    private float _searchEndTime; //Biến để theo dõi thời gian kết thúc của quá trình nghi ngờ, giúp kiểm soát thời gian mà Enemy sẽ tiếp tục nghi ngờ và tìm kiếm mục tiêu trước khi quay về trạng thái mặc định hoặc trạng thái khác nếu không tìm thấy mục tiêu
    private bool _isWaiting; //Biến để theo dõi xem Enemy có đang trong quá trình chờ đợi ở vị trí cuối cùng biết của mục tiêu hay không, giúp kiểm soát logic di chuyển và tìm kiếm mục tiêu trong trạng thái Suspicion
    private float _waitTimer; //Biến để theo dõi thời gian đã chờ đợi ở vị trí cuối cùng biết của mục tiêu, giúp kiểm soát thời gian mà Enemy sẽ tiếp tục chờ đợi trước khi bắt đầu di chuyển xung quanh khu vực đó để tìm kiếm mục tiêu
    private Vector3 _searchPos; //Biến để lưu trữ vị trí cuối cùng biết của mục tiêu, giúp Enemy có thể di chuyển đến đó và bắt đầu quá trình nghi ngờ và tìm kiếm mục tiêu một cách chính xác hơn thay vì chỉ dựa vào vị trí hiện tại của mục tiêu nếu mục tiêu đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash

    private bool _isInvestigatingScene; //Biến để theo dõi xem Enemy có đang trong quá trình điều tra hiện trường nghi ngờ hay không, giúp kiểm soát logic di chuyển và tìm kiếm mục tiêu trong trạng thái Suspicion khi đã hết thời gian nghi ngờ và tìm kiếm mục tiêu ban đầu nhưng vẫn chưa tìm thấy mục tiêu và muốn tiếp tục điều tra xung quanh khu vực đó để tìm kiếm mục tiêu

    private float _nextStandoffTime; //Biến để theo dõi thời gian tiếp theo mà Enemy có thể thực hiện động tác đối mặt (standoff) với người chơi khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash, giúp tạo sự đa dạng trong cách Enemy phản ứng khi nghi ngờ và tìm kiếm mục tiêu
    private Vector3 _standoffPos; //Biến để lưu trữ vị trí mục tiêu cho động tác đối mặt (standoff) với người chơi khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash, giúp Enemy có thể di chuyển đến đó để tạo sự tương tác và tăng tính chân thực của Enemy khi nghi ngờ và tìm kiếm mục tiêu
    private bool _isStandoff; //Biến để theo dõi xem Enemy có đang trong động tác đối mặt (standoff) với người chơi hay không khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash, giúp kiểm soát logic di chuyển và tìm kiếm mục tiêu trong trạng thái Suspicion khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash
    private bool _hasStandoffPoint;
    private float _standoffMoveTimeout;
    private float _standoffLookUntil;
    private bool _isLookingAtPlayer;

    public EnemyState_Suspicion(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();

        Debug.Log(
            $"{_enemyBase.gameObject.name} mất dấu mục tiêu! Vào trạng thái NGHI NGỜ."
        );

        _isWaiting = false;
        _isStandoff = false;
        _hasStandoffPoint = false;
        _isLookingAtPlayer = false;
        _standoffLookUntil = 0f;
        _standoffMoveTimeout = 0f;

        _searchPos = _enemyBase.Detection.ClampPointToLeash(
            _enemyBase.Detection.LastKnownTargetPosition
        );

        _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed);
        _enemyBase.Locomotion.SetAngularSpeed(120f);
        bool shouldGuardBorder = _enemyBase.Detection.Player != null && !_enemyBase.Detection.IsPlayerInLeashRange() && _enemyBase.Detection.HasLineOfSightTo(_enemyBase.Detection.Player, false);

        if (shouldGuardBorder)
        {
            _enemyBase.Locomotion.StopMoving();
            _enemyBase.Locomotion.SetUpdateRotation(false);
            _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash);
        }
        else
        {
            _enemyBase.Locomotion.SetUpdateRotation(true);
            _enemyBase.Locomotion.MoveToTarget(_searchPos);
            _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash);
        }

        _searchEndTime = Time.time + 15f;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (_enemyBase.Detection.CurrentTarget != null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState); // Chuyển sang trạng thái Chase
            return;
        }

        //Thấy người chơi , nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash, nghi ngờ và tìm kiếm
        if (_enemyBase.Detection.Player != null && _enemyBase.Detection.HasLineOfSightTo(_enemyBase.Detection.Player, false) && !_enemyBase.Detection.IsPlayerInLeashRange())
        {

            //Xoay mặt lườm player 
            Vector3 dirToPlayer = (_enemyBase.Detection.Player.position - _enemyBase.MyTransform.position).normalized;
            dirToPlayer.y = 0; // Giữ nguyên trục Y để tránh nghiêng lên xuống

            _enemyBase.Locomotion.SetAngularSpeed(360f);

            float distanceToPlayer = Vector3.Distance(_enemyBase.MyTransform.position, _enemyBase.Detection.Player.position);

            AttackDataSO rangedAttack = _enemyBase.Combat.ChooseAttack(distanceToPlayer); // Chọn đòn tấn công dựa trên khoảng cách đến người chơi, có thể mở rộng sau này để có nhiều loại tấn công khác nhau và logic chọn đòn tấn công phức tạp hơn

            if (rangedAttack != null && rangedAttack.attackType == AttackType.Ranged)
            {
                _enemyBase.Locomotion.StopMoving(); // Dừng di chuyển để thực hiện đòn tấn công tầm xa, có thể điều chỉnh lại logic này nếu muốn Enemy vẫn di chuyển khi thực hiện đòn tấn công tầm xa để tạo sự đa dạng trong cách Enemy phản ứng khi nghi ngờ và tìm kiếm mục tiêu
                _enemyBase.Combat.PerformAttack(rangedAttack); // Thực hiện đòn tấn công đã chọn nếu nó là đòn tấn công tầm xa, có thể điều chỉnh lại logic này nếu muốn Enemy có thể thực hiện cả đòn tấn công cận chiến và tầm xa khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash để tạo sự đa dạng trong cách Enemy phản ứng khi nghi ngờ và tìm kiếm mục tiêu

                _searchEndTime = Time.time + 15f; // Thiết lập lại thời gian kết thúc của quá trình nghi ngờ và tìm kiếm mục tiêu sau khi thực hiện đòn tấn công để đảm bảo rằng Enemy sẽ tiếp tục nghi ngờ và tìm kiếm mục tiêu lâu hơn trước khi quay về trạng thái mặc định hoặc trạng thái khác nếu không tìm thấy mục tiêu
                return; // Kết thúc logic Update sau khi thực hiện đòn tấn công để tránh xung đột logic di chuyển và tìm kiếm mục tiêu trong cùng một khung hình, có thể điều chỉnh lại logic này nếu muốn Enemy vẫn tiếp tục di chuyển và tìm kiếm mục tiêu sau khi thực hiện đòn tấn công để tạo sự đa dạng trong cách Enemy phản ứng khi nghi ngờ và tìm kiếm mục tiêu
            }

            //Logic lượn lờ 
            if (!_isStandoff)
            {
                _isStandoff = true;
                _hasStandoffPoint = false;
                _standoffLookUntil = 0f;

                int movementHash = _enemyBase.AnimatorController.HasAnimationState(_enemyBase.AnimatorController.WalkHash) ? _enemyBase.AnimatorController.WalkHash : _enemyBase.AnimatorController.ChaseHash;

                _enemyBase.AnimatorController.PlayAnimation(movementHash);
            }

            _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed); //Đi từ từ xung quanh khu vực cuối cùng biết của mục tiêu để tìm kiếm, có thể điều chỉnh lại tốc độ này nếu muốn Enemy di chuyển nhanh hơn hoặc chậm hơn khi nghi ngờ và tìm kiếm mục tiêu

            if (!_hasStandoffPoint)
            {
                CalculateStandoffPoint(dirToPlayer);
                _hasStandoffPoint = true;
                _standoffLookUntil = 0f;
                float distanceToNewPoint = Vector3.Distance(_enemyBase.MyTransform.position, _standoffPos);
                float moveTime = distanceToNewPoint / Mathf.Max(0.1f, _enemyBase.Data.patrolSpeed);
                _standoffMoveTimeout = Time.time + Mathf.Clamp(moveTime + 1.0f, 2.0f, 5.0f);
            }

            float distanceToStandoffPoint = Vector3.Distance(_enemyBase.MyTransform.position, _standoffPos);

            bool hasArrived = distanceToStandoffPoint <= 1.0f;
            bool moveTimedOut = Time.time >= _standoffMoveTimeout;
            bool shouldStopAndLook = hasArrived || moveTimedOut;

            if (!shouldStopAndLook)
            {
                _isLookingAtPlayer = false;
                _enemyBase.Locomotion.SetUpdateRotation(true);
                _enemyBase.Locomotion.MoveToTarget(_standoffPos, 0.8f);
            }
            else
            {
                _enemyBase.Locomotion.StopMoving();
                _enemyBase.Locomotion.SetUpdateRotation(false);

                if (!_isLookingAtPlayer)
                {
                    _isLookingAtPlayer = true;
                    _enemyBase.AnimatorController.PlayAnimation(
                        _enemyBase.AnimatorController.IdleHash
                    );

                    _standoffLookUntil =
                        Time.time + Random.Range(1.2f, 2.0f);
                }

                FacePlayerSoft(180f);

                if (Time.time >= _standoffLookUntil)
                {
                    _isLookingAtPlayer = false;
                    _hasStandoffPoint = false;
                    _standoffLookUntil = 0f;
                    _standoffMoveTimeout = 0f;

                    int movementHash =
                        _enemyBase.AnimatorController.HasAnimationState(_enemyBase.AnimatorController.WalkHash)
                            ? _enemyBase.AnimatorController.WalkHash
                            : _enemyBase.AnimatorController.ChaseHash;

                    _enemyBase.AnimatorController.PlayAnimation(movementHash);
                }
            }

            _searchEndTime = Time.time + 15f; // Thiết lập thời gian kết thúc của quá trình nghi ngờ và tìm kiếm mục tiêu là 15 giây, có thể điều chỉnh lại nếu muốn Enemy tiếp tục nghi ngờ và tìm kiếm mục tiêu lâu hơn hoặc ngắn hơn trước khi quay về trạng thái mặc định hoặc trạng thái khác nếu không tìm thấy mục tiêu
            return;
        }
        else //Người chơi biến mất hoàn toàn vào góc khuất hoặc chạy mất dạng => Nghi ngờ và tìm kiếm
        {
            if (_isStandoff)
            {
                _isStandoff = false;
                _enemyBase.Locomotion.SetAngularSpeed(120f); // Bật lại tốc độ quay về mặc định để Enemy có thể tự động quay về hướng di chuyển khi đi tuần, giúp hành vi đi tuần trông tự nhiên hơn và tránh lỗi Enemy di chuyển đến điểm tuần tra mới nhưng vẫn quay mặt về hướng cũ thay vì hướng về điểm tuần tra mới khi đã hết thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash
                _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash); // Phát animation Chase khi đã hết thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash để tạo hiệu ứng nghi ngờ và tìm kiếm mục tiêu, hoặc có thể tạo một animation riêng cho trạng thái nghi ngờ và tìm kiếm nếu muốn tạo sự khác biệt rõ ràng giữa hai trạng thái này

                _searchEndTime = Time.time + 15f;
                _isWaiting = false;
                PickNewSearchPoint();
            }
        }

        if (Time.time >= _searchEndTime) // Nếu đã hết thời gian nghi ngờ và tìm kiếm mục tiêu thì quay về trạng thái mặc định hoặc trạng thái khác nếu không tìm thấy mục tiêu
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyReturnState); // Quay về trạng thái mặc định, có thể là Idle hoặc Patrol tùy thiết kế của Enemy
            return;
        }

        if (_isWaiting)
        {
            if (Time.time >= _waitTimer)
            {
                _isWaiting = false; // Kết thúc quá trình chờ đợi và bắt đầu di chuyển xung quanh khu vực cuối cùng biết của mục tiêu để tìm kiếm
                PickNewSearchPoint(); // Chọn một điểm mới xung quanh vị trí cuối cùng biết của mục tiêu để di chuyển đến và tiếp tục quá trình nghi ngờ và tìm kiếm mục tiêu
            }
        }
        else
        {
            Vector3 myPos = new Vector3(_enemyBase.MyTransform.position.x, 0, _enemyBase.MyTransform.position.z);
            Vector3 targetPos = new Vector3(_searchPos.x, 0, _searchPos.z);

            if (Vector3.Distance(myPos, targetPos) <= 0.5f) // Nếu đã đến gần vị trí cuối cùng biết của mục tiêu thì bắt đầu quá trình chờ đợi trước khi di chuyển xung quanh khu vực đó để tìm kiếm mục tiêu
            {
                _isWaiting = true;
                _waitTimer = Time.time + Random.Range(2f, 4f); // Thiết lập thời gian chờ đợi là 3 giây, có thể điều chỉnh lại nếu muốn Enemy chờ lâu hơn hoặc ngắn hơn trước khi bắt đầu di chuyển xung quanh khu vực đó để tìm kiếm mục tiêu

                _enemyBase.Locomotion.StopMoving(); // Dừng di chuyển khi bắt đầu quá trình chờ đợi để tìm kiếm mục tiêu
                _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash); // Phát animation Idle khi bắt đầu quá trình chờ đợi để tìm kiếm mục tiêu

                if (_isInvestigatingScene) _isInvestigatingScene = false; // Kết thúc quá trình điều tra hiện trường nghi ngờ nếu đang trong quá trình đó để tránh xung đột logic khi đã hết thời gian nghi ngờ và tìm kiếm mục tiêu ban đầu nhưng vẫn chưa tìm thấy mục tiêu và muốn tiếp tục điều tra xung quanh khu vực đó để tìm kiếm mục tiêu
            }
        }
    }

    private void CalculateStandoffPoint(Vector3 dirToPlayer)
    {
        Vector3 borderAnchor = _enemyBase.Detection.ClampPointToLeash(_enemyBase.Detection.Player.position, 1.5f);

        Vector3 enemyToPlayer = _enemyBase.Detection.Player.position - borderAnchor;

        enemyToPlayer.y = 0f;

        if (enemyToPlayer.sqrMagnitude <= 0.001f)
            enemyToPlayer = dirToPlayer;

        enemyToPlayer.Normalize();

        for (int i = 0; i < 6; i++)
        {
            Vector3 sideDirection = Random.value > 0.5f ? Vector3.Cross(Vector3.up, enemyToPlayer) : Vector3.Cross(enemyToPlayer, Vector3.up);

            float sideWeight = Random.Range(0.6f, 1.0f);
            float backWeight = Random.Range(0.1f, 0.35f);

            Vector3 moveDirection = (sideDirection * sideWeight - enemyToPlayer * backWeight).normalized;
            Vector3 potentialPoint = borderAnchor + moveDirection * Random.Range(1.8f, 3.0f);
            potentialPoint = _enemyBase.Detection.ClampPointToLeash(potentialPoint, 1.2f);
            Vector3 sampledPoint = _enemyBase.Locomotion.GetRandomRoamPosition(potentialPoint, 1f);

            if (IsGoodStandoffPoint(sampledPoint))
            {
                _standoffPos = sampledPoint;
                return;
            }
        }

        // Fallback cuối: nếu không tìm được điểm tốt thì đứng yên và nhìn player.
        _standoffPos = _enemyBase.MyTransform.position;
    }

    private void PickNewSearchPoint()
    {
        _searchPos = _enemyBase.Locomotion.GetRandomRoamPosition(_enemyBase.Detection.LastKnownTargetPosition, 5f);
        _searchPos = _enemyBase.Detection.ClampPointToLeash(_searchPos);
        _enemyBase.Locomotion.SetAngularSpeed(120f);
        _enemyBase.Locomotion.MoveToTarget(_searchPos); // Di chuyển đến điểm mới để tiếp tục tìm kiếm mục tiêu
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash);

    }

    private void FacePlayer()
    {
        Transform player = _enemyBase.Detection.Player;
        if (player == null) return;

        Vector3 dirToPlayer = player.position - _enemyBase.MyTransform.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer.normalized);

        _enemyBase.MyTransform.rotation = Quaternion.RotateTowards(
            _enemyBase.MyTransform.rotation,
            targetRotation,
            540f * Time.deltaTime
        );
    }

    private void FacePlayerSoft(float turnSpeed)
    {
        Transform player = _enemyBase.Detection.Player;
        if (player == null) return;

        Vector3 dirToPlayer = player.position - _enemyBase.MyTransform.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer.normalized);

        _enemyBase.MyTransform.rotation = Quaternion.RotateTowards(
            _enemyBase.MyTransform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private bool IsGoodStandoffPoint(Vector3 point)
    {
        float distanceFromEnemy = Vector3.Distance(
            _enemyBase.MyTransform.position,
            point
        );

        if (distanceFromEnemy < 1.5f)
            return false;

        if (!_enemyBase.Detection.IsPointInLeash(point))
            return false;

        return true;
    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Locomotion.SetUpdateRotation(true); // Bật lại việc tự động xoay mặt của NavMeshAgent khi rời khỏi trạng thái Suspicion để đảm bảo rằng Enemy sẽ tiếp tục xoay mặt theo hướng di chuyển khi đã chuyển sang trạng thái khác, giúp hành vi di chuyển của Enemy trông tự nhiên hơn và tránh lỗi Enemy không xoay mặt theo hướng di chuyển khi đã chuyển sang trạng thái khác
        _enemyBase.Locomotion.StopMoving(); // Dừng di chuyển khi rời khỏi trạng thái Suspicion để tránh lỗi di chuyển không mong muốn khi đã chuyển sang trạng thái khác
    }
}
