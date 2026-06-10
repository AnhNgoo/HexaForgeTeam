using UnityEngine;

public enum AttackType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "New Attack Data", menuName = "Enemy/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    [Header("Attack Info")]
    public string attackName; //Tên của đòn tấn công, có thể dùng để phân biệt giữa các loại tấn công khác nhau
    public string animationStateName; //Tên state trong Animator để kích hoạt animation tấn công tương ứng
    public AttackType attackType; //Loại tấn công (cận chiến hoặc tầm xa), có thể dùng để xác định cách thức tấn công và hiệu ứng của đòn tấn công
    [Header("Timings")]
    public float transitionDuration = 0.1f; //Thời gian chuyển đổi vào animation tấn công, có thể dùng để điều chỉnh độ mượt của việc chuyển trạng thái khi tấn công
    public float attackDuration = 0.5f; //Thời gian của đòn tấn công, có thể dùng để kiểm soát thời gian mở hitbox và đóng hitbox trong animation tấn công
    public float cooldown = 1f; //Thời gian hồi chiêu của đòn tấn công, có thể dùng để kiểm soát thời gian giữa các đòn tấn công khác nhau
    [Header("Stats Modifiers")]
    public float damageMultiplier = 1f; //Hệ số sát thương, có thể dùng để điều chỉnh sát thương của đòn tấn công dựa trên sát thương cơ bản của Enemy
    public float poiseDamage = 10f; //Damage phá thế khi tấn công player
    [Header("Range Requirements")]
    public float minAttackRange = 0f; //Khoảng cách mà đòn tấn công có thể đánh trúng mục tiêu, có thể dùng để kiểm tra nếu player nằm trong phạm vi tấn công
    public float maxAttackRange = 2f; //Khoảng cách tối đa mà đòn tấn công có thể đánh trúng mục tiêu, có thể dùng để kiểm tra nếu player nằm trong phạm vi tấn công
    [Header("Range Settings (Ranged Attacks Only)")]
    public GameObject projectilePrefab; //Prefab của projectile được sử dụng cho đòn tấn công tầm xa, có thể dùng để tạo ra projectile khi tấn công
    public float projectileSpeed = 10f; //Tốc độ của projectile (chỉ áp dụng cho tấn công tầm xa)
    [Header("VFX Settings")]
    public PoolType hitVFX; //Loại VFX được sử dụng khi đòn tấn công đánh trúng mục tiêu, có thể dùng để kích hoạt hiệu ứng tương ứng khi tấn công trúng player
    public PoolType missVFX; //Loại VFX được sử dụng khi đòn tấn công không đánh trúng mục tiêu, có thể dùng để kích hoạt hiệu ứng tương ứng khi tấn công hụt player
}
