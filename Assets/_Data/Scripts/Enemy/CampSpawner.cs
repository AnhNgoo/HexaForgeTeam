using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnNode
{
    [Header("Setup")]
    public PoolType enemyType = PoolType.Enemy; // Loại enemy để spawn, có thể được thiết lập trong Inspector để xác định loại enemy nào sẽ được spawn từ pool
    public Transform spawnPoint; // Điểm spawn của enemy, có thể được thiết lập trong Inspector để xác định vị trí spawn của enemy

    [Header("Behavior")]
    public bool isPatroller; // Công tắc để tắt bật đi tuần, có thể được thiết lập trong Inspector để tạo ra sự đa dạng về hành vi di chuyển của các enemy khác nhau trong cùng một camp

    [Header("Memory Management")]
    public bool isDead = false; // Biến để theo dõi trạng thái sống/chết của enemy, có thể dùng để kiểm soát việc spawn/despawn enemy dựa trên trạng thái này
    public float savedHealth = -1f; // Biến để lưu trữ lượng máu của enemy khi despawn, có thể dùng để khôi phục lượng máu khi enemy được spawn lại

    [HideInInspector] public GameObject spawnedEnemyObject; // Biến để lưu trữ reference đến enemy được spawn, có thể dùng để kiểm soát việc spawn/despawn enemy và truy cập các thành phần của enemy khi cần thiết
    [HideInInspector] public EnemyBase enemyInstance; // Biến để lưu trữ
}

public class CampSpawner : MonoBehaviour
{
    [Header("Camp Settings")]
    public float spawnDistance = 120f; // Khoảng cách player bước vào -> spawn enemy 
    public float despawnDistance = 150f; // Khoảng cách player bước vào ->

    [Header("Army Roster")]
    public List<SpawnNode> enemiesInCamp; // Danh sách các SpawnNode để quản lý việc spawn/despawn nhiều enemy trong cùng một camp, có thể được thiết lập trong Inspector để dễ dàng quản lý và điều chỉnh các enemy trong camp

    private Transform _playerTransform; // Biến để lưu trữ reference đến Transform của player, có thể dùng để kiểm tra khoảng cách giữa player và camp để quyết định khi nào spawn/despawn enemy
    private bool _isCampActive = false; // Biến để theo dõi trạng thái hoạt động của camp, có thể dùng để kiểm soát việc spawn/despawn enemy dựa trên trạng thái này

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Tìm game object có tag "Player" để lấy reference đến player, có thể dùng để kiểm tra khoảng cách giữa player và camp
        if (player != null) _playerTransform = player.transform; // Lưu reference đến Transform của player để sử dụng sau này

        //Bắt đầu kiểm tra khoảng cách giữa player và camp để quyết định khi nào spawn/despawn enemy
        CampCheckRoutine().Forget(); // Sử dụng UniTask để chạy routine kiểm tra khoảng cách một cách hiệu quả mà không cần phải tạo nhiều coroutine hoặc sử dụng Update, có thể dùng để tiết kiệm hiệu năng và đảm bảo rằng việc kiểm tra khoảng cách được thực hiện một cách mượt mà
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
        deadNode.spawnedEnemyObject = null; // Reset reference đến enemy được spawn để chuẩn bị cho lần spawn tiếp theo
        deadNode.enemyInstance = null; // Reset reference đến EnemyBase của enemy được spawn để chuẩn bị cho lần spawn tiếp theo
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnDistance); // Vẽ một hình tròn để hiển thị khoảng cách spawn của camp trong scene view, có thể điều chỉnh màu sắc và kích thước của hình tròn nếu cần thiết
    }
}
