using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class TwilightEscortSpawnData
{
    public PoolType enemyType;

    [Min(1)]
    public int count = 1;
}

[System.Serializable]
public class TwilightBossSpawnData
{
    public PoolType bossPoolType;
    public List<TwilightEscortSpawnData> escortGroups = new();
}

public class TwilightTerrorEncounterDirector : LoadComponents
{
    [SerializeField] private int triggerPhase = 1;
    [SerializeField] private List<TwilightBossSpawnData> bossPool = new();

    [ReadOnly, SerializeField] private List<EnemyBase> aliveEscorts = new();
    [ReadOnly, SerializeField] private List<PoolType> usedBosses = new();
    [ReadOnly, SerializeField] private EnemyBase boss;

    private TwilightTerrorSpawnPointGroup currentSpawnGroup;
    private TwilightBossSpawnData currentBossData;

    private Transform player;
    private bool started;

    private void Start()
    {
        LoadComponentRuntime();

        if (SafeZoneManager.Instance != null)
        {
            SafeZoneManager.Instance.OnSafeZonePhaseCompleted -= OnPhaseCompleted;
            SafeZoneManager.Instance.OnSafeZonePhaseCompleted += OnPhaseCompleted;
        }
    }

    protected override void LoadComponent()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    private void OnEnable()
    {
        if (SafeZoneManager.Instance != null)
            SafeZoneManager.Instance.OnSafeZonePhaseCompleted += OnPhaseCompleted;
    }

    private void OnDisable()
    {
        if (SafeZoneManager.Instance != null)
            SafeZoneManager.Instance.OnSafeZonePhaseCompleted -= OnPhaseCompleted;

        foreach (EnemyBase escort in aliveEscorts)
            if (escort != null) escort.EventManager.OnDead -= OnEscortDead;
    }

    private void OnBossDead()
    {
        if (boss != null)
            boss.EventManager.OnDead -= OnBossDead;

        boss = null;
        currentSpawnGroup = null;
        currentBossData = null;

        if (!HasUnusedBoss())
        {
            Debug.Log("[Twilight] Đã hoàn thành toàn bộ boss phase.");
            return;
        }

        started = false;
        SafeZoneManager.Instance?.ResetSafeZoneAfterBossDead();
    }

    private bool HasUnusedBoss()
    {
        foreach (TwilightBossSpawnData data in bossPool)
        {
            if (data == null || data.bossPoolType == PoolType.None)
                continue;

            if (!usedBosses.Contains(data.bossPoolType))
                return true;
        }

        return false;
    }

    private void OnPhaseCompleted(int phase, Transform targetPoint)
    {
        if (started || phase != triggerPhase) return;

        currentSpawnGroup = targetPoint.GetComponent<TwilightTerrorSpawnPointGroup>();
        if (currentSpawnGroup == null)
        {
            Debug.LogWarning($"[Twilight] {targetPoint.name} thiếu TwilightTerrorSpawnPointGroup.");
            return;
        }

        started = true;
        SpawnEscorts();
    }

    private void SpawnEscorts()
    {
        aliveEscorts.Clear();

        currentBossData = PickBossData();
        if (currentBossData == null)
        {
            Debug.LogWarning("[Twilight] Không còn boss nào để spawn.");
            return;
        }

        if (currentBossData.escortGroups.Count == 0)
        {
            SpawnBoss();
            return;
        }

        IReadOnlyList<Transform> points = currentSpawnGroup.MinionSpawnPoints;

        if (points.Count == 0)
        {
            Debug.LogWarning("[Twilight] Không có Minion Spawn Point.");
            return;
        }

        int pointIndex = Random.Range(0, points.Count);

        foreach (TwilightEscortSpawnData group in currentBossData.escortGroups)
        {
            if (group == null || group.enemyType == PoolType.None)
                continue;

            for (int i = 0; i < group.count; i++)
            {
                Transform spawnPoint = points[pointIndex % points.Count];
                pointIndex++;

                EnemyBase enemy = SpawnEnemy(
                    group.enemyType,
                    spawnPoint,
                    true
                );

                if (enemy == null)
                    continue;

                aliveEscorts.Add(enemy);
                enemy.EventManager.OnDead += OnEscortDead;
            }
        }

        if (aliveEscorts.Count == 0)
            SpawnBoss();
    }
    private void OnEscortDead()
    {
        for (int i = aliveEscorts.Count - 1; i >= 0; i--)
        {
            if (aliveEscorts[i] != null && aliveEscorts[i].Health.CurrentHealth > 0f)
                continue;

            if (aliveEscorts[i] != null)
                aliveEscorts[i].EventManager.OnDead -= OnEscortDead;

            aliveEscorts.RemoveAt(i);
        }

        if (aliveEscorts.Count == 0)
            SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (boss != null || currentBossData == null || currentSpawnGroup == null)
            return;

        boss = SpawnEnemy(
            currentBossData.bossPoolType,
            currentSpawnGroup.BossSpawnPoint,
            false
        );

        if (boss == null)
            return;

        usedBosses.Add(currentBossData.bossPoolType);
        boss.EventManager.OnDead += OnBossDead;
    }

    private EnemyBase SpawnEnemy(PoolType poolType, Transform spawnPoint, bool isPatroller)
    {
        if (poolType == PoolType.None || spawnPoint == null)
            return null;

        GameObject obj = ObjectPooling.Instance.SpawnFromPool(
            poolType,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (obj == null) return null;

        EnemyBase enemy = obj.GetComponent<EnemyBase>();
        if (enemy == null) return null;

        SpawnNode runtimeNode = new SpawnNode
        {
            enemyType = poolType,
            spawnPoint = spawnPoint,
            isPatroller = isPatroller,
            savedHealth = -1f
        };

        enemy.InitFromCamp(null, runtimeNode, player);
        return enemy;
    }

    private TwilightBossSpawnData PickBossData()
    {
        List<TwilightBossSpawnData> available = new();

        foreach (TwilightBossSpawnData data in bossPool)
        {
            if (data == null || data.bossPoolType == PoolType.None) continue;
            if (usedBosses.Contains(data.bossPoolType)) continue;

            available.Add(data);
        }

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }
}