using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class FinalBossEncounterDirector : MonoBehaviour
{
    [SerializeField] private TwilightTerrorEncounterDirector twilightDirector;
    [SerializeField] private FinalBossArena arena;
    [SerializeField] private PoolType fallbackFinalBossPool = PoolType.EnemyEarthshakerBoss;
    [SerializeField] private bool startOnSceneLoad;

    private EnemyBase _boss;
    private bool _started;

    public event Action OnFinalBossDefeated;

    private void Start()
    {
        if (startOnSceneLoad)
            StartEncounter();
    }

    private void OnEnable()
    {
        if (twilightDirector != null)
            twilightDirector.OnAllTwilightTerrorsDefeated += StartEncounter;
    }

    private void OnDisable()
    {
        if (twilightDirector != null)
            twilightDirector.OnAllTwilightTerrorsDefeated -= StartEncounter;

        if (_boss != null)
            _boss.EventManager.OnDead -= HandleBossDead;
    }

    [Button("Debug: Start Final Boss")]
    public void StartEncounter()
    {
        if (_started || arena == null)
            return;

        if (arena.PlayerSpawnPoint == null || arena.BossSpawnPoint == null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        Transform player = playerObject.transform;
        TeleportPlayer(player, arena.PlayerSpawnPoint);
        arena.SetLocked(true);

        PoolType type = RunManager.ResolveSelectedFinalBossPool(fallbackFinalBossPool);
        Debug.Log($"[FinalBoss] Spawn boss đã chọn: {type} ({(int)type})");

        if (type == PoolType.None)
        {
            Debug.LogError("Chưa chọn Final Boss Pool.");
            arena.SetLocked(false);
            return;
        }

        Transform point = arena.BossSpawnPoint;
        if (ObjectPooling.Instance == null)
        {
            Debug.LogError("[FinalBoss] Không tìm thấy ObjectPooling.");
            return;
        }
        GameObject instance = ObjectPooling.Instance.SpawnFromPool(
            type, point.position, point.rotation
        );

        _boss = instance != null ? instance.GetComponent<EnemyBase>() : null;
        if (_boss == null)
        {
            if (instance != null)
                ObjectPooling.Instance.ReturnToPool(type, instance);
            arena.SetLocked(false);
            return;
        }

        SpawnNode node = new SpawnNode
        {
            enemyType = type,
            spawnPoint = point,
            isPatroller = false,
            savedHealth = -1f
        };

        _started = true;
        _boss.GetComponent<EnemyFinalBossBehaviour>()?.ConfigureArena(arena);
        _boss.InitFromCamp(null, node, player);
        _boss.EventManager.OnDead += HandleBossDead;
    }

    private static void TeleportPlayer(Transform player, Transform point)
    {
        if (point == null) return;

        CharacterController controller = player.GetComponent<CharacterController>();
        bool wasEnabled = controller != null && controller.enabled;
        if (wasEnabled) controller.enabled = false;
        player.SetPositionAndRotation(point.position, point.rotation);
        if (wasEnabled) controller.enabled = true;
    }

    private void HandleBossDead()
    {
        _boss.EventManager.OnDead -= HandleBossDead;
        _boss = null;
        arena.SetLocked(false);
        OnFinalBossDefeated?.Invoke();
    }
}
