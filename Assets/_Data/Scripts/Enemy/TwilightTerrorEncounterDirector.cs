using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class TwilightSpawnNode
{
    public PoolType enemyType;
    public Transform spawnPoint;
    public bool isPatroller;
}

public class TwilightTerrorEncounterDirector : LoadComponents
{
    [SerializeField] private int triggerPhase = 1;
    [SerializeField] private PoolType bossPoolType;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private List<TwilightSpawnNode> escortNodes = new();

    [ReadOnly, SerializeField] private List<EnemyBase> aliveEscorts = new();
    [ReadOnly, SerializeField] private EnemyBase boss;

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

        SafeZoneManager.Instance?.ResetSafeZoneAfterBossDead();
    }

    private void OnPhaseCompleted(int phase, Vector3 center)
    {
        if (started || phase != triggerPhase) return;

        started = true;
        SpawnEscorts();
    }

    private void SpawnEscorts()
    {
        aliveEscorts.Clear();

        foreach (TwilightSpawnNode node in escortNodes)
        {
            EnemyBase enemy = SpawnEnemy(node);
            if (enemy == null) continue;

            aliveEscorts.Add(enemy);
            enemy.EventManager.OnDead += OnEscortDead;
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
        if (boss != null || bossSpawnPoint == null) return;

        boss = SpawnEnemy(new TwilightSpawnNode
        {
            enemyType = bossPoolType,
            spawnPoint = bossSpawnPoint
        });

        if (boss != null)
            boss.EventManager.OnDead += OnBossDead;
    }

    private EnemyBase SpawnEnemy(TwilightSpawnNode node)
    {
        if (node == null || node.spawnPoint == null || node.enemyType == PoolType.None)
            return null;

        GameObject obj = ObjectPooling.Instance.SpawnFromPool(
            node.enemyType,
            node.spawnPoint.position,
            node.spawnPoint.rotation
        );

        if (obj == null) return null;

        EnemyBase enemy = obj.GetComponent<EnemyBase>();
        if (enemy == null) return null;

        SpawnNode runtimeNode = new SpawnNode
        {
            enemyType = node.enemyType,
            spawnPoint = node.spawnPoint,
            isPatroller = node.isPatroller,
            savedHealth = -1f
        };

        enemy.InitFromCamp(null, runtimeNode, player);
        return enemy;
    }
}