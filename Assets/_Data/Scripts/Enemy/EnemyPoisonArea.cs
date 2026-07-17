using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyPoisonArea : MonoBehaviour, IPoolable
{
    [SerializeField] private PoolType poolType;

    public PoolType PoolType => poolType;

    private readonly Dictionary<ITakeDamage, float> _nextTick = new();

    private EnemyBase _source;
    private AttackDataSO _attack;
    private float _runtimeMultiplier;
    private float _tickInterval;
    private float _tickDamageMultiplier;
    private float _exposurePerTick;
    private CancellationTokenSource _lifeCts;

    public void Initialize(
        EnemyBase source,
        AttackDataSO attack,
        float runtimeMultiplier,
        float duration,
        float tickInterval,
        float tickDamageMultiplier,
        float exposurePerTick,
        float scale)
    {
        _source = source;
        _attack = attack;
        _runtimeMultiplier = runtimeMultiplier;
        _tickInterval = tickInterval;
        _tickDamageMultiplier = tickDamageMultiplier;
        _exposurePerTick = exposurePerTick;

        transform.localScale = Vector3.one * scale;

        _lifeCts?.Cancel();
        _lifeCts = new CancellationTokenSource();

        ReturnAfterAsync(duration, _lifeCts.Token).Forget();
    }

    private void OnTriggerStay(Collider other)
    {
        if (_source == null)
            return;

        ITakeDamage target =
            other.GetComponentInParent<ITakeDamage>();

        if (target == null)
            return;

        if (_nextTick.TryGetValue(target, out float nextTime) &&
            Time.time < nextTime)
            return;

        _nextTick[target] = Time.time + _tickInterval;

        float attackMultiplier =
            _attack != null ? _attack.damageMultiplier : 1f;

        target.TakeDamage(new DamageInfo
        {
            damageAmount =
                _source.Data.damage *
                attackMultiplier *
                _runtimeMultiplier *
                _tickDamageMultiplier,

            attacker = _source.gameObject,
            isFromSafeZoneEffect = true
        });

        other.GetComponentInParent<CharacterPoisonStatus>()?
            .AddExposure(_exposurePerTick, _source.gameObject);
    }

    private async UniTaskVoid ReturnAfterAsync(
        float duration,
        CancellationToken token)
    {
        bool cancelled = await UniTask.Delay(
            System.TimeSpan.FromSeconds(duration),
            cancellationToken: token
        ).SuppressCancellationThrow();

        if (!cancelled && gameObject.activeInHierarchy)
        {
            ObjectPooling.Instance.ReturnToPool(
                poolType,
                gameObject
            );
        }
    }

    public void OnSpawnFromPool() { }

    public void OnReturnToPool()
    {
        _lifeCts?.Cancel();
        _lifeCts?.Dispose();
        _lifeCts = null;

        _nextTick.Clear();
        _source = null;
        _attack = null;
        transform.localScale = Vector3.one;
    }
}