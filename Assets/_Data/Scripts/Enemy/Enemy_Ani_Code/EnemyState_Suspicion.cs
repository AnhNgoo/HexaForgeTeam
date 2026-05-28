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

        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.TurnLeftHash); // Bắt đầu xoay sang trái
        await RotateTransformOverTime(-45f, 1f, token); // Vừa phát animation vừa xoay 45 độ sang trái trong 1 giây
        await UniTask.Delay(1000, cancellationToken: token); // Chờ 1 giây sau khi xoay sang trái

        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.TurnRightHash); // Bắt đầu xoay sang phải
        await RotateTransformOverTime(90f, 1f, token); // Vừa phát animation vừa xoay 90 độ sang phải trong 1 giây
        await UniTask.Delay(1000, cancellationToken: token); // Chờ 1 giây sau khi xoay sang phải

        // Nếu sau 3 giây vẫn chưa tìm thấy mục tiêu, quay về trạng thái mặc định
        _enemyBase.StateMachine.ResetToDefaultState(); // Quay về trạng thái mặc định (ví dụ: Đi tuần)
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
