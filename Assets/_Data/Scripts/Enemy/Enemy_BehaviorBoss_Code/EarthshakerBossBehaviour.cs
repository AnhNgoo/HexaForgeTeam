using System.Collections.Generic;
using UnityEngine;

public class EarthshakerBossBehaviour : EnemyFinalBossBehaviour
{
    [Header("Attack References")]
    [SerializeField] private AttackDataSO slashRightAttack;
    [SerializeField] private AttackDataSO slashLeftAttack;
    [SerializeField] private AttackDataSO earthSmashAttack;
    [SerializeField] private AttackDataSO crushingChargeAttack;
    [SerializeField] private AttackDataSO earthPillarsAttack;
    [SerializeField] private AttackDataSO worldBreakerPillarsAttack;

    [Header("Passives")]
    [SerializeField, Range(0f, 1f)] private float stoneArmorReduction = 0.4f;
    [SerializeField] private float momentumDamageMultiplier = 1.3f;
    [SerializeField] private float poiseRecoveryDelay = 15f;
    [SerializeField] private float poiseRecoveryPerSecond = 20f;

    [Header("Phase")]
    [SerializeField] private float phase2ChargeSpeedMultiplier = 1.25f;
    [SerializeField, Range(0f, 1f)] private float phase2ExtensionChance = 0.3f;
    [SerializeField, Range(0f, 1f)] private float phase3ChainChance = 0.55f;
    [SerializeField] private float extensionCooldown = 6f;

    [Header("Punisher / Wall Crusher")]
    [SerializeField] private float punisherRange = 4f;
    [SerializeField] private float wallCheckDistance = 4f;
    [SerializeField] private float wallCrusherCooldown = 10f;

    [Header("Ultimate")]
    [SerializeField, Range(0f, 1f)] private float ultimateHealthRatio = 0.1f;

    private readonly Queue<AttackDataSO> _forcedAttacks = new();
    private bool _momentumReady;
    private bool _punisherReady;
    private bool _ultimateUsed;
    private bool _worldBreakerActive;
    private float _nextExtensionTime;
    private float _nextWallCrusherTime;
    private float _lastDamageTime;

    public bool IsPhase2Active => HasEnteredPhase2;
    public bool IsWorldBreakerActive => _worldBreakerActive;

    private void OnEnable()
    {
        if (Enemy == null) return;
        Enemy.EventManager.OnTakeDamage -= HandleTakeDamage;
        Enemy.EventManager.OnTakeDamage += HandleTakeDamage;
        _lastDamageTime = Time.time;
    }

    protected override void OnDisable()
    {
        if (Enemy != null)
            Enemy.EventManager.OnTakeDamage -= HandleTakeDamage;
        base.OnDisable();
    }

    protected override void Update()
    {
        base.Update();

        if (Enemy == null || Enemy.Health.CurrentHealth <= 0f ||
            Time.time < _lastDamageTime + poiseRecoveryDelay)
            return;

        Enemy.PoiseSystem.RecoverPoise(poiseRecoveryPerSecond * Time.deltaTime);
    }

    public override float ModifyIncomingDamage(float damage, Transform attacker)
    {
        return damage * (1f - stoneArmorReduction);
    }

    public override float ModifyAttackAnimationSpeed(float speed)
    {
        return HasEnteredPhase2 ? speed * Phase2AttackSpeedMultiplier : speed;
    }

    public override float ModifyChargeDuration(float duration)
    {
        float multiplier = HasEnteredPhase2 ? phase2ChargeSpeedMultiplier : 1f;
        if (_worldBreakerActive) multiplier *= 1.15f;
        return duration / multiplier;
    }

    public override float ConsumeNextAttackDamageMultiplier()
    {
        float multiplier = 1f;
        if (_momentumReady)
        {
            _momentumReady = false;
            multiplier *= momentumDamageMultiplier;
        }
        return ApplyEnrageDamage(multiplier);
    }

    public override AttackDataSO ChooseForcedAttack(float distance)
    {
        if (!_ultimateUsed && Enemy.Health.CurrentHealth <=
            Enemy.Data.maxHealth * ultimateHealthRatio)
        {
            _ultimateUsed = true;
            _worldBreakerActive = true;
            _forcedAttacks.Clear();
            _forcedAttacks.Enqueue(crushingChargeAttack);
            _forcedAttacks.Enqueue(earthSmashAttack);
            _forcedAttacks.Enqueue(worldBreakerPillarsAttack);
        }

        AttackDataSO queued = TakeQueuedAttack(distance);
        if (queued != null) return queued;

        if (_punisherReady)
        {
            _punisherReady = false;
            if (CanUse(slashRightAttack, distance)) return slashRightAttack;
        }

        Transform target = Enemy.Detection.CurrentTarget;
        if (Arena != null && target != null && Time.time >= _nextWallCrusherTime && Arena.IsNearWall(target.position, wallCheckDistance) && CanUse(crushingChargeAttack, distance))
        {
            _nextWallCrusherTime = Time.time + wallCrusherCooldown;
            return crushingChargeAttack;
        }

        return null;
    }

    public override void OnAttackStarted(AttackDataSO attack)
    {
        if (attack == worldBreakerPillarsAttack)
        {
            _worldBreakerActive = false;
            return;
        }

        if (_worldBreakerActive) return;

        if (attack == slashRightAttack)
        {
            _forcedAttacks.Enqueue(slashLeftAttack);
            return;
        }

        if (attack != slashLeftAttack || Time.time < _nextExtensionTime)
            return;

        float chance = IsPhase3Active ? phase3ChainChance :
            HasEnteredPhase2 ? phase2ExtensionChance : 0f;

        if (Random.value > chance) return;
        _nextExtensionTime = Time.time + extensionCooldown;

        if (IsPhase3Active)
        {
            _forcedAttacks.Enqueue(crushingChargeAttack);
            _forcedAttacks.Enqueue(earthSmashAttack);
            _forcedAttacks.Enqueue(earthPillarsAttack);
        }
        else
        {
            _forcedAttacks.Enqueue(Random.value < 0.5f
                ? earthSmashAttack
                : crushingChargeAttack);
        }
    }

    public void NotifyEarthSmashImpact(Transform target)
    {
        _momentumReady = true;
        if (target == null) return;
        Vector3 delta = target.position - Enemy.MyTransform.position;
        delta.y = 0f;
        _punisherReady = delta.magnitude <= punisherRange;
    }

    private AttackDataSO TakeQueuedAttack(float distance)
    {
        while (_forcedAttacks.Count > 0)
        {
            AttackDataSO attack = _forcedAttacks.Dequeue();
            if (attack != null && (_worldBreakerActive || CanUse(attack, distance)))
                return attack;
        }
        return null;
    }

    private void HandleTakeDamage(float damage) => _lastDamageTime = Time.time;

    private static bool CanUse(AttackDataSO attack, float distance)
    {
        return attack != null && distance >= attack.minAttackRange &&
               distance <= attack.maxAttackRange;
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();
        _forcedAttacks.Clear();
        _momentumReady = false;
        _punisherReady = false;
        _ultimateUsed = false;
        _worldBreakerActive = false;
        _nextExtensionTime = 0f;
        _nextWallCrusherTime = 0f;
        _lastDamageTime = Time.time;
    }
}
