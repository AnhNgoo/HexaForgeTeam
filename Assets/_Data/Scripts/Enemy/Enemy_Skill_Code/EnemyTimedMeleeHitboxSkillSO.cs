using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTimedMeleeHitboxSkill",
    menuName = "Enemy/Skills/Timed Melee Hitbox")]
public class EnemyTimedMeleeHitboxSkillSO : EnemyAttackSkillSO
{
    [SerializeField, Min(0.02f)] private float activeDuration = 0.12f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsValid(context)) return;
        context.Enemy.Combat.EnableHitbox(context.AttackData.hitboxType);
        DisableLaterAsync(context).Forget();
    }

    public override void OnAttackEnd(EnemyAttackContext context)
    {
        if (IsValid(context))
            context.Enemy.Combat.DisableHitbox(context.AttackData.hitboxType);
    }

    private async UniTaskVoid DisableLaterAsync(EnemyAttackContext context)
    {
        float speed = context.Enemy.MinibossBehaviour?
            .ModifyAttackAnimationSpeed(1f) ?? 1f;
        await UniTask.Delay(System.TimeSpan.FromSeconds(activeDuration / speed));

        if (context.Enemy != null &&
            context.Enemy.Combat.CurrentAttackData == context.AttackData)
            context.Enemy.Combat.DisableHitbox(context.AttackData.hitboxType);
    }

    private static bool IsValid(EnemyAttackContext context)
    {
        return context?.Enemy != null && context.AttackData != null;
    }
}
