using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyDarkOrbVolleySkill",
    menuName = "Enemy/Skills/Dark Mage/Dark Orb Volley")]
public class EnemyDarkOrbVolleySkillSO : EnemyAttackSkillSO
{
    [Header("Aimed Volley")]
    [SerializeField, Min(1)] private int baseProjectileCount = 3;
    [SerializeField, Min(0)] private int phase2AdditionalProjectiles = 2;
    [SerializeField, Min(0f)] private float projectileInterval = 0.08f;
    [SerializeField, Range(0f, 90f)] private float spreadAngle = 24f;
    [SerializeField, Min(0f)] private float targetPredictionTime = 0.15f;

    [Header("Bullet Hell")]
    [SerializeField, Min(1)] private int bulletHellProjectilesPerWave = 10;
    [SerializeField, Min(1)] private int bulletHellWaves = 2;
    [SerializeField, Min(0f)] private float bulletHellProjectileInterval = 0.025f;
    [SerializeField, Min(0f)] private float bulletHellWaveInterval = 0.25f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsContextValid(context))
            return;

        DarkMageBossBehaviour boss =
            context.Enemy.GetComponent<DarkMageBossBehaviour>();

        if (boss != null && boss.ConsumeBulletHellRequest())
        {
            CastBulletHellAsync(
                context,
                bulletHellWaves,
                bulletHellProjectilesPerWave,
                bulletHellWaveInterval
            ).Forget();

            return;
        }

        FireAimedVolleyAsync(context, boss).Forget();
    }

    public async UniTask CastBulletHellAsync(
        EnemyAttackContext context,
        int waveCount,
        int projectilesPerWave,
        float waveInterval)
    {
        if (!IsContextValid(context))
            return;

        waveCount = Mathf.Max(1, waveCount);
        projectilesPerWave = Mathf.Max(1, projectilesPerWave);

        for (int wave = 0; wave < waveCount; wave++)
        {
            if (!IsEnemyAlive(context))
                return;

            float waveOffset = wave * (180f / projectilesPerWave);

            for (int i = 0; i < projectilesPerWave; i++)
            {
                if (!IsEnemyAlive(context))
                    return;

                float angle =
                    waveOffset + i * (360f / projectilesPerWave);

                Vector3 direction =
                    Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

                SpawnProjectile(context, direction);

                if (bulletHellProjectileInterval > 0f)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(bulletHellProjectileInterval)
                    );
                }
            }

            if (wave < waveCount - 1 && waveInterval > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(waveInterval));
        }
    }

    private async UniTaskVoid FireAimedVolleyAsync(
        EnemyAttackContext context,
        DarkMageBossBehaviour boss)
    {
        int count = boss != null ? boss.ModifyDarkOrbCount(baseProjectileCount, phase2AdditionalProjectiles) : baseProjectileCount;

        count = Mathf.Max(1, count);

        for (int i = 0; i < count; i++)
        {
            if (!IsEnemyAlive(context))
                return;

            Vector3 direction = GetPredictedDirection(context);

            float normalizedIndex = count == 1
                ? 0f
                : i / (float)(count - 1) - 0.5f;

            direction = Quaternion.AngleAxis(
                normalizedIndex * spreadAngle,
                Vector3.up
            ) * direction;

            SpawnProjectile(context, direction);

            if (i < count - 1 && projectileInterval > 0f)
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(projectileInterval)
                );
            }
        }
    }

    private Vector3 GetPredictedDirection(EnemyAttackContext context)
    {
        Transform anchor = ResolveAnchor(context);
        Vector3 targetPosition = context.Target != null
            ? context.Target.position
            : anchor.position + context.Enemy.MyTransform.forward * 10f;

        CharacterController controller = context.Target != null
            ? context.Target.GetComponentInParent<CharacterController>()
            : null;

        if (controller != null)
        {
            targetPosition +=
                controller.velocity * targetPredictionTime;
        }

        Vector3 direction = targetPosition - anchor.position;

        return direction.sqrMagnitude > 0.001f
            ? direction.normalized
            : context.Enemy.MyTransform.forward;
    }

    private void SpawnProjectile(
        EnemyAttackContext context,
        Vector3 direction)
    {
        Transform anchor = ResolveAnchor(context);
        PoolType poolType = context.AttackData.projectilePoolType;

        GameObject projectileObject = ObjectPooling.Instance.SpawnFromPool(
            poolType,
            anchor.position,
            Quaternion.LookRotation(direction)
        );

        if (projectileObject == null)
            return;

        EnemyProjectile projectile =
            projectileObject.GetComponent<EnemyProjectile>();

        if (projectile == null)
        {
            ObjectPooling.Instance.ReturnToPool(
                poolType,
                projectileObject
            );
            return;
        }

        DarkMageBossBehaviour boss =
            context.Enemy.GetComponent<DarkMageBossBehaviour>();

        float speed = boss != null
            ? boss.ModifyProjectileSpeed(
                context.AttackData.projectileSpeed)
            : context.AttackData.projectileSpeed;

        float damage =
            context.Enemy.Data.damage *
            context.AttackData.damageMultiplier *
            context.RuntimeDamageMultiplier;

        projectile.Launch(
            context.Enemy,
            damage,
            speed,
            direction,
            context.AttackData.projectileLifetime
        );
    }

    private Transform ResolveAnchor(EnemyAttackContext context)
    {
        Transform anchor = context.Enemy.Combat.ResolveProjectileAnchor(
            context.AttackData
        );

        return anchor != null
            ? anchor
            : context.Enemy.MyTransform;
    }

    private static bool IsContextValid(EnemyAttackContext context)
    {
        return context != null &&
               context.Enemy != null &&
               context.AttackData != null &&
               ObjectPooling.Instance != null;
    }

    private static bool IsEnemyAlive(EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.Enemy.gameObject.activeInHierarchy &&
               context.Enemy.Health != null &&
               context.Enemy.Health.CurrentHealth > 0f;
    }

    private static bool IsCurrentAttack(EnemyAttackContext context)
    {
        return IsEnemyAlive(context) &&
               context.Enemy.Combat.CurrentAttackData ==
               context.AttackData;
    }
}