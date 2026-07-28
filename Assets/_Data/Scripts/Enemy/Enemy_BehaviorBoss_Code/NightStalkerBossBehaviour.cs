using UnityEngine;
using UnityEngine.AI;

public class NightStalkerBossBehaviour : EnemyBossBehaviour
{
    [Header("Attack References")]
    [SerializeField] private AttackDataSO biteAttack;
    [SerializeField] private AttackDataSO kickAttack;
    [SerializeField] private AttackDataSO vacuumSpinAttack;
    [SerializeField] private AttackDataSO shadowRainAttack;
    [SerializeField] private AttackDataSO eclipseAttack;

    [Header("Night Hunter")]
    [SerializeField] private float hunterRange = 10f;
    [SerializeField] private float hunterSpeedMultiplier = 1.4f;

    [Header("Phase 2")]
    [SerializeField] private float phase2VacuumPullMultiplier = 1.35f;
    [SerializeField] private float phase2VacuumRadiusMultiplier = 1.2f;
    [SerializeField] private float phase2ShadowRainCooldownMultiplier = 1.5f;

    [Header("Sky Hunter")]
    [SerializeField] private float skyHunterInitialDelay = 3f;
    [SerializeField] private float skyHunterCooldown = 7f;
    [SerializeField] private float orbitDuration = 1.8f;
    [SerializeField] private float orbitRadius = 7f;
    [SerializeField] private float orbitAngularSpeed = 150f;
    [SerializeField] private float orbitSpeedMultiplier = 1.25f;
    [SerializeField] private float diveSpeedMultiplier = 2f;
    [SerializeField] private float diveStopDistance = 2f;
    [SerializeField] private float diveTrackingDuration = 0.25f;
    [SerializeField] private float diveTimeout = 1.2f;
    [SerializeField] private float diveArrivalDistance = 0.6f;
    [SerializeField] private float navMeshSampleRadius = 3f;
    [SerializeField] private float maxVerticalDifference = 1.5f;
    [SerializeField] private TrailRenderer flightTrail;

    [Header("Ultimate")]
    [SerializeField, Range(0f, 1f)]
    private float ultimateHealthRatio = 0.2f;

    private enum SkyHunterState
    {
        None,
        Orbit,
        Dive
    }

    private SkyHunterState _skyHunterState;
    private AttackDataSO _forcedAttack;
    private Vector3 _diveDestination;
    private float _orbitAngle;
    private float _skyStateEndTime;
    private float _diveTrackingEndTime;
    private float _nextSkyHunterTime;

    private bool _vacuumActive;
    private bool _vacuumConsumedForCurrentAttack;
    private bool _currentAttackIsEclipse;
    private bool _ultimateQueued;
    private bool _ultimateUsed;

    public float VacuumPullMultiplier => IsPhase2 ? phase2VacuumPullMultiplier : 1f;

    public float VacuumRadiusMultiplier => IsPhase2 ? phase2VacuumRadiusMultiplier : 1f;

    public AttackDataSO ShadowRainAttack => shadowRainAttack;

    protected override void Awake()
    {
        base.Awake();
        SetTrail(false);
    }

    private void OnEnable()
    {
        _nextSkyHunterTime = Time.time + skyHunterInitialDelay;
    }

    private void Update()
    {
        if (_skyHunterState == SkyHunterState.None || Enemy == null)
            return;

        if (Enemy.StateMachine.CurrentState !=
            Enemy.StateMachine.EnemyAttackState)
        {
            CancelSkyHunter();
        }
    }

    public override float ModifyMoveSpeed(float speed)
    {
        if (_skyHunterState != SkyHunterState.None)
            return speed;

        Transform target = Enemy.Detection.CurrentTarget;
        if (target == null)
            return speed;

        Vector3 offset = target.position - Enemy.MyTransform.position;
        offset.y = 0f;

        return offset.sqrMagnitude > hunterRange * hunterRange
            ? speed * hunterSpeedMultiplier
            : speed;
    }

    public override float ModifyAttackAnimationSpeed(float speed)
    {
        return IsPhase2 ? speed * Phase2AttackSpeedMultiplier : speed;
    }

    public override float ModifyAttackCooldown(
        float cooldown,
        AttackDataSO attack)
    {
        float result =
            base.ModifyAttackCooldown(cooldown, attack);

        if (IsPhase2 && attack == shadowRainAttack)
        {
            result /= Mathf.Max(
                1f,
                phase2ShadowRainCooldownMultiplier
            );
        }

        return result;
    }


    public override AttackDataSO ChooseForcedAttack(float distance)
    {
        if (!_ultimateUsed && Enemy.Health.CurrentHealth <= Enemy.Data.maxHealth * ultimateHealthRatio && CanUse(eclipseAttack, distance))
        {
            _ultimateQueued = true;
            return eclipseAttack;
        }

        if (_forcedAttack == null)
            return null;

        AttackDataSO result = _forcedAttack;
        _forcedAttack = null;

        return CanUse(result, distance) ? result : null;
    }

    public override void OnAttackStarted(AttackDataSO attack)
    {
        if (attack == biteAttack)
            _forcedAttack = kickAttack;
        else if (attack == kickAttack)
            _forcedAttack = null;

        if (attack != vacuumSpinAttack && attack != eclipseAttack)
            return;

        _vacuumConsumedForCurrentAttack = false;
        _currentAttackIsEclipse = attack == eclipseAttack && _ultimateQueued;

        if (!_currentAttackIsEclipse)
            return;

        _ultimateQueued = false;
        _ultimateUsed = true;
    }

    public bool TryBeginVacuum(out bool isEclipse)
    {
        isEclipse = false;

        if (_vacuumActive ||
            _vacuumConsumedForCurrentAttack)
        {
            return false;
        }

        _vacuumActive = true;
        _vacuumConsumedForCurrentAttack = true;
        _currentAttackIsEclipse = Enemy.Combat.CurrentAttackData == eclipseAttack;

        IsActionLocked = true;
        isEclipse = _currentAttackIsEclipse;
        Enemy.Locomotion.StopMoving();
        return true;
    }

    public void EndVacuum()
    {
        _vacuumActive = false;
        _currentAttackIsEclipse = false;
        IsActionLocked = false;
    }

    public override bool UpdateSpecialMovement(Transform target)
    {
        if (target == null || _vacuumActive)
            return false;

        if (_skyHunterState == SkyHunterState.Orbit)
            return UpdateOrbit(target);

        if (_skyHunterState == SkyHunterState.Dive)
            return UpdateDive(target);

        if (ShouldUseUltimate() ||
            Time.time < _nextSkyHunterTime)
        {
            return false;
        }

        BeginOrbit(target);
        return true;
    }

    private void BeginOrbit(Transform target)
    {
        Vector3 offset = Enemy.MyTransform.position - target.position;
        offset.y = 0f;

        _orbitAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;

        _skyHunterState = SkyHunterState.Orbit;
        _skyStateEndTime = Time.time + orbitDuration;

        Enemy.AnimatorController.PlayAnimation(Enemy.AnimatorController.ChaseHash);

        Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed * orbitSpeedMultiplier);

        SetTrail(false);
    }

    private bool UpdateOrbit(Transform target)
    {
        if (Time.time >= _skyStateEndTime)
        {
            BeginDive(target);
            return _skyHunterState != SkyHunterState.None;
        }

        _orbitAngle += orbitAngularSpeed * Time.deltaTime;

        float radians = _orbitAngle * Mathf.Deg2Rad;
        Vector3 rawPoint = target.position + new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * orbitRadius;

        if (TrySampleMovementPoint(rawPoint, out Vector3 point))
            Enemy.Locomotion.MoveToTarget(point, 0.2f);

        return true;
    }

    private void BeginDive(Transform target)
    {
        if (!TryUpdateDiveDestination(target))
        {
            CancelSkyHunter();
            return;
        }

        _skyHunterState = SkyHunterState.Dive;
        _skyStateEndTime = Time.time + diveTimeout;
        _diveTrackingEndTime = Time.time + diveTrackingDuration;

        Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed * diveSpeedMultiplier);

        SetTrail(true);
    }

    private bool UpdateDive(Transform target)
    {
        if (Time.time <= _diveTrackingEndTime)
            TryUpdateDiveDestination(target);

        Vector3 remaining = _diveDestination - Enemy.MyTransform.position;
        remaining.y = 0f;

        if (remaining.sqrMagnitude <= diveArrivalDistance * diveArrivalDistance || Time.time >= _skyStateEndTime)
        {
            FinishSkyHunter();
            return false;
        }

        Enemy.Locomotion.MoveToTarget(_diveDestination, diveArrivalDistance);

        return true;
    }

    private bool TryUpdateDiveDestination(Transform target)
    {
        Vector3 direction = target.position - Enemy.MyTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            direction = Enemy.MyTransform.forward;

        Vector3 rawPoint = target.position - direction.normalized * diveStopDistance;

        if (!TrySampleMovementPoint(rawPoint, out Vector3 point))
            return false;

        _diveDestination = point;
        return true;
    }

    private void FinishSkyHunter()
    {
        _skyHunterState = SkyHunterState.None;
        _nextSkyHunterTime = Time.time + skyHunterCooldown;

        Enemy.Locomotion.StopMoving();
        Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
        SetTrail(false);

        _forcedAttack = biteAttack;
    }

    private void CancelSkyHunter()
    {
        _skyHunterState = SkyHunterState.None;
        _nextSkyHunterTime = Time.time + skyHunterCooldown;

        if (Enemy != null)
        {
            Enemy.Locomotion.StopMoving();
            Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
        }

        SetTrail(false);
    }

    private bool TrySampleMovementPoint(Vector3 rawPoint, out Vector3 point)
    {
        point = default;

        if (!NavMesh.SamplePosition(Enemy.MyTransform.position, out NavMeshHit originHit, navMeshSampleRadius + maxVerticalDifference, NavMesh.AllAreas) || !NavMesh.SamplePosition(rawPoint, out NavMeshHit destinationHit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            return false;
        }

        if (Mathf.Abs(destinationHit.position.y - originHit.position.y) > maxVerticalDifference)
        {
            return false;
        }

        if (!Enemy.Detection.IsPointInLeash(destinationHit.position))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(originHit.position, destinationHit.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        point = destinationHit.position;
        return true;
    }

    private void SetTrail(bool enabled)
    {
        if (flightTrail == null) return;

        if (enabled) flightTrail.Clear();

        flightTrail.emitting = enabled;
    }

    private bool ShouldUseUltimate()
    {
        return !_ultimateUsed && Enemy.Health.CurrentHealth <= Enemy.Data.maxHealth * ultimateHealthRatio;
    }

    private static bool CanUse(AttackDataSO attack, float distance)
    {
        return attack != null && distance >= attack.minAttackRange && distance <= attack.maxAttackRange;
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        _skyHunterState = SkyHunterState.None;
        _forcedAttack = null;
        _vacuumActive = false;
        _vacuumConsumedForCurrentAttack = false;
        _currentAttackIsEclipse = false;
        _ultimateQueued = false;
        _ultimateUsed = false;
        _nextSkyHunterTime = Time.time + skyHunterInitialDelay;

        if (Enemy != null)
        {
            Enemy.Locomotion.StopMoving();
            Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
        }

        SetTrail(false);
    }
}
