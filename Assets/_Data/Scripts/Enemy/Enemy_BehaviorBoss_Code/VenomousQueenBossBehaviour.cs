using UnityEngine;
using UnityEngine.AI;

public class VenomousQueenBossBehaviour : EnemyBossBehaviour
{
    [Header("Combo")]
    [SerializeField] private AttackDataSO clawRightAttack;
    [SerializeField] private AttackDataSO clawLeftAttack;
    [SerializeField] private AttackDataSO biteAttack;

    [Header("Special Attacks")]
    [SerializeField] private AttackDataSO venomBloomAttack;

    [Header("Toxic Body")]
    [SerializeField] private float toxicBodyRange = 3.5f;
    [SerializeField] private float exposureInterval = 0.5f;
    [SerializeField] private float exposurePerInterval = 8f;

    [Header("Ultimate")]
    [SerializeField, Range(0f, 1f)]
    private float ultimateHealthRatio = 0.2f;

    [Header("Retreat Hunter")]
    [SerializeField] private float retreatTriggerRange = 3f;
    [SerializeField] private float retreatDistance = 6f;
    [SerializeField] private float retreatSpeedMultiplier = 1.35f;
    [SerializeField] private float retreatCooldown = 5f;
    [SerializeField] private float retreatArrivalDistance = 0.5f;
    [SerializeField] private float retreatTimeout = 2.5f;
    [SerializeField] private float retreatTurnSpeed = 900f;
    [SerializeField] private float navMeshSampleRadius = 1.5f;
    [SerializeField] private float maxVerticalDifference = 1.5f;


    private bool _isRetreating;
    private float _nextRetreatTime;
    private float _retreatEndTime;
    private Vector3 _retreatDestination;

    private AttackDataSO _forcedAttack;
    private float _nextExposureTime;
    private bool _ultimateUsed;

    public bool IsPhase2Active => IsPhase2;

    private void Update()
    {
        if (Enemy == null || Enemy.Health.CurrentHealth <= 0f)
            return;

        Transform target = Enemy.Detection.CurrentTarget;

        if (target == null || Time.time < _nextExposureTime)
            return;

        float distance = Vector3.Distance(
            Enemy.MyTransform.position,
            target.position
        );

        if (distance > toxicBodyRange)
            return;

        target.GetComponentInParent<CharacterPoisonStatus>()?.AddExposure(exposurePerInterval, Enemy.gameObject);

        _nextExposureTime = Time.time + exposureInterval;
    }

    public override float ModifyMoveSpeed(float speed)
    {
        return IsPhase2 ? speed * Phase2AttackSpeedMultiplier : speed;
    }

    public override float ModifyAttackAnimationSpeed(float speed)
    {
        return IsPhase2 ? speed * Phase2AttackSpeedMultiplier : speed;
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
            CanUse(venomBloomAttack, distance))
        {
            _ultimateUsed = true;
            return venomBloomAttack;
        }

        return null;
    }

    public override void OnAttackStarted(AttackDataSO attack)
    {
        if (attack == clawRightAttack)
            _forcedAttack = clawLeftAttack;
        else if (attack == clawLeftAttack)
            _forcedAttack = biteAttack;
        else if (attack == biteAttack)
            _forcedAttack = null;
    }

    public override bool UpdateSpecialMovement(Transform target)
    {
        if (target == null)
            return false;

        if (_isRetreating)
        {
            Vector3 remaining = _retreatDestination - Enemy.MyTransform.position;
            remaining.y = 0f;

            bool arrived =
                remaining.sqrMagnitude <= retreatArrivalDistance * retreatArrivalDistance;

            if (arrived || Time.time >= _retreatEndTime)
            {
                FinishRetreat();
                return false;
            }

            Enemy.Locomotion.MoveToTarget(_retreatDestination, 0f, false);

            FaceTargetDuringRetreat(target);
            return true;
        }

        if (Time.time < _nextRetreatTime)
            return false;

        Vector3 toTarget = target.position - Enemy.MyTransform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude >
            retreatTriggerRange * retreatTriggerRange)
        {
            return false;
        }

        if (!TryFindRetreatDestination(
                target,
                out _retreatDestination))
        {

            _nextRetreatTime = Time.time + 1f;
            return false;
        }

        _isRetreating = true;

        _retreatEndTime = Time.time + retreatTimeout;

        Enemy.Locomotion.SetSpeed(
            Enemy.Data.moveSpeed * retreatSpeedMultiplier
        );

        Enemy.AnimatorController.PlayAnimation(
            Enemy.AnimatorController.ChaseHash
        );

        Enemy.Locomotion.MoveToTarget(_retreatDestination, 0f, false);

        FaceTargetDuringRetreat(target);
        return true;
    }

    private bool TryFindRetreatDestination(
        Transform target,
        out Vector3 destination)
    {
        destination = default;

        Vector3 transformPosition =
            Enemy.MyTransform.position;

        // Root của Queen nằm cao hơn mặt NavMesh do scale và Base Offset.
        if (!NavMesh.SamplePosition(
                transformPosition,
                out NavMeshHit originHit,
                maxVerticalDifference + 1f,
                NavMesh.AllAreas))
        {
            return false;
        }

        Vector3 navOrigin = originHit.position;

        Vector3 awayDirection =
            navOrigin - target.position;

        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.01f)
            awayDirection = -Enemy.MyTransform.forward;

        awayDirection.Normalize();

        Vector3 currentOffset =
            navOrigin - target.position;

        currentOffset.y = 0f;

        float bestDistanceSqr =
            currentOffset.sqrMagnitude;

        bool found = false;
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < 8; i++)
        {
            Vector3 direction =
                Quaternion.Euler(0f, i * 45f, 0f) *
                awayDirection;

            Vector3 candidate =
                navOrigin + direction * retreatDistance;

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleRadius,
                    NavMesh.AllAreas))
            {
                continue;
            }

            if (Mathf.Abs(hit.position.y - navOrigin.y) >
                maxVerticalDifference)
            {
                continue;
            }

            if (!Enemy.Detection.IsPointInLeash(hit.position))
                continue;

            if (!NavMesh.CalculatePath(
                    navOrigin,
                    hit.position,
                    NavMesh.AllAreas,
                    path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            Vector3 candidateOffset =
                hit.position - target.position;

            candidateOffset.y = 0f;

            float distanceSqr =
                candidateOffset.sqrMagnitude;

            if (distanceSqr <= bestDistanceSqr)
                continue;

            bestDistanceSqr = distanceSqr;
            destination = hit.position;
            found = true;
        }

        return found;
    }

    private void FinishRetreat()
    {
        _isRetreating = false;
        _nextRetreatTime = Time.time + retreatCooldown;

        Enemy.Locomotion.StopMoving();
        Enemy.Locomotion.SetUpdateRotation(true);
        Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
    }

    private void FaceTargetDuringRetreat(Transform target)
    {
        Vector3 direction =
            target.position - Enemy.MyTransform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        Enemy.MyTransform.rotation = Quaternion.RotateTowards(
            Enemy.MyTransform.rotation,
            targetRotation,
            retreatTurnSpeed * Time.deltaTime
        );
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();

        _forcedAttack = null;
        _nextExposureTime = 0f;
        _ultimateUsed = false;
        _isRetreating = false;
        _nextRetreatTime = 0f;
        _retreatEndTime = 0f;
        _retreatDestination = default;

        if (Enemy != null)
        {
            Enemy.Locomotion.StopMoving();
            Enemy.Locomotion.SetSpeed(Enemy.Data.moveSpeed);
            Enemy.Locomotion.SetUpdateRotation(true);
        }
    }

    private static bool CanUse(AttackDataSO attack, float distance)
    {
        return attack != null && distance >= attack.minAttackRange && distance <= attack.maxAttackRange;
    }
}