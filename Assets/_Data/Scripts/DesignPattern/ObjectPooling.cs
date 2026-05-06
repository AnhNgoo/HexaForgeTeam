using UnityEngine;
using System.Collections.Generic;

public enum PoolType
{
    None = 0,
    Enemy = 1,
    SafeZone = 2,
}

[System.Serializable]
public class Pool
{
    public PoolType poolType;
    public GameObject prefab;
    public int initialSize = 10;
    public int maxSize = 50;
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
    private void Start()
    {
        LoadPoolData();
        InitializePools();
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

    public void ReturnToPool(PoolType poolType, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(poolType))
        {
            Debug.LogWarning($"Pool {poolType} does not exist!");
            Destroy(obj);
            return;
        }

        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnReturnToPool();

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
}