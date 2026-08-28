using UnityEngine;

public class EnemyLootDropper : MonoBehaviour
{
    [Header("Boss Reward")]
    [SerializeField] private GameObject _lootPrefab;
    [SerializeField] private LayerMask groundMask;
    [Min(0f)]
    [SerializeField] private float groundOffset = 0.2f;

    private EnemyBase enemyBase;
    private bool isSubscribed;

    // Gọi phương thức này để khởi tạo EnemyLootDropper với EnemyBase cụ thể.
    public void Initialize(EnemyBase owner)
    {
        Unsubscribe();
        enemyBase = owner;
        Subscribe();
    }

    // Đăng ký sự kiện khi đối tượng được kích hoạt và hủy đăng ký khi bị vô hiệu hóa để tránh rò rỉ bộ nhớ hoặc gọi lại không mong muốn.
    private void OnEnable()
    {
        Subscribe();
    }

    // Hủy đăng ký sự kiện khi đối tượng bị vô hiệu hóa để tránh rò rỉ bộ nhớ hoặc gọi lại không mong muốn.
    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (isSubscribed || enemyBase?.EventManager == null)
            return;

        enemyBase.EventManager.OnDead += AwardEnemy;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || enemyBase?.EventManager == null)
            return;

        enemyBase.EventManager.OnDead -= AwardEnemy;
        isSubscribed = false;
    }

    // Phương thức này được gọi khi kẻ thù chết để trao phần thưởng.
    private void AwardEnemy()
    {
        int min = enemyBase.Data.minGoldReward;
        int max = Mathf.Max(min, enemyBase.Data.maxGoldReward);

        GoldManager.Instance?.AddGold(Random.Range(min, max + 1));

        if (!enemyBase.Data.isBoss || _lootPrefab == null || enemyBase.Data.bossRewardTable == null)
        {
            return;
        }

        GameObject loot = Instantiate(_lootPrefab, FindGroundPosition(), Quaternion.identity);

        if (!loot.TryGetComponent(out DormantPowerInteractable dormantPower))
        {
            Debug.LogError("Boss loot prefab thiếu DormantPowerInteractable.");
            Destroy(loot);
            return;
        }

        dormantPower.Initialize(enemyBase.Data.bossRewardTable);
    }

    // Tìm vị trí mặt đất gần nhất để đặt phần thưởng, sử dụng raycast từ trên xuống.
    private Vector3 FindGroundPosition()
    {
        Vector3 origin = enemyBase.MyTransform.position + Vector3.up * 5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 50f, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * groundOffset;
        }

        Debug.LogWarning($"{enemyBase.name}: Không raycast được mặt đất cho boss loot.");

        return enemyBase.MyTransform.position + Vector3.up * groundOffset;
    }
}
