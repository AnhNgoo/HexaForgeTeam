using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Data", menuName = "Enemy/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    [Header("Attack Info")]
    public string attackName; //Tên của đòn tấn công, có thể dùng để phân biệt giữa các loại tấn công khác nhau
    public string animationStateName; //Tên state trong Animator để kích hoạt animation tấn công tương ứng
    public float transitionDuration = 0.1f; //Thời gian chuyển đổi vào animation tấn công, có thể dùng để điều chỉnh độ mượt của việc chuyển trạng thái khi tấn công
    [Header("Stats Modifiers")]
    public float damageMultiplier = 1f; //Hệ số sát thương, có thể dùng để điều chỉnh sát thương của đòn tấn công dựa trên sát thương cơ bản của Enemy
    public float poiseDamage = 10f; //Damage phá thế khi tấn công player
    public float attackRange = 2f; //Khoảng cách mà đòn tấn công có thể đánh trúng mục tiêu, có thể dùng để kiểm tra nếu player nằm trong phạm vi tấn công
}
