using System;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private EnemyBase _enemyBase;
    private Collider _hitboxCollider;
    private AttackDataSO _attackDataSnapshot;
    public event Action<Collider> OnHitTarget;
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
        if (_hitboxCollider != null) _hitboxCollider.enabled = true;
        Debug.Log($"{gameObject.name} đã kích hoạt hitbox!");
    }

    public void DisableHitBox()
    {
        if (_hitboxCollider != null) _hitboxCollider.enabled = false;
        Debug.Log($"{gameObject.name} đã vô hiệu hóa hitbox!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_dealDamageOnHit)
            {
                if (_enemyBase == null)
                {
                    Debug.LogWarning($"{gameObject.name} chưa được Initialize.");
                    return;
                }

                AttackDataSO attackData = _attackDataSnapshot ?? _enemyBase.Combat.CurrentAttackData;

                float multiplier = attackData != null ? attackData.damageMultiplier : 1f;

                float finalDamage = _enemyBase.Data.damage * multiplier;

                _enemyBase.ExtendLeash(_enemyBase.Data.maxLeashDistance + 5f);

                Debug.Log($"{gameObject.name} gây sát thương {finalDamage} lên Player " + $"(sát thương cơ bản: {_enemyBase.Data.damage}, " + $"hệ số: {multiplier})");

                // To_Do: Gọi hàm xử lý sát thương lên Player tại đây, ví dụ: other.GetComponent<PlayerHealth>().TakeDamage(attackData.damage);
            }
            OnHitTarget?.Invoke(other);
        }
    }
}
