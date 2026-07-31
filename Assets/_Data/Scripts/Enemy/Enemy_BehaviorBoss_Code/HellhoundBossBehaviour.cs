using UnityEngine;

public class HellhoundBossBehaviour : EnemyBossBehaviour
{
    [Header("Attack References")]
    [SerializeField] private AttackDataSO leftSlashAttack;
    [SerializeField] private AttackDataSO rightSlashAttack;
    [SerializeField] private AttackDataSO biteAttack;
    [SerializeField] private AttackDataSO pounceSmashAttack;
    [SerializeField] private AttackDataSO infernalHowlAttack;

    [Header("Bloodthirst")]
    [SerializeField] private int maxBloodthirstStacks = 5;
    [SerializeField] private float attackSpeedPerStack = 0.03f;

    [Header("Infernal Howl")]
    [SerializeField] private float howlDuration = 8f;
    [SerializeField] private float howlMoveSpeedMultiplier = 1.25f;
    [SerializeField] private float howlAttackSpeedMultiplier = 1.25f;
    [SerializeField] private ParticleSystem howlAura;

    [Header("Pounce Smash")]
    [SerializeField] private float pounceStunDuration = 0.45f;
    [SerializeField] private float phase2PounceCooldownMultiplier = 1.5f;

    [Header("Ultimate")]
    [SerializeField, Range(0f, 1f)]
    private float ultimateHealthRatio = 0.2f;
    [SerializeField] private int ultimatePounceCount = 3;

    private AttackDataSO _forcedAttack;
    private int _bloodthirstStacks;
    private int _biteExtensionsUsed;
    private int _remainingUltimatePounces;
    private float _howlEndTime;
    private bool _howlActive;
    private bool _ultimateActive;
    private bool _ultimateUsed;

    private void Update()
    {
        if (!_howlActive ||
            Time.time < _howlEndTime ||
            Enemy == null)
        {
            return;
        }

        _howlActive = false;
        SetHowlAura(false);
        Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
    }

    public override float ModifyMoveSpeed(float speed)
    {
        return _howlActive
            ? speed * howlMoveSpeedMultiplier
            : speed;
    }

    public override float ModifyAttackAnimationSpeed(float speed)
    {
        speed *= 1f +
                 _bloodthirstStacks *
                 attackSpeedPerStack;

        if (_howlActive)
            speed *= howlAttackSpeedMultiplier;

        return speed;
    }

    public override float ModifyAttackCooldown(
        float cooldown,
        AttackDataSO attack)
    {
        float result =
            base.ModifyAttackCooldown(cooldown, attack);

        if (IsPhase2 &&
            attack == pounceSmashAttack)
        {
            result /= Mathf.Max(
                1f,
                phase2PounceCooldownMultiplier
            );
        }

        return result;
    }

    public override AttackDataSO ChooseForcedAttack(
        float distance)
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
            CanUse(infernalHowlAttack, distance))
        {
            _ultimateUsed = true;
            _ultimateActive = true;
            _remainingUltimatePounces =
                Mathf.Max(1, ultimatePounceCount);

            return infernalHowlAttack;
        }

        return null;
    }

    public override void OnAttackStarted(AttackDataSO attack)
    {
        if (attack == leftSlashAttack)
        {
            _biteExtensionsUsed = 0;
            _forcedAttack = rightSlashAttack;
            return;
        }

        if (attack == rightSlashAttack)
        {
            _forcedAttack = biteAttack;
            return;
        }

        if (attack == infernalHowlAttack)
        {
            ActivateHowl();

            if (_ultimateActive)
                _forcedAttack = pounceSmashAttack;

            return;
        }

        if (attack != pounceSmashAttack ||
            !_ultimateActive)
        {
            return;
        }

        _remainingUltimatePounces--;

        if (_remainingUltimatePounces > 0)
            _forcedAttack = pounceSmashAttack;
        else
            _ultimateActive = false;
    }

    public override void OnAttackHit(
        AttackDataSO attack,
        Collider target)
    {
        if (IsBloodthirstAttack(attack))
        {
            _bloodthirstStacks = Mathf.Min(
                maxBloodthirstStacks,
                _bloodthirstStacks + 1
            );
        }

        if (attack == pounceSmashAttack)
        {
            target.GetComponentInParent<CharacterMovement>()?
                .LockMovement(pounceStunDuration);
        }

        if (attack != biteAttack)
            return;

        int allowedExtensions = IsPhase2 ? 2 : 1;

        if (_biteExtensionsUsed >= allowedExtensions)
            return;

        _biteExtensionsUsed++;
        _forcedAttack = biteAttack;
    }

    private void ActivateHowl()
    {
        _howlActive = true;
        _howlEndTime = Time.time + howlDuration;
        SetHowlAura(true);
        Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
    }

    private void SetHowlAura(bool value)
    {
        if (howlAura == null)
            return;

        if (value)
            howlAura.Play(true);
        else
            howlAura.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmittingAndClear
            );
    }

    private bool IsBloodthirstAttack(
        AttackDataSO attack)
    {
        return attack == leftSlashAttack ||
               attack == rightSlashAttack ||
               attack == biteAttack ||
               attack == pounceSmashAttack;
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        _forcedAttack = null;
        _bloodthirstStacks = 0;
        _biteExtensionsUsed = 0;
        _remainingUltimatePounces = 0;
        _howlEndTime = 0f;
        _howlActive = false;
        _ultimateActive = false;
        _ultimateUsed = false;

        SetHowlAura(false);

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
