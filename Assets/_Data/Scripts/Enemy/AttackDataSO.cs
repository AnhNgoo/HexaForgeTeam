using UnityEngine;
using Sirenix.OdinInspector;

public enum AttackType
{
    Melee,
    Ranged
}

public enum AttackSelectionMode
{
    Random,
    Tactical
}

public enum EnemyAttackAnchorType
{
    None,
    Root,
    Head,
    Mouth,
    Chest,
    Hand,
    Leg,
    Weapon,
    ProjectileSpawn,
    Hitbox,
    Target,
}

public enum EnemyHitboxType
{
    None,
    Weapon,
    Mouth,
    Body,
    Leg,
    Hand,
    ClawLeft,
    ClawRight,
    Spin,
    Sting,
    Explosion,
    Shield,
    GroundImpact,
}

[CreateAssetMenu(fileName = "New Attack Data", menuName = "Enemy/Attack Data")]
public class AttackDataSO : ScriptableObject
{
    [Header("Attack Info")]
    public string attackName; //Tên của đòn tấn công, có thể dùng để phân biệt giữa các loại tấn công khác nhau
    public string animationStateName; //Tên state trong Animator để kích hoạt animation tấn công tương ứng
    [EnumToggleButtons] public AttackType attackType; //Loại tấn công (cận chiến hoặc tầm xa), có thể dùng để xác định cách thức tấn công và hiệu ứng của đòn tấn công
    private bool IsMelee => attackType == AttackType.Melee; //Thuộc tính tiện lợi để kiểm tra nhanh nếu đòn tấn công là cận chiến, có thể dùng trong logic của EnemyAttackSkillSO để xử lý khác nhau giữa tấn công cận chiến và tầm xa
    private bool IsRanged => attackType == AttackType.Ranged; //Thuộc tính tiện lợi để kiểm tra nhanh nếu đòn tấn công là tầm xa, có thể dùng trong logic của EnemyAttackSkillSO để xử lý khác nhau giữa tấn công cận chiến và tầm xa
    [Header("Timings")]
    public float transitionDuration = 0.1f; //Thời gian chuyển đổi vào animation tấn công, có thể dùng để điều chỉnh độ mượt của việc chuyển trạng thái khi tấn công
    public float attackDuration = 0.5f; //Thời gian của đòn tấn công, có thể dùng để kiểm soát thời gian mở hitbox và đóng hitbox trong animation tấn công
    public float cooldown = 1f; //Thời gian hồi chiêu của đòn tấn công, có thể dùng để kiểm soát thời gian giữa các đòn tấn công khác nhau
    [Header("Stats Modifiers")]
    public float damageMultiplier = 1f; //Hệ số sát thương, có thể dùng để điều chỉnh sát thương của đòn tấn công dựa trên sát thương cơ bản của Enemy
    public float poiseDamage = 10f; //Damage phá thế khi tấn công player
    [Header("Audio Settings")]
    public AudioClip attackSound; //Âm thanh phát đúng thời điểm Animation Event AttackImpact được gọi
    [Range(0f, 1f)] public float attackSoundVolume = 1f;
    [Header("Range Requirements")]
    public float minAttackRange = 0f; //Khoảng cách mà đòn tấn công có thể đánh trúng mục tiêu, có thể dùng để kiểm tra nếu player nằm trong phạm vi tấn công
    public float maxAttackRange = 2f; //Khoảng cách tối đa mà đòn tấn công có thể đánh trúng mục tiêu, có thể dùng để kiểm tra nếu player nằm trong phạm vi tấn công
    [ShowIf(nameof(IsRanged))]
    [Header("Range Settings")]
    [ShowIf(nameof(IsRanged))]
    public EnemyAttackAnchorType projectileAnchor = EnemyAttackAnchorType.ProjectileSpawn; //Điểm neo để xuất hiện projectile khi thực hiện đòn tấn công tầm xa, có thể dùng để xác định vị trí xuất hiện của projectile khi tấn công
    [ShowIf(nameof(IsRanged))]
    public PoolType projectilePoolType = PoolType.None; //Loại pool để lấy projectile khi tấn công, có thể dùng để quản lý các loại projectile khác nhau trong hệ thống pooling
    [ShowIf(nameof(IsRanged))]
    public float projectileSpeed = 10f; //Tốc độ của projectile (chỉ áp dụng cho tấn công tầm xa)
    [ShowIf(nameof(IsRanged))]
    public float projectileLifetime = 4f; //Thời gian tồn tại của projectile trước khi tự hủy (chỉ áp dụng cho tấn công tầm xa)
    [Header("VFX Settings")]
    public PoolType attackVFX; //VFX gán khi đòn đánh được đánh ra
    public EnemyAttackAnchorType vfxAnchor = EnemyAttackAnchorType.Root; //Điểm neo để gắn VFX khi đòn đánh được đánh ra, có thể dùng để xác định vị trí gắn hiệu ứng khi tấn công
    public Vector3 vfxOffset; //Offset để điều chỉnh vị trí gắn VFX khi đòn đánh được đánh ra, có thể dùng để tinh chỉnh vị trí gắn hiệu ứng khi tấn công để phù hợp với animation và mô hình của Enemy
    public Vector3 vfxEuler; //Rotation để điều chỉnh hướng gắn VFX khi đòn đánh được đánh ra, có thể dùng để tinh chỉnh hướng gắn hiệu ứng khi tấn công để phù hợp với animation và mô hình của Enemy
    public float vfxScale = 1f; //Scale để điều chỉnh kích thước gắn VFX khi đòn đánh được đánh ra, có thể dùng để tinh chỉnh kích thước gắn hiệu ứng khi tấn công để phù hợp với animation và mô hình của Enemy
    [Header("Selection")]
    public bool isFollowUpOnly; //Nếu true, đòn tấn công này chỉ có thể được sử dụng như một đòn tấn công tiếp theo sau một đòn tấn công khác, có thể dùng để tạo ra các combo tấn công hoặc các chuỗi tấn công đặc biệt
    [ShowIf(nameof(IsMelee))]
    [Header("Hitbox Settings")]
    [ShowIf(nameof(IsMelee))]
    public EnemyHitboxType hitboxType = EnemyHitboxType.None; //Loại hitbox được sử dụng cho đòn tấn công, có thể dùng để xác định loại hitbox và cách xử lý va chạm
    [Header("Skill Logic")]
    [InlineEditor()] public EnemyAttackSkillSO skillLogic; //Tham chiếu đến EnemyAttackSkillSO để định nghĩa logic đặc biệt của đòn tấn công, có thể dùng để tạo ra các đòn tấn công có hiệu ứng đặc biệt hoặc logic phức tạp hơn so với các đòn tấn công thông thường
}
