using Cysharp.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyJumpChopSkillSO", menuName = "Enemy/Skills/JumpChop")]
public class EnemyJumpChopSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float maximumJumpDistance = 5f;
    [SerializeField] private float landingOffset = 0.5f;
    [SerializeField] private float navMeshSampleRadius = 1f;

    [SerializeField] private AnimationCurve horizontalCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField] private AnimationCurve heightCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

    public override void OnAttackMovement(EnemyAttackContext context)
    {
        if (context == null || context.Enemy == null || context.Target == null)
        {
            Debug.LogWarning("[JumpChop] Movement event thiếu context/target.");
            return;
        }

        Debug.Log($"[JumpChop] Bắt đầu lao tới {context.Target.name}");
        JumpAsync(context).Forget();
    }

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context == null || context.Enemy == null || context.AttackData == null)
            return;
        context.Enemy.Combat.EnableHitbox(context.AttackData.hitboxType);
    }

    public override void OnAttackEnd(EnemyAttackContext context)
    {
        if (context == null || context.Enemy == null || context.AttackData == null)
            return;
        context.Enemy.Combat.DisableHitbox(context.AttackData.hitboxType);
    }

    private async UniTaskVoid JumpAsync(EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        Transform target = context.Target;

        Vector3 start = enemy.MyTransform.position;

        Vector3 targetSnapshot = target.position;
        Vector3 direction = targetSnapshot - start;
        direction.y = 0f;

        float distance = direction.magnitude;
        if (distance <= 0.1f) return;

        direction.Normalize();

        float travelDistance = Mathf.Min(maximumJumpDistance, Mathf.Max(0f, distance - landingOffset));

        if (!NavMesh.SamplePosition(
                start,
                out NavMeshHit startHit,
                navMeshSampleRadius + 2f,
                NavMesh.AllAreas))
        {
            Debug.LogWarning("[JumpChop] Start không gần NavMesh.");
            return;
        }

        float rootHeightOffset = start.y - startHit.position.y;

        Vector3 end = start;
        bool foundLanding = false;

        for (float sampleDistance = travelDistance;
             sampleDistance >= 0.5f;
             sampleDistance -= 0.5f)
        {
            Vector3 candidate = start + direction * sampleDistance;
            candidate = enemy.Detection.ClampPointToLeash(candidate);

            // Sample từ cùng độ cao với mặt NavMesh.
            candidate.y = startHit.position.y;

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleRadius,
                    NavMesh.AllAreas))
                continue;

            end = hit.position + Vector3.up * rootHeightOffset;
            foundLanding = true;
            break;
        }



        if (!foundLanding)
        {
            Debug.LogWarning(
        $"[JumpChop] Không tìm được điểm đáp. " +
        $"Start={start}, TravelDistance={travelDistance:F2}"
    );
            return;
        }

        Debug.Log($"[JumpChop] Start={start}, End={end}, " + $"Distance={Vector3.Distance(start, end):F2}");
        bool agentDisabled = false;

        try
        {
            enemy.Locomotion.StopMoving();
            enemy.Locomotion.SetAgentActive(false);
            agentDisabled = true;

            float timer = 0f;

            while (timer < jumpDuration)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) return;

                if (enemy.StateMachine.CurrentState != enemy.StateMachine.EnemyAttackState)
                {
                    Debug.LogWarning($"[JumpChop] Bị ngắt bởi state: " + $"{enemy.StateMachine.CurrentState?.GetType().Name}");
                    return;
                }

                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / jumpDuration);
                float horizontalT = horizontalCurve.Evaluate(t);

                Vector3 position = Vector3.Lerp(start, end, horizontalT);
                position.y += heightCurve.Evaluate(t) * jumpHeight;

                enemy.MyTransform.position = position;
                enemy.MyTransform.rotation = Quaternion.LookRotation(direction);

                await UniTask.Yield();
            }
            enemy.MyTransform.position = end;
            Debug.Log($"[JumpChop] Đã đáp tại {end}");
        }
        finally
        {
            if (enemy != null &&
                enemy.gameObject.activeInHierarchy &&
                agentDisabled &&
                enemy.StateMachine.CurrentState != enemy.StateMachine.EnemyDeadState)
            {
                Vector3 recoveryPosition = enemy.MyTransform.position;

                if (NavMesh.SamplePosition(
                    recoveryPosition,
                    out NavMeshHit recoveryHit,
                    navMeshSampleRadius + jumpHeight,
                    NavMesh.AllAreas))
                {
                    recoveryPosition = recoveryHit.position + Vector3.up * rootHeightOffset;
                }

                enemy.MyTransform.position = recoveryPosition;
                enemy.Locomotion.SetAgentActive(true);
                enemy.Locomotion.WarpTo(recoveryPosition);
                enemy.Locomotion.StopMoving();
            }
        }
    }
}
