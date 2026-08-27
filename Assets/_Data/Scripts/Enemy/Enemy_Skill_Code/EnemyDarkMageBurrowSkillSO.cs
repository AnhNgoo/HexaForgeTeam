using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyDarkMageBurrowSkill",
    menuName = "Enemy/Skills/Dark Mage Burrow")]
public class EnemyDarkMageBurrowSkillSO : EnemyAttackSkillSO
{
    [Header("VFX")]
    [SerializeField] private PoolType emergeTelegraphPool;

    [Header("Animation States")]
    [SerializeField] private string burrowDownState = "Idle To Underground";
    [SerializeField] private string undergroundState = "Underground";
    [SerializeField] private string emergeState = "Underground To Idle";
    [SerializeField] private string biteState = "Bite Attack";
    [SerializeField] private string headEmergeState = "Underground To Head Only";
    [SerializeField] private string headIdleState = "Head Only Idle";
    [SerializeField] private string headBurrowState = "Head Only To Underground";

    [Header("Timing")]
    [SerializeField] private float burrowDownDuration = 0.67f;
    [SerializeField] private float emergeWarningDuration = 0.45f;
    [SerializeField] private float emergeDuration = 0.83f;
    [SerializeField] private float biteImpactDelay = 0.33f;
    [SerializeField] private float biteHitboxDuration = 0.16f;
    [SerializeField] private float fakeHeadDuration = 0.4f;
    [SerializeField] private float fakeBurrowDuration = 0.5f;

    [Header("Placement")]
    [SerializeField] private float emergeBehindDistance = 2f;
    [SerializeField] private float repositionDistance = 8f;
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float maxVerticalDifference = 1.5f;
    [SerializeField] private float minimumEdgeDistance = 0.8f;

    public override void OnAttackStart(EnemyAttackContext context)
    {
        if (!IsValid(context)) return;

        DarkMageBossBehaviour boss =
            context.Enemy.GetComponent<DarkMageBossBehaviour>();

        if (boss == null || !boss.BeginSpecialAction())
            return;

        DarkMageBurrowMode mode = boss.ConsumeBurrowMode();
        RunBurrowAsync(context, boss, mode).Forget();
    }

    private async UniTaskVoid RunBurrowAsync(
        EnemyAttackContext context,
        DarkMageBossBehaviour boss,
        DarkMageBurrowMode mode)
    {
        EnemyBase enemy = context.Enemy;
        Animator animator = enemy.AnimatorController.Animator;
        GameObject telegraph = null;

        float speed = Mathf.Max(
            0.01f,
            boss.ModifyAttackAnimationSpeed(1f)
        );

        EnemyHitbox biteHitbox = null;

        try
        {
            enemy.Locomotion.StopMoving();
            animator?.CrossFadeInFixedTime(burrowDownState, 0.05f);

            await DelayScaled(burrowDownDuration, speed);
            if (!IsCurrent(context)) return;

            boss.EnterDarkVeil(context.Target);
            animator?.CrossFadeInFixedTime(undergroundState, 0.05f);

            if (mode == DarkMageBurrowMode.FakeAmbush)
            {
                await RunFakeAmbushAsync(
                    context,
                    boss,
                    animator,
                    speed
                );
                return;
            }

            Vector3 destination;
            bool found = mode == DarkMageBurrowMode.Reposition
                ? TryFindRepositionPoint(context, out destination)
                : TryFindAmbushPoint(context, out destination);

            if (!found) return;

            telegraph = SpawnTelegraph(destination);
            await DelayScaled(emergeWarningDuration, speed);
            if (!IsCurrent(context)) return;

            enemy.Locomotion.WarpTo(destination);
            FaceTarget(enemy, context.Target);
            boss.ExitDarkVeil();

            animator?.CrossFadeInFixedTime(emergeState, 0.05f);
            await DelayScaled(emergeDuration, speed);
            if (!IsCurrent(context)) return;

            if (mode == DarkMageBurrowMode.Reposition)
                return;

            FaceTarget(enemy, context.Target);
            animator?.CrossFadeInFixedTime(biteState, 0.05f);

            await DelayScaled(biteImpactDelay, speed);
            if (!IsCurrent(context)) return;

            enemy.Combat.PlayAttackVFX();

            biteHitbox = enemy.HitboxRegistry.GetHitbox(
                context.AttackData.hitboxType
            );

            if (biteHitbox != null)
            {
                biteHitbox.Initialize(
                    enemy,
                    context.AttackData,
                    context.RuntimeDamageMultiplier
                );

                biteHitbox.EnableHitBox();

                await DelayScaled(biteHitboxDuration, speed);

                biteHitbox.DisableHitBox();
                biteHitbox.Initialize(enemy);
                biteHitbox = null;
            }
        }
        finally
        {
            ReturnTelegraph(telegraph);

            if (enemy != null)
            {
                if (biteHitbox != null)
                {
                    biteHitbox.DisableHitBox();
                    biteHitbox.Initialize(enemy);
                }

                if (enemy.Health.CurrentHealth > 0f &&
                    enemy.gameObject.activeInHierarchy)
                {
                    enemy.AnimatorController.PlayAnimation(
                        enemy.AnimatorController.IdleHash,
                        0.08f
                    );
                }
            }

            boss?.EndSpecialAction();
        }
    }

    private async UniTask RunFakeAmbushAsync(
        EnemyAttackContext context,
        DarkMageBossBehaviour boss,
        Animator animator,
        float speed)
    {
        if (!TryFindAmbushPoint(context, out Vector3 decoyPoint))
            return;

        GameObject decoyTelegraph = SpawnTelegraph(decoyPoint);

        await DelayScaled(emergeWarningDuration, speed);
        if (!IsCurrent(context))
        {
            ReturnTelegraph(decoyTelegraph);
            return;
        }

        context.Enemy.Locomotion.WarpTo(decoyPoint);
        FaceTarget(context.Enemy, context.Target);
        boss.ExitDarkVeil();

        animator?.CrossFadeInFixedTime(headEmergeState, 0.05f);
        await DelayScaled(emergeDuration, speed);

        ReturnTelegraph(decoyTelegraph);
        if (!IsCurrent(context)) return;

        animator?.CrossFadeInFixedTime(headIdleState, 0.05f);
        await DelayScaled(fakeHeadDuration, speed);
        if (!IsCurrent(context)) return;

        animator?.CrossFadeInFixedTime(headBurrowState, 0.05f);
        await DelayScaled(fakeBurrowDuration, speed);
        if (!IsCurrent(context)) return;

        boss.EnterDarkVeil(context.Target);

        if (TryFindRepositionPoint(context, out Vector3 retreatPoint))
            context.Enemy.Locomotion.WarpTo(retreatPoint);

        boss.ExitDarkVeil();
        animator?.CrossFadeInFixedTime(emergeState, 0.05f);
        await DelayScaled(emergeDuration, speed);
    }

    private bool TryFindAmbushPoint(
        EnemyAttackContext context,
        out Vector3 destination)
    {
        destination = default;

        Vector3 backward = -context.Target.forward;
        backward.y = 0f;

        if (backward.sqrMagnitude <= 0.01f)
        {
            backward =
                context.Enemy.MyTransform.position -
                context.Target.position;
            backward.y = 0f;
        }

        backward.Normalize();

        float[] angles = { 0f, 35f, -35f, 70f, -70f, 180f };

        foreach (float angle in angles)
        {
            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) * backward;

            Vector3 candidate =
                context.Target.position +
                direction * emergeBehindDistance;

            if (TrySamplePoint(context, candidate, out destination))
                return true;
        }

        return false;
    }

    private bool TryFindRepositionPoint(
        EnemyAttackContext context,
        out Vector3 destination)
    {
        destination = default;

        Vector3 origin = context.Enemy.MyTransform.position;
        Vector3 away = origin - context.Target.position;
        away.y = 0f;

        if (away.sqrMagnitude <= 0.01f)
            away = -context.Target.forward;

        away.Normalize();

        float bestDistanceSqr = float.MinValue;
        bool found = false;

        for (int i = 0; i < 8; i++)
        {
            Vector3 direction =
                Quaternion.Euler(0f, i * 45f, 0f) * away;

            Vector3 candidate =
                origin + direction * repositionDistance;

            if (!TrySamplePoint(context, candidate, out Vector3 point))
                continue;

            Vector3 targetOffset = point - context.Target.position;
            targetOffset.y = 0f;

            if (targetOffset.sqrMagnitude <= bestDistanceSqr)
                continue;

            bestDistanceSqr = targetOffset.sqrMagnitude;
            destination = point;
            found = true;
        }

        return found;
    }

    private bool TrySamplePoint(
        EnemyAttackContext context,
        Vector3 candidate,
        out Vector3 point)
    {
        point = default;

        candidate = context.Enemy.Detection.ClampPointToLeash(candidate);

        if (!NavMesh.SamplePosition(
                context.Target.position,
                out NavMeshHit targetGround,
                navMeshSampleRadius + maxVerticalDifference,
                NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
        {
            return false;
        }

        if (Mathf.Abs(hit.position.y - targetGround.position.y) >
            maxVerticalDifference)
        {
            return false;
        }

        if (!context.Enemy.Detection.IsPointInLeash(hit.position))
            return false;

        if (NavMesh.FindClosestEdge(
                hit.position,
                out NavMeshHit edge,
                NavMesh.AllAreas) &&
            Vector3.Distance(hit.position, edge.position) < minimumEdgeDistance)
        {
            return false;
        }

        point = hit.position;
        return true;
    }

    private GameObject SpawnTelegraph(Vector3 position)
    {
        if (emergeTelegraphPool == PoolType.None ||
            ObjectPooling.Instance == null)
        {
            return null;
        }

        return ObjectPooling.Instance.SpawnFromPool(
            emergeTelegraphPool,
            position + Vector3.up * 0.03f,
            Quaternion.identity
        );
    }

    private void ReturnTelegraph(GameObject telegraph)
    {
        if (telegraph == null ||
            !telegraph.activeInHierarchy ||
            ObjectPooling.Instance == null)
        {
            return;
        }

        ObjectPooling.Instance.ReturnToPool(
            emergeTelegraphPool,
            telegraph
        );
    }

    private static void FaceTarget(EnemyBase enemy, Transform target)
    {
        if (enemy == null || target == null) return;

        Vector3 direction = target.position - enemy.MyTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
            enemy.MyTransform.rotation = Quaternion.LookRotation(direction);
    }

    private static async UniTask DelayScaled(float duration, float speed)
    {
        await UniTask.Delay(
            System.TimeSpan.FromSeconds(duration / speed)
        );
    }

    private static bool IsValid(EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.Target != null &&
               context.AttackData != null;
    }

    private static bool IsCurrent(EnemyAttackContext context)
    {
        return IsValid(context) &&
               context.Enemy.gameObject.activeInHierarchy &&
               context.Enemy.Health.CurrentHealth > 0f &&
               context.Enemy.Combat.CurrentAttackData == context.AttackData;
    }
}
