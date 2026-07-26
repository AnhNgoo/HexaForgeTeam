using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class SpawnNode
{
    [FoldoutGroup("Node Setup", true)]
    [HorizontalGroup("Node Setup/SetupRow")]
    [Tooltip("Chọn loại quái vật muốn sinh ra tại điểm này")]
    [HideLabel] // Ẩn nhãn chữ đi để vẽ side-by-side cho gọn
    public PoolType enemyType = PoolType.Enemy; // Loại enemy để spawn, có thể được thiết lập trong Inspector để xác định loại enemy nào sẽ được spawn từ pool
    [FoldoutGroup("Node Setup")]
    [HorizontalGroup("Node Setup/SetupRow")]
    [Tooltip("Kéo điểm Transform rỗng ngoài Scene vào đây")]
    [HideLabel]
    public Transform spawnPoint; // Điểm spawn của enemy, có thể được thiết lập trong Inspector để xác định vị trí spawn của enemy

    [FoldoutGroup("Node Setup")]
    [LabelText("Có Đi Tuần Không?")]
    public bool isPatroller; // Công tắc để tắt bật đi tuần, có thể được thiết lập trong Inspector để tạo ra sự đa dạng về hành vi di chuyển của các enemy khác nhau trong cùng một camp

    [FoldoutGroup("Node Setup")]
    [ReadOnly]
    [LabelText("Đã Bị Tiêu Diệt?")]
    public bool isDead = false; // Biến để theo dõi trạng thái sống/chết của enemy, có thể dùng để kiểm soát việc spawn/despawn enemy dựa trên trạng thái này
    [FoldoutGroup("Node Setup")]
    [ReadOnly]
    [LabelText("Lượng Máu Đã Lưu")]
    public float savedHealth = -1f; // Biến để lưu trữ lượng máu của enemy khi despawn, có thể dùng để khôi phục lượng máu khi enemy được spawn lại

    [HideInInspector] public GameObject spawnedEnemyObject; // Biến để lưu trữ reference đến enemy được spawn, có thể dùng để kiểm soát việc spawn/despawn enemy và truy cập các thành phần của enemy khi cần thiết
    [HideInInspector] public EnemyBase enemyInstance; // Biến để lưu trữ
}

[System.Serializable]
public class EnemyGizmoData
{
    public PoolType enemyType;
    public EnemyData enemyData;
}

public class CampSpawner : MonoBehaviour
{
    [Header("Camp Settings")]
    public float spawnDistance = 120f; // Khoảng cách player bước vào -> spawn enemy 
    public float despawnDistance = 150f; // Khoảng cách player bước ra -> despawn enemy

    [Header("Army Roster")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "enemyType")] //Hiển thị chỉ số Element rực rỡ và lấy tên quái làm tiêu đề hiển thị ngoài danh sách
    public List<SpawnNode> enemiesInCamp; // Danh sách các SpawnNode để quản lý việc spawn/despawn nhiều enemy trong cùng một camp, có thể được thiết lập trong Inspector để dễ dàng quản lý và điều chỉnh các enemy trong camp

    [Header("Quickly Setup Tool")]
    [SerializeField] private Transform spawnPointsParent; //Kéo thả cha chứa các điểm spawn point vào đây
    [SerializeField] private PoolType defaultEnemyType = PoolType.Enemy; //Chọn loại enemy mặc định cho các spawn point khi sử dụng công cụ thiết lập nhanh, có thể được thiết lập trong Inspector để dễ dàng áp dụng loại enemy cho nhiều spawn point cùng một lúc
    [SerializeField] private bool defaultIsPatroller = false; //Chọn hành vi đi tuần mặc định cho các spawn point khi sử dụng công cụ thiết lập nhanh, có thể được thiết lập trong Inspector để dễ dàng áp dụng hành vi cho nhiều spawn point cùng một lúc

    [Button("Tự động setup SpawnNodes từ spawnPointsParent", ButtonSizes.Large)]
    private void AutoSetupSpawnNodes()
    {
        if (spawnPointsParent == null)
        {
            Debug.LogWarning($"<color=yellow>CẢNH BÁO: spawnPointsParent chưa được gán! Vui lòng kéo thả một Transform chứa các điểm spawn vào trường này trước khi sử dụng công cụ thiết lập nhanh.</color>");
            return;
        }

        if (enemiesInCamp == null) enemiesInCamp = new List<SpawnNode>(); // Khởi tạo danh sách SpawnNode nếu chưa có để tránh lỗi khi thêm phần tử vào danh sách

        int newlyAdded = 0; // Biến đếm số lượng SpawnNode mới được thêm vào danh sách để hiển thị thông tin sau khi hoàn thành thiết lập nhanh
        foreach (Transform child in spawnPointsParent)
        {
            bool isAlreadyRegistered = false;
            foreach (var node in enemiesInCamp)
            {
                if (node.spawnPoint == child)
                {
                    isAlreadyRegistered = true;
                    break;
                }
            }
            if (!isAlreadyRegistered)
            {
                SpawnNode newNode = new SpawnNode
                {
                    enemyType = defaultEnemyType,
                    spawnPoint = child,
                    isPatroller = defaultIsPatroller
                };
                enemiesInCamp.Add(newNode);
                newlyAdded++;
            }
        }
        Debug.Log($"<color=green><b>THÀNH CÔNG: Đã tự động tạo và gán {newlyAdded} Spawn Nodes mới dựa trên thư mục {spawnPointsParent.name}!</b></color>");
    }

    [FoldoutGroup("Gizmo Settings", true)]
    [ToggleLeft][SerializeField] private bool showSpawnPointsGizmos = true; // Công tắc để bật/tắt hiển thị gizmo cho spawn points, có thể được thiết lập trong Inspector để dễ dàng kiểm soát việc hiển thị gizmo
    [FoldoutGroup("Gizmo Settings")]
    [SerializeField] private List<EnemyGizmoData> enemyGizmoDataList;
    [FoldoutGroup("Gizmo Settings")]
    [SerializeField] private float spawnPointGizmoHeight = 1f;

    private Transform _playerTransform; // Biến để lưu trữ reference đến Transform của player, có thể dùng để kiểm tra khoảng cách giữa player và camp để quyết định khi nào spawn/despawn enemy
    private bool _isCampActive = false; // Biến để theo dõi trạng thái hoạt động của camp, có thể dùng để kiểm soát việc spawn/despawn enemy dựa trên trạng thái này

    private void Start()
    {
        GetPlayerTransform(); // Lấy reference đến Transform của player

        //Bắt đầu kiểm tra khoảng cách giữa player và camp để quyết định khi nào spawn/despawn enemy
        CampCheckRoutine().Forget(); // Sử dụng UniTask để chạy routine kiểm tra khoảng cách một cách hiệu quả mà không cần phải tạo nhiều coroutine hoặc sử dụng Update, có thể dùng để tiết kiệm hiệu năng và đảm bảo rằng việc kiểm tra khoảng cách được thực hiện một cách mượt mà
        EventManager.Subscribe(GameEvent.OnPlayerSpawned, GetPlayerTransform);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnPlayerSpawned, GetPlayerTransform);
    }

    private void GetPlayerTransform(object data = null)
    {
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _playerTransform = player.transform;
        }
    }

    private async UniTaskVoid CampCheckRoutine()
    {
        while (this != null) //Nếu camp bị huỷ -> dừng vĩnh viễn
        {
            if (_playerTransform != null)
            {
                float distanceToCamp = Vector3.Distance(transform.position, _playerTransform.position); // Tính khoảng cách giữa camp và player để quyết định khi nào spawn/despawn enemy

                if (!_isCampActive && distanceToCamp <= spawnDistance) //Nếu camp chưa active và player bước vào khoảng cách spawn -> spawn enemy
                {
                    await SpawnEnemiesInCamp();
                }
                else if (_isCampActive && distanceToCamp > despawnDistance) //Nếu camp đã active và player bước ra khỏi khoảng cách despawn -> despawn enemy
                {
                    DespawnEnemiesInCamp(); //Cất quân
                }
            }
            await UniTask.Delay(500); //Không check mỗi frame để tiết kiệm hiệu năng, có thể điều chỉnh thời gian delay giữa các lần kiểm tra khoảng cách
        }
    }

    private async UniTask SpawnEnemiesInCamp()
    {
        _isCampActive = true;
        Debug.Log($"color=cyan><b>Player đã bước vào khu vực camp, bắt đầu spawn quân!</b></color>");

        foreach (var node in enemiesInCamp)
        {
            //Bỏ qua nếu lính này đã bị giết trước đó (Dead is Dead)
            if (node.isDead) continue;

            if (node.spawnPoint == null)
            {
                Debug.LogWarning($"<color=yellow>CẢNH BÁO: Quái loại {node.enemyType} trong {gameObject.name} CHƯA CÓ Spawn Point! Đã bị bỏ qua.</color>");
                continue;
            }

            node.spawnedEnemyObject = ObjectPooling.Instance.SpawnFromPool(node.enemyType, node.spawnPoint.position, node.spawnPoint.rotation); // Spawn enemy từ pool tại vị trí của spawn point với rotation mặc định

            if (node.spawnedEnemyObject != null)
            {
                node.enemyInstance = node.spawnedEnemyObject.GetComponent<EnemyBase>(); // Lấy reference đến EnemyBase của enemy được spawn để sử dụng sau này

                node.enemyInstance.InitFromCamp(this, node, _playerTransform); // Khởi tạo enemy với reference đến camp và SpawnNode để thiết lập trạng thái ban đầu của enemy khi được spawn, có thể dùng để quản lý việc spawn/despawn enemy dựa trên SpawnNode và đảm bảo rằng mỗi enemy được khởi tạo với thông tin chính xác từ SpawnNode
            }
            else
            {
                Debug.LogError($"<color=red>LỖI POOLING: Không thể đẻ ra {node.enemyType}! Có thể kho (Max Size) đã cạn hoặc gõ sai Enum.</color>");
            }
            await UniTask.Delay(50); // Delay giữa các lần spawn enemy để tạo hiệu ứng spawn quân không quá dồn dập, có thể điều chỉnh thời gian delay giữa các lần spawn
        }
    }

    private void DespawnEnemiesInCamp()
    {
        _isCampActive = false;
        foreach (var node in enemiesInCamp)
        {
            if (node.spawnedEnemyObject != null)
            {
                if (node.enemyInstance != null && !node.isDead)
                {
                    node.savedHealth = node.enemyInstance.Health.CurrentHealth; // Lưu lượng máu hiện tại của enemy trước khi despawn để khôi phục khi spawn lại, chỉ lưu nếu enemy chưa chết để duy trì tính liên tục của trạng thái enemy giữa các lần spawn
                }

                ObjectPooling.Instance.ReturnToPool(node.enemyType, node.spawnedEnemyObject); // Trả enemy về pool để tái sử dụng, có thể dùng để kiểm soát việc despawn enemy và đảm bảo rằng enemy được trả về pool thay vì bị huỷ
                node.spawnedEnemyObject = null; // Reset reference đến enemy được spawn để chuẩn bị cho lần spawn tiếp theo
                node.enemyInstance = null; // Reset reference đến EnemyBase của enemy được spawn để chuẩn bị cho lần spawn tiếp theo
            }
        }
    }

    public void NotifyEnemyDied(SpawnNode deadNode)
    {
        deadNode.isDead = true;
    }

    public void ClearEnemyReference(SpawnNode node, EnemyBase enemy)
    {
        if (node == null) return;
        if (node.enemyInstance != enemy) return;

        node.spawnedEnemyObject = null;
        node.enemyInstance = null;
    }

    private EnemyData GetEnemyDataForType(PoolType type)
    {
        if (enemyGizmoDataList == null) return null;

        foreach (var data in enemyGizmoDataList)
        {
            if (data.enemyType == type)
            {
                return data.enemyData;
            }
        }
        return null; // Trả về null nếu không tìm thấy dữ liệu phù hợp, có thể dùng để xử lý lỗi hoặc thiết lập mặc định khi không có dữ liệu
    }

    private void OnDrawGizmos()
    {
        if (!showSpawnPointsGizmos) return; // Kiểm tra công tắc hiển thị gizmo trước khi vẽ để tiết kiệm hiệu năng và tránh làm rối mắt nếu không muốn hiển thị gizmo

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnDistance); // Vẽ một hình tròn để hiển thị khoảng cách spawn của camp trong scene view, có thể điều chỉnh màu sắc và kích thước của hình tròn nếu cần thiết

        if (enemiesInCamp == null) return;

        foreach (var node in enemiesInCamp)
        {
            if (node == null || node.spawnPoint == null) continue;

            EnemyData Data = GetEnemyDataForType(node.enemyType); // Lấy dữ liệu EnemyData tương ứng với loại enemy của SpawnNode để hiển thị thông tin trong gizmo, có thể dùng để cung cấp thông tin chi tiết về enemy khi vẽ gizmo

            float viewRange = Data != null ? Data.detectRange : 5f; // Sử dụng viewRange từ EnemyData nếu có, nếu không có thì sử dụng giá trị mặc định, có thể điều chỉnh giá trị mặc định này nếu muốn hiển thị phạm vi nhìn của enemy một cách rõ ràng hơn trong gizmo
            float povRange = Data != null ? Data.povAngle : 90f; // Sử dụng povRange từ EnemyData nếu có, nếu không có thì sử dụng giá trị mặc định, có thể điều chỉnh giá trị mặc định này nếu muốn hiển thị góc nhìn của enemy một cách rõ ràng hơn trong gizmo
            float leash = Data != null ? Data.maxLeashDistance : 20f; // Sử dụng leash từ EnemyData nếu có, nếu không có thì sử dụng giá trị mặc định, có thể điều chỉnh giá trị mặc định này nếu muốn hiển thị phạm vi leash của enemy một cách rõ ràng hơn trong gizmo

            Transform point = node.spawnPoint;
            Vector3 gizmoPos = point.position + Vector3.forward * spawnPointGizmoHeight; // Vẽ gizmo ở một vị trí cao hơn một chút so với spawn point để dễ nhìn hơn, có thể điều chỉnh chiều cao này nếu cần thiết

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gizmoPos, 0.5f); // Vẽ một hình tròn nhỏ để đánh dấu vị trí của spawn point, có thể điều chỉnh màu sắc và kích thước của hình tròn nếu cần thiết

            Gizmos.color = Color.black;
            Gizmos.DrawLine(point.position, gizmoPos); // Vẽ một đường thẳng từ spawn point đến gizmo để tạo sự liên kết trực quan giữa spawn point và thông tin được hiển thị trong gizmo

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(gizmoPos, viewRange); // Vẽ một hình tròn để hiển thị phạm vi nhìn của enemy, có thể điều chỉnh màu sắc và kích thước của hình tròn nếu cần thiết

            Vector3 leftDir = Quaternion.AngleAxis(-povRange / 2f, Vector3.up) * point.forward; // Tính toán hướng bên trái của góc nhìn dựa trên povRange để vẽ góc nhìn của enemy, có thể điều chỉnh cách tính toán này nếu muốn tạo sự khác biệt giữa các loại enemy
            Vector3 rightDir = Quaternion.AngleAxis(povRange / 2f, Vector3.up) * point.forward; // Tính toán hướng bên phải của góc nhìn dựa trên povRange để vẽ góc nhìn của enemy, có thể điều chỉnh cách tính toán này nếu muốn tạo sự khác biệt giữa các loại enemy

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(gizmoPos, gizmoPos + leftDir * viewRange); // Vẽ một đường thẳng để hiển thị góc nhìn bên trái của enemy
            Gizmos.DrawLine(gizmoPos, gizmoPos + rightDir * viewRange); // Vẽ một đường thẳng để hiển thị góc nhìn bên phải của enemy

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(gizmoPos, leash); // Vẽ một hình tròn để hiển thị phạm vi leash của enemy, có thể điều chỉnh màu sắc và kích thước của hình tròn nếu cần thiết
        }
    }
}
