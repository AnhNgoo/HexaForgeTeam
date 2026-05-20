using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Threading;
using Cysharp.Threading.Tasks.CompilerServices;

public class EnemyCombat : MonoBehaviour
{
    private EnemyBase _enemyBase;
    private CancellationTokenSource _attackCts; // CancellationTokenSource để quản lý việc hủy bỏ các tác vụ tấn công nếu cần thiết, ví dụ khi vào trạng thái Stagger hoặc Dead
    [Header("Combat Settings")]
    [InlineEditor()]
    [SerializeField] private AttackDataSO currentAttackData; //Dữ liệu tấn công, có thể mở rộng sau này để có nhiều loại tấn công khác nhau
    public AttackDataSO CurrentAttackData => currentAttackData; //Cho phép các lớp khác truy cập dữ liệu tấn công hiện tại nhưng không cho phép thay đổi trực tiếp
    [Header("Hitbox Settings")]
    [SerializeField] private EnemyHitbox _weaponHitbox; //Tham chiếu đến hitbox của Enemy, có thể gán trực tiếp trên editor
    private float lastAttackTime; //Thời gian của lần tấn công cuối cùng, dùng để kiểm soát thời gian giữa các đòn tấn công
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        lastAttackTime = Time.time;
        Debug.Log($"{gameObject.name} - EnemyCombat đã được khởi tạo!");

        if (_weaponHitbox != null)
        {
            _weaponHitbox.Initialize(_enemyBase);
        }
    }

    //Hàm tấn công mục tiêu
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + _enemyBase.Data.attackCooldown; //Kiểm tra nếu đã đủ thời gian giữa các đòn tấn công
    }

    public void PerformAttack()
    {
        if (currentAttackData == null)
        {
            Debug.LogWarning($"{gameObject.name} không có dữ liệu tấn công để thực hiện đòn tấn công!");
            return;
        }
        lastAttackTime = Time.time; //Cập nhật thời gian của lần tấn công cuối cùng
        Debug.Log($"{gameObject.name} đã vung vũ khí chém player với sát thương {_enemyBase.Data.damage}!");
        if (_enemyBase.AnimatorController.Animator != null)
        {
            _enemyBase.AnimatorController.PlayAttackAnimation(currentAttackData); //Gọi hàm chơi animation tấn công từ EnemyAnimatorController
            _attackCts = new CancellationTokenSource(); //Tạo mới CancellationTokenSource cho tác vụ tấn công hiện tại
            ReturnToIdleVisualAsync(_attackCts.Token).Forget(); //Bắt đầu tác vụ trả về trạng thái hình ảnh sau khi tấn công, có thể điều chỉnh thời gian chờ trong hàm này tùy thuộc vào thiết kế của animation tấn công
        }
    }

    private async UniTaskVoid ReturnToIdleVisualAsync(CancellationToken token)
    {
        //Chờ cho đến khi animation tấn công kết thúc trước khi trả về trạng thái hình ảnh ban đầu, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        bool isCancelled = await UniTask.Delay(System.TimeSpan.FromSeconds(currentAttackData.attackDuration), cancellationToken: token).SuppressCancellationThrow(); //Chờ 0.5 giây trước khi trả về trạng thái hình ảnh ban đầu, có thể điều chỉnh thời gian này tùy thuộc vào thiết kế của animation tấn công

        //Nếu bị hủy bỏ, không cần thực hiện việc trả về trạng thái hình ảnh
        if (isCancelled) return;

        // Nếu sống sót qua Delay và quái vẫn đang ở trạng thái Attack
        if (_enemyBase.StateMachine.CurrentState == _enemyBase.StateMachine.EnemyAttackState)
        {
            _enemyBase.AnimatorController.PlayAnimation(_enemyBase.AnimatorController.IdleHash); //Trả về trạng thái hình ảnh ban đầu (Idle) sau khi animation tấn công kết thúc
        }
    }

    private void CancelVisualTask()
    {
        if (_attackCts != null && !_attackCts.IsCancellationRequested)
        {
            _attackCts.Cancel(); //Hủy bỏ tác vụ trả về trạng thái hình ảnh nếu đang tồn tại, ví dụ khi vào trạng thái Stagger hoặc Dead
            _attackCts.Dispose();
            _attackCts = null;
        }
    }

    #region Animation Controller
    public void OpenHitbox()
    {
        if (_weaponHitbox != null) _weaponHitbox.EnableHitBox(); //Gọi hàm mở hitbox từ EnemyHitbox để đảm bảo rằng hitbox sẽ được kích hoạt đúng thời điểm, tránh lỗi hitbox không mở khi animation tấn công đang diễn ra
    }

    public void CloseHitbox()
    {
        if (_weaponHitbox != null) _weaponHitbox.DisableHitBox(); //Gọi hàm đóng hitbox từ EnemyHitbox để đảm bảo rằng hitbox sẽ được đóng đúng thời điểm, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
    }
    public void ForceCloseHitbox()
    {
        if (_weaponHitbox != null) _weaponHitbox.DisableHitBox(); //Gọi hàm đóng hitbox từ EnemyHitbox để đảm bảo rằng hitbox sẽ được đóng đúng thời điểm, tránh lỗi hitbox vẫn mở sau khi animation kết thúc, dùng trong trường hợp cần thiết như khi vào trạng thái Stagger
        CancelVisualTask(); //Hủy bỏ tác vụ trả về trạng thái hình ảnh nếu đang tồn tại để tránh lỗi khi vào trạng thái Stagger hoặc Dead
        Debug.Log($"{gameObject.name} đã bị ép đóng hitbox!");
    }
    #endregion
}
