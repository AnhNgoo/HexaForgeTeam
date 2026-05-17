using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyCombat : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [Header("Combat Settings")]
    [InlineEditor()]
    [SerializeField] private AttackDataSO currentAttackData; //Dữ liệu tấn công, có thể mở rộng sau này để có nhiều loại tấn công khác nhau
    private float lastAttackTime; //Thời gian của lần tấn công cuối cùng, dùng để kiểm soát thời gian giữa các đòn tấn công
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        lastAttackTime = Time.time;
        Debug.Log($"{gameObject.name} - EnemyCombat đã được khởi tạo!");
    }

    //Hàm tấn công mục tiêu
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + _enemyBase.enemyData.attackCooldown; //Kiểm tra nếu đã đủ thời gian giữa các đòn tấn công
    }

    public void PerformAttack()
    {
        if (currentAttackData == null)
        {
            Debug.LogWarning($"{gameObject.name} không có dữ liệu tấn công để thực hiện đòn tấn công!");
            return;
        }
        lastAttackTime = Time.time; //Cập nhật thời gian của lần tấn công cuối cùng
        Debug.Log($"{gameObject.name} đã vung vũ khí chém player với sát thương {_enemyBase.enemyData.damage}!");
        if (_enemyBase.AnimatorController.Animator != null)
        {
            _enemyBase.AnimatorController.PlayAttackAnimation(currentAttackData); //Gọi hàm chơi animation tấn công từ EnemyAnimatorController
        }
    }

    public void ForceCloseHitbox()
    {
        //Gọi hàm đóng hitbox từ EnemyAnimatorController để đảm bảo rằng hitbox sẽ được đóng đúng thời điểm, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
        if (_enemyBase.AnimatorController != null)
        {
            _enemyBase.AnimatorController.CloseHitBox();
        }
    }
}
