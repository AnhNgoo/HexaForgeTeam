using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EnemyState_Suspicion : EnemyState
{
    private CancellationTokenSource _searchCts;
    private float _searchEndTime; //Biến để theo dõi thời gian kết thúc của quá trình nghi ngờ, giúp kiểm soát thời gian mà Enemy sẽ tiếp tục nghi ngờ và tìm kiếm mục tiêu trước khi quay về trạng thái mặc định hoặc trạng thái khác nếu không tìm thấy mục tiêu
    private bool _isWaiting; //Biến để theo dõi xem Enemy có đang trong quá trình chờ đợi ở vị trí cuối cùng biết của mục tiêu hay không, giúp kiểm soát logic di chuyển và tìm kiếm mục tiêu trong trạng thái Suspicion
    private float _waitTimer; //Biến để theo dõi thời gian đã chờ đợi ở vị trí cuối cùng biết của mục tiêu, giúp kiểm soát thời gian mà Enemy sẽ tiếp tục chờ đợi trước khi bắt đầu di chuyển xung quanh khu vực đó để tìm kiếm mục tiêu
    private Vector3 _searchPos; //Biến để lưu trữ vị trí cuối cùng biết của mục tiêu, giúp Enemy có thể di chuyển đến đó và bắt đầu quá trình nghi ngờ và tìm kiếm mục tiêu một cách chính xác hơn thay vì chỉ dựa vào vị trí hiện tại của mục tiêu nếu mục tiêu đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash

    private bool _isTurning; //Biến để theo dõi xem Enemy có đang trong quá trình quay để tìm kiếm mục tiêu hay không, giúp kiểm soát logic quay và tìm kiếm mục tiêu trong trạng thái Suspicion
    private bool _isInvestigatingScene; //Biến để theo dõi xem Enemy có đang trong quá trình điều tra hiện trường nghi ngờ hay không, giúp kiểm soát logic di chuyển và tìm kiếm mục tiêu trong trạng thái Suspicion khi đã hết thời gian nghi ngờ và tìm kiếm mục tiêu ban đầu nhưng vẫn chưa tìm thấy mục tiêu và muốn tiếp tục điều tra xung quanh khu vực đó để tìm kiếm mục tiêu

    private float _nextStandoffTime; //Biến để theo dõi thời gian tiếp theo mà Enemy có thể thực hiện động tác đối mặt (standoff) với người chơi khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash, giúp tạo sự đa dạng trong cách Enemy phản ứng khi nghi ngờ và tìm kiếm mục tiêu
    private Vector3 _standoffPos; //Biến để lưu trữ vị trí mục tiêu cho động tác đối mặt (standoff) với người chơi khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash, giúp Enemy có thể di chuyển đến đó để tạo sự tương tác và tăng tính chân thực của Enemy khi nghi ngờ và tìm kiếm mục tiêu
    private bool _isStandoff; //Biến để theo dõi xem Enemy có đang trong động tác đối mặt (standoff) với người chơi hay không khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash, giúp kiểm soát logic di chuyển và tìm kiếm mục tiêu trong trạng thái Suspicion khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash

    public EnemyState_Suspicion(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"{_enemyBase.gameObject.name} mất dấu mục tiêu! Vào trạng thái NGHI NGỜ.");
        _enemyBase.Locomotion.MoveToTarget(_enemyBase.Detection.LastKnownTargetPosition); // Di chuyển đến vị trí cuối cùng biết của mục tiêu
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash);

        _isStandoff = false; // Đảm bảo rằng Enemy sẽ không ở trạng thái đối mặt (standoff) khi bắt đầu nghi ngờ và tìm kiếm mục tiêu

        _searchEndTime = Time.time + 15f;

        _searchCts = new CancellationTokenSource();
        TurnTowardsSuspicious(_searchCts.Token).Forget(); // Bắt đầu quá trình quay để tìm kiếm mục tiêu ngay khi vào trạng thái Suspicion, có thể điều chỉnh lại thời gian bắt đầu quay nếu muốn Enemy chờ đợi một chút trước khi bắt đầu quay để tìm kiếm mục tiêu
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
        if (_enemyBase.Detection.Player != null && _enemyBase.Detection.IsTargetVisible(_enemyBase.Detection.Player) && !_enemyBase.Detection.IsPlayerInLeashRange())
        {
            if (_searchCts != null)  //Hủy quá trình quay để tìm kiếm mục tiêu ban đầu nếu đang chạy để tránh xung đột logic khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash và muốn thực hiện động tác đối mặt (standoff) với người chơi
            {
                _searchCts.Cancel();
                _searchCts.Dispose();
                _searchCts = null;
            }
            _isTurning = false;

            //Xoay mặt lườm player 
            Vector3 dirToPlayer = (_enemyBase.Detection.Player.position - _enemyBase.MyTransform.position).normalized;
            dirToPlayer.y = 0; // Giữ nguyên trục Y để tránh nghiêng lên xuống

            _enemyBase.Locomotion.SetAngularSpeed(0); // Tạm thời đặt tốc độ quay về 0 để đảm bảo rằng Enemy sẽ chỉ quay về hướng của người chơi mà không bị ảnh hưởng bởi tốc độ quay mặc định, có thể điều chỉnh lại logic này nếu muốn tạo sự khác biệt giữa các loại Enemy (ví dụ: một số loại Enemy có thể vẫn giữ tốc độ quay mặc định khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash để tạo sự đa dạng trong cách Enemy phản ứng khi nghi ngờ và tìm kiếm mục tiêu)
            _enemyBase.MyTransform.rotation = Quaternion.Slerp(_enemyBase.MyTransform.rotation, Quaternion.LookRotation(dirToPlayer), Time.deltaTime * 5f); // Quay về hướng của người chơi với tốc độ mượt mà, có thể điều chỉnh tốc độ quay nếu cần thiết để tạo hiệu ứng nghi ngờ và tìm kiếm mục tiêu

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
                _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash);
            }

            if (Time.time >= _nextStandoffTime || Vector3.Distance(_enemyBase.MyTransform.position, _standoffPos) < 0.5f)
            {
                CalculateStandoffPoint(dirToPlayer); // Tính toán vị trí mới cho động tác đối mặt (standoff) với người chơi để tạo sự đa dạng trong cách Enemy phản ứng khi nghi ngờ và tìm kiếm mục tiêu
            }

            _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed); //Đi từ từ xung quanh khu vực cuối cùng biết của mục tiêu để tìm kiếm, có thể điều chỉnh lại tốc độ này nếu muốn Enemy di chuyển nhanh hơn hoặc chậm hơn khi nghi ngờ và tìm kiếm mục tiêu
            _enemyBase.Locomotion.MoveToTarget(_standoffPos); //Di chuyển đến vị trí đối mặt (standoff) với người chơi để tạo sự tương tác và tăng tính chân thực của Enemy khi nghi ngờ và tìm kiếm mục tiêu

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

        if (_isTurning) return; // Nếu đang trong quá trình quay để tìm kiếm mục tiêu thì không thực hiện logic di chuyển hoặc tìm kiếm mới để tránh xung đột logic

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
        //Đi ngang qua để vờn mồi 
        Vector3 strafeDirection = (Random.value > 0.5f) ? Vector3.Cross(dirToPlayer, Vector3.up) : Vector3.Cross(dirToPlayer, Vector3.down);
        Vector3 potentialPoint = _enemyBase.MyTransform.position + strafeDirection * 2f; // Tính toán điểm đối mặt (standoff) tiềm năng cách người chơi một khoảng nhất định, có thể điều chỉnh khoảng cách này nếu muốn Enemy di chuyển gần hơn hoặc xa hơn khi thực hiện động tác đối mặt (standoff) với người chơi

        _standoffPos = _enemyBase.Locomotion.GetRandomRoamPosition(potentialPoint, 1f); // Điều chỉnh điểm đối mặt (standoff) tiềm năng để đảm bảo rằng nó nằm trong khu vực có thể di chuyển được và không bị chặn bởi địa hình hoặc vật cản, giúp Enemy có thể di chuyển đến vị trí đối mặt (standoff) với người chơi một cách linh hoạt hơn thay vì chỉ đứng yên tại chỗ khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash
        _nextStandoffTime = Time.time + Random.Range(1.5f, 3f); // Thiết lập thời gian tiếp theo mà Enemy có thể thực hiện động tác đối mặt (standoff) với người chơi là 5 giây, có thể điều chỉnh lại thời gian này nếu muốn Enemy thực hiện động tác đối mặt (standoff) với người chơi thường xuyên hơn hoặc ít hơn khi đã thấy người chơi nhưng người chơi đã chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách leash
    }

    private async UniTaskVoid TurnTowardsSuspicious(CancellationToken token)
    {
        if (_enemyBase == null || _enemyBase.MyTransform == null) return; //Kiểm tra nếu Enemy hoặc Transform của Enemy bị hủy hoặc không tồn tại trước khi thực hiện quá trình quay để tìm kiếm mục tiêu để tránh lỗi NullReferenceException
        Vector3 dirToSound = (_enemyBase.Detection.LastKnownTargetPosition - _enemyBase.MyTransform.position).normalized;
        dirToSound.y = 0; // Giữ nguyên trục Y để tránh nghiêng lên xuống

        if (dirToSound.sqrMagnitude > 0.01f) // Đảm bảo không bị lỗi toán học khi nghe thấy tiếng ở phía dưới chân
        {
            Quaternion targetRotation = Quaternion.LookRotation(dirToSound);
            float angleToSound = Vector3.SignedAngle(_enemyBase.MyTransform.forward, dirToSound, Vector3.up);

            int turnHash = (angleToSound > 0) ? _enemyBase.AnimatorController.TurnRightHash : _enemyBase.AnimatorController.TurnLeftHash;
            _enemyBase.AnimatorController.PlayAnimation(turnHash); // Phát animation quay trái hoặc quay

            float time = 0f; // Biến để theo dõi thời gian đã quay, có thể điều chỉnh lại nếu muốn Enemy quay nhanh hơn hoặc chậm hơn để tìm kiếm mục tiêu
            Quaternion startRot = _enemyBase.MyTransform.rotation;

            while (time < 1f)
            {
                time += Time.deltaTime; // Điều chỉnh tốc độ quay nếu cần thiết

                if (_enemyBase == null || _enemyBase.MyTransform == null) return;

                _enemyBase.MyTransform.rotation = Quaternion.Slerp(startRot, targetRotation, time / 0.5f); // Quay mượt mà về hướng của tiếng động để tìm kiếm mục tiêu

                bool isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token).SuppressCancellationThrow(); // Cho phép hủy bỏ quá trình quay nếu trạng thái bị thay đổi hoặc Enemy tìm thấy mục tiêu trong khi đang quay để tránh lỗi và xung đột logic
                if (isCancelled) return; // Nếu quá trình quay bị hủy bỏ thì dừng ngay lập tức để tránh lỗi và xung đột logic
            }
            if (_enemyBase == null || _enemyBase.MyTransform == null) return;
            _enemyBase.MyTransform.rotation = targetRotation; // Đảm bảo rằng Enemy sẽ quay chính xác về hướng của tiếng động để tìm kiếm mục tiêu sau khi quá trình quay kết thúc
        }

        if (_enemyBase == null || _enemyBase.MyTransform == null) return;
        //Đứng nhìn chằm chằm 1 giây để đánh giá tình hình sau khi quay về hướng của tiếng động để tìm kiếm mục tiêu, có thể điều chỉnh lại thời gian này nếu muốn Enemy đứng nghi ngờ lâu hơn hoặc ngắn hơn trước khi bắt đầu di chuyển xung quanh khu vực đó để tìm kiếm mục tiêu
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash); // Phát animation Idle khi đứng nghi ngờ để tìm kiếm mục tiêu
        bool waitCancelled = await UniTask.Delay(1000, cancellationToken: token).SuppressCancellationThrow(); // Cho phép hủy bỏ quá trình chờ đợi nếu trạng thái bị thay đổi hoặc Enemy tìm thấy mục tiêu trong khi đang chờ đợi để tránh lỗi và xung đột logic
        if (waitCancelled) return; // Nếu quá trình chờ đợi bị hủy bỏ thì dừng ngay lập tức để tránh lỗi và xung đột logic

        if (_enemyBase == null || _enemyBase.MyTransform == null) return;

        Debug.Log($"{_enemyBase.gameObject.name} đã hoàn thành quá trình quay nghi ngờ và đánh giá tình hình, bắt đầu di chuyển xung quanh khu vực cuối cùng biết của mục tiêu để tìm kiếm mục tiêu.");
        _isTurning = false; // Kết thúc quá trình quay để tìm kiếm mục tiêu và bắt đầu di chuyển xung quanh khu vực đó để tìm kiếm mục tiêu

        _searchEndTime = Time.time + 15f; // Thiết lập thời gian kết thúc của quá trình nghi ngờ và tìm kiếm mục tiêu là 15 giây, có thể điều chỉnh lại nếu muốn Enemy tiếp tục nghi ngờ và tìm kiếm mục tiêu lâu hơn hoặc ngắn hơn trước khi quay về trạng thái mặc định hoặc trạng thái khác nếu không tìm thấy mục tiêu
        _isWaiting = false; // Đảm bảo rằng Enemy sẽ không ở trạng thái chờ đợi khi bắt đầu di chuyển xung quanh khu vực cuối cùng biết của mục tiêu để tìm kiếm mục tiêu

        _enemyBase.Locomotion.SetSpeed(_enemyBase.Data.patrolSpeed); //Đi từ từ để tìm kiếm mục tiêu xung quanh khu vực cuối cùng biết của mục tiêu, có thể điều chỉnh lại tốc độ này nếu muốn Enemy di chuyển nhanh hơn hoặc chậm hơn khi đang nghi ngờ và tìm kiếm mục tiêu
        _enemyBase.Locomotion.SetAngularSpeed(120f); // Bật tốc độ xoay mặt tự động của NavMesh lên 120 độ/s

        _searchPos = _enemyBase.Detection.LastKnownTargetPosition; // Điểm đến đầu tiên là HIỆN TRƯỜNG MẤT DẤU
        _enemyBase.Locomotion.MoveToTarget(_searchPos);
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash);
    }

    private void PickNewSearchPoint()
    {
        _searchPos = _enemyBase.Locomotion.GetRandomRoamPosition(_enemyBase.Detection.LastKnownTargetPosition, 5f);
        _enemyBase.Locomotion.SetAngularSpeed(120f);
        _enemyBase.Locomotion.MoveToTarget(_searchPos); // Di chuyển đến điểm mới để tiếp tục tìm kiếm mục tiêu
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash);

    }

    public override void Exit()
    {
        base.Exit();
        _enemyBase.Locomotion.StopMoving(); // Dừng di chuyển khi rời khỏi trạng thái Suspicion để tránh lỗi di chuyển không mong muốn khi đã chuyển sang trạng thái khác

        if (_searchCts != null && !_searchCts.IsCancellationRequested)
        {
            _searchCts.Cancel(); // Hủy bỏ đếm ngược nếu rời khỏi trạng thái này
            _searchCts.Dispose();
            _searchCts = null;
        }
    }
}
