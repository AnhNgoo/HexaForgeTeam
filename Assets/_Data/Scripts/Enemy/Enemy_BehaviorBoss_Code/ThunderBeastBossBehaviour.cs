using UnityEngine;

public class ThunderBeastBossBehaviour : EnemyBossBehaviour
{
    [Header("Attack References")]
    [SerializeField] private AttackDataSO biteAttack;
    [SerializeField] private AttackDataSO pounceBiteAttack;
    [SerializeField] private AttackDataSO chargeTelegraphAttack;
    [SerializeField] private AttackDataSO chargeDashAttack;
    [SerializeField] private AttackDataSO lightningPillarsAttack;

    [Header("Predator Instinct")]
    [SerializeField] private float predatorChargeRange = 10f;

    [Header("Lightning Body")]
    [SerializeField] private float lightningBodySpeedMultiplier = 1.3f;
    [SerializeField] private float lightningBodyDuration = 3f;

    [Header("Double Dash")]
    [SerializeField, Range(0f, 1f)]
    private float doubleDashChance = 0.55f;
    [SerializeField] private float phase2ChargeSpeedMultiplier = 1.25f;

    [Header("Ultimate")]
    [SerializeField, Range(0f, 1f)]
    private float ultimateHealthRatio = 0.2f;
    [SerializeField] private int ultimateDashCount = 3;

    [Header("Optional Visual")]
    [SerializeField] private TrailRenderer chargeTrail;

    private AttackDataSO _forcedAttack;
    private int _remainingChargeDashes;
    private float _nextChargeTime;
    private float _lightningBodyEndTime;
    private bool _lightningBodyActive;
    private bool _ultimateSequence;
    private bool _ultimateUsed;

    public bool IsPhase2Active => IsPhase2;

    private void Update()
    {
        if (!_lightningBodyActive ||
            Time.time < _lightningBodyEndTime ||
            Enemy == null)
        {
            return;
        }

        _lightningBodyActive = false;
        Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
    }

    public override float ModifyMoveSpeed(float speed)
    {
        return _lightningBodyActive
            ? speed * lightningBodySpeedMultiplier
            : speed;
    }

    public override float ModifyAttackAnimationSpeed(float speed)
    {
        return IsPhase2
            ? speed * Phase2AttackSpeedMultiplier
            : speed;
    }

    public float ModifyChargeDuration(float duration)
    {
        return IsPhase2
            ? duration / phase2ChargeSpeedMultiplier
            : duration;
    }

    public override AttackDataSO ChooseForcedAttack(float distance)
    {
        if (_forcedAttack != null)
        {
            AttackDataSO result = _forcedAttack;
            _forcedAttack = null;
            return CanUse(result, distance) ? result : null;
        }

        if (!_ultimateUsed &&
            Enemy.Health.CurrentHealth <=
            Enemy.Data.maxHealth * ultimateHealthRatio &&
            CanUse(chargeTelegraphAttack, distance))
        {
            _ultimateUsed = true;
            _ultimateSequence = true;
            return chargeTelegraphAttack;
        }

        if (distance >= predatorChargeRange &&
            Time.time >= _nextChargeTime &&
            CanUse(chargeTelegraphAttack, distance))
        {
            return chargeTelegraphAttack;
        }

        return null;
    }

    public override void OnAttackStarted(AttackDataSO attack)
    {
        if (attack == biteAttack)
        {
            _forcedAttack = pounceBiteAttack;
            return;
        }

        if (attack == chargeTelegraphAttack)
        {
            _nextChargeTime =
                Time.time + Mathf.Max(0.1f, attack.cooldown);

            _remainingChargeDashes = _ultimateSequence
                ? Mathf.Max(1, ultimateDashCount)
                : IsPhase2 && Random.value <= doubleDashChance
                    ? 2
                    : 1;

            _forcedAttack = chargeDashAttack;
            return;
        }

        if (attack != chargeDashAttack)
            return;

        _remainingChargeDashes--;

        if (_remainingChargeDashes > 0)
        {
            _forcedAttack = chargeDashAttack;
            return;
        }

        if (_ultimateSequence)
        {
            _ultimateSequence = false;
            _forcedAttack = lightningPillarsAttack;
        }
    }

    public override void OnAttackHit(
        AttackDataSO attack,
        Collider target)
    {
        if (attack != chargeDashAttack)
            return;

        _lightningBodyActive = true;
        _lightningBodyEndTime =
            Time.time + lightningBodyDuration;

        Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
    }

    public void SetChargeTrail(bool value)
    {
        if (chargeTrail == null)
            return;

        if (value)
            chargeTrail.Clear();

        chargeTrail.emitting = value;
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        _forcedAttack = null;
        _remainingChargeDashes = 0;
        _nextChargeTime = 0f;
        _lightningBodyEndTime = 0f;
        _lightningBodyActive = false;
        _ultimateSequence = false;
        _ultimateUsed = false;

        SetChargeTrail(false);

        if (Enemy != null)
            Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
    }

    private static bool CanUse(
        AttackDataSO attack,
        float distance)
    {
        return attack != null &&
               distance >= attack.minAttackRange &&
               distance <= attack.maxAttackRange;
    }
}
