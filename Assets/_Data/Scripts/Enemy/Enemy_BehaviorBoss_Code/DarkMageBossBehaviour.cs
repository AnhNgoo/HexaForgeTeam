using System.Collections.Generic;
using UnityEngine;

public enum DarkMageBurrowMode
{
    Ambush,
    Reposition,
    FakeAmbush
}

public class DarkMageBossBehaviour : EnemyFinalBossBehaviour
{
    [Header("Combo cơ bản")]
    [SerializeField] private AttackDataSO rightSlashAttack;
    [SerializeField] private AttackDataSO leftSlashAttack;
    [SerializeField] private AttackDataSO biteAttack;

    [Header("Phép thuật")]
    [SerializeField] private AttackDataSO burrowAmbushAttack;
    [SerializeField] private AttackDataSO darkOrbAttack;
    [SerializeField] private AttackDataSO shadowRitualAttack;
    [SerializeField] private AttackDataSO meteorRainAttack;
    [SerializeField] private AttackDataSO laserBarrageAttack;
    [SerializeField] private AttackDataSO eclipseOfRuinAttack;

    [Header("Arcane Mastery")]
    [SerializeField, Range(0f, 0.2f)] private float castSpeedPerStack = 0.05f;
    [SerializeField, Min(1)] private int maximumCastStacks = 5;

    [Header("Shadow Escape")]
    [SerializeField, Range(0.05f, 0.5f)] private float healthLostPerEscape = 0.2f;

    [Header("Burrow Hunter")]
    [SerializeField, Min(0f)] private float burrowHunterRange = 4f;
    [SerializeField, Min(0f)] private float closeRangeDuration = 2.2f;
    [SerializeField, Min(0f)] private float burrowHunterCooldown = 7f;

    [Header("Fake Ambush")]
    [SerializeField, Range(0f, 1f)] private float fakeAmbushChance = 0.25f;
    [SerializeField, Min(0f)] private float fakeAmbushCooldown = 12f;

    [Header("Spell Chain")]
    [SerializeField, Range(0f, 1f)] private float spellChainChance = 0.35f;
    [SerializeField, Min(0f)] private float spellChainCooldown = 6f;

    [Header("Phase 2")]
    [SerializeField, Range(0.1f, 1f)] private float phase2BurrowCooldownMultiplier = 0.7f;
    [SerializeField, Range(0.1f, 1f)] private float phase2MeteorCooldownMultiplier = 0.7f;

    [Header("Phase 3")]
    [SerializeField, Min(0f)] private float phase3LaserInterval = 8f;
    [SerializeField, Min(0f)] private float phase3ChainCooldown = 6f;

    [Header("Enrage")]
    [SerializeField, Min(1f)] private float enrageCastSpeedMultiplier = 1.3f;
    [SerializeField, Min(1f)] private float enrageBurrowSpeedMultiplier = 1.5f;

    [Header("Ultimate")]
    [SerializeField, Range(0f, 1f)] private float ultimateHealthRatio = 0.1f;

    private readonly Queue<AttackDataSO> _forcedAttacks = new();

    private int _arcaneStacks;
    private float _currentCastSpeedMultiplier = 1f;
    private float _nextShadowEscapeRatio = 0.8f;
    private float _closeRangeStartTime = -1f;
    private float _nextBurrowHunterTime;
    private float _nextFakeAmbushTime;
    private float _nextSpellChainTime;
    private float _nextLaserTime;
    private float _nextPhase3ChainTime;

    private bool _shadowEscapePending;
    private bool _shadowEscapeForCurrentBurrow;
    private bool _bulletHellForNextDarkOrb;
    private bool _ultimateUsed;

    public bool IsUnderground { get; private set; }
    public bool IsPhase2Active => HasEnteredPhase2;

    private void OnEnable()
    {
        if (Enemy == null)
            return;

        Enemy.EventManager.OnTakeDamage -= HandleTakeDamage;
        Enemy.EventManager.OnTakeDamage += HandleTakeDamage;
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

        if (Enemy == null || Enemy.Health.CurrentHealth <= 0f)
            return;

        Transform target = Enemy.Detection.CurrentTarget;
        if (target == null)
        {
            _closeRangeStartTime = -1f;
            return;
        }

        Vector3 offset = target.position - Enemy.MyTransform.position;
        offset.y = 0f;

        if (offset.sqrMagnitude > burrowHunterRange * burrowHunterRange)
        {
            _closeRangeStartTime = -1f;
            return;
        }

        if (_closeRangeStartTime < 0f)
            _closeRangeStartTime = Time.time;
    }

    public override AttackDataSO ChooseForcedAttack(float distance)
    {
        if (!_ultimateUsed &&
            Enemy.Health.CurrentHealth <= Enemy.Data.maxHealth * ultimateHealthRatio &&
            CanUse(eclipseOfRuinAttack, distance))
        {
            _ultimateUsed = true;
            _shadowEscapePending = false;
            _forcedAttacks.Clear();
            return eclipseOfRuinAttack;
        }

        if (_shadowEscapePending && CanUse(burrowAmbushAttack, distance))
        {
            _shadowEscapePending = false;
            _shadowEscapeForCurrentBurrow = true;
            return burrowAmbushAttack;
        }

        AttackDataSO queuedAttack = TakeQueuedAttack(distance);
        if (queuedAttack != null)
            return queuedAttack;

        if (IsPhase3Active &&
            Time.time >= _nextLaserTime &&
            CanUse(laserBarrageAttack, distance))
        {
            _nextLaserTime = Time.time + phase3LaserInterval;
            return laserBarrageAttack;
        }

        bool stayedCloseLongEnough =
            _closeRangeStartTime >= 0f &&
            Time.time >= _closeRangeStartTime + closeRangeDuration;

        if (stayedCloseLongEnough &&
            Time.time >= _nextBurrowHunterTime &&
            CanUse(burrowAmbushAttack, distance))
        {
            _closeRangeStartTime = -1f;
            _nextBurrowHunterTime = Time.time + burrowHunterCooldown;
            return burrowAmbushAttack;
        }

        return null;
    }

    public override void OnAttackStarted(AttackDataSO attack)
    {
        _currentCastSpeedMultiplier = 1f;

        if (attack == rightSlashAttack)
            _forcedAttacks.Enqueue(leftSlashAttack);
        else if (attack == leftSlashAttack)
            _forcedAttacks.Enqueue(biteAttack);

        if (IsCastSpell(attack))
        {
            _currentCastSpeedMultiplier =
                1f + _arcaneStacks * castSpeedPerStack;

            _arcaneStacks = Mathf.Min(maximumCastStacks, _arcaneStacks + 1);
        }

        if (attack == shadowRitualAttack && HasEnteredPhase2)
        {
            _bulletHellForNextDarkOrb = true;
            _forcedAttacks.Enqueue(darkOrbAttack);
            return;
        }

        bool ritualFollowUp =
            attack == darkOrbAttack && _bulletHellForNextDarkOrb;

        if (attack == darkOrbAttack &&
            !ritualFollowUp &&
            Time.time >= _nextSpellChainTime &&
            Random.value <= spellChainChance)
        {
            _nextSpellChainTime = Time.time + spellChainCooldown;
            _forcedAttacks.Enqueue(
                Random.value < 0.5f ? shadowRitualAttack : meteorRainAttack
            );
        }

        if (IsPhase3Active && attack == laserBarrageAttack)
            _forcedAttacks.Enqueue(burrowAmbushAttack);
    }

    public DarkMageBurrowMode ConsumeBurrowMode()
    {
        if (_shadowEscapeForCurrentBurrow)
        {
            _shadowEscapeForCurrentBurrow = false;
            return DarkMageBurrowMode.Reposition;
        }

        if (Time.time >= _nextFakeAmbushTime &&
            Random.value <= fakeAmbushChance)
        {
            _nextFakeAmbushTime = Time.time + fakeAmbushCooldown;
            return DarkMageBurrowMode.FakeAmbush;
        }

        if (IsPhase3Active && Time.time >= _nextPhase3ChainTime)
        {
            _nextPhase3ChainTime = Time.time + phase3ChainCooldown;
            _bulletHellForNextDarkOrb = true;
            _forcedAttacks.Enqueue(darkOrbAttack);
        }

        return DarkMageBurrowMode.Ambush;
    }

    public bool ConsumeBulletHellRequest()
    {
        bool requested = _bulletHellForNextDarkOrb;
        _bulletHellForNextDarkOrb = false;
        return requested;
    }

    public int ModifyDarkOrbCount(int baseCount, int phase2AdditionalCount)
    {
        int result = baseCount;

        if (HasEnteredPhase2)
            result += phase2AdditionalCount;

        if (IsEnraged)
            result += 2;

        return result;
    }

    public override float ModifyAttackCooldown(float cooldown, AttackDataSO attack)
    {
        float result = base.ModifyAttackCooldown(cooldown, attack);

        if (HasEnteredPhase2 && attack == burrowAmbushAttack)
            result *= phase2BurrowCooldownMultiplier;

        if (HasEnteredPhase2 && attack == meteorRainAttack)
            result *= phase2MeteorCooldownMultiplier;

        return result;
    }

    public override float ModifyAttackAnimationSpeed(float speed)
    {
        AttackDataSO attack = Enemy?.Combat?.CurrentAttackData;
        float result = speed;

        if (IsCastSpell(attack))
            result *= _currentCastSpeedMultiplier;

        if (IsEnraged && IsCastSpell(attack))
            result *= enrageCastSpeedMultiplier;

        if (IsEnraged &&
            (attack == burrowAmbushAttack || attack == eclipseOfRuinAttack))
        {
            result *= enrageBurrowSpeedMultiplier;
        }

        return result;
    }

    public override float ConsumeNextAttackDamageMultiplier()
    {
        return ApplyEnrageDamage(1f);
    }

    public override float ModifyIncomingDamage(float damage, Transform attacker)
    {
        return IsUnderground ? 0f : damage;
    }

    public bool BeginSpecialAction()
    {
        if (IsActionLocked || Enemy == null)
            return false;

        IsActionLocked = true;
        Enemy.Locomotion.StopMoving();
        Enemy.Combat.ForceCloseHitbox();
        return true;
    }

    public void EnterDarkVeil(Transform target)
    {
        IsUnderground = true;
        Enemy.Combat.ForceCloseHitbox();

        if (Enemy.MainCollider != null)
            Enemy.MainCollider.enabled = false;

        target?.GetComponentInParent<CharacterLockTarget>()?.ForceUnlockTarget();
    }

    public void ExitDarkVeil()
    {
        IsUnderground = false;

        if (Enemy != null &&
            Enemy.Health.CurrentHealth > 0f &&
            Enemy.MainCollider != null)
        {
            Enemy.MainCollider.enabled = true;
        }
    }

    public void EndSpecialAction()
    {
        ExitDarkVeil();
        IsActionLocked = false;
    }

    private void HandleTakeDamage(float damage)
    {
        float healthRatio = Enemy.Health.CurrentHealth / Enemy.Data.maxHealth;

        if (healthRatio > _nextShadowEscapeRatio)
            return;

        while (_nextShadowEscapeRatio > 0f &&
               healthRatio <= _nextShadowEscapeRatio)
        {
            _nextShadowEscapeRatio -= healthLostPerEscape;
        }

        _shadowEscapePending = true;
    }

    private AttackDataSO TakeQueuedAttack(float distance)
    {
        while (_forcedAttacks.Count > 0)
        {
            AttackDataSO attack = _forcedAttacks.Dequeue();
            if (CanUse(attack, distance))
                return attack;
        }

        return null;
    }

    private bool IsCastSpell(AttackDataSO attack)
    {
        return attack == darkOrbAttack ||
               attack == shadowRitualAttack ||
               attack == meteorRainAttack ||
               attack == laserBarrageAttack ||
               attack == eclipseOfRuinAttack;
    }

    private static bool CanUse(AttackDataSO attack, float distance)
    {
        return attack != null &&
               distance >= attack.minAttackRange &&
               distance <= attack.maxAttackRange;
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        _forcedAttacks.Clear();
        _arcaneStacks = 0;
        _currentCastSpeedMultiplier = 1f;
        _nextShadowEscapeRatio = 0.8f;
        _closeRangeStartTime = -1f;
        _nextBurrowHunterTime = 0f;
        _nextFakeAmbushTime = 0f;
        _nextSpellChainTime = 0f;
        _nextLaserTime = 0f;
        _nextPhase3ChainTime = 0f;
        _shadowEscapePending = false;
        _shadowEscapeForCurrentBurrow = false;
        _bulletHellForNextDarkOrb = false;
        _ultimateUsed = false;

        EndSpecialAction();
    }
}
