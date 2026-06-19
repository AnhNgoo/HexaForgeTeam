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

    [SerializeField] private AttackSelectionMode selectionMode = AttackSelectionMode.Random;
    private AttackDataSO _lastAttack;
    public bool IsPerformingAttack { get; private set; }
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        _attackCooldownTimers = new Dictionary<AttackDataSO, float>(); //Khởi tạo Dictionary để theo dõi thời gian hồi chiêu của từng đòn tấn công
        foreach (var attackData in attackArsenal)
        {
            if (attackData == null) continue;
            _attackCooldownTimers[attackData] = -100f; //Khởi tạo thời gian hồi chiêu ban đầu cho mỗi đòn tấn công, có thể đặt thành một giá trị âm lớn để đảm bảo rằng tất cả các đòn tấn công đều có thể được sử dụng ngay từ đầu
        }
    }

    private EnemyAttackContext CreateAttackContext()
    {
        return new EnemyAttackContext(_enemyBase, currentAttackData, _enemyBase.Detection.CurrentTarget); //Tạo một EnemyAttackContext mới với thông tin về EnemyBase, dữ liệu tấn công hiện tại và mục tiêu hiện tại của Enemy, có thể mở rộng sau này để thêm các thông tin khác như vị trí của attacker, hướng tấn công, v.v.)
    }

    public void PerformAttack(AttackDataSO chosenAttack)
    {
        IsPerformingAttack = true;
        currentAttackData = chosenAttack;
        _attackCooldownTimers[currentAttackData] = Time.time; //Cập nhật thời gian hồi chiêu của đòn tấn công đã chọn, giúp kiểm soát thời gian giữa các đòn tấn công khác nhau

        currentAttackData.skillLogic?.OnAttackStart(CreateAttackContext());

        Debug.Log($"[EnemyCombat] {gameObject.name} dùng attack: {currentAttackData.attackName}, skillLogic: {(currentAttackData.skillLogic != null ? currentAttackData.skillLogic.name : "NULL - fallback")}");

        if (_enemyBase.AnimatorController.Animator != null)
        {
            _enemyBase.AnimatorController.PlayAttackAnimation(currentAttackData); //Gọi hàm chơi animation tấn công từ EnemyAnimatorController

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
                float effectiveCooldown = _enemyBase.MinibossBehaviour != null ? _enemyBase.MinibossBehaviour.ModifyAttackCooldown(attackData.cooldown) : attackData.cooldown;
                bool isCooldownReady = Time.time >= _attackCooldownTimers[attackData] + effectiveCooldown;
                bool isInRange = distanceToPlayer >= attackData.minAttackRange && distanceToPlayer <= attackData.maxAttackRange; //Kiểm tra nếu player nằm trong phạm vi tấn công của đòn tấn công
                if (isCooldownReady && isInRange)
                {
                    availableAttacks.Add(attackData); //Nếu đòn tấn công đã sẵn sàng và player nằm trong phạm vi tấn công, thêm vào danh sách các đòn tấn công có thể sử dụng
                }
            }
        }

        if (availableAttacks.Count == 0)
            return null;

        if (selectionMode == AttackSelectionMode.Random)
        {
            return availableAttacks[
                Random.Range(0, availableAttacks.Count)
            ];
        }

        AttackDataSO bestAttack = null;
        float bestScore = float.MinValue;


        foreach (AttackDataSO attack in availableAttacks)
        {
            float preferredRange =
                (attack.minAttackRange + attack.maxAttackRange) * 0.5f;

            float score =
                -Mathf.Abs(distanceToPlayer - preferredRange);

            if (attack == _lastAttack)
                score -= 3f;

            score += Random.Range(0f, 0.5f);

            if (score > bestScore)
            {
                bestScore = score;
                bestAttack = attack;
            }
        }

        _lastAttack = bestAttack;
        return bestAttack;
    }

    private async UniTaskVoid ReturnToIdleVisualAsync(CancellationToken token)
    {
        //Chờ cho đến khi animation tấn công kết thúc trước khi trả về trạng thái hình ảnh ban đầu, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        bool isCancelled = await UniTask.Delay(System.TimeSpan.FromSeconds(currentAttackData.attackDuration), cancellationToken: token).SuppressCancellationThrow(); //Chờ 0.5 giây trước khi trả về trạng thái hình ảnh ban đầu, có thể điều chỉnh thời gian này tùy thuộc vào thiết kế của animation tấn công

        //Nếu bị hủy bỏ, không cần thực hiện việc trả về trạng thái hình ảnh
        if (isCancelled) return;

        IsPerformingAttack = false;

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

    private void SpawnProjectile()
    {

        Transform target = _enemyBase.Detection.CurrentTarget; //Lấy vị trí của player để làm hướng di chuyển cho projectile
        if (target == null) return; //Nếu không có mục tiêu, không cần tạo ra projectile

        Transform spawnPoint = ResolveProjectileAnchor(currentAttackData);
        GameObject projectileGo = ObjectPooling.Instance.SpawnFromPool(currentAttackData.projectilePoolType, spawnPoint.position, Quaternion.identity); //Tạo ra projectile tại điểm xuất hiện với hướng mặc định
        EnemyProjectile projectileScripts = projectileGo.GetComponent<EnemyProjectile>(); //Lấy component EnemyProjectile từ prefab để thiết lập sát thương và tốc độ
        if (projectileScripts != null)
        {
            float finalDamage = _enemyBase.Data.damage * currentAttackData.damageMultiplier; //Tính toán sát thương cuối cùng của projectile dựa trên sát thương cơ bản của Enemy và hệ số sát thương của đòn tấn công
            Vector3 shootDirection = (target.position + Vector3.up * 0.5f) - spawnPoint.position; //Tính toán hướng bắn từ điểm xuất hiện đến vị trí của player, có thể điều chỉnh thêm Vector3.up để bắn vào phần thân trên của player thay vì chân
            projectileScripts.Launch(_enemyBase, finalDamage, currentAttackData.projectileSpeed, shootDirection, currentAttackData.projectileLifetime); //Gọi hàm Launch của EnemyProjectile để thiết lập sát thương, tốc độ và hướng di chuyển cho projectile
        }
    }

    public void ForceCloseHitbox()
    {
        _enemyBase.HitboxRegistry.DisableAllHitboxes(); //Gọi hàm đóng tất cả hitbox từ EnemyHitboxRegistry để đảm bảo rằng tất cả hitbox sẽ được đóng, tránh lỗi hitbox vẫn mở sau khi animation kết thúc hoặc khi vào trạng thái Stagger hoặc Dead
        CancelVisualTask(); //Hủy bỏ tác vụ trả về trạng thái hình ảnh nếu đang tồn tại để tránh lỗi khi vào trạng thái Stagger hoặc Dead
        IsPerformingAttack = false;
    }

    public void EnableHitbox(EnemyHitboxType type)
    {
        EnemyHitbox hitbox = _enemyBase.HitboxRegistry.GetHitbox(type);
        if (hitbox != null)
            hitbox.EnableHitBox();
    }

    public void DisableHitbox(EnemyHitboxType type)
    {
        EnemyHitbox hitbox = _enemyBase.HitboxRegistry.GetHitbox(type);
        if (hitbox != null)
            hitbox.DisableHitBox();
    }
    public void PlayAttackVFX()
    {
        if (_enemyBase.StateMachine.CurrentState != _enemyBase.StateMachine.EnemyAttackState) return;
        if (currentAttackData == null) return;
        if (currentAttackData.attackVFX == PoolType.None) return;

        Transform anchor = ResolveVFXAnchor(currentAttackData);
        Vector3 position = anchor.position + anchor.TransformDirection(currentAttackData.vfxOffset);
        Quaternion rotation = Quaternion.Euler(currentAttackData.vfxEuler) * anchor.rotation;

        GameObject vfx = ObjectPooling.Instance.SpawnFromPool(currentAttackData.attackVFX, position, rotation);

        if (vfx != null && currentAttackData.vfxScale > 0f)
        {
            vfx.transform.localScale = Vector3.one * currentAttackData.vfxScale;
        }
    }

    public Transform ResolveVFXAnchor(AttackDataSO attackData)
    {
        if (attackData.vfxAnchor == EnemyAttackAnchorType.Hitbox)
        {
            EnemyHitbox hitbox = _enemyBase.HitboxRegistry.GetHitbox(attackData.hitboxType);
            if (hitbox != null)
                return hitbox.transform;
        }

        if (attackData.vfxAnchor == EnemyAttackAnchorType.Target)
        {
            Transform target = _enemyBase.Detection.CurrentTarget;
            if (target != null)
                return target;
        }
        return _enemyBase.AttackAnchors.GetAnchor(attackData.vfxAnchor);
    }

    public Transform ResolveProjectileAnchor(AttackDataSO attackData)
    {
        if (attackData == null) return transform;

        if (attackData.projectileAnchor == EnemyAttackAnchorType.Hitbox)
        {
            EnemyHitbox hitbox = _enemyBase.HitboxRegistry.GetHitbox(attackData.hitboxType);
            if (hitbox != null)
                return hitbox.transform;
        }

        return _enemyBase.AttackAnchors.GetAnchor(attackData.projectileAnchor);
    }

    public void HandleAttackImpactEvent()
    {
        if (_enemyBase.StateMachine.CurrentState != _enemyBase.StateMachine.EnemyAttackState) return;
        if (currentAttackData == null) return;

        Debug.Log($"[EnemyCombat] AttackImpact event: {currentAttackData.attackName}");
        Debug.Log($"[EnemyCombat] Gọi skill impact: {(currentAttackData.skillLogic != null ? currentAttackData.skillLogic.name : "NULL - fallback")}");

        if (currentAttackData.skillLogic != null)
        {
            currentAttackData.skillLogic.OnAttackImpact(CreateAttackContext());
            return;
        }

        if (currentAttackData.attackType == AttackType.Melee)
        {
            EnableHitbox(currentAttackData.hitboxType);
        }
        else if (currentAttackData.attackType == AttackType.Ranged)
        {
            SpawnProjectile();
        }
    }

    public void HandleAttackEndEvent()
    {
        if (_enemyBase.StateMachine.CurrentState != _enemyBase.StateMachine.EnemyAttackState) return;
        if (currentAttackData == null) return;

        if (currentAttackData.skillLogic != null)
        {
            currentAttackData.skillLogic.OnAttackEnd(CreateAttackContext());
        }
        else if (currentAttackData.attackType == AttackType.Melee)
        {
            DisableHitbox(currentAttackData.hitboxType);
        }

        IsPerformingAttack = false;
    }

    public void HandleAttackMovementEvent()
    {
        if (_enemyBase.StateMachine.CurrentState != _enemyBase.StateMachine.EnemyAttackState) return;
        if (currentAttackData == null) return;

        currentAttackData.skillLogic?.OnAttackMovement(CreateAttackContext());
    }
    #endregion
}
