using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyBruteJumpSmashSkill",
    menuName = "Enemy/Skills/Brute Jump Smash")]
public class EnemyBruteJumpSmashSkillSO : EnemyAttackSkillSO
{
    [Header("Shockwave")]
    [SerializeField] private PoolType shockwavePool;
    [SerializeField] private int ringCount = 3;
    [SerializeField] private float intervalBetweenRings = 0.18f;
    [SerializeField] private float ringLifetime = 0.65f;
    [SerializeField] private float firstRingScale = 1f;
    [SerializeField] private float ringScaleStep = 0.55f;
    [SerializeField] private float jumpHeight = 2.5f;

    [Header("Titan Leap")]
    [SerializeField] private float leapDuration = 0.55f;
    [SerializeField] private float trackingDuration = 0.22f;
    [SerializeField] private float targetPredictionTime = 0.2f;
    [SerializeField] private float landingDistance = 1.8f;
    [SerializeField] private float destinationStopDistance = 0.15f;
    [SerializeField] private float navMeshSampleRadius = 2.5f;
    [SerializeField] private float maxVerticalDifference = 1.5f;

    [Header("Phase 2")]
    [SerializeField] private float repeatAnimationDelay = 0.2f;
    [SerializeField] private float secondImpactDelay = 0.75f;

    [Header("Cataclysm")]
    [SerializeField] private EnemyEarthPillarsSkillSO earthPillarsSkill;

    public override void OnAttackMovement(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        LeapTowardsTargetAsync(context).Forget();
    }

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        BruteBossBehaviour brute =
            context?.Enemy?.GetComponent<BruteBossBehaviour>();

        if (brute == null ||
            !brute.TryBeginJumpSequence(
                out bool doubleSmash,
                out bool cataclysm))
        {
            return;
        }

        context.Enemy.Locomotion.StopMoving();
        context.Enemy.Locomotion.SetSpeed(
            context.Enemy.Data.moveSpeed
        );

        ExecuteImpactSequenceAsync(
            context,
            brute,
            doubleSmash,
            cataclysm
        ).Forget();
    }

    private async UniTaskVoid LeapTowardsTargetAsync(
        EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        BruteBossBehaviour brute =
            enemy.GetComponent<BruteBossBehaviour>();

        float attackSpeed =
            brute != null
                ? brute.ModifyAttackAnimationSpeed(1f)
                : 1f;

        float realLeapDuration =
            leapDuration / Mathf.Max(0.01f, attackSpeed);

        float realTrackingDuration =
            trackingDuration / Mathf.Max(0.01f, attackSpeed);

        if (!TryFindLeapDestination(
                context,
                out Vector3 destination))
        {
            return;
        }

        Vector3 start = enemy.MyTransform.position;

        NavMesh.SamplePosition(
            start,
            out NavMeshHit startHit,
            navMeshSampleRadius + maxVerticalDifference,
            NavMesh.AllAreas
        );

        float rootHeightOffset = start.y - startHit.position.y;
        Vector3 end = destination + Vector3.up * rootHeightOffset;
        Vector3 landingGroundPoint = destination;

        enemy.Locomotion.StopMoving();
        enemy.Locomotion.SetAgentActive(false);

        try
        {
            float elapsed = 0f;

            while (elapsed < realLeapDuration && IsCurrentAttack(context))
            {
                elapsed += Time.deltaTime;

                if (elapsed <= realTrackingDuration &&
                    TryFindLeapDestination(context, out Vector3 trackedPoint))
                {
                    landingGroundPoint = trackedPoint;
                    end = landingGroundPoint + Vector3.up * rootHeightOffset;
                }

                float t = Mathf.Clamp01(elapsed / realLeapDuration);
                float horizontalT = Mathf.SmoothStep(0f, 1f, t);

                Vector3 position = Vector3.Lerp(start, end, horizontalT);
                position.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

                Vector3 lookDirection = end - position;
                lookDirection.y = 0f;

                if (lookDirection.sqrMagnitude > 0.01f)
                    enemy.MyTransform.rotation =
                        Quaternion.LookRotation(lookDirection);

                enemy.MyTransform.position = position;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (IsCurrentAttack(context))
                enemy.MyTransform.position = end;
        }
        finally
        {
            if (enemy != null &&
                enemy.gameObject.activeInHierarchy &&
                enemy.StateMachine.CurrentState !=
                enemy.StateMachine.EnemyDeadState)
            {
                enemy.MyTransform.position = landingGroundPoint;
                enemy.Locomotion.SetAgentActive(true);
                enemy.Locomotion.WarpTo(landingGroundPoint);
                enemy.Locomotion.StopMoving();
                enemy.Locomotion.SetSpeed(enemy.Data.moveSpeed);
            }
        }
    }

    private async UniTaskVoid ExecuteImpactSequenceAsync(
        EnemyAttackContext context,
        BruteBossBehaviour brute,
        bool doubleSmash,
        bool cataclysm)
    {
        try
        {
            brute.SetJumpSequenceLocked(doubleSmash);
            brute.NotifyJumpSmashFinished(context.Target);
            await SpawnRingsAsync(context);

            if (cataclysm)
            {
                if (earthPillarsSkill != null)
                {
                    await earthPillarsSkill
                        .CastCataclysmAsync(context);
                }

                return;
            }

            if (doubleSmash && IsCasterAlive(context))
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(
                        repeatAnimationDelay)
                );

                if (!IsCasterAlive(context))
                    return;

                Animator animator =
                    context.Enemy.AnimatorController.Animator;

                float attackSpeed =
                    brute.ModifyAttackAnimationSpeed(1f);

                animator.speed = attackSpeed;
                animator.CrossFadeInFixedTime(
                    context.AttackData.animationStateName,
                    0.05f
                );

                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(
                        secondImpactDelay / attackSpeed)
                );

                if (IsCasterAlive(context))
                    await SpawnRingsAsync(context);
            }

            if (doubleSmash && IsCasterAlive(context))
                brute.NotifyJumpSmashFinished(context.Target);
        }
        finally
        {
            if (brute != null)
                brute.EndJumpSequence();
        }
    }

    private async UniTask SpawnRingsAsync(
        EnemyAttackContext context)
    {
        Vector3 spawnPosition = context.Enemy.MyTransform.position;

        if (NavMesh.SamplePosition(
                spawnPosition,
                out NavMeshHit groundHit,
                navMeshSampleRadius + maxVerticalDifference,
                NavMesh.AllAreas))
        {
            spawnPosition = groundHit.position;
        }
        else
        {
            Debug.LogWarning("[Brute] Không tìm thấy mặt đất cho Shockwave.");
            return;
        }

        spawnPosition += context.Enemy.MyTransform.TransformDirection(
            context.AttackData.vfxOffset
        );

        for (int i = 0; i < ringCount; i++)
        {
            if (!IsCasterAlive(context))
                return;

            GameObject ring =
                ObjectPooling.Instance.SpawnFromPool(
                    shockwavePool,
                    spawnPosition,
                    Quaternion.identity
                );

            if (ring != null)
            {
                float scale = firstRingScale + ringScaleStep * i;

                ring.transform.localScale = Vector3.one * context.AttackData.vfxScale * scale;

                EnemyHitbox hitbox = ring.GetComponentInChildren<EnemyHitbox>(true);

                hitbox?.Initialize(
                    context.Enemy,
                    context.AttackData,
                    context.RuntimeDamageMultiplier
                );

                hitbox?.EnableHitBox();
                ReturnRingAsync(ring, hitbox).Forget();
            }

            if (i < ringCount - 1)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(
                        intervalBetweenRings)
                );
            }
        }
    }

    private async UniTaskVoid ReturnRingAsync(
        GameObject ring,
        EnemyHitbox hitbox)
    {
        await UniTask.Delay(
            System.TimeSpan.FromSeconds(ringLifetime)
        );

        hitbox?.DisableHitBox();

        if (ring != null &&
            ring.activeInHierarchy &&
            ObjectPooling.Instance != null)
        {
            ObjectPooling.Instance.ReturnToPool(
                shockwavePool,
                ring
            );
        }
    }

    private bool TryFindLeapDestination(
        EnemyAttackContext context,
        out Vector3 destination)
    {
        destination = default;

        if (!NavMesh.SamplePosition(
                context.Enemy.MyTransform.position,
                out NavMeshHit originHit,
                navMeshSampleRadius + maxVerticalDifference,
                NavMesh.AllAreas))
        {
            return false;
        }

        Vector3 predictedTarget =
            context.Target.position;

        CharacterMovement movement =
            context.Target.GetComponentInParent<CharacterMovement>();

        if (movement?.CC != null)
        {
            Vector3 velocity = movement.CC.velocity;
            velocity.y = 0f;
            predictedTarget += velocity * targetPredictionTime;
        }

        Vector3 approach =
            predictedTarget - originHit.position;
        approach.y = 0f;

        if (approach.sqrMagnitude <= 0.01f)
            approach = context.Enemy.MyTransform.forward;

        Vector3 rawDestination =
            predictedTarget -
            approach.normalized * landingDistance;

        if (!NavMesh.SamplePosition(
                rawDestination,
                out NavMeshHit hit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
        {
            return false;
        }

        if (Mathf.Abs(
                hit.position.y - originHit.position.y) >
            maxVerticalDifference)
        {
            return false;
        }

        if (!context.Enemy.Detection.IsPointInLeash(
                hit.position))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(
                originHit.position,
                hit.position,
                NavMesh.AllAreas,
                path) ||
            path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        destination = hit.position;
        return true;
    }

    private static bool IsValid(
        EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.AttackData != null &&
               context.Target != null;
    }

    private static bool IsCasterAlive(
        EnemyAttackContext context)
    {
        return IsValid(context) &&
               context.Enemy.gameObject.activeInHierarchy &&
               context.Enemy.Health.CurrentHealth > 0f;
    }

    private static bool IsCurrentAttack(
        EnemyAttackContext context)
    {
        return IsCasterAlive(context) &&
               context.Enemy.Combat.IsPerformingAttack &&
               context.Enemy.Combat.CurrentAttackData ==
               context.AttackData;
    }
}
