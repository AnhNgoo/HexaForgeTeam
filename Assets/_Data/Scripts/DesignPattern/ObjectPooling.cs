using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;

public enum PoolType
{
    None = 0,
    Enemy = 1,
    SafeZone = 2,
    SlashEffect_1 = 3,
    LockTargetMarker = 4,
    PunchEffect_1 = 9,
    PunchEffect_2 = 10,
    Earthquake_1 = 13,
    EarthBreaker_2 = 14,
    AuraEffect_1 = 15,
    AuraEffect_2 = 16,
    AuraEffect_3 = 17,
    AuraEffect_4 = 18,
    AuraEffect_5 = 19,
    AuraEffect_6 = 22,
    KaelGiantPunchEffect_1 = 23,
    KaelGiantPunchEffect_2 = 24,
    KaelGiantAuraEffect_1 = 25,
    HitEffect_1 = 26,
    HitEffect_2 = 27,
    RecoveryBottle = 28,
    HealingEffect = 29,
    RustyIronAxe = 30,
    RustyIronSword = 31,
    Pickup_RustyIronAxe = 32,
    Pickup_RustyIronSword = 33,
    ReceiveRecoveryBottleEffect = 34,
    WoodenWand = 35,
    Pickup_WoodenWand = 36,
    ArcaneChargeEffect = 37,
    LyraProjectile = 38,
    HitEffect_3 = 39,
    CinematicAnchor = 40,
    CinematicUI = 41,
    NotifyUI = 42,
    LyraAuraSkill_2_1 = 43,
    LyraAuraSkill_2_2 = 44,
    LyraAuraSkill_2_3 = 45,
    LyraSkill_2_DetectionAreaEffect = 46,
    LyraSkill_2_Projectile = 47,
    LyraSkill_2_HitEffect = 48,
    LyraSkill_1_Projectile = 49,
    EnemyBat = 50,
    EnemySkeletonMelee = 51,
    EnemyMushroom = 52,
    EnemySkeletonRouge = 53,
    EnemyBee = 54,
    EnemySpider = 55,
    EnemySpiderToxin = 56,
    EnemyDogPup = 57,
    EnemyDogBark = 58,
    Projectile_1 = 59,
    Stun_Loop = 60,
    LightningTelegraph = 61,
    LightningStrike = 62,
    Blink = 63,
    Projectile_Binding = 64,
    EnemyMinibossWarrior = 65,
    EnemyMinibossPhantom = 66,
    EnemyMinibossShade = 67,
    EnemyMinibossMage = 68,
    EnemyMinibossBrurrow = 69,
    MissHitEffect_1 = 100,
    Kael = 101,
    Lyra = 102,
    SpawnCharacterEffect = 103,
    PickedUpItemEffect = 104,
    LevelUpEffect = 105,
    GoldFalling = 106,
    EnemyBruteBoss = 1001,
    EnemyVenomousQueenBoss = 1002,
    EnemyNightStalkerBoss = 1003,
    EnemyThunderBeastBoss = 1004,
    EnemyHellhoundBoss = 1005,
    Pillar = 1006,
    PillarTelegraph = 1007,
    Shockwave = 1008,
    EnemyVenomPoisonArea = 1009,
    EnemyVenomSpray = 1010,
    EnemyVenomTelegraph = 1011,
    EnemyVenomPillar = 1012,
    NightStalkerDarkOrb = 1013,
    NightStalkerVacuum = 1014,
    NightStalkerRainTelegraph = 1015,
    NightStalkerRainStrike = 1016,
    EnemyEarthshakerBoss = 1101,
    EnemyDarkMageBoss = 1102,
    EarthshakerThorn = 1103,
    EarthshakerSandBreath = 1104,
    EarthshakerCrackTelegraph = 1105,
    EarthshakerCrackEruption = 1106,
    DarkMageBurrowTelegraph = 1107,
    DarkMageRitualTelegraph = 1108,
    DarkMageRitualPillar = 1109,
    DarkMageMeteorTelegraph = 1110,
    DarkMageMeteorStrike = 1111,
    DarkMageLaserBeam = 1112,
    TutorialSafeZone = 2001,
    EnemyBatAttackVFX = 3002,
    EnemySlashVFX = 3003,
    BeeStingerVFX = 3004,
    DogBarkVFX = 3005,
    DogPupVFX = 3006,
    SpiderVFX = 3010,
    SpiderToxinVFX = 3007,
    Skeleton_ArrowVFX = 3008,
    SkeletonMageAttackVFX = 3009,
    SkeletonMageSummonVFX = 3026,
    MushroomAttackVFX = 3011,
    MinibossWarriorAttackVFX = 3012,
    MinibossWarriorCastSpellVFX = 3020,
    MinibossPhantomAttackVFX = 3013,
    MinibossBruteSwingVFX = 3014,
    MinibossBruteSlashVFX = 3015,
    MinibossBruteKickVFX = 3016,
    MinibossBruteThrowBoulderVFX = 3017,
    MinibossBruteJumpSmashVFX = 3018,
    MinibossBruteEarthPillarVFX = 3019,
    MinibossPhantomCastSpellVFX = 3021,
    MinibossShadeAttackVFX = 3022,
    MinibossShadeCastSpellVFX = 3023,
    MinibossBrurrowAttackVFX = 3024,
    MinibossBrurrowCastSpellVFX = 3025,
    DormantPowerDropVFX = 3101,
    DormantPowerFlickerVFX = 3102,
    DormantPowerPickupVFX = 3103,


}

[System.Serializable]
public class Pool
{
    public PoolType poolType;
    public GameObject prefab;
    public int initialSize = 10;
    public int maxSize = 50;
    [System.NonSerialized]
    public Transform parent;
}
public class ObjectPooling : Singleton<ObjectPooling>
{
    [SerializeField] string poolDataPath = "ScriptableObjects/PoolData";

    [Header("Pool Settings")]
    public List<Pool> pools = new List<Pool>();

    private Dictionary<PoolType, Queue<GameObject>> poolDictionary;
    private Dictionary<PoolType, Pool> poolSettings;
    private Dictionary<PoolType, int> activeCount;


    protected override void LoadComponent()
    {
        base.LoadComponent();

    }
    protected override void Awake()
    {
        LoadTrace.Mark("ObjectPooling Awake begin");

        base.Awake();

        // Singleton cũ vừa lên lịch Destroy object trùng,
        // không được tiếp tục tạo toàn bộ pool.
        if (Instance != this)
        {
            LoadTrace.Mark("Duplicate ObjectPooling skipped");
            return;
        }

        LoadPoolData();
        LoadTrace.Mark($"PoolData loaded: {pools.Count} entries");

        InitializePools();
        LoadTrace.Mark("ObjectPooling initialization completed");
    }

    private void LoadPoolData()
    {
        PoolData[] poolDataArray = Resources.LoadAll<PoolData>(poolDataPath);
        foreach (PoolData poolData in poolDataArray)
        {
            if (pools.Contains(poolData.pool))
                continue;

            pools.Add(poolData.pool);
        }
        Debug.Log($"Loaded {poolDataArray.Length} pools from {poolDataPath}");
    }

    void InitializePools()
    {
        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();
        poolSettings = new Dictionary<PoolType, Pool>();
        activeCount = new Dictionary<PoolType, int>();

        foreach (Pool pool in pools)
        {
            float poolStart = Time.realtimeSinceStartup;
            if (pool.prefab == null)
            {
                Debug.LogWarning($"Pool {pool.poolType} has no prefab assigned!");
                continue;
            }

            Queue<GameObject> objectQueue = new Queue<GameObject>();
            poolSettings[pool.poolType] = pool;
            activeCount[pool.poolType] = 0;

            if (pool.parent == null)
            {
                GameObject parentObj = new GameObject($"Pool_{pool.poolType}");
                parentObj.transform.SetParent(transform);
                pool.parent = parentObj.transform;
            }

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = CreateNewObject(pool);
                objectQueue.Enqueue(obj);
            }

            poolDictionary[pool.poolType] = objectQueue;

            float duration = Time.realtimeSinceStartup - poolStart;

            if (duration >= 0.05f)
            {
                Debug.LogWarning(
                    $"[LOAD-TRACE] Slow pool: {pool.poolType} | " +
                    $"InitialSize={pool.initialSize} | {duration:F2}s"
                );
            }
        }
    }

    GameObject CreateNewObject(Pool pool)
    {
        GameObject obj = Instantiate(pool.prefab, pool.parent);
        obj.SetActive(false);

        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable == null)
        {
        }

        return obj;
    }

    #region SpawnFromPool
    public GameObject SpawnFromPool(PoolType poolType, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(poolType))
        {
            return null;
        }

        GameObject obj = null;

        if (poolDictionary[poolType].Count > 0)
        {
            obj = poolDictionary[poolType].Dequeue();
        }
        else
        {
            if (activeCount[poolType] < poolSettings[poolType].maxSize)
            {
                obj = CreateNewObject(poolSettings[poolType]);
            }
            else
            {
                return null;
            }
        }

        Transform targetParent = parent != null
            ? parent
            : (poolSettings[poolType].parent != null ? poolSettings[poolType].parent : transform);

        obj.transform.SetParent(targetParent);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        activeCount[poolType]++;

        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawnFromPool();

        return obj;
    }

    public GameObject SpawnFromPool(PoolType poolType, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(poolType))
        {
            return null;
        }

        GameObject obj = null;

        if (poolDictionary[poolType].Count > 0)
        {
            obj = poolDictionary[poolType].Dequeue();
        }
        else
        {
            if (activeCount[poolType] < poolSettings[poolType].maxSize)
            {
                obj = CreateNewObject(poolSettings[poolType]);
            }
            else
            {
                return null;
            }
        }

        Transform targetParent = parent != null
            ? parent
            : (poolSettings[poolType].parent != null ? poolSettings[poolType].parent : transform);

        obj.transform.SetParent(targetParent);
        obj.SetActive(true);

        activeCount[poolType]++;

        IPoolable[] poolables = obj.GetComponentsInChildren<IPoolable>(true);

        foreach (IPoolable poolable in poolables)
        {
            poolable.OnSpawnFromPool();
        }

        return obj;
    }

    #endregion
    public void ReturnToPool(PoolType poolType, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(poolType))
        {
            Debug.LogWarning($"Pool {poolType} does not exist!");
            Destroy(obj);
            return;
        }

        IPoolable[] poolables = obj.GetComponentsInChildren<IPoolable>(true);

        foreach (IPoolable poolable in poolables)
        {
            poolable.OnReturnToPool();
        }

        obj.SetActive(false);
        obj.transform.SetParent(poolSettings[poolType].parent);
        poolDictionary[poolType].Enqueue(obj);

        activeCount[poolType]--;
    }

    public string GetPoolInfo(PoolType poolType)
    {
        if (!poolDictionary.ContainsKey(poolType))
            return $"{poolType}: Not found";

        int available = poolDictionary[poolType].Count;
        int active = activeCount[poolType];
        int total = available + active;

        return $"{poolType}: Total={total}, Active={active}, Available={available}";
    }

    /// <summary>
    /// Đưa GameObject về đúng parent gốc của pool (Pool_{poolType}).
    /// Dùng khi GameObject bị lôi ra ngoài hierarchy trong lúc runtime.
    /// Không ảnh hưởng đến queue hay activeCount — chỉ đặt lại parent.
    /// </summary>
    public void RestoreToPoolParent(PoolType poolType, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning($"[ObjectPooling] RestoreToPoolParent: obj is null (poolType={poolType})");
            return;
        }

        if (!poolSettings.ContainsKey(poolType))
        {
            Debug.LogWarning($"[ObjectPooling] RestoreToPoolParent: Pool {poolType} không tồn tại!");
            return;
        }

        Transform poolParent = poolSettings[poolType].parent;
        if (obj.transform.parent == poolParent)
            return; // Đã đúng chỗ rồi, không cần làm gì

        obj.transform.SetParent(poolParent, true); // true = giữ world position
        Debug.Log($"[ObjectPooling] {obj.name} đã được đưa về parent: {poolParent.name}");
    }


    internal void ReturnToPool(PoolType poolType, object gameObject)
    {

    }
}