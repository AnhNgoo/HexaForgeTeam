using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyState_Idle : EnemyState
{
    public EnemyState_Idle(EnemyBase enemyBase) : base(enemyBase) { }
    private CancellationTokenSource _idleCts;
    private float _nextTurnTime;
    private bool _isTurning;

    public override void Enter()
    {
        base.Enter();
        _enemyBase.Locomotion.StopMoving(); // Đảm bảo rằng Enemy sẽ dừng lại khi vào trạng thái Idle
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash); // Phát animation Idle khi vào trạng thái này

        _isTurning = false;
        SetNextTurnTime(); // Thiết lập thời gian cho lần xoay đầu tiên

        _idleCts = new CancellationTokenSource();
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Idle.");
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        //Nếu Radar báo có mục tiêu -> Chuyển sang rượt đuổi ngay!
        if (_enemyBase.Detection.CurrentTarget != null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState);
        }

        if (!_isTurning && Time.time >= _nextTurnTime)
        {
            float randomAngle = Random.Range(90f, 180f); // Góc ngẫu nhiên để xoay (90-180 độ)
            if (Random.value > 0.5f) randomAngle = -randomAngle; // Ngẫu nhiên xoay trái hoặc phải

            PerformTurnSync(randomAngle, _idleCts.Token).Forget(); // Xoay ngay lập tức mà không cần đợi
        }
    }

    private async UniTaskVoid PerformTurnSync(float angle, CancellationToken token)
    {
        _isTurning = true;

        // Phát animation xoay tương ứng với hướng xoay
        int turnHash = angle > 0 ? _enemyBase.AnimatorController.TurnRightHash : _enemyBase.AnimatorController.TurnLeftHash;
        _enemyBase.AnimatorController.PlayAnimation(turnHash);

        // Xoay ngay lập tức mà không cần đợi animation hoàn thành
        float duration = 1f; // Thời gian xoay
        Quaternion startRot = _enemyBase.MyTransform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angle, 0);
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            _enemyBase.MyTransform.rotation = Quaternion.Slerp(startRot, endRot, time / duration);

            //Dùng SuppressCancellationThrow để ngắt êm ái nếu trạng thái bị thay đổi trong khi đang xoay
            bool isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled) return; // Nếu bị hủy, thoát khỏi phương thức ngay lập tức
        }

        _enemyBase.MyTransform.rotation = endRot; // Đảm bảo rằng đã xoay chính xác đến góc cuối cùng sau khi hoàn thành

        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash); // Quay lại animation Idle sau khi xoay xong

        SetNextTurnTime(); // Thiết lập thời gian cho lần xoay tiếp theo
        _isTurning = false;
    }

    private void SetNextTurnTime()
    {
        _nextTurnTime = Time.time + Random.Range(3f, 6f); // Thiết lập thời gian ngẫu nhiên cho lần xoay tiếp theo (3-6 giây)
    }

    public override void Exit()
    {
        base.Exit();

        if (_idleCts != null && !_idleCts.IsCancellationRequested)
        {
            _idleCts.Cancel(); // Hủy bỏ bất kỳ tác vụ nào đang chờ nếu trạng thái bị thay đổi
            _idleCts.Dispose();
            _idleCts = null;
        }
    }
}
