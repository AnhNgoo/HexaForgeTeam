using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMeleeHitboxSkill", menuName = "Enemy/Skills/MeleeHitbox")]
public class EnemyMeleeHitboxSkillSO : EnemyAttackSkillSO
{
    public override void OnAttackImpact(EnemyAttackContext context)
    {
        base.OnAttackImpact(context);
        if (context == null || context.Enemy == null) return;

        context.Enemy.Combat.EnableHitbox(context.AttackData.hitboxType);
        Debug.Log($"[EnemyMeleeHitboxSkill] Open hitbox: {context.AttackData.attackName} - Hitbox Type: {context.AttackData.hitboxType}");
    }

    public override void OnAttackEnd(EnemyAttackContext context)
    {
        base.OnAttackEnd(context);
        if (context == null || context.Enemy == null) return;

        context.Enemy.HitboxRegistry.DisableAllHitboxes();
        Debug.Log($"[EnemyMeleeHitboxSkill] Close hitbox: {context.AttackData.attackName} - Hitbox Type: {context.AttackData.hitboxType}");
    }
}
