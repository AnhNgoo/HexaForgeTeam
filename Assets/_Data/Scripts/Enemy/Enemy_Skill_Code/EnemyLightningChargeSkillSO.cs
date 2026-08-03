using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyLightningChargeSkill",
    menuName = "Enemy/Skills/Lightning Charge")]
public class EnemyLightningChargeSkillSO : EnemyAttackSkillSO
{
    [Header("Charge")]
    [SerializeField] private float maxChargeDistance = 18f;
    [SerializeField] private float overshootDistance = 2.5f;
    [SerializeField] private float chargeDuration = 0.42f;
    [SerializeField] private float trackingDuration = 0.12f;
    [SerializeField] private float turnSpeed = 1200f;
    [SerializeField] private float wallPadding = 0.8f;
    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("NavMesh")]
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float maxVerticalDifference = 1.5f;

    public override void OnAttackMovement(
        EnemyAttackContext context)
    {
        if (!IsValid(context))
        {
            return;
        }

        ChargeAsync(context).Forget();
    }

    public override void OnAttackEnd(
        EnemyAttackContext context)
    {
        if (context?.Enemy == null ||
            context.AttackData == null)
        {
            return;
        }

        context.Enemy.Combat.DisableHitbox(
            context.AttackData.hitboxType
        );
    }

    private async UniTaskVoid ChargeAsync(
        EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        Transform target = context.Target;

        if (!TryFindDestination(
                enemy,
                target,
                enemy.MyTransform.position,
                out Vector3 destination))
        {
            return;
        }

        EnemyMinibossBehaviour behaviour = enemy.MinibossBehaviour;
        float realDuration = behaviour?.ModifyChargeDuration(chargeDuration) ?? chargeDuration;


        Vector3 start = enemy.MyTransform.position;
        float lockedY = start.y;
        float elapsed = 0f;

        enemy.Locomotion.StopMoving();
        enemy.Locomotion.SetAgentActive(false);
        enemy.Combat.EnableHitbox(
            context.AttackData.hitboxType
        );
        behaviour?.SetChargeTrail(true);

        try
        {
            while (elapsed < realDuration &&
                   IsCurrentAttack(context))
            {
                elapsed += Time.deltaTime;

                if (elapsed <= trackingDuration &&
                    TryFindDestination(
                        enemy,
                        target,
                        start,
                        out Vector3 trackedDestination))
                {
                    destination = trackedDestination;
                }

                Vector3 direction =
                    destination - enemy.MyTransform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation =
                        Quaternion.LookRotation(direction);

                    enemy.MyTransform.rotation =
                        Quaternion.RotateTowards(
                            enemy.MyTransform.rotation,
                            targetRotation,
                            turnSpeed * Time.deltaTime
                        );
                }

                float t = Mathf.Clamp01(
                    elapsed / realDuration
                );

                Vector3 position = Vector3.Lerp(
                    start,
                    destination,
                    movementCurve.Evaluate(t)
                );

                position.y = lockedY;
                enemy.MyTransform.position = position;

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (IsCurrentAttack(context))
                enemy.MyTransform.position = destination;
        }
        finally
        {
            if (enemy != null)
            {
                enemy.Combat.DisableHitbox(
                    context.AttackData.hitboxType
                );
                behaviour?.SetChargeTrail(false);

                if (enemy.gameObject.activeInHierarchy &&
                    enemy.Health.CurrentHealth > 0f)
                {
                    enemy.Locomotion.SetAgentActive(true);
                    enemy.Locomotion.WarpTo(
                        enemy.MyTransform.position
                    );
                    enemy.Locomotion.StopMoving();
                }
            }
        }
    }

    private bool TryFindDestination(
    EnemyBase enemy,
    Transform target,
    Vector3 start,
    out Vector3 destination)
    {
        destination = start;

        if (target == null)
            return false;

        if (!NavMesh.SamplePosition(
                start,
                out NavMeshHit originHit,
                navMeshSampleRadius + maxVerticalDifference,
                NavMesh.AllAreas))
        {
            Debug.LogWarning(
                $"[LightningCharge] Start không gần NavMesh: {start}"
            );
            return false;
        }

        Vector3 toTarget = target.position - start;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.01f)
            toTarget = enemy.MyTransform.forward;

        Vector3 direction = toTarget.normalized;

        float desiredDistance = Mathf.Min(
            maxChargeDistance,
            toTarget.magnitude + overshootDistance
        );

        // Lùi dần điểm cuối về gần boss cho tới khi tìm được
        // một vị trí hợp lệ và có thể lao thẳng tới.
        for (float distance = desiredDistance;
             distance >= 0.5f;
             distance -= 0.5f)
        {
            Vector3 candidate =
                start + direction * distance;

            candidate =
                enemy.Detection.ClampPointToLeash(candidate);

            candidate.y = originHit.position.y;

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit destinationHit,
                    navMeshSampleRadius,
                    NavMesh.AllAreas))
            {
                continue;
            }

            if (Mathf.Abs(
                    destinationHit.position.y -
                    originHit.position.y) >
                maxVerticalDifference)
            {
                continue;
            }

            Vector3 reachablePoint = destinationHit.position;

            if (NavMesh.Raycast(
                    originHit.position,
                    reachablePoint,
                    out NavMeshHit blockedHit,
                    NavMesh.AllAreas))
            {
                reachablePoint =
                    blockedHit.position -
                    direction * wallPadding;

                if (!NavMesh.SamplePosition(
                        reachablePoint,
                        out destinationHit,
                        navMeshSampleRadius,
                        NavMesh.AllAreas))
                {
                    continue;
                }

                reachablePoint = destinationHit.position;
            }

            Vector3 horizontalMovement =
                reachablePoint - originHit.position;

            horizontalMovement.y = 0f;

            if (horizontalMovement.sqrMagnitude < 0.25f)
                continue;

            destination = reachablePoint;

            // Root của enemy giữ nguyên độ cao,
            // NavMesh chỉ dùng để xác định mặt phẳng di chuyển.
            destination.y = start.y;

            return true;
        }

        Debug.LogWarning(
            $"[LightningCharge] Không tìm được đường lao. " +
            $"Start={start}, Target={target.position}, " +
            $"Leash={enemy.CurrentLeash:F1}"
        );

        return false;
    }

    private static bool IsCurrentAttack(
        EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.Enemy.gameObject.activeInHierarchy &&
               context.Enemy.Health.CurrentHealth > 0f &&
               context.Enemy.Combat.IsPerformingAttack &&
               context.Enemy.Combat.CurrentAttackData ==
               context.AttackData;
    }

    private static bool IsValid(
        EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.Target != null &&
               context.AttackData != null;
    }
}
