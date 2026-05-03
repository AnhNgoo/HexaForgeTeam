public interface IPoolable
{
    PoolType PoolType { get; }
    void OnSpawnFromPool();
    void OnReturnToPool();
}