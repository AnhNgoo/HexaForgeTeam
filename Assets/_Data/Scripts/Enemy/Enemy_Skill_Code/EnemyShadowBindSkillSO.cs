using UnityEngine;

[CreateAssetMenu(fileName = "EnemyShadowBindSkillSO", menuName = "Enemy/Skills/ShadowBind")]
public class EnemyShadowBindSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private PoolType shadowProjectilePool;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context?.Enemy == null || context.Target == null || context.AttackData == null) return;

        Transform spawnPoint = context.Enemy.Combat.ResolveProjectileAnchor(context.AttackData);
        if (spawnPoint == null) return;

        GameObject obj = ObjectPooling.Instance.SpawnFromPool(shadowProjectilePool, spawnPoint.position, Quaternion.identity);

        EnemyProjectile projectile = obj.GetComponent<EnemyProjectile>();
        if (projectile == null) return;

        float finalDamage = context.Enemy.Data.damage * context.AttackData.damageMultiplier;
        Vector3 dir = context.Target.position + Vector3.up * 0.5f - spawnPoint.position;

        projectile.Launch(context.Enemy, finalDamage, context.AttackData.projectileSpeed, dir, context.AttackData.projectileLifetime);
    }
}