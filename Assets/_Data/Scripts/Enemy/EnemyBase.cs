using UnityEngine;
using Sirenix.OdinInspector;
public class EnemyBase : LoadComponents, IPoolable
{
    //Configuration Dùng InLineEditor để chỉnh sửa thông số nhanh
    [Header("Configuration")]
    [InlineEditor()]
    [SerializeField] public EnemyData enemyData;

    //Internal Modules Dùng SerializeField để gán component trực tiếp trên editor, không cần phải kéo tay
    [Header("Internal Modules")]
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyHealth _heath;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyCombat _combat;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyDetection _detection;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyStateMachine _stateMachine;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyEventManager _eventManager;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyDamageReceiver _damageReceiver;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyPoiseSystem _poiseSystem;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyLocomotion _locomotion;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyAnimatorController _animatorController;
    [FoldoutGroup("Modules")]
    [SerializeField] private Collider _mainCollider;
    [FoldoutGroup("Modules")]
    [SerializeField] private Transform _myTransform;
    [FoldoutGroup("Modules")]

    //Định dạng EnemyBase như một đối tượng có thể được quản lý bởi Object Pooling
    public PoolType PoolType => PoolType.Enemy;
    //Mở cửa số  để các module khác có thể gọi nhau thông qua EnemyBase mà không cần phải biết đến nhau
    public EnemyEventManager EventManager => _eventManager;
    public EnemyHealth Heath => _heath;
    public EnemyCombat Combat => _combat;
    public EnemyDetection Detection => _detection;
    public EnemyStateMachine StateMachine => _stateMachine;
    public EnemyDamageReceiver DamageReceiver => _damageReceiver;
    public EnemyPoiseSystem PoiseSystem => _poiseSystem;
    public EnemyLocomotion Locomotion => _locomotion;
    public EnemyAnimatorController AnimatorController => _animatorController;
    public Transform MyTransform => _myTransform;
    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (enemyData == null)
        {
            Debug.LogError("EnemyData is not assigned on " + gameObject.name);
            return;
        }

        _eventManager.Initialize(this);
        _heath.Initialize(this);
        _combat.Initialize(this);
        _detection.Initialize(this);
        _stateMachine.Initialize(this);
        _damageReceiver.Initialize(this);
        _poiseSystem.Initialize(this);
        _locomotion.Initialize(this);
        _animatorController.Initialize(this);
    }

    private void ResetEnemy()
    {
        //To_Do: Reset tất cả các module về trạng thái ban đầu để chuẩn bị cho lần spawn tiếp theo
        _heath.ResetHealth();
        _poiseSystem.ResetPoise();
        // _combat.ResetCombat();
        _detection.ResetDetection();
        _stateMachine.ResetToDefaultState();
    }

    private void CacheReferences()
    {
        _myTransform = transform;
        _mainCollider = GetComponent<Collider>();

        if (!TryGetComponent(out _heath))
            Debug.LogError("EnemyHealth component is missing on " + gameObject.name);

        if (!TryGetComponent(out _combat))
            Debug.LogError("EnemyCombat component is missing on " + gameObject.name);

        if (!TryGetComponent(out _detection))
            Debug.LogError("EnemyDetection component is missing on " + gameObject.name);

        if (!TryGetComponent(out _stateMachine))
            Debug.LogError("EnemyStateMachine component is missing on " + gameObject.name);

        if (!TryGetComponent(out _eventManager))
            Debug.LogError("EnemyEventManager component is missing on " + gameObject.name);

        if (!TryGetComponent(out _damageReceiver))
            Debug.LogError("EnemyDamageReceiver component is missing on " + gameObject.name);

        if (!TryGetComponent(out _poiseSystem))
            Debug.LogError("EnemyPoiseSystem component is missing on " + gameObject.name);

        if (!TryGetComponent(out _locomotion))
            Debug.LogError("EnemyLocomotion component is missing on " + gameObject.name);

        _animatorController = GetComponentInChildren<EnemyAnimatorController>();
    }

    protected override void LoadComponent()
    {
        CacheReferences();
    }

    protected override void LoadComponentRuntime()
    {
        CacheReferences();
    }

    public void OnSpawnFromPool()
    {
        gameObject.SetActive(true);
    }

    public void OnReturnToPool()
    {
        gameObject.SetActive(false);
        ResetEnemy();
    }

    #region Debug
    [Button("Test: Đánh 1 đòn (Raw Dmg: 20, Poise Dmg: 30)", ButtonSizes.Large)]
    public void DebugTakeHit()
    {
        _damageReceiver.TakeHit(20f, 30f);
    }
    #endregion
}
