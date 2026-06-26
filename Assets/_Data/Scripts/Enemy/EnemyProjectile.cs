using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public enum EnemyProjectileEffectType
{
    None,
    Stun,
    Root
}

public class EnemyProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private PoolType poolType;
    public PoolType PoolType => poolType;

    [SerializeField] private EnemyProjectileEffectType effectType;
    [SerializeField] private float effectDuration = 1.5f;

    private EnemyBase _sourceEnemy;
    private float _damage;
    private float _speed;
    private Vector3 _direction;
    private bool _isLaunched = false;

    private CancellationTokenSource _lifeCts;

    public void Launch(EnemyBase sourceEnemy, float damage, float speed, Vector3 direction, float lifetime)
    {
        _sourceEnemy = sourceEnemy;
        _damage = damage;
        _speed = speed;
        _direction = direction.normalized;
        _isLaunched = true;

        transform.forward = _direction; // Xoay hướng viên đạn về hướng di chuyển

        _lifeCts?.Cancel();
        _lifeCts = new CancellationTokenSource();
        ReturnAfterLifetime(lifetime, _lifeCts.Token).Forget();
    }

    private async UniTaskVoid ReturnAfterLifetime(float lifetime, CancellationToken token)
    {
        bool cancelled = await UniTask.Delay(System.TimeSpan.FromSeconds(lifetime), cancellationToken: token).SuppressCancellationThrow();

        if (!cancelled)
        {
            DebugNote.Red("Đạn hết thời gian tồn tại và bị hủy!");
            ReturnToPool();
        }
    }

    private void Update()
    {
        if (!_isLaunched) return;
        transform.position += _direction * _speed * Time.deltaTime; // Di chuyển viên đạn theo hướng đã định
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyEffect(other);

            DebugNote.Green("Đạn trúng Player mất " + _damage + " máu!");

            if (_sourceEnemy != null)
            {
                _sourceEnemy.ExtendLeash(_sourceEnemy.Data.maxLeashDistance + 5f);

                if (effectType == EnemyProjectileEffectType.Root)
                    _sourceEnemy.GetComponent<ShadeMinibossBehaviour>()?.NotifyShadowBindSuccess();
            }

            ReturnToPool();
        }
        // Kiểm tra nếu va chạm với lớp chướng ngại vật (Sử dụng dịch Bit để so sánh lớp sau này có thể mở rộng để kiểm tra nhiều lớp khác nhau mà không cần nhiều ||)
        else if (((1 << other.gameObject.layer) & LayerMask.GetMask("Obstacle", "Environment")) != 0)
        {
            DebugNote.Red("Đạn va chạm với môi trường và bị hủy!");
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (!_isLaunched) return;
        ObjectPooling.Instance.ReturnToPool(poolType, gameObject);
    }

    private void ApplyEffect(Collider other)
    {
        if (effectType == EnemyProjectileEffectType.None)
            return;

        CharacterMovement movement = other.GetComponent<CharacterMovement>();
        if (movement == null)
            return;

        movement.LockMovement(effectDuration);

        if (effectType == EnemyProjectileEffectType.Stun)
            DebugNote.Yellow("Đạn trúng Player và làm choáng!");

        if (effectType == EnemyProjectileEffectType.Root)
            DebugNote.Yellow("Đạn trúng Player và trói chân!");
    }

    public void OnSpawnFromPool()
    {

    }
    public void OnReturnToPool()
    {
        _lifeCts?.Cancel();
        _lifeCts?.Dispose();
        _lifeCts = null;

        _sourceEnemy = null;
        _isLaunched = false;
        _damage = 0f;
        _speed = 0f;
        _direction = Vector3.zero;
    }
}
