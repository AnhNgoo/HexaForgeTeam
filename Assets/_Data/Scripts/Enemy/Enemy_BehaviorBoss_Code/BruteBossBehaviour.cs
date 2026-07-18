using UnityEngine;

public class BruteBossBehaviour : EnemyBossBehaviour
{
    [Header("Attack References")]
    [SerializeField] private AttackDataSO slashAttack;
    [SerializeField] private AttackDataSO swingAttack;
    [SerializeField] private AttackDataSO kickAttack;
    [SerializeField] private AttackDataSO jumpSmashAttack;

    [Header("Passives")]
    [SerializeField, Range(0f, 1f)] private float titanSkinReduction = 0.3f;
    [SerializeField] private float momentumDamageMultiplier = 1.3f;
    [SerializeField] private float phase2BoulderSpeedMultiplier = 1.35f;

    [Header("Punisher")]
    [SerializeField] private float punisherRange = 3f;

    [Header("Ultimate")]
    [SerializeField, Range(0f, 1f)] private float ultimateHealthRatio = 0.2f;

    private AttackDataSO _forcedAttack;
    private bool _momentumReady;
    private bool _punisherReady;
    private bool _ultimateQueued;
    private bool _ultimateUsed;
    private bool _currentJumpIsCataclysm;
    private bool _jumpSequenceActive;

    public bool IsPhase2Active => IsPhase2;

    public override float ModifyIncomingDamage(float damage, Transform attacker)
    {
        // DamageReceiver không gọi modifier khi đang stagger,
        // nên Titan Skin tự mất hiệu lực khi poise bị phá.
        return damage * (1f - titanSkinReduction);
    }

    public override float ModifyAttackAnimationSpeed(float speed)
    {
        return IsPhase2
            ? speed * Phase2AttackSpeedMultiplier
            : speed;
    }

    public override float ModifyProjectileSpeed(float speed)
    {
        return IsPhase2
            ? speed * phase2BoulderSpeedMultiplier
            : speed;
    }

    public override float ConsumeNextAttackDamageMultiplier()
    {
        if (!_momentumReady)
            return 1f;

        _momentumReady = false;
        return momentumDamageMultiplier;
    }

    public override AttackDataSO ChooseForcedAttack(float distance)
    {
        if (!_ultimateUsed &&
            Enemy.Health.CurrentHealth <=
            Enemy.Data.maxHealth * ultimateHealthRatio &&
            CanUse(jumpSmashAttack, distance))
        {
            _ultimateQueued = true;
            return jumpSmashAttack;
        }

        if (_punisherReady)
        {
            _punisherReady = false;

            if (CanUse(kickAttack, distance))
                return kickAttack;
        }

        if (_forcedAttack == null)
            return null;

        AttackDataSO result = _forcedAttack;
        _forcedAttack = null;

        return CanUse(result, distance) ? result : null;
    }

    public override void OnAttackStarted(AttackDataSO attack)
    {
        if (attack == slashAttack)
            _forcedAttack = swingAttack;
        else if (attack == swingAttack)
            _forcedAttack = kickAttack;
        else if (attack == kickAttack)
            _forcedAttack = null;

        if (attack != jumpSmashAttack)
            return;

        _currentJumpIsCataclysm = _ultimateQueued;

        if (_ultimateQueued)
        {
            _ultimateQueued = false;
            _ultimateUsed = true;
        }
    }

    public bool TryBeginJumpSequence(
        out bool useDoubleSmash,
        out bool useCataclysm)
    {
        useDoubleSmash = false;
        useCataclysm = false;

        if (_jumpSequenceActive)
            return false;

        _jumpSequenceActive = true;
        useCataclysm = _currentJumpIsCataclysm;
        useDoubleSmash = IsPhase2 && !useCataclysm;
        return true;
    }

    public void NotifyJumpSmashFinished(Transform target)
    {
        _momentumReady = true;

        if (target != null)
        {
            float distance = Vector3.Distance(
                Enemy.MyTransform.position,
                target.position
            );

            _punisherReady = distance <= punisherRange;
        }
    }

    public void SetJumpSequenceLocked(bool locked)
    {
        IsActionLocked = locked;

        if (locked)
            Enemy.Locomotion.StopMoving();
    }

    public void EndJumpSequence()
    {
        _jumpSequenceActive = false;
        _currentJumpIsCataclysm = false;
        IsActionLocked = false;
    }

    private bool CanUse(AttackDataSO attack, float distance)
    {
        return attack != null &&
               distance >= attack.minAttackRange &&
               distance <= attack.maxAttackRange;
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        _forcedAttack = null;
        _momentumReady = false;
        _punisherReady = false;
        _ultimateQueued = false;
        _ultimateUsed = false;
        _currentJumpIsCataclysm = false;
        _jumpSequenceActive = false;
    }
}