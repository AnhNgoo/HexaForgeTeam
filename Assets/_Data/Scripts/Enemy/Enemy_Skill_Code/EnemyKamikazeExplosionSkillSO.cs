using UnityEngine;
using Cysharp.Threading.Tasks; // Sử dụng UniTask để hỗ trợ async/await trong Unity, giúp quản lý thời gian và hiệu ứng của vụ nổ một cách dễ dàng hơn

[CreateAssetMenu(fileName = "EnemyKamikazeExplosionSkillSO", menuName = "Enemy/Skills/Kamikaze Explosion")]
public class EnemyKamikazeExplosionSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private EnemyHitboxType explosionHitboxType = EnemyHitboxType.Explosion; //Loại hitbox để kích hoạt khi nổ, có thể dùng để xác định phạm vi và hiệu ứng của vụ nổ
    [SerializeField] private EnemyHitboxType stingHitboxType = EnemyHitboxType.Sting; //Loại hitbox để kích hoạt khi tấn công chích, có thể dùng để tạo ra một đòn tấn công phụ trước khi nổ để làm cho kỹ năng này có thêm chiều sâu và đa dạng trong cách sử dụng
    [SerializeField] private float explosionActiveTime = 0.3f; //Thời gian mà hitbox vụ nổ được kích hoạt, có thể dùng để điều chỉnh độ rộng của vụ nổ và thời gian mà kẻ địch có thể bị ảnh hưởng bởi vụ nổ
    [SerializeField] private float selfKillDelay = 0.5f; //Thời gian sau khi kích hoạt vụ nổ mà enemy sẽ tự hủy, có thể dùng để tạo ra một khoảng thời gian ngắn giữa
    public override void OnAttackImpact(EnemyAttackContext context)
    {
        base.OnAttackImpact(context);

        if (context == null || context.Enemy == null || context.AttackData == null) return;

        EnemyBase enemy = context.Enemy;

        EnemyHitbox stingHitbox = enemy.HitboxRegistry.GetHitbox(stingHitboxType);
        if (stingHitbox == null) return;

        stingHitbox.OnHitTarget -= HandleStingHit; //Hủy đăng ký sự kiện sau khi đã xử lý để tránh bị gọi nhiều lần nếu hitbox được kích hoạt lại trong tương lai
        stingHitbox.OnHitTarget += HandleStingHit; //Đăng ký sự kiện khi hitbox chích trúng mục tiêu, có thể dùng để tạo ra hiệu ứng hoặc sát thương phụ khi chích trúng player

        stingHitbox.EnableHitBox(); //Kích hoạt hitbox chích để thực hiện đòn tấn công phụ, có thể dùng để tạo ra một đòn tấn công nhanh trước khi vụ nổ chính xảy ra

        void HandleStingHit(Collider target)
        {
            if (target == null || !target.CompareTag("Player")) return;

            stingHitbox.OnHitTarget -= HandleStingHit; //Hủy đăng ký sự kiện ngay sau khi xử lý để tránh bị gọi nhiều lần nếu hitbox được kích hoạt lại trong tương lai
            stingHitbox.DisableHitBox(); //Vô hiệu hóa hitbox chích sau khi đã trúng mục tiêu

            ExplosionAsync(context).Forget(); //Bắt đầu quá trình kích hoạt vụ nổ sau khi chích trúng mục tiêu, có thể dùng để tạo ra một chuỗi hành động liên tiếp giữa đòn chích và vụ nổ
        }
    }

    public override void OnAttackEnd(EnemyAttackContext context)
    {
        base.OnAttackEnd(context);

        if (context == null || context.Enemy == null) return;

        EnemyHitbox stingHitbox = context.Enemy.HitboxRegistry.GetHitbox(stingHitboxType);
        if (stingHitbox != null)
        {
            stingHitbox.DisableHitBox(); //Đảm bảo hitbox chích được vô hiệu hóa khi kết thúc đòn tấn công, có thể dùng để tránh việc hitbox chích vẫn còn kích hoạt nếu đòn tấn công kết thúc mà không trúng mục tiêu nào
        }
    }

    private async UniTask ExplosionAsync(EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        if (enemy == null) return;

        enemy.Locomotion.StopMoving(); //Dừng di chuyển của enemy ngay khi bắt đầu vụ nổ để tạo ra một khoảnh khắc tĩnh lặng trước khi vụ nổ xảy ra, có thể dùng để tăng tính kịch tính của kỹ năng này

        enemy.Combat.PlayAttackVFX();

        EnemyHitbox explosionHitbox = enemy.HitboxRegistry.GetHitbox(explosionHitboxType);
        if (explosionHitbox != null)
        {
            explosionHitbox.EnableHitBox(); //Kích hoạt hitbox vụ nổ
            await UniTask.Delay(System.TimeSpan.FromSeconds(explosionActiveTime)); //Đợi trong khoảng thời gian vụ nổ được kích hoạt để tạo ra hiệu ứng và sát thương cho kẻ địch trong phạm vi vụ nổ
            explosionHitbox.DisableHitBox(); //Vô hiệu hóa hitbox vụ nổ sau
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(selfKillDelay)); //Đợi một khoảng thời gian ngắn sau khi vụ nổ để tạo ra một khoảnh khắc trước khi enemy tự hủy, có thể dùng để cho phép các hiệu ứng vụ nổ diễn ra hoàn chỉnh trước khi enemy biến mất

        enemy.Health.TakeDamage(enemy.Health.CurrentHealth);
    }
}
