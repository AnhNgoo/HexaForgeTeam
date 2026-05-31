using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

public class EnemyState_Suspicion : EnemyState
{
    private CancellationTokenSource _searchCts;
    public EnemyState_Suspicion(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"{_enemyBase.gameObject.name} mất dấu mục tiêu! Vào trạng thái NGHI NGỜ.");
        _enemyBase.Locomotion.MoveToTarget(_enemyBase.Detection.LastKnownTargetPosition); // Di chuyển đến vị trí cuối cùng biết của mục tiêu
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.ChaseHash);

        _searchCts = new CancellationTokenSource();
        WaitAndGiveUp(_searchCts.Token).Forget(); // Bắt đầu đếm ngược để từ bỏ tìm kiếm nếu không tìm thấy mục tiêu sau 3 giây
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (_enemyBase.Detection.CurrentTarget != null)
        {
            _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState); // Chuyển sang trạng thái Chase
        }
    }

    private async UniTaskVoid WaitAndGiveUp(CancellationToken token)
    {
        _enemyBase.Locomotion.StopMoving(); // Dừng lại khi tới nơi cuối cùng biết của mục tiêu

        Vector3 dirToSound = (_enemyBase.Detection.LastKnownTargetPosition - _enemyBase.MyTransform.position).normalized; //Hướng từ Enemy đến vị trí cuối cùng biết của mục tiêu
        dirToSound.y = 0; //Giữ nguyên trục Y để tránh nghiêng lên xuống
        Quaternion targetRotion = Quaternion.LookRotation(dirToSound); //Hướng cần quay về để nhìn về phía vị trí cuối cùng biết của mục tiêu

        float angleToSound = Vector3.SignedAngle(_enemyBase.MyTransform.forward, dirToSound, Vector3.up); // Góc giữa hướng hiện tại của Enemy và hướng đến vị trí cuối cùng biết của mục tiêu, dùng SignedAngle để biết được hướng quay (trái hay phải)
        int turnHash = angleToSound > 0 ? _enemyBase.AnimatorController.TurnRightHash : _enemyBase.AnimatorController.TurnLeftHash; // Chọn animation xoay tương ứng với hướng cần quay
        _enemyBase.AnimatorController.PlayAnimation(turnHash); // Phát animation xoay tương ứng với hướng cần quay

        float time = 0f;
        Quaternion startRot = _enemyBase.MyTransform.rotation;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            _enemyBase.MyTransform.rotation = Quaternion.Slerp(startRot, targetRotion, time / 0.5f); // Quay dần dần về hướng vị trí cuối cùng biết của mục tiêu trong 0.5 giây
            bool isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token).SuppressCancellationThrow(); // Chờ đến frame tiếp theo để tiếp tục quay, đồng thời kiểm tra nếu trạng thái bị thay đổi trong khi đang quay để ngắt êm ái
            if (isCancelled) return; // Nếu trạng thái bị thay đổi trong khi đang quay thì ngắt êm ái để tránh lỗi
        }

        _enemyBase.MyTransform.rotation = targetRotion; // Đảm bảo rằng đã quay chính xác về hướng vị trí cuối cùng biết của mục tiêu sau khi hoàn thành
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash); // Chuyển về animation Idle sau khi quay xong

        bool isWaitCancelled = await UniTask.Delay(2500, cancellationToken: token).SuppressCancellationThrow(); // Đợi 2.5 giây trước khi từ bỏ tìm kiếm, đồng thời kiểm tra nếu trạng thái bị thay đổi trong khi đang đợi để ngắt êm ái
        if (isWaitCancelled) return; // Nếu trạng thái bị thay đổi trong khi đang đợi thì ngắt êm ái để tránh lỗi

        _enemyBase.StateMachine.ResetToDefaultState(); // Từ bỏ tìm kiếm và trở về trạng thái mặc định (có thể là Idle hoặc Patrol tùy thuộc vào thiết kế của Enemy) nếu không tìm thấy mục tiêu sau 3 giây
    }

    private async UniTask RotateTransformOverTime(float angle, float duration, CancellationToken token)
    {
        Quaternion startRot = _enemyBase.MyTransform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angle, 0);
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime; // Cộng dồn thời gian đã trôi qua
            //Xoay dần dần từ startRot đến endRot trong khoảng thời gian duration
            _enemyBase.MyTransform.rotation = Quaternion.Slerp(startRot, endRot, time / duration);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token); // Chờ đến frame tiếp theo để tiếp tục xoay
        }
        _enemyBase.MyTransform.rotation = endRot; // Đảm bảo rằng đã xoay chính xác đến góc cuối cùng sau khi hoàn thành
    }


    public override void Exit()
    {
        base.Exit();

        if (_searchCts != null && !_searchCts.IsCancellationRequested)
        {
            _searchCts.Cancel(); // Hủy bỏ đếm ngược nếu rời khỏi trạng thái này
            _searchCts.Dispose();
            _searchCts = null;
        }
    }
}
