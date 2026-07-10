using UnityEngine;

public abstract class EnemyBossBehaviour : EnemyMinibossBehaviour
{
    [SerializeField, Range(0f, 1f)] private float phase2HealthPercent = 0.5f;
    [SerializeField] private float phase2AttackSpeedMultiplier = 1.2f;

    protected bool IsPhase2 =>
        Enemy != null &&
        Enemy.Health.CurrentHealth <= Enemy.Data.maxHealth * phase2HealthPercent;

    public override float ModifyAttackCooldown(float cooldown)
    {
        return IsPhase2 ? cooldown / phase2AttackSpeedMultiplier : cooldown;
    }
}