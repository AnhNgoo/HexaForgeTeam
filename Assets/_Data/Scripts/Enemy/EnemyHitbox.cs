using System;
using UnityEngine;
using System.Collections.Generic;

public class EnemyHitbox : MonoBehaviour
{
    private EnemyBase _enemyBase;
    private Collider _hitboxCollider;
    private AttackDataSO _attackDataSnapshot;
    public event Action<Collider> OnHitTarget;
    private readonly HashSet<ITakeDamage> _damagedTargets = new HashSet<ITakeDamage>();
    [SerializeField] private bool _dealDamageOnHit = true; // Thêm biến để kiểm soát việc

    private void Awake()
    {
        _hitboxCollider = GetComponent<Collider>();
        DisableHitBox(); // Đảm bảo hitbox được tắt khi khởi tạo

    }
    public void Initialize(EnemyBase enemyBase, AttackDataSO attackData = null)
    {
        _enemyBase = enemyBase;
        _attackDataSnapshot = attackData;
    }

    public void EnableHitBox()
    {
        _damagedTargets.Clear();
        if (_hitboxCollider != null) _hitboxCollider.enabled = true;
    }

    public void DisableHitBox()
    {
        if (_hitboxCollider != null) _hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        ITakeDamage damageable = other.GetComponentInParent<ITakeDamage>();
        if (damageable == null) return;

        if (_dealDamageOnHit)
        {
            if (_enemyBase == null)
            {
                Debug.LogWarning($"{gameObject.name} chưa được Initialize.");
                return;
            }

            if (_damagedTargets.Contains(damageable))
                return;

            _damagedTargets.Add(damageable);

            AttackDataSO attackData = _attackDataSnapshot ?? _enemyBase.Combat.CurrentAttackData;

            float multiplier = attackData != null ? attackData.damageMultiplier : 1f;
            float finalDamage = _enemyBase.Data.damage * multiplier;

            _enemyBase.ExtendLeash(_enemyBase.Data.maxLeashDistance + 5f);

            DamageInfo damageInfo = new DamageInfo
            {
                damageAmount = finalDamage,
                attacker = _enemyBase.gameObject
            };

            damageable.TakeDamage(damageInfo);
            CameraShake.Instance?.Shake();

            Debug.Log($"{gameObject.name} gây sát thương {finalDamage} lên Player " + $"(sát thương cơ bản: {_enemyBase.Data.damage}, hệ số: {multiplier})");
        }

        OnHitTarget?.Invoke(other);
    }
}
