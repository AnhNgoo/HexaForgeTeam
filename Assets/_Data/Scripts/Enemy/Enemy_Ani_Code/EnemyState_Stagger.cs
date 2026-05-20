using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class EnemyState_Stagger : EnemyState
{
    //Tạo 1 công tắc để huỷ task
    private CancellationTokenSource _cts;
    public EnemyState_Stagger(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        // To_Do: Thực hiện các hành động khi vào trạng thái Stagger, ví dụ: phát animation Stagger
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Stagger.");
        _enemyBase.Combat.ForceCloseHitbox(); //Đảm bảo rằng hitbox sẽ được đóng đúng thời điểm khi vào trạng thái Stagger, tránh lỗi hitbox vẫn mở sau khi animation kết thúc

        //Khởi tạo CancellationTokenSource mới mỗi khi vào trạng thái 
        _cts = new CancellationTokenSource();
        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.StaggerHash); // Phát animation Stagger khi vào trạng thái này
        //Tạo ra 1 task chạy ngầm để xử lý logic vỡ trạng thái, ví dụ: đếm thời gian vỡ trạng thái
        HandleStaggerRoutine(_cts.Token).Forget();
    }

    private async UniTaskVoid HandleStaggerRoutine(CancellationToken token)
    {
        //Lấy thời gian vỡ trạng thái từ EnemyData
        float waitTime = _enemyBase.Data.staggerDuration;
        //Dùng unitask.delay để đợi trong khoảng thời gian vỡ trạng thái, có hỗ trợ huỷ bỏ bằng token
        await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime), cancellationToken: token);
        //Sau khi đợi xong, kiểm tra nếu token chưa bị huỷ thì chuyển về trạng thái Idle
        _enemyBase.StateMachine.ResetToDefaultState();
    }

    public override void Exit()
    {
        base.Exit();
        // To_Do: Thực hiện các hành động khi thoát khỏi trạng thái Stagger, ví dụ: dừng animation Stagger
        _enemyBase.PoiseSystem.ResetPoise();

        //Huỷ bỏ task đang chạy khi thoát khỏi trạng thái để tránh lỗi hoặc hành vi không mong muốn
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }
}
