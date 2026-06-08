using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
public class EnemyBase : LoadComponents, IPoolable
{
    //Configuration Dùng InLineEditor để chỉnh sửa thông số nhanh
    [Header("Configuration")]
    [InlineEditor()]
    [SerializeField] private EnemyData enemyData;

    //Internal Modules Dùng SerializeField để gán component trực tiếp trên editor, không cần phải kéo tay
    [Header("Internal Modules")]
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyHealth _health;
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
    [SerializeField] private EnemyLootDropper _lootDropper;
    [FoldoutGroup("Modules")]
    [SerializeField] private EnemyVFXManager _vfxManager;
    [FoldoutGroup("Modules")]
    [SerializeField] private Collider _mainCollider;
    [FoldoutGroup("Modules")]
    [SerializeField] private Transform _myTransform;
    [FoldoutGroup("Modules")]

    //Định dạng EnemyBase như một đối tượng có thể được quản lý bởi Object Pooling
    private PoolType _poolType = PoolType.Enemy;
    public PoolType PoolType => _poolType;
    //Mở cửa số  để các module khác có thể gọi nhau thông qua EnemyBase mà không cần phải biết đến nhau
    public EnemyData Data => enemyData;
    public EnemyEventManager EventManager => _eventManager;
    public EnemyHealth Health => _health;
    public EnemyCombat Combat => _combat;
    public EnemyDetection Detection => _detection;
    public EnemyStateMachine StateMachine => _stateMachine;
    public EnemyDamageReceiver DamageReceiver => _damageReceiver;
    public EnemyPoiseSystem PoiseSystem => _poiseSystem;
    public EnemyLocomotion Locomotion => _locomotion;
    public EnemyAnimatorController AnimatorController => _animatorController;
    public EnemyLootDropper LootDropper => _lootDropper;
    public EnemyVFXManager VFXManager => _vfxManager;
    public Transform MyTransform => _myTransform;
    public Collider MainCollider => _mainCollider;

    //Reference đến CampSpawner quản lý việc spawn và pool đối tượng này, có thể dùng để gọi phương thức trả Enemy về pool khi Enemy chết hoặc không còn cần thiết nữa
    private CampSpawner _myCamp;
    //Vị trí gốc của Enemy khi được spawn, có thể dùng để reset vị trí của Enemy khi cần thiết
    private Vector3 _spawnOrigin;
    public Vector3 SpawnOrigin => _spawnOrigin;
    //Biến để theo dõi khoảng cách hiện tại đến vị trí xuất hiện ban đầu, có thể dùng để kiểm tra dây xích và quyết định khi nào cần quay về vị trí xuất hiện ban đầu hoặc chuyển sang trạng thái nghi ngờ
    private float _currentLeash;
    public float CurrentLeash => _currentLeash;
    //Hướng gốc của Enemy khi được spawn, có thể dùng để reset hướng của Enemy khi cần thiết
    private Quaternion _spawnRotation;
    public Quaternion SpawnRotation => _spawnRotation;
    //Reference đến SpawnNode quản lý việc spawn và pool đối tượng này, có thể dùng để gọi phương thức trả Enemy về pool khi Enemy chết hoặc không còn cần thiết nữa
    private SpawnNode _myNode;
    //Biến cờ để đảm bảo rằng Enemy chỉ được khởi tạo một lần duy nhất, tránh việc khởi tạo lại nhiều lần
    private bool _isInitialized;
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    public void InitFromCamp(CampSpawner camp, SpawnNode node, Transform playerTransform)
    {
        _myCamp = camp; //Lưu reference đến CampSpawner quản lý việc spawn và pool đối tượng này để sử dụng sau này
        _myNode = node; //Lưu reference đến SpawnNode quản lý việc spawn và pool đối tượng này để sử dụng sau này
        _poolType = node.enemyType; //Đặt PoolType của Enemy dựa trên thiết lập trong SpawnNode để đảm bảo rằng Enemy được trả về đúng pool khi chết hoặc không còn cần thiết nữa

        _spawnOrigin = node.spawnPoint.position; //Lưu vị trí gốc của Enemy khi được spawn từ SpawnNode để sử dụng sau này, có thể dùng để reset vị trí của Enemy khi cần thiết
        _spawnRotation = node.spawnPoint.rotation; //Lưu hướng gốc của Enemy khi được spawn từ SpawnNode để sử dụng sau này, có thể dùng để reset hướng của Enemy khi cần thiết
        _currentLeash = enemyData.maxLeashDistance; //Khởi tạo khoảng cách hiện tại đến vị trí xuất hiện ban đầu bằng khoảng cách dây xích tối đa, có thể dùng để kiểm tra dây xích và quyết định khi nào cần quay về vị trí xuất hiện ban đầu hoặc chuyển sang trạng thái nghi ngờ

        _locomotion.WarpTo(_spawnOrigin); //Đặt vị trí của Enemy về vị trí gốc khi được spawn, có thể dùng để đảm bảo rằng Enemy luôn được spawn tại vị trí mong muốn và tránh việc spawn nhầm vị trí do lỗi hoặc thay đổi trong scene
        _health.LoadSavedHealth(node.savedHealth); //Khôi phục lượng máu đã lưu của Enemy từ SpawnNode để duy trì tính liên tục của trạng thái Enemy giữa các lần spawn, có thể dùng để tạo ra sự đa dạng về trạng thái ban đầu của Enemy khi được spawn lại
        _locomotion.isPatroller = node.isPatroller; //Đặt hành vi đi tuần của Enemy dựa trên thiết lập trong SpawnNode để tạo ra sự đa dạng về hành vi di chuyển của các Enemy khác nhau trong cùng một camp
        _detection.SetPlayerReference(playerTransform); //Đặt reference đến player cho detection để theo dõi vị trí của player

        ActivateAIAfterFrame().Forget();
    }

    public void Initialize()
    {
        if (_isInitialized) return; //Nếu đã được khởi tạo rồi thì không khởi tạo lại để tránh lỗi và đảm bảo rằng Enemy chỉ được khởi tạo một lần duy nhất
        if (enemyData == null)
        {
            Debug.LogError("EnemyData is not assigned on " + gameObject.name);
            return;
        }

        _eventManager.Initialize(this);
        _health.Initialize(this);
        _combat.Initialize(this);
        _detection.Initialize(this);
        _stateMachine.Initialize(this);
        _damageReceiver.Initialize(this);
        _poiseSystem.Initialize(this);
        _locomotion.Initialize(this);
        _animatorController.Initialize(this);
        _lootDropper.Initialize(this);
        _vfxManager.Initialize(this);

        _eventManager.OnDead -= HandleDeathReport; //Đảm bảo không đăng ký trùng lặp sự kiện khi khởi tạo lại nhiều lần
        _eventManager.OnDead += HandleDeathReport; //Đăng ký sự kiện khi Enemy chết để gọi phương thức trả Enemy về pool

        _isInitialized = true;
    }

    private void ResetEnemy()
    {
        _health.ResetHealth();
        _poiseSystem.ResetPoise();
        _detection.ResetDetection();
    }

    public void ExtendLeash(float newDistance)
    {
        if (_currentLeash < newDistance)
        {
            _currentLeash = newDistance;
            Debug.Log($"<color=orange>{gameObject.name} đang hăng máu! Nới lỏng xích lên {_currentLeash}m</color>");
        }
    }
    public void ResetLeash()
    {
        _currentLeash = enemyData.maxLeashDistance;
        Debug.Log($"<color=green>{gameObject.name} đã bình tĩnh trở lại. Dây xích được reset về {_currentLeash}m</color>");
    }

    private async UniTaskVoid ActivateAIAfterFrame()
    {
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate); // Đợi đến cuối frame để đảm bảo rằng tất cả các module đã được khởi tạo và sẵn sàng trước khi kích hoạt AI để tránh lỗi và xung đột logic

        if (gameObject != null && gameObject.activeInHierarchy)
        {
            _stateMachine.ResetToDefaultState(); //Kích hoạt trạng thái mặc định của Enemy sau khi đã đảm bảo rằng tất cả các module đã được khởi tạo và sẵn sàng, có thể điều chỉnh lại thời điểm kích hoạt trạng thái mặc định này nếu muốn Enemy bắt đầu với một trạng thái khác thay vì trạng thái mặc định ngay sau khi được spawn
        }
    }

    private void HandleDeathReport()
    {
        if (_myCamp != null)
        {
            _myCamp.NotifyEnemyDied(_myNode); //Gọi phương thức thông báo Enemy đã chết đến CampSpawner để quản lý việc spawn/despawn
        }
    }

    public void Despawn()
    {
        ObjectPooling.Instance.ReturnToPool(PoolType, gameObject); //Gọi phương thức trả Enemy về pool để tái sử dụng, có thể dùng để kiểm soát việc despawn Enemy và đảm bảo rằng Enemy được trả về pool thay vì bị huỷ
    }

    private void CacheReferences()
    {
        _myTransform = transform;
        _mainCollider = GetComponent<Collider>();

        if (!TryGetComponent(out _health))
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

        if (!TryGetComponent(out _lootDropper))
            Debug.LogError("EnemyLootDropper component is missing on " + gameObject.name);

        if (!TryGetComponent(out _vfxManager))
            Debug.LogError("EnemyVFXManager component is missing on " + gameObject.name);

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
        if (_mainCollider != null) _mainCollider.enabled = true;
        if (_locomotion != null)
        {
            _locomotion.SetAgentActive(true);
            _locomotion.SetAngularSpeed(120f);
            _locomotion.StopMoving(); // Đảm bảo không bị chạy bậy lúc mới đẻ
        }
    }

    public void OnReturnToPool()
    {

        _stateMachine.ChangeState(null); //Đặt trạng thái hiện tại về null để đảm bảo rằng khi Enemy được spawn lại, nó sẽ bắt đầu từ trạng thái mặc định mà không bị ảnh hưởng bởi trạng thái cũ, tránh lỗi và xung đột logic giữa các lần spawn

        //Reset tất cả các module về trạng thái ban đầu để chuẩn bị cho lần spawn tiếp theo, có thể gọi một phương thức ResetEnemy() để gom tất cả các thao tác reset vào một chỗ để dễ quản lý và tránh lỗi
        ResetEnemy();

        if (_myCamp != null) _myCamp.ClearEnemyReference(_myNode, this); //Gọi phương thức để xóa reference đến Enemy này trong SpawnNode của CampSpawner để tránh lỗi khi Enemy được spawn lại bởi một CampSpawner khác và vẫn còn reference đến Enemy cũ

        gameObject.SetActive(false);
        _myCamp = null; //Reset reference đến CampSpawner để chuẩn bị cho lần spawn tiếp theo, tránh việc gọi nhầm phương thức trả về pool khi Enemy đã được spawn lại bởi một CampSpawner khác
        _myNode = null; //Reset reference đến SpawnNode để chuẩn bị cho lần spawn tiếp theo
    }

    #region Debug
    [Button("Test: Đánh 1 đòn (Raw Dmg: 20, Poise Dmg: 30)", ButtonSizes.Large)]
    public void DebugTakeHit()
    {
        _damageReceiver.TakeHit(20f, 30f);
    }
    #endregion
}
