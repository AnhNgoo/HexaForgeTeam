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

    private async UniTaskVoid WaitAndGiveUp(CancellationToken cancellationToken)
    {
        await UniTask.Delay(3000, cancellationToken: cancellationToken); // Chờ 3 giây

        // Nếu sau 3 giây vẫn chưa tìm thấy mục tiêu, quay về trạng thái mặc định
        _enemyBase.StateMachine.ResetToDefaultState(); // Quay về trạng thái mặc định (ví dụ: Đi tuần)
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
