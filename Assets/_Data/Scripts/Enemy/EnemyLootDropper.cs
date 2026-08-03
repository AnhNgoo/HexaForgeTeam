using System;
using UnityEngine;

[Serializable]
public class EnemyBossLootEntry
{
    public EnemyBossCategory category;
    public GameObject prefab;
    [Min(1)] public int count = 1;
}

public class EnemyLootDropper : MonoBehaviour
{
    [SerializeField] private GameObject _lootPrefab;
    [SerializeField]
    private EnemyBossLootEntry[] bossLoot = Array.Empty<EnemyBossLootEntry>();

    private EnemyBase _enemyBase;
    private bool _isSubscribed;

    public void Initialize(EnemyBase enemyBase)
    {
        Unsubscribe();
        _enemyBase = enemyBase;
        Subscribe();
    }

    private void OnEnable() => Subscribe();
    private void OnDisable() => Unsubscribe();

    // Đăng ký sự kiện khi Enemy bị tiêu diệt để trao thưởng cho player và thả vật phẩm nếu là boss
    private void Subscribe()
    {
        if (_isSubscribed || _enemyBase?.EventManager == null) return;
        _enemyBase.EventManager.OnDead += AwardEnemy;
        _isSubscribed = true;
    }

    // Hủy đăng ký sự kiện khi Enemy bị tiêu diệt để tránh việc gọi lại nhiều lần hoặc gây lỗi khi Enemy bị hủy
    private void Unsubscribe()
    {
        if (!_isSubscribed || _enemyBase?.EventManager == null) return;
        _enemyBase.EventManager.OnDead -= AwardEnemy;
        _isSubscribed = false;
    }

    // Gọi khi Enemy bị tiêu diệt, để trao thưởng cho player và thả vật phẩm nếu là boss
    private void AwardEnemy()
    {
        int min = _enemyBase.Data.minGoldReward;
        int max = Mathf.Max(min, _enemyBase.Data.maxGoldReward);
        GoldManager.Instance?.AddGold(UnityEngine.Random.Range(min, max + 1));

        if (!_enemyBase.Data.isBoss) return;

        bool matched = false;
        int dropIndex = 0;
        foreach (EnemyBossLootEntry entry in bossLoot)
        {
            if (entry == null || entry.prefab == null ||
                entry.category != _enemyBase.Data.bossCategory)
                continue;

            matched = true;
            for (int i = 0; i < entry.count; i++)
                DropItem(entry.prefab, dropIndex++);
        }

        if (!matched && _lootPrefab != null)
            DropItem(_lootPrefab, 0);
    }

    // Thả vật phẩm tại vị trí của Enemy khi bị tiêu diệt, với một số offset để tránh chồng lên nhau
    private void DropItem(GameObject prefab, int index)
    {
        float angle = index * 137.5f * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.6f;
        GameObject item = Instantiate(
            prefab,
            _enemyBase.MyTransform.position + Vector3.up * 0.5f + offset,
            Quaternion.identity
        );

        if (!item.TryGetComponent(out Rigidbody body)) return;
        body.AddForce((offset.normalized + Vector3.up * 1.5f) * 5f, ForceMode.Impulse);
        body.AddTorque(UnityEngine.Random.insideUnitSphere * 5f, ForceMode.Impulse);
    }
}
