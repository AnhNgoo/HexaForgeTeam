using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProjectileVolleySkill",
    menuName = "Enemy/Skills/Projectile Volley")]
public class EnemyProjectileVolleySkillSO : EnemyAttackSkillSO
{
    [SerializeField, Min(1)] private int projectileCount = 3;
    [SerializeField, Min(0)] private int phase2AdditionalProjectiles = 2;
    [SerializeField] private float interval = 0.1f;
    [SerializeField, Range(0f, 90f)] private float spreadAngle = 24f;
    [SerializeField] private float predictionTime = 0.15f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context?.Enemy == null || context.Target == null ||
            context.AttackData == null) return;
        FireAsync(context).Forget();
    }

    private async UniTaskVoid FireAsync(EnemyAttackContext context)
    {
        EarthshakerBossBehaviour boss =
            context.Enemy.GetComponent<EarthshakerBossBehaviour>();
        int count = projectileCount +
            (boss != null && boss.IsPhase2Active ? phase2AdditionalProjectiles : 0);
        float attackSpeed = context.Enemy.MinibossBehaviour?
            .ModifyAttackAnimationSpeed(1f) ?? 1f;
        float realInterval = interval / Mathf.Max(0.01f, attackSpeed);

        for (int i = 0; i < count; i++)
        {
            if (!IsCurrentAttack(context)) return;

            Transform spawn = context.Enemy.Combat.ResolveProjectileAnchor(
                context.AttackData
            );
            Vector3 target = PredictTarget(context.Target);
            Vector3 direction = target - spawn.position;
            direction.y += 0.5f;

            float t = count == 1 ? 0.5f : i / (count - 1f);
            direction = Quaternion.Euler(
                0f, Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t), 0f
            ) * direction.normalized;

            GameObject instance = ObjectPooling.Instance.SpawnFromPool(
                context.AttackData.projectilePoolType,
                spawn.position,
                Quaternion.LookRotation(direction)
            );

            EnemyProjectile projectile =
                instance != null ? instance.GetComponent<EnemyProjectile>() : null;
            if (projectile != null)
            {
                float damage = context.Enemy.Data.damage *
                    context.AttackData.damageMultiplier *
                    context.RuntimeDamageMultiplier;
                float speed = context.Enemy.MinibossBehaviour?
                    .ModifyProjectileSpeed(context.AttackData.projectileSpeed) ??
                    context.AttackData.projectileSpeed;
                projectile.Launch(context.Enemy, damage, speed, direction,
                    context.AttackData.projectileLifetime);
            }
            else if (instance != null)
            {
                ObjectPooling.Instance.ReturnToPool(
                    context.AttackData.projectilePoolType, instance
                );
            }

            if (i < count - 1)
                await UniTask.Delay(System.TimeSpan.FromSeconds(realInterval));
        }
    }

    private Vector3 PredictTarget(Transform target)
    {
        Vector3 result = target.position;
        CharacterMovement movement = target.GetComponentInParent<CharacterMovement>();
        if (movement?.CC != null)
        {
            Vector3 velocity = movement.CC.velocity;
            velocity.y = 0f;
            result += velocity * predictionTime;
        }
        return result;
    }

    private static bool IsCurrentAttack(EnemyAttackContext context)
    {
        return context.Enemy != null && context.Enemy.Health.CurrentHealth > 0f &&
               context.Enemy.Combat.IsPerformingAttack &&
               context.Enemy.Combat.CurrentAttackData == context.AttackData;
    }
}
