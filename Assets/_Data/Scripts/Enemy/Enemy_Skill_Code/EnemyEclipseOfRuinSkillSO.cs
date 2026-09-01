using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyEclipseOfRuinSkill",
    menuName = "Enemy/Skills/Dark Mage/Eclipse Of Ruin")]
public class EnemyEclipseOfRuinSkillSO : EnemyAttackSkillSO
{
    [Header("Animations")]
    [SerializeField]
    private string burrowDownState =
        "Idle To Underground";
    [SerializeField]
    private string undergroundState =
        "Underground";
    [SerializeField]
    private string emergeState =
        "Underground To Idle";
    [SerializeField]
    private string biteState =
        "Bite Attack";

    [Header("Referenced Skills")]
    [SerializeField] private EnemyDarkOrbVolleySkillSO darkOrbSkill;
    [SerializeField] private EnemyShadowRainSkillSO meteorRainSkill;

    [Header("Bullet Hell")]
    [SerializeField, Min(1)] private int bulletHellWaves = 4;
    [SerializeField, Min(1)] private int projectilesPerWave = 14;
    [SerializeField, Min(0f)] private float bulletWaveInterval = 0.2f;

    [Header("Meteor Rain")]
    [SerializeField, Min(1)] private int meteorWaves = 4;
    [SerializeField, Min(0f)] private float meteorWaveInterval = 0.25f;

    [Header("Damage")]
    [SerializeField, Range(0.05f, 1f)]
    private float hazardDamageScale = 0.3f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float burrowDownDuration = 0.7f;
    [SerializeField, Min(0f)] private float hiddenHazardDuration = 3.5f;
    [SerializeField, Min(0f)] private float emergeWarningDuration = 0.65f;
    [SerializeField, Min(0f)] private float emergeDuration = 0.55f;
    [SerializeField, Min(0f)] private float biteImpactDelay = 0.3f;
    [SerializeField, Min(0f)] private float biteHitboxDuration = 0.22f;

    [Header("Emergence")]
    [SerializeField] private PoolType emergeTelegraphPool;
    [SerializeField, Min(0f)] private float emergeDistanceBehind = 1.8f;
    [SerializeField, Min(0.1f)] private float navMeshSampleRadius = 2f;
    [SerializeField, Min(0f)] private float maxVerticalDifference = 1.5f;
    [SerializeField, Min(0f)] private float minimumEdgeDistance = 0.75f;

    public override void OnAttackStart(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        DarkMageBossBehaviour boss =
            context.Enemy.GetComponent<DarkMageBossBehaviour>();

        if (boss == null || !boss.BeginSpecialAction())
            return;

        RunEclipseAsync(context, boss).Forget();
    }

    private async UniTaskVoid RunEclipseAsync(
        EnemyAttackContext context,
        DarkMageBossBehaviour boss)
    {
        EnemyBase enemy = context.Enemy;
        Animator animator = enemy.AnimatorController.Animator;
        EnemyHitbox biteHitbox = null;
        GameObject telegraph = null;

        try
        {
            // Đợi PerformAttack tạo visual task rồi mới hủy.
            await UniTask.Yield(PlayerLoopTiming.Update);

            if (!CanContinue(enemy))
                return;

            float animationSpeed = Mathf.Max(
                0.01f,
                animator != null ? animator.speed : 1f
            );

            enemy.Combat.ForceCloseHitbox();
            if (animator != null)
                animator.speed = animationSpeed;
            enemy.Locomotion.StopMoving();

            animator?.CrossFadeInFixedTime(
                burrowDownState,
                0.05f
            );

            await DelayScaled(
                burrowDownDuration,
                animationSpeed
            );

            if (!CanContinue(enemy))
                return;

            boss.EnterDarkVeil(context.Target);

            animator?.CrossFadeInFixedTime(
                undergroundState,
                0.05f
            );

            Transform target =
                enemy.Detection.CurrentTarget ??
                context.Target;

            EnemyAttackContext hazardContext =
                new EnemyAttackContext(
                    enemy,
                    context.AttackData,
                    target,
                    context.RuntimeDamageMultiplier *
                    hazardDamageScale
                );

            darkOrbSkill?.CastBulletHellAsync(
                hazardContext,
                bulletHellWaves,
                projectilesPerWave,
                bulletWaveInterval
            ).Forget();

            meteorRainSkill?.CastRainAsync(
                hazardContext,
                meteorWaves,
                meteorWaveInterval
            ).Forget();

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(
                    hiddenHazardDuration
                )
            );

            if (!CanContinue(enemy))
                return;

            target =
                enemy.Detection.CurrentTarget ??
                context.Target;

            if (!TryFindEmergencePoint(
                    enemy,
                    target,
                    out Vector3 emergencePoint))
            {
                emergencePoint = enemy.MyTransform.position;
            }

            telegraph = SpawnTelegraph(emergencePoint);

            await DelayScaled(
                emergeWarningDuration,
                animationSpeed
            );

            if (!CanContinue(enemy))
                return;

            ReturnTelegraph(telegraph);
            telegraph = null;

            enemy.Locomotion.WarpTo(emergencePoint);
            FaceTarget(enemy, target);

            animator?.CrossFadeInFixedTime(
                emergeState,
                0.05f
            );

            await DelayScaled(
                emergeDuration,
                animationSpeed
            );

            if (!CanContinue(enemy))
                return;

            boss.ExitDarkVeil();
            FaceTarget(enemy, target);

            animator?.CrossFadeInFixedTime(
                biteState,
                0.05f
            );

            await DelayScaled(
                biteImpactDelay,
                animationSpeed
            );

            if (!CanContinue(enemy))
                return;

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

                await DelayScaled(
                    biteHitboxDuration,
                    animationSpeed
                );

                biteHitbox.DisableHitBox();
                biteHitbox.Initialize(enemy);
                biteHitbox = null;
            }
        }
        finally
        {
            ReturnTelegraph(telegraph);

            if (biteHitbox != null)
            {
                biteHitbox.DisableHitBox();
                biteHitbox.Initialize(enemy);
            }

            if (enemy != null &&
                enemy.gameObject.activeInHierarchy &&
                enemy.Health.CurrentHealth > 0f)
            {
                if (animator != null) animator.speed = 1f;
                enemy.AnimatorController.PlayAnimation(enemy.AnimatorController.IdleHash, 0.08f);
            }

            boss?.EndSpecialAction();
        }
    }

    private bool TryFindEmergencePoint(
        EnemyBase enemy,
        Transform target,
        out Vector3 point)
    {
        point = default;

        if (enemy == null || target == null)
            return false;

        Vector3 backward = -target.forward;
        backward.y = 0f;

        if (backward.sqrMagnitude <= 0.01f)
        {
            backward =
                enemy.MyTransform.position - target.position;
            backward.y = 0f;
        }

        backward.Normalize();

        float[] angles = { 0f, 35f, -35f, 70f, -70f, 180f };

        foreach (float angle in angles)
        {
            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) * backward;

            Vector3 candidate =
                target.position +
                direction * emergeDistanceBehind;

            candidate =
                enemy.Detection.ClampPointToLeash(candidate);

            if (!NavMesh.SamplePosition(
                    target.position,
                    out NavMeshHit targetGround,
                    navMeshSampleRadius +
                    maxVerticalDifference,
                    NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleRadius,
                    NavMesh.AllAreas))
            {
                continue;
            }

            if (Mathf.Abs(
                    hit.position.y -
                    targetGround.position.y) >
                maxVerticalDifference)
            {
                continue;
            }

            if (!enemy.Detection.IsPointInLeash(hit.position))
                continue;

            if (NavMesh.FindClosestEdge(
                    hit.position,
                    out NavMeshHit edge,
                    NavMesh.AllAreas) &&
                Vector3.Distance(
                    hit.position,
                    edge.position) <
                minimumEdgeDistance)
            {
                continue;
            }

            point = hit.position;
            return true;
        }

        return false;
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

    private static void FaceTarget(
        EnemyBase enemy,
        Transform target)
    {
        if (enemy == null || target == null)
            return;

        Vector3 direction =
            target.position - enemy.MyTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            enemy.MyTransform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    private static async UniTask DelayScaled(
        float duration,
        float speed)
    {
        await UniTask.Delay(
            System.TimeSpan.FromSeconds(duration / speed)
        );
    }

    private static bool CanContinue(EnemyBase enemy)
    {
        return enemy != null &&
               enemy.gameObject.activeInHierarchy &&
               enemy.Health.CurrentHealth > 0f &&
               enemy.StateMachine.CurrentState ==
               enemy.StateMachine.EnemyAttackState;
    }

    private static bool IsValid(EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.Target != null &&
               context.AttackData != null;
    }
}