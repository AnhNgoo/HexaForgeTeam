using UnityEngine;
using Cysharp.Threading.Tasks; // Sử dụng UniTask để hỗ trợ async/await trong Unity, giúp quản lý thời gian và hiệu ứng của vụ nổ một cách dễ dàng hơn

[CreateAssetMenu(fileName = "EnemyKamikazeExplosionSkillSO", menuName = "Enemy/Skills/Kamikaze Explosion")]
public class EnemyKamikazeExplosionSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private EnemyHitboxType explosionHitboxType = EnemyHitboxType.Explosion; //Loại hitbox để kích hoạt khi nổ, có thể dùng để xác định phạm vi và hiệu ứng của vụ nổ
    [SerializeField] private EnemyHitboxType stingHitboxType = EnemyHitboxType.Sting; //Loại hitbox để kích hoạt khi tấn công chích, có thể dùng để tạo ra một đòn tấn công phụ trước khi nổ để làm cho kỹ năng này có thêm chiều sâu và đa dạng trong cách sử dụng
    [SerializeField] private float explosionActiveTime = 0.3f; //Thời gian mà hitbox vụ nổ được kích hoạt, có thể dùng để điều chỉnh độ rộng của vụ nổ và thời gian mà kẻ địch có thể bị ảnh hưởng bởi vụ nổ
    [SerializeField] private float selfKillDelay = 0.5f; //Thời gian sau khi kích hoạt vụ nổ mà enemy sẽ tự hủy, có thể dùng để tạo ra một khoảng thời gian ngắn giữa
    [SerializeField] private float stingActiveTime = 0.25f; //Thời gian mà hitbox chích được kích hoạt, có thể dùng để điều chỉnh độ rộng của đòn tấn công chích và thời gian mà kẻ địch có thể bị ảnh hưởng bởi đòn chích

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        base.OnAttackImpact(context);

        if (context?.Enemy == null || context.AttackData == null)
            return;

        WaitForStingHitAsync(context).Forget();
    }

    private async UniTaskVoid WaitForStingHitAsync(
        EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        EnemyHitbox sting =
            enemy.HitboxRegistry.GetHitbox(stingHitboxType);

        if (sting == null) return;

        bool hitPlayer = false;

        void HandleHit(Collider target)
        {
            if (target != null &&
                target.GetComponentInParent<CharacterBase>() != null)
            {
                hitPlayer = true;
            }
        }

        sting.OnHitTarget += HandleHit;
        sting.EnableHitBox();

        float elapsed = 0f;

        while (!hitPlayer &&
               elapsed < stingActiveTime &&
               enemy != null &&
               enemy.Health.CurrentHealth > 0f &&
               enemy.Combat.CurrentAttackData == context.AttackData)
        {
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        sting.OnHitTarget -= HandleHit;
        sting.DisableHitBox();

        if (hitPlayer && enemy != null &&
            enemy.Health.CurrentHealth > 0f)
        {
            await ExplosionAsync(context);
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
