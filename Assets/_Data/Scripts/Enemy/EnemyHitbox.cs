using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private EnemyBase _enemyBase;
    private Collider _hitboxCollider;

    private void Awake()
    {
        _hitboxCollider = GetComponent<Collider>();
        DisableHitBox(); // Đảm bảo hitbox được tắt khi khởi tạo

    }
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        Debug.Log($"{gameObject.name} - EnemyHitbox đã được khởi tạo!");
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
            float finalDamage = _enemyBase.Data.damage; // Lấy sát thương cơ bản từ EnemyData

            if (_enemyBase.Combat.CurrentAttackData != null)
            {
                finalDamage *= _enemyBase.Combat.CurrentAttackData.damageMultiplier; // Nhân thêm sát thương từ AttackData nếu có
            }
            Debug.Log($"{gameObject.name} gây sát thương {finalDamage} lên Player (sát thương cơ bản: {_enemyBase.Data.damage}, hệ số từ AttackData: {_enemyBase.Combat.CurrentAttackData.damageMultiplier})");
            // To_Do: Gọi hàm xử lý sát thương lên Player tại đây, ví dụ: other.GetComponent<PlayerHealth>().TakeDamage(attackData.damage);
        }
    }
}
