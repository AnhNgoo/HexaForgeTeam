using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "EnemyEarthshakerSmashSkill",
    menuName = "Enemy/Skills/Earthshaker Smash")]
public class EnemyEarthshakerSmashSkillSO : EnemyAttackSkillSO
{
    [Header("Leap")]
    [SerializeField] private float leapDuration = 0.5f;
    [SerializeField] private float trackingDuration = 0.18f;
    [SerializeField] private float predictionTime = 0.16f;
    [SerializeField] private float jumpHeight = 2.5f;
    [SerializeField] private float landingDistance = 1.8f;

    [Header("Shockwave")]
    [SerializeField] private PoolType shockwavePool = PoolType.Shockwave;
    [SerializeField] private int phase1RingCount = 1;
    [SerializeField] private int phase3RingCount = 2;
    [SerializeField] private int ultimateRingCount = 3;
    [SerializeField] private float ringInterval = 0.16f;
    [SerializeField] private float ringLifetime = 0.65f;
    [SerializeField] private float ringScaleStep = 0.55f;

    [Header("Delayed Cracks")]
    [SerializeField] private PoolType crackTelegraphPool;
    [SerializeField] private PoolType crackEruptionPool;
    [SerializeField] private int crackCount = 4;
    [SerializeField] private float crackRadius = 5f;
    [SerializeField] private float crackWarningDuration = 1.1f;
    [SerializeField] private float crackLifetime = 0.7f;
    [SerializeField] private float crackDamageMultiplier = 0.55f;

    [Header("NavMesh")]
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float maxVerticalDifference = 1.5f;

    public override void OnAttackMovement(EnemyAttackContext context)
    {
        if (IsValid(context)) LeapAsync(context).Forget();
    }

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsValid(context)) return;

        EarthshakerBossBehaviour boss =
            context.Enemy.GetComponent<EarthshakerBossBehaviour>();
        int rings = boss != null && boss.IsWorldBreakerActive
            ? ultimateRingCount
            : boss != null && boss.IsPhase3Active
                ? phase3RingCount
                : phase1RingCount;

        boss?.NotifyEarthSmashImpact(context.Target);
        SpawnImpactAsync(context, rings).Forget();
    }

    private async UniTaskVoid LeapAsync(EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        if (!TryGetDestination(context, out Vector3 destination) ||
            !NavMesh.SamplePosition(enemy.MyTransform.position,
                out NavMeshHit startHit,
                navMeshSampleRadius + maxVerticalDifference,
                NavMesh.AllAreas)) return;

        float speed = enemy.MinibossBehaviour?.ModifyAttackAnimationSpeed(1f) ?? 1f;
        float duration = leapDuration / speed;
        float tracking = trackingDuration / speed;
        Vector3 start = enemy.MyTransform.position;
        float rootOffset = start.y - startHit.position.y;
        Vector3 groundEnd = destination;
        Vector3 end = destination + Vector3.up * rootOffset;

        enemy.Locomotion.StopMoving();
        enemy.Locomotion.SetAgentActive(false);

        try
        {
            float elapsed = 0f;
            while (elapsed < duration && IsCurrentAttack(context))
            {
                elapsed += Time.deltaTime;
                if (elapsed <= tracking && TryGetDestination(context, out Vector3 tracked))
                {
                    groundEnd = tracked;
                    end = tracked + Vector3.up * rootOffset;
                }

                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
                position.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
                enemy.MyTransform.position = position;

                Vector3 look = end - position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    enemy.MyTransform.rotation = Quaternion.LookRotation(look);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        finally
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy &&
                enemy.Health.CurrentHealth > 0f)
            {
                enemy.MyTransform.position = groundEnd;
                enemy.Locomotion.SetAgentActive(true);
                enemy.Locomotion.WarpTo(groundEnd);
                enemy.Locomotion.StopMoving();
            }
        }
    }

    private async UniTaskVoid SpawnImpactAsync(
        EnemyAttackContext context, int ringCount)
    {
        Vector3 center = context.Enemy.MyTransform.position;
        if (!NavMesh.SamplePosition(center, out NavMeshHit hit,
            navMeshSampleRadius + maxVerticalDifference, NavMesh.AllAreas))
        {
            Debug.LogWarning("[Earthshaker] Không tìm thấy mặt đất cho Smash.");
            return;
        }
        center = hit.position;

        center += context.Enemy.MyTransform.TransformDirection(
            context.AttackData.vfxOffset
        );

        for (int i = 0; i < ringCount; i++)
        {
            SpawnDamageObject(context, shockwavePool, center,
                context.AttackData.vfxScale + ringScaleStep * i,
                context.RuntimeDamageMultiplier, ringLifetime);
            if (i < ringCount - 1)
                await UniTask.Delay(System.TimeSpan.FromSeconds(ringInterval));
        }

        SpawnCracksAsync(context, center).Forget();
    }

    private async UniTaskVoid SpawnCracksAsync(
        EnemyAttackContext context, Vector3 center)
    {
        List<Vector3> positions = BuildCrackPositions(context, center);
        List<GameObject> warnings = new(positions.Count);

        foreach (Vector3 position in positions)
        {
            GameObject warning = crackTelegraphPool == PoolType.None ? null :
                ObjectPooling.Instance.SpawnFromPool(
                    crackTelegraphPool, position, Quaternion.identity
                );
            warnings.Add(warning);
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(crackWarningDuration));

        for (int i = 0; i < positions.Count; i++)
        {
            if (warnings[i] != null && warnings[i].activeInHierarchy)
                ObjectPooling.Instance.ReturnToPool(crackTelegraphPool, warnings[i]);

            if (!IsCasterAlive(context)) continue;
            SpawnDamageObject(context, crackEruptionPool, positions[i], 1f,
                context.RuntimeDamageMultiplier * crackDamageMultiplier,
                crackLifetime);
        }
    }

    private List<Vector3> BuildCrackPositions(
        EnemyAttackContext context, Vector3 center)
    {
        List<Vector3> result = new(crackCount);
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < crackCount; i++)
        {
            float angle = (startAngle + 360f * i / crackCount) * Mathf.Deg2Rad;
            float distance = Random.Range(crackRadius * 0.55f, crackRadius);
            Vector3 candidate = center +
                new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit,
                    navMeshSampleRadius, NavMesh.AllAreas) ||
                Mathf.Abs(hit.position.y - center.y) > maxVerticalDifference ||
                !context.Enemy.Detection.IsPointInLeash(hit.position))
                continue;

            result.Add(hit.position);
        }
        return result;
    }

    private static void SpawnDamageObject(
        EnemyAttackContext context, PoolType pool, Vector3 position,
        float scale, float multiplier, float lifetime)
    {
        if (pool == PoolType.None) return;
        GameObject instance = ObjectPooling.Instance.SpawnFromPool(
            pool, position, Quaternion.identity
        );
        if (instance == null) return;

        instance.transform.localScale = Vector3.one * scale;
        EnemyHitbox hitbox = instance.GetComponentInChildren<EnemyHitbox>(true);
        hitbox?.Initialize(context.Enemy, context.AttackData, multiplier);
        hitbox?.EnableHitBox();
        ReturnDamageObjectAsync(pool, instance, hitbox, lifetime).Forget();
    }

    private static async UniTaskVoid ReturnDamageObjectAsync(
        PoolType pool, GameObject instance, EnemyHitbox hitbox, float lifetime)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(lifetime));
        hitbox?.DisableHitBox();
        if (instance != null && instance.activeInHierarchy)
            ObjectPooling.Instance.ReturnToPool(pool, instance);
    }

    private bool TryGetDestination(
        EnemyAttackContext context, out Vector3 destination)
    {
        destination = default;
        if (!NavMesh.SamplePosition(context.Enemy.MyTransform.position,
                out NavMeshHit originHit,
                navMeshSampleRadius + maxVerticalDifference,
                NavMesh.AllAreas))
            return false;

        Vector3 target = context.Target.position;
        CharacterMovement movement = context.Target.GetComponentInParent<CharacterMovement>();
        if (movement?.CC != null)
        {
            Vector3 velocity = movement.CC.velocity;
            velocity.y = 0f;
            target += velocity * predictionTime;
        }

        Vector3 approach = target - context.Enemy.MyTransform.position;
        approach.y = 0f;
        if (approach.sqrMagnitude < 0.01f)
            approach = context.Enemy.MyTransform.forward;

        Vector3 raw = target - approach.normalized * landingDistance;
        if (!NavMesh.SamplePosition(raw, out NavMeshHit hit,
                navMeshSampleRadius, NavMesh.AllAreas) ||
            Mathf.Abs(hit.position.y - originHit.position.y) >
                maxVerticalDifference ||
            !context.Enemy.Detection.IsPointInLeash(hit.position))
            return false;

        destination = hit.position;
        return true;
    }

    private static bool IsValid(EnemyAttackContext context) =>
        context?.Enemy != null && context.Target != null &&
        context.AttackData != null;

    private static bool IsCasterAlive(EnemyAttackContext context) =>
        IsValid(context) && context.Enemy.gameObject.activeInHierarchy &&
        context.Enemy.Health.CurrentHealth > 0f;

    private static bool IsCurrentAttack(EnemyAttackContext context) =>
        IsCasterAlive(context) && context.Enemy.Combat.IsPerformingAttack &&
        context.Enemy.Combat.CurrentAttackData == context.AttackData;
}
