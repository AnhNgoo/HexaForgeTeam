using UnityEngine;
using Sirenix.OdinInspector;
public class EnemyBase : LoadComponents, IPoolable
{
    //Configuration Dùng InLineEditor để chỉnh sửa thông số nhanh
    [Header("Configuration")]
    [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
    [Searchable] public EnemyData enemyData;

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
    [SerializeField] private Collider _mainCollider;
    [FoldoutGroup("Modules")]
    [SerializeField] private Transform _myTransform;
    [FoldoutGroup("Modules")]

    //Định dạng EnemyBase như một đối tượng có thể được quản lý bởi Object Pooling
    public PoolType PoolType => PoolType.Enemy;
    //Mở cửa số EnemyEventManager để các module khác có thể đăng ký sự kiện
    public EnemyEventManager EventManager => _eventManager;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        // if (enemyData == null)
        // {
        //     Debug.LogError("EnemyData is not assigned on " + gameObject.name);
        //     return;
        // }

        _heath.Initialize(this);
        _combat.Initialize(this);
        _detection.Initialize(this);
        _stateMachine.Initialize(this);
        _eventManager.Initialize(this);
    }

    private void ResetEnemy()
    {
        //To_Do: Reset tất cả các module về trạng thái ban đầu để chuẩn bị cho lần spawn tiếp theo
        // _heath.ResetHealth();
        // _combat.ResetCombat();
        // _detection.ResetDetection();
        // _stateMachine.ResetStateMachine();
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


}
