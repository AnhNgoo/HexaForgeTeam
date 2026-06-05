using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Threading;
using Cysharp.Threading.Tasks.CompilerServices;
using System.Collections.Generic;

public class EnemyCombat : MonoBehaviour
{
    private EnemyBase _enemyBase;
    private CancellationTokenSource _attackCts; // CancellationTokenSource để quản lý việc hủy bỏ các tác vụ tấn công nếu cần thiết, ví dụ khi vào trạng thái Stagger hoặc Dead
    [Header("Combat Settings")]
    [InlineEditor()]
    [SerializeField] private AttackDataSO[] attackArsenal; //Mảng dữ liệu tấn công, có thể dùng để lưu trữ nhiều loại tấn công khác nhau và chọn ngẫu nhiên hoặc theo thứ tự khi tấn công
    public AttackDataSO[] AttackArsenal => attackArsenal; //Cho phép các lớp khác truy cập mảng dữ liệu tấn công nhưng không cho phép thay đổi trực tiếp

    private Dictionary<AttackDataSO, float> _attackCooldownTimers; //Dictionary để theo dõi thời gian hồi chiêu của từng đòn tấn công, giúp kiểm soát thời gian giữa các đòn tấn công khác nhau

    private AttackDataSO currentAttackData; //Dữ liệu tấn công, có thể mở rộng sau này để có nhiều loại tấn công khác nhau
    public AttackDataSO CurrentAttackData => currentAttackData; //Cho phép các lớp khác truy cập dữ liệu tấn công hiện tại nhưng không cho phép thay đổi trực tiếp
    [Header("Hitbox Settings")]
    [SerializeField] private EnemyHitbox _weaponHitbox; //Tham chiếu đến hitbox của Enemy, có thể gán trực tiếp trên editor
    [Header("Range Setup")]
    [SerializeField] private Transform _projectileSpawnPoint; //Điểm xuất hiện của projectile, có thể gán trực tiếp trên editor
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        _attackCooldownTimers = new Dictionary<AttackDataSO, float>(); //Khởi tạo Dictionary để theo dõi thời gian hồi chiêu của từng đòn tấn công
        foreach (var attackData in attackArsenal)
        {
            _attackCooldownTimers[attackData] = -100f; //Khởi tạo thời gian hồi chiêu ban đầu cho mỗi đòn tấn công, có thể đặt thành một giá trị âm lớn để đảm bảo rằng tất cả các đòn tấn công đều có thể được sử dụng ngay từ đầu
        }
        Debug.Log($"{gameObject.name} - EnemyCombat đã được khởi tạo!");

        if (_weaponHitbox != null)
        {
            _weaponHitbox.Initialize(_enemyBase);
        }
    }

    public void PerformAttack(AttackDataSO chosenAttack)
    {
        currentAttackData = chosenAttack;
        _attackCooldownTimers[currentAttackData] = Time.time; //Cập nhật thời gian hồi chiêu của đòn tấn công đã chọn, giúp kiểm soát thời gian giữa các đòn tấn công khác nhau

        Debug.Log($"{gameObject.name} Xài chiêu {currentAttackData.attackName}!");

        if (_enemyBase.AnimatorController.Animator != null)
        {
            _enemyBase.AnimatorController.PlayAttackAnimation(currentAttackData); //Gọi hàm chơi animation tấn công từ EnemyAnimatorController

            if (currentAttackData.missVFX != PoolType.None)
            {
                if (currentAttackData.attackType == AttackType.Melee)
                {
                    // CẬN CHIẾN: Phụt hiệu ứng xé gió ngay phía trước mặt quái
                    Vector3 spawnPos = _enemyBase.MyTransform.position + _enemyBase.MyTransform.forward * 1f;
                    ObjectPooling.Instance.SpawnFromPool(currentAttackData.missVFX, spawnPos, _enemyBase.MyTransform.rotation);
                }
                else if (currentAttackData.attackType == AttackType.Ranged)
                {
                    // BẮN XA: Phụt hiệu ứng lóe sáng (Muzzle Flash) ngay tại đầu nòng súng/mồm quái khi chuẩn bị nhả đạn!
                    if (_projectileSpawnPoint != null)
                    {
                        ObjectPooling.Instance.SpawnFromPool(currentAttackData.missVFX, _projectileSpawnPoint.position, _projectileSpawnPoint.rotation);
                    }
                }
            }
            _attackCts = new CancellationTokenSource(); //Tạo mới CancellationTokenSource cho tác vụ tấn công hiện tại
            ReturnToIdleVisualAsync(_attackCts.Token).Forget(); //Bắt đầu tác vụ trả về trạng thái hình ảnh sau khi tấn công, có thể điều chỉnh thời gian chờ trong hàm này tùy thuộc vào thiết kế của animation tấn công
        }
    }

    public AttackDataSO ChooseAttack(float distanceToPlayer)
    {
        List<AttackDataSO> availableAttacks = new List<AttackDataSO>();
        foreach (var attackData in attackArsenal)
        {
            if (attackData == null) continue; //Nếu có phần tử null trong mảng dữ liệu tấn công, bỏ qua để tránh lỗi

            if (_attackCooldownTimers.ContainsKey(attackData))
            {
                bool isCooldownReady = Time.time >= _attackCooldownTimers[attackData] + attackData.cooldown; //Kiểm tra nếu đòn tấn công đã sẵn sàng để sử dụng dựa trên thời gian hồi chiêu
                bool isInRange = distanceToPlayer >= attackData.minAttackRange && distanceToPlayer <= attackData.maxAttackRange; //Kiểm tra nếu player nằm trong phạm vi tấn công của đòn tấn công
                if (isCooldownReady && isInRange)
                {
                    availableAttacks.Add(attackData); //Nếu đòn tấn công đã sẵn sàng và player nằm trong phạm vi tấn công, thêm vào danh sách các đòn tấn công có thể sử dụng
                }
            }
        }

        if (availableAttacks.Count > 0)
        {
            return availableAttacks[Random.Range(0, availableAttacks.Count)]; //Chọn ngẫu nhiên một đòn tấn công từ danh sách các đòn tấn công có thể sử dụng
        }

        return null;
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
        if (currentAttackData == null) return; //Nếu không có đòn tấn công nào được chọn, không cần mở hitbox
        if (currentAttackData.attackType == AttackType.Melee) //Chỉ mở hitbox nếu đòn tấn công là cận chiến, tránh lỗi mở hitbox cho đòn tấn công tầm xa
        {
            if (_weaponHitbox != null) _weaponHitbox.EnableHitBox(); //Gọi hàm mở hitbox từ EnemyHitbox để đảm bảo rằng hitbox sẽ được mở đúng thời điểm trong animation tấn công
        }
        else if (currentAttackData.attackType == AttackType.Ranged)//Nêu đòn tấn công tầm xa gọi hàm SpawnProjectile để tạo ra projectile, tránh lỗi mở hitbox cho đòn tấn công tầm xa
        {
            SpawnProjectile();
        }
    }

    private void SpawnProjectile()
    {
        if (currentAttackData.projectilePrefab == null || _projectileSpawnPoint == null) return; //Nếu không có prefab projectile hoặc điểm xuất hiện được gán, không thể tạo ra projectile

        Transform target = _enemyBase.Detection.CurrentTarget; //Lấy vị trí của player để làm hướng di chuyển cho projectile
        if (target == null) return; //Nếu không có mục tiêu, không cần tạo ra projectile

        GameObject projectileGo = Instantiate(currentAttackData.projectilePrefab, _projectileSpawnPoint.position, Quaternion.identity); //Tạo ra projectile tại điểm xuất hiện với hướng mặc định
        EnemyProjectile projectileScripts = projectileGo.GetComponent<EnemyProjectile>(); //Lấy component EnemyProjectile từ prefab để thiết lập sát thương và tốc độ
        if (projectileScripts != null)
        {
            float finalDamage = _enemyBase.Data.damage * currentAttackData.damageMultiplier; //Tính toán sát thương cuối cùng của projectile dựa trên sát thương cơ bản của Enemy và hệ số sát thương của đòn tấn công
            Vector3 shootDirection = (target.position + Vector3.up * 0.5f) - _projectileSpawnPoint.position; //Tính toán hướng bắn từ điểm xuất hiện đến vị trí của player, có thể điều chỉnh thêm Vector3.up để bắn vào phần thân trên của player thay vì chân
            projectileScripts.Launch(_enemyBase, finalDamage, currentAttackData.projectileSpeed, shootDirection, currentAttackData.hitVFX); //Gọi hàm Launch của EnemyProjectile để thiết lập sát thương, tốc độ và hướng di chuyển cho projectile
        }
    }

    public void CloseHitbox()
    {
        if (currentAttackData != null && currentAttackData.attackType == AttackType.Melee) //Chỉ đóng hitbox nếu đòn tấn công là cận chiến, tránh lỗi đóng hitbox cho đòn tấn công tầm xa
        {
            if (_weaponHitbox != null) _weaponHitbox.DisableHitBox(); //Gọi hàm đóng hitbox từ EnemyHitbox để đảm bảo rằng hitbox sẽ được đóng đúng thời điểm, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        }
    }
    public void ForceCloseHitbox()
    {
        if (currentAttackData != null && currentAttackData.attackType == AttackType.Melee) //Chỉ đóng hitbox nếu đòn tấn công là cận chiến, tránh lỗi đóng hitbox cho đòn tấn công tầm xa
            if (_weaponHitbox != null) _weaponHitbox.DisableHitBox(); //Gọi hàm đóng hitbox từ EnemyHitbox để đảm bảo rằng hitbox sẽ được đóng đúng thời điểm, tránh lỗi hitbox vẫn mở sau khi animation kết thúc, dùng trong trường hợp cần thiết như khi vào trạng thái Stagger
        CancelVisualTask(); //Hủy bỏ tác vụ trả về trạng thái hình ảnh nếu đang tồn tại để tránh lỗi khi vào trạng thái Stagger hoặc Dead
        Debug.Log($"{gameObject.name} đã bị ép đóng hitbox!");
    }
    #endregion
}
