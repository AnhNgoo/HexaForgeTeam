using UnityEngine;
using System.Collections.Generic;

public class EnemyAnimatorController : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    [Header("Animations StringToHash")]
    [SerializeField] private int idleHash;
    [SerializeField] private int chaseHash;
    [SerializeField] private int staggerHash;
    [SerializeField] private int dieHash;
    [SerializeField] private int turnleftHash;
    [SerializeField] private int turnrightHash;
    [SerializeField] private int walkHash;
    [Header("Animation Variants")]
    [SerializeField] private string[] idleVariantStateNames;
    [SerializeField] private string[] dieVariantStateNames;
    [SerializeField] private string[] staggerVariantStateNames;
    [SerializeField] private string[] chaseVariantStateNames;
    [SerializeField] private string[] walkVariantStateNames;

    private int[] _idleVariantHashes;
    private int[] _dieVariantHashes;
    private int[] _staggerVariantHashes;
    private int[] _chaseVariantHashes;
    private int[] _walkVariantHashes;

    #region Getters
    public int IdleHash => idleHash;
    public int ChaseHash => chaseHash;
    public int StaggerHash => staggerHash;
    public int DieHash => dieHash;
    public int TurnLeftHash => turnleftHash;
    public int TurnRightHash => turnrightHash;
    public int WalkHash => walkHash;
    #endregion

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        if (_animator == null) _animator = GetComponent<Animator>();
        idleHash = Animator.StringToHash("Idle");
        chaseHash = Animator.StringToHash("Chase");
        dieHash = Animator.StringToHash("Die");
        staggerHash = Animator.StringToHash("Take Damage");
        turnleftHash = Animator.StringToHash("Turn Left");
        turnrightHash = Animator.StringToHash("Turn Right");
        walkHash = Animator.StringToHash("Walk");

        _idleVariantHashes = BuildVariantHashes(idleVariantStateNames);
        _dieVariantHashes = BuildVariantHashes(dieVariantStateNames);
        _staggerVariantHashes = BuildVariantHashes(staggerVariantStateNames);
        _chaseVariantHashes = BuildVariantHashes(chaseVariantStateNames);
        _walkVariantHashes = BuildVariantHashes(walkVariantStateNames);
    }

    private void OnValidate()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
            {
                Debug.LogError("Animator component is missing on " + gameObject.name);
            }
        }
    }

    private int[] BuildVariantHashes(string[] stateNames)
    {
        if (stateNames == null || stateNames.Length == 0) return null;

        List<int> hashes = new List<int>();

        foreach (var stateName in stateNames)
        {
            if (string.IsNullOrWhiteSpace(stateName)) continue;
            hashes.Add(Animator.StringToHash(stateName));
        }

        return hashes.Count > 0 ? hashes.ToArray() : null;
    }

    private int ResolveAnimationVariant(int animationHash)
    {
        if (animationHash == idleHash)
        {
            return GetRandomHashOrDefault(_idleVariantHashes, idleHash);
        }

        if (animationHash == dieHash)
        {
            return GetRandomHashOrDefault(_dieVariantHashes, dieHash);
        }

        if (animationHash == staggerHash)
        {
            return GetRandomHashOrDefault(_staggerVariantHashes, staggerHash);
        }

        if (animationHash == chaseHash)
        {
            return GetRandomHashOrDefault(_chaseVariantHashes, chaseHash);
        }

        if (animationHash == walkHash)
        {
            return GetRandomHashOrDefault(_walkVariantHashes, walkHash);
        }

        return animationHash; //Không phải animation có biến thể, trả về hash gốc
    }

    private int GetRandomHashOrDefault(int[] variants, int fallbackHash)
    {
        if (variants == null || variants.Length == 0) return fallbackHash; //Nếu không có biến thể nào được thiết lập, trả về hash gốc để tránh lỗi
        ;
        return variants[Random.Range(0, variants.Length)];
    }

    public bool HasAnimationState(int animationHash)
    {
        return _animator != null && _animator.HasState(0, animationHash);
    }

    public void PlayAttackAnimation(AttackDataSO attackData)
    {
        if (_animator != null && attackData != null)
        {
            _animator.CrossFadeInFixedTime(attackData.animationStateName, attackData.transitionDuration);
        }
    }

    public void PlayAnimation(int animationHash, float transitionDuration = 0.1f)
    {
        if (_animator == null) return;

        int finalHash = ResolveAnimationVariant(animationHash);
        _animator.CrossFadeInFixedTime(finalHash, transitionDuration);
    }

    public void AttackImpact()
    {
        _enemyBase.Combat.HandleAttackImpactEvent(); //Gọi hàm mở hitbox từ EnemyCombat để đảm bảo rằng hitbox sẽ được kích hoạt đúng thời điểm, tránh lỗi hitbox không mở khi animation tấn công đang diễn ra
    }

    public void AttackEnd()
    {
        _enemyBase.Combat.HandleAttackEndEvent(); //Gọi hàm đóng hitbox từ EnemyCombat để đảm bảo rằng hitbox sẽ được đóng đúng thời điểm, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
    }

    public void AttackMovement()
    {
        _enemyBase.Combat.HandleAttackMovementEvent(); //Gọi hàm xử lý logic di chuyển đặc biệt của đòn tấn công từ EnemyCombat để đảm bảo rằng logic di chuyển sẽ được thực hiện đúng thời điểm trong animation tấn công
    }
    public void PlayAttackVFX()
    {
        _enemyBase.Combat.PlayAttackVFX(); //Gọi hàm phát hiệu ứng tấn công từ EnemyCombat để đảm bảo rằng hiệu ứng sẽ được kích hoạt đúng thời điểm trong animation tấn công
    }

    private void EnableHitbox(EnemyHitboxType type)
    {
        if (!_enemyBase.Combat.IsPerformingAttack)
            return;

        _enemyBase.Combat.EnableHitbox(type);
    }

    private void DisableHitbox(EnemyHitboxType type)
    {
        _enemyBase.Combat.DisableHitbox(type);
    }

    public void BlockReady()
    {
        _enemyBase.Guard?.OnBlockReady();
    }

    public void ShieldBashImpact()
    {
        _enemyBase.Guard?.OnShieldBashImpact();
    }

    public void ShieldBashEnd()
    {
        _enemyBase.Guard?.OnShieldBashEnd();
    }
}
