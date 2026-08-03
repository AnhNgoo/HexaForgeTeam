using UnityEngine;

public abstract class EnemyFinalBossBehaviour : EnemyBossBehaviour
{
    [Header("Final Boss Phase")]
    [SerializeField, Range(0f, 1f)] private float phase3HealthPercent = 0.2f;

    [Header("Enrage")]
    [SerializeField] private float enrageDelay = 360f;
    [SerializeField] private float enrageDamageMultiplier = 1.3f;
    [SerializeField] private float enrageCooldownMultiplier = 0.7f;
    [SerializeField] private float enrageMoveSpeedMultiplier = 1.2f;

    private float _combatStartTime = -1f;

    public bool IsPhase3Active => Enemy != null && Enemy.Health.CurrentHealth <= Enemy.Data.maxHealth * phase3HealthPercent;

    public bool IsEnraged { get; private set; }

    protected FinalBossArena Arena { get; private set; }

    public void ConfigureArena(FinalBossArena arena)
    {
        Arena = arena;
    }

    protected virtual void Update()
    {
        if (Enemy == null || Enemy.Health.CurrentHealth <= 0f || IsEnraged)
            return;

        if (_combatStartTime < 0f)
        {
            if (Enemy.Detection.CurrentTarget != null)
                _combatStartTime = Time.time;
            return;
        }

        IsEnraged = Time.time >= _combatStartTime + enrageDelay;
    }

    public override float ModifyAttackCooldown(float cooldown)
    {
        float result = base.ModifyAttackCooldown(cooldown);
        return IsEnraged ? result * enrageCooldownMultiplier : result;
    }

    public override float ModifyMoveSpeed(float speed)
    {
        return IsEnraged ? speed * enrageMoveSpeedMultiplier : speed;
    }

    protected float ApplyEnrageDamage(float multiplier)
    {
        return IsEnraged ? multiplier * enrageDamageMultiplier : multiplier;
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        Arena = null;
        _combatStartTime = -1f;
        IsEnraged = false;
    }
}
