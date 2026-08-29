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

    [Header("Per-Enemy Attack Audio Overrides")]
    [Tooltip("Sound riêng cho đòn Melee của enemy này. Để trống để dùng sound trong AttackDataSO.")]
    [SerializeField] private AudioClip meleeAttackSoundOverride;
    [SerializeField, Range(0f, 1f)] private float meleeAttackSoundVolume = 1f;

    [Tooltip("Sound riêng cho đòn Ranged của enemy này. Để trống để dùng sound trong AttackDataSO.")]
    [SerializeField] private AudioClip rangedAttackSoundOverride;
    [SerializeField, Range(0f, 1f)] private float rangedAttackSoundVolume = 1f;

    private Dictionary<AttackDataSO, float> _attackCooldownTimers; //Dictionary để theo dõi thời gian hồi chiêu của từng đòn tấn công, giúp kiểm soát thời gian giữa các đòn tấn công khác nhau

    private AttackDataSO currentAttackData; //Dữ liệu tấn công, có thể mở rộng sau này để có nhiều loại tấn công khác nhau
    public AttackDataSO CurrentAttackData => currentAttackData; //Cho phép các lớp khác truy cập dữ liệu tấn công hiện tại nhưng không cho phép thay đổi trực tiếp

    public float CurrentAttackDamageMultiplier { get; private set; } = 1f;
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
        return new EnemyAttackContext(_enemyBase, currentAttackData, _enemyBase.Detection.CurrentTarget, CurrentAttackDamageMultiplier); //Tạo một EnemyAttackContext mới với thông tin về EnemyBase, dữ liệu tấn công hiện tại và mục tiêu hiện tại của Enemy, có thể mở rộng sau này để thêm các thông tin khác như vị trí của attacker, hướng tấn công, v.v.)
    }

    public float PerformAttack(AttackDataSO chosenAttack)
    {
        IsPerformingAttack = true;
        currentAttackData = chosenAttack;
        _attackCooldownTimers[currentAttackData] = Time.time;

        EnemyMinibossBehaviour behaviour = _enemyBase.MinibossBehaviour;
        CurrentAttackDamageMultiplier =
            behaviour?.ConsumeNextAttackDamageMultiplier() ?? 1f;

        behaviour?.OnAttackStarted(currentAttackData);
        currentAttackData.skillLogic?.OnAttackStart(CreateAttackContext());

        float attackSpeed = Mathf.Max(
            0.01f,
            behaviour?.ModifyAttackAnimationSpeed(1f) ?? 1f
        );

        float effectiveDuration = currentAttackData.attackDuration / attackSpeed;

        Animator animator = _enemyBase.AnimatorController.Animator;
        if (animator != null)
        {
            animator.speed = attackSpeed;
            _enemyBase.AnimatorController.PlayAttackAnimation(currentAttackData);

            CancelVisualTask();
            _attackCts = new CancellationTokenSource();
            ReturnToIdleVisualAsync(effectiveDuration, _attackCts.Token).Forget();
        }

        return effectiveDuration;
    }

    public AttackDataSO ChooseAttack(float distanceToPlayer)
    {
        AttackDataSO forcedAttack = _enemyBase.MinibossBehaviour?.ChooseForcedAttack(distanceToPlayer);

        if (forcedAttack != null) return forcedAttack;

        List<AttackDataSO> availableAttacks = new List<AttackDataSO>();
        foreach (var attackData in attackArsenal)
        {
            if (attackData == null) continue; //Nếu có phần tử null trong mảng dữ liệu tấn công, bỏ qua để tránh lỗi

            if (attackData.isFollowUpOnly)
                continue;

            if (_attackCooldownTimers.ContainsKey(attackData))
            {
                float effectiveCooldown = _enemyBase.MinibossBehaviour != null ? _enemyBase.MinibossBehaviour.ModifyAttackCooldown(attackData.cooldown, attackData) : attackData.cooldown;
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

    private async UniTaskVoid ReturnToIdleVisualAsync(float duration, CancellationToken token)
    {
        //Chờ cho đến khi animation tấn công kết thúc trước khi trả về trạng thái hình ảnh ban đầu, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        bool isCancelled = await UniTask.Delay(System.TimeSpan.FromSeconds(duration), cancellationToken: token).SuppressCancellationThrow(); //Chờ 0.5 giây trước khi trả về trạng thái hình ảnh ban đầu, có thể điều chỉnh thời gian này tùy thuộc vào thiết kế của animation tấn công

        //Nếu bị hủy bỏ, không cần thực hiện việc trả về trạng thái hình ảnh
        if (isCancelled) return;

        ResetAttackSpeed();
        CurrentAttackDamageMultiplier = 1f;
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
        if (projectileGo == null) return;
        EnemyProjectile projectileScripts = projectileGo.GetComponent<EnemyProjectile>(); //Lấy component EnemyProjectile từ prefab để thiết lập sát thương và tốc độ
        if (projectileScripts != null)
        {
            float finalDamage = _enemyBase.Data.damage * currentAttackData.damageMultiplier * CurrentAttackDamageMultiplier;

            float projectileSpeed = _enemyBase.MinibossBehaviour?.ModifyProjectileSpeed(currentAttackData.projectileSpeed) ?? currentAttackData.projectileSpeed;
            Vector3 shootDirection = (target.position + Vector3.up * 0.5f) - spawnPoint.position; //Tính toán hướng bắn từ điểm xuất hiện đến vị trí của player, có thể điều chỉnh thêm Vector3.up để bắn vào phần thân trên của player thay vì chân
            projectileScripts.Launch(_enemyBase, finalDamage, projectileSpeed, shootDirection, currentAttackData.projectileLifetime); //Gọi hàm Launch của EnemyProjectile để thiết lập sát thương, tốc độ và hướng di chuyển cho projectile
        }
    }

    public void ForceCloseHitbox()
    {
        _enemyBase.HitboxRegistry.DisableAllHitboxes(); //Gọi hàm đóng tất cả hitbox từ EnemyHitboxRegistry để đảm bảo rằng tất cả hitbox sẽ được đóng, tránh lỗi hitbox vẫn mở sau khi animation kết thúc hoặc khi vào trạng thái Stagger hoặc Dead
        CancelVisualTask(); //Hủy bỏ tác vụ trả về trạng thái hình ảnh nếu đang tồn tại để tránh lỗi khi vào trạng thái Stagger hoặc Dead

        ResetAttackSpeed();
        CurrentAttackDamageMultiplier = 1f;
        IsPerformingAttack = false;
    }

    private void ResetAttackSpeed()
    {
        Animator animator = _enemyBase.AnimatorController.Animator;
        if (animator != null) animator.speed = 1f;
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

        PlayCurrentAttackSound();

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

    private void PlayCurrentAttackSound()
    {
        if (currentAttackData == null)
            return;

        AudioManager audioManager = AudioManager.GetOrCreateInstance();
        if (audioManager == null)
            return;

        AudioClip selectedSound = currentAttackData.attackSound;
        float selectedVolume = currentAttackData.attackSoundVolume;

        if (currentAttackData.attackType == AttackType.Melee &&
            meleeAttackSoundOverride != null)
        {
            selectedSound = meleeAttackSoundOverride;
            selectedVolume = meleeAttackSoundVolume;
        }
        else if (currentAttackData.attackType == AttackType.Ranged &&
                 rangedAttackSoundOverride != null)
        {
            selectedSound = rangedAttackSoundOverride;
            selectedVolume = rangedAttackSoundVolume;
        }

        if (selectedSound == null)
            return;

        audioManager.PlaySfx(
            selectedSound,
            selectedVolume
        );
    }

    public void HandleAttackEndEvent()
    {
        if (_enemyBase.StateMachine.CurrentState != _enemyBase.StateMachine.EnemyAttackState) return;

        ResetAttackSpeed();

        if (currentAttackData == null) return;

        if (currentAttackData.skillLogic != null)
        {
            currentAttackData.skillLogic.OnAttackEnd(CreateAttackContext());
        }
        else if (currentAttackData.attackType == AttackType.Melee)
        {
            DisableHitbox(currentAttackData.hitboxType);
        }

        CurrentAttackDamageMultiplier = 1f;
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
