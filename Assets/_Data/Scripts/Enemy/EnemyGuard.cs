using UnityEngine;

public enum EnemyGuardResult
{
    NotBlocked,
    Blocked,
    Broken
}

public class EnemyGuard : MonoBehaviour
{
    [Header("Guard")]
    [SerializeField, Range(0f, 180f)] private float guardAngle = 120f; //Góc mà Enemy có thể block được, có thể dùng để kiểm tra nếu đòn tấn công đến từ phía trước và nằm trong góc này thì có thể block
    [SerializeField, Range(0f, 1f)] private float damageReduction = 0.85f; //Hệ số giảm sát thương khi block thành công, có thể dùng để điều chỉnh lượng sát thương bị giảm khi block, ví dụ 0.85f có nghĩa là chỉ nhận 85% sát thương gốc khi block thành công
    [SerializeField] private float maxGuardPoise = 80f; //Lượng poise tối đa mà Enemy có thể sử dụng để block, có thể dùng để kiểm soát khả năng block của Enemy, khi poise giảm xuống 0 thì block bị phá vỡ
    [SerializeField] private float guardCooldown = 5f; //Thời gian hồi chiêu của khả năng block, có thể dùng để kiểm soát tần suất mà Enemy có thể block, sau khi block xong sẽ không thể block lại ngay lập tức mà phải chờ đến khi cooldown kết thúc
    [SerializeField] private Vector2 guardDuration = new Vector2(1.2f, 2.2f); //Khoảng thời gian mà Enemy sẽ duy trì trạng thái block, có thể dùng để tạo sự đa dạng trong hành vi block của Enemy, mỗi lần block sẽ có thời gian ngẫu nhiên trong khoảng này
    [SerializeField, Range(0f, 1f)] private float enterGuardChance = 0.4f; //Xác suất để Enemy bắt đầu block khi bị tấn công, có thể dùng để điều chỉnh mức độ phòng thủ của Enemy
    [SerializeField] private float decisionInterval = 1f; //Khoảng thời gian giữa các lần ra quyết định block, có thể dùng để kiểm soát tần suất mà Enemy có thể block

    [Header("Shield Bash")]
    [SerializeField] private EnemyHitboxType shieldHitboxType = EnemyHitboxType.Shield;
    [SerializeField] private float bashRange = 1.4f; //Khoảng cách tối đa mà Enemy có thể thực hiện đòn bash sau khi block thành công, có thể dùng để kiểm tra nếu player nằm trong phạm vi này thì Enemy sẽ thực hiện đòn bash
    [SerializeField] private float minimumGuardBeforeBash = 0.4f; //Tỷ lệ phần trăm guard poise tối thiểu phải có để Enemy thực hiện đòn bash sau khi block thành công, có thể dùng để đảm bảo rằng Enemy chỉ thực hiện đòn bash khi còn đủ poise sau khi block
    [SerializeField] private float bashDuration = 0.7f; //Thời gian mà Enemy sẽ duy trì trạng thái bash sau khi block thành công, có thể dùng để tạo sự đa dạng trong hành vi tấn công của Enemy sau khi block, mỗi lần block thành công sẽ có thời gian bash khác nhau trong khoảng này

    [Header("Animation")]
    [SerializeField] private string blockEnterState = "Melee_Block";
    [SerializeField] private string blockingState = "Melee_Blocking";
    [SerializeField] private string blockAttackState = "Melee_Block_Attack";
    [SerializeField] private string blockHitState = "Melee_Block_Hit";
    [SerializeField] private float raiseDurationFallback = 0.35f; //Thời gian dự phòng để chuyển sang trạng thái raise khi block, có thể dùng để đảm bảo rằng Enemy sẽ chuyển sang trạng thái raise sau khi block thành công nếu vì lý do nào đó mà animation không tự động chuyển sau khi block
    [SerializeField] private float blockHitDuration = 0.3f; //Thời gian mà Enemy sẽ duy trì trạng thái block hit sau khi block thành công và bị tấn công lại, có thể dùng để tạo sự đa dạng trong hành vi của Enemy khi bị tấn công lại trong khi đang block, mỗi lần bị tấn công lại sẽ có thời gian block hit khác nhau trong khoảng này

    private EnemyBase _enemyBase;
    private float _guardPoise;
    private float _cooldownEndTime;
    private float _nextDecisionTime;
    private float _guardStartTime;
    private float _guardReadyFallbackTime;
    private float _blockHitEndTime;
    private float _bashEndFallbackTime;

    public bool IsGuarding { get; private set; } //Trạng thái đang block, có thể dùng để kiểm tra nếu Enemy đang block để xử lý logic khác nhau, ví dụ chỉ cho phép block hit hoặc shield bash khi đang ở trạng thái này
    public bool IsGuardActive { get; private set; } //Trạng thái đang thực hiện block sau khi đã vào animation block, có thể dùng để kiểm tra nếu Enemy đang trong trạng thái này để xử lý logic khác nhau, ví dụ chỉ cho phép block hit hoặc shield bash khi đang ở trạng thái này
    public bool IsBashing { get; private set; } //Trạng thái đang thực hiện đòn bash sau khi block thành công, có thể dùng để kiểm tra nếu Enemy đang trong trạng thái này để xử lý logic khác nhau, ví dụ không cho phép block thêm khi đang bash hoặc thay đổi hành vi của Enemy khi đang bash

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
    }

    private void Update()
    {
        if (!IsGuarding) return;

        if (!IsGuardActive && !IsBashing && Time.time >= _guardReadyFallbackTime)
        {
            OnBlockReady();
        }

        if (_blockHitEndTime > 0f && Time.time >= _blockHitEndTime)
        {
            _blockHitEndTime = 0f;
            ReturnToBlocking();
        }

        if (IsBashing && Time.time >= _bashEndFallbackTime)
            OnShieldBashEnd();
    }

    public bool ShouldEnterGuard(float distanceToPlayer)
    {
        if (IsGuarding || Time.time < _cooldownEndTime) return false;
        if (Time.time < _nextDecisionTime) return false;
        if (distanceToPlayer < 1.2f || distanceToPlayer > 5f) return false;

        _nextDecisionTime = Time.time + decisionInterval;
        return Random.value <= enterGuardChance;
    }

    public float GetGuardDuration()
    {
        return Random.Range(guardDuration.x, guardDuration.y);
    }

    public void BeginGuard()
    {
        _guardPoise = 0f;
        IsGuarding = true;
        IsGuardActive = false;
        IsBashing = false;

        _guardStartTime = Time.time;
        _guardReadyFallbackTime = Time.time + raiseDurationFallback;

        _enemyBase.Locomotion.StopMoving();
        PlayState(blockEnterState);
    }

    public void OnBlockReady()
    {
        if (!IsGuarding || IsBashing) return;

        IsGuardActive = true;
        PlayState(blockingState);
    }

    public EnemyGuardResult TryBlock(
        float rawDamage,
        float poiseDamage,
        Transform attacker,
        out float damageAfterBlock)
    {
        damageAfterBlock = rawDamage;

        if (!IsGuarding || !IsGuardActive || IsBashing)
            return EnemyGuardResult.NotBlocked;

        if (attacker == null)
            return EnemyGuardResult.NotBlocked;

        Vector3 toAttacker = attacker.position - transform.position;
        toAttacker.y = 0f;

        if (toAttacker.sqrMagnitude <= 0.001f)
            return EnemyGuardResult.NotBlocked;

        float angle = Vector3.Angle(
            transform.forward,
            toAttacker.normalized
        );

        if (angle > guardAngle * 0.5f)
            return EnemyGuardResult.NotBlocked;

        damageAfterBlock = rawDamage * (1f - damageReduction);
        _guardPoise += poiseDamage;

        if (_guardPoise >= maxGuardPoise)
        {
            IsGuardActive = false;
            IsGuarding = false;
            return EnemyGuardResult.Broken;
        }

        return EnemyGuardResult.Blocked;
    }

    public void PlayBlockHit()
    {
        if (!IsGuarding || IsBashing) return;

        PlayState(blockHitState);
        _blockHitEndTime = Time.time + blockHitDuration;
    }

    private void ReturnToBlocking()
    {
        if (!IsGuarding || IsBashing) return;
        PlayState(blockingState);
    }

    public bool CanShieldBash(float distanceToPlayer)
    {
        return IsGuarding &&
               IsGuardActive &&
               !IsBashing &&
               distanceToPlayer <= bashRange &&
               Time.time >= _guardStartTime + minimumGuardBeforeBash;
    }

    public void StartShieldBash()
    {
        if (!IsGuarding || IsBashing) return;

        IsBashing = true;
        IsGuardActive = false;
        _bashEndFallbackTime = Time.time + bashDuration;

        PlayState(blockAttackState);
    }

    public void OnShieldBashImpact()
    {
        if (!IsBashing) return;
        _enemyBase.Combat.EnableHitbox(shieldHitboxType);
    }

    public void OnShieldBashEnd()
    {
        _enemyBase.Combat.DisableHitbox(shieldHitboxType);

        if (!IsGuarding) return;

        IsBashing = false;
        IsGuardActive = true;
        PlayState(blockingState);
    }

    public void EndGuard()
    {
        _enemyBase.Combat.DisableHitbox(shieldHitboxType);

        IsGuarding = false;
        IsGuardActive = false;
        IsBashing = false;

        _guardPoise = 0f;
        _blockHitEndTime = 0f;
        _cooldownEndTime = Time.time + guardCooldown;
    }

    private void PlayState(string stateName)
    {
        Animator animator = _enemyBase.AnimatorController.Animator;
        if (animator != null)
            animator.CrossFadeInFixedTime(stateName, 0.1f);
    }
}
