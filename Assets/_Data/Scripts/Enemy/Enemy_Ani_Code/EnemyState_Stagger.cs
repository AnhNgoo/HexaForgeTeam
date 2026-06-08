using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class EnemyState_Stagger : EnemyState
{
    private float _staggerEndTime; //Biến để lưu thời điểm kết thúc stagger, có thể dùng để tính toán logic đặc biệt nếu bị đánh trúng trong khi đang stagger
    private bool _isRecovering; //Biến để theo dõi xem có đang trong quá trình hồi phục sau khi bị đánh trúng trong khi đang stagger hay không, có thể dùng để điều chỉnh logic khi bị đánh trúng trong trạng thái stagger (ví dụ như không cho bị stagger liên tiếp nếu đang trong quá trình hồi phục)
    public EnemyState_Stagger(EnemyBase enemyBase) : base(enemyBase) { }

    public override void Enter()
    {
        base.Enter();
        // To_Do: Thực hiện các hành động khi vào trạng thái Stagger, ví dụ: phát animation Stagger
        Debug.Log($"{_enemyBase.gameObject.name} đã vào trạng thái Stagger.");

        _isRecovering = false; //Khi mới vào trạng thái Stagger thì chưa có gì để hồi phục, nên đặt biến này về false

        _enemyBase.Locomotion.StopMoving(); //Dừng di chuyển ngay khi vào trạng thái Stagger để đảm bảo rằng enemy sẽ không tiếp tục di chuyển trong khi đang bị stagger
        _enemyBase.Combat.ForceCloseHitbox(); //Đảm bảo rằng hitbox sẽ được đóng đúng thời điểm khi vào trạng thái Stagger, tránh lỗi hitbox vẫn mở sau khi animation kết thúc

        _staggerEndTime = Time.time + _enemyBase.Data.staggerDuration; //Tính toán thời điểm kết thúc stagger dựa trên thời gian stagger được định nghĩa trong EnemyData, có thể dùng để điều chỉnh logic đặc biệt nếu bị đánh trúng trong khi đang stagger

        _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.StaggerHash); // Phát animation Stagger khi vào trạng thái này
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (_isRecovering) return;

        // 4. Liên tục so khớp thời gian thực tế để đánh giá hồi phục tỉnh táo
        if (Time.time >= _staggerEndTime)
        {
            _isRecovering = true;
            Debug.Log($"{_enemyBase.gameObject.name} đã hồi phục hoàn toàn sau đơ, bật chế độ phản đòn!");

            if (_enemyBase.Detection.CurrentTarget != null)
            {
                _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemyChaseState);
            }
            else
            {
                _enemyBase.StateMachine.ChangeState(_enemyBase.StateMachine.EnemySuspicionState);
            }
        }
    }

    public void OnHitDuringStagger()
    {
        if (_isRecovering) return;

        // Rút bớt thời gian đơ
        _staggerEndTime -= 0.5f;

        // Ngưỡng bảo vệ hoạt ảnh tối thiểu (0.2 giây) để quái không bị lỗi nháy/giật animation khi bị chém quá nhanh
        float minStaggerLimit = Time.time + 0.2f;
        if (_staggerEndTime < minStaggerLimit)
        {
            _staggerEndTime = minStaggerLimit;
        }

        Debug.Log($"<color=orange>{_enemyBase.gameObject.name} bị chém bồi! Co ngắn thời gian đơ còn {_staggerEndTime - Time.time:F2}s!</color>");
    }

    public override void Exit()
    {
        base.Exit();
        // To_Do: Thực hiện các hành động khi thoát khỏi trạng thái Stagger, ví dụ: dừng animation Stagger
        _enemyBase.PoiseSystem.ResetPoise();
    }
}
