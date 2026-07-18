using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProjectileSkill", menuName = "Enemy/Skills/Projectile")]
public class EnemyProjectileSkillSO : EnemyAttackSkillSO
{
    public override void OnAttackImpact(EnemyAttackContext context)
    {
        base.OnAttackImpact(context);
        if (context == null || context.Enemy == null || context.AttackData == null) return;

        Debug.Log($"[EnemyProjectileSkill] Fire projectile: {context.AttackData.attackName}");

        AttackDataSO attackData = context.AttackData;
        EnemyBase enemy = context.Enemy;
        Transform target = context.Target;

        if (target == null) return;

        Transform spawnPoint = enemy.Combat.ResolveProjectileAnchor(attackData); //Sử dụng phương thức ResolveProjectileAnchor để lấy điểm xuất hiện của projectile, có thể mở rộng sau này để hỗ trợ nhiều điểm khác nhau dựa trên loại tấn công hoặc trạng thái của enemy
        if (spawnPoint == null) return;

        GameObject projectileInstance = ObjectPooling.Instance.SpawnFromPool(attackData.projectilePoolType, spawnPoint.position, Quaternion.identity);

        if (projectileInstance == null) return;

        EnemyProjectile projectileScript = projectileInstance.GetComponent<EnemyProjectile>();
        if (projectileScript == null) return;

        float finalDamage = enemy.Data.damage * attackData.damageMultiplier * context.RuntimeDamageMultiplier;
        float projectileSpeed = enemy.MinibossBehaviour?.ModifyProjectileSpeed(attackData.projectileSpeed) ?? attackData.projectileSpeed;

        Vector3 shootDirection = (target.position + Vector3.up * 0.5f) - spawnPoint.position; //Điều chỉnh hướng bắn để nhắm vào phần thân trên của target

        projectileScript.Launch(enemy, finalDamage, projectileSpeed, shootDirection, attackData.projectileLifetime);
    }
}
