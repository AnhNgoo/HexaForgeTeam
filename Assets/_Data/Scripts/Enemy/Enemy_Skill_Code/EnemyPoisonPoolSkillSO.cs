using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyPoisonPoolSkill",
    menuName = "Enemy/Skills/Poison Pool")]
public class EnemyPoisonPoolSkillSO : EnemyAttackSkillSO
{
    [Header("Pool")]
    [SerializeField]
    private PoolType poisonAreaPool =
        PoolType.EnemyVenomPoisonArea;

    [Header("Poison")]
    [SerializeField] private float duration = 6f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float tickDamageMultiplier = 0.3f;
    [SerializeField] private float exposurePerTick = 12f;

    [Header("Prediction")]
    [SerializeField] private float predictionTime = 0.55f;
    [SerializeField] private float maxPredictionDistance = 4f;

    [Header("Phase 2 Trail")]
    [SerializeField] private int extraPoolCount = 3;
    [SerializeField] private float trailSpacing = 2f;
    [SerializeField] private float extraPoolScale = 0.65f;

    [Header("Venom Bloom")]
    [SerializeField] private int bloomPoolCount = 9;
    [SerializeField] private float bloomRadius = 9f;
    [SerializeField] private float bloomPoolScale = 0.85f;

    [Header("Placement")]
    [SerializeField] private float navMeshSampleRadius = 1.5f;
    [SerializeField] private float maxVerticalDifference = 1.5f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        Vector3 velocity = GetHorizontalVelocity(context.Target);
        Vector3 prediction =
            Vector3.ClampMagnitude(
                velocity * predictionTime,
                maxPredictionDistance
            );

        Vector3 center =
            context.Target.position + prediction;

        SpawnArea(context, center, 1f);

        VenomousQueenBossBehaviour queen =
            context.Enemy.GetComponent<
                VenomousQueenBossBehaviour>();

        if (queen == null || !queen.IsPhase2Active)
            return;

        Vector3 trailDirection =
            velocity.sqrMagnitude > 0.01f
                ? velocity.normalized
                : context.Enemy.MyTransform.forward;

        for (int i = 0; i < extraPoolCount; i++)
        {
            Vector3 trailPosition =
                center -
                trailDirection * trailSpacing * (i + 1);

            SpawnArea(
                context,
                trailPosition,
                extraPoolScale
            );
        }
    }

    public void CastBloom(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        Vector3 center = context.Target.position;
        SpawnArea(context, center, 1f);

        for (int i = 0; i < bloomPoolCount; i++)
        {
            float angle =
                360f * i / Mathf.Max(1, bloomPoolCount);

            float radians = angle * Mathf.Deg2Rad;

            Vector3 position =
                center +
                new Vector3(
                    Mathf.Cos(radians),
                    0f,
                    Mathf.Sin(radians)
                ) * bloomRadius;

            SpawnArea(
                context,
                position,
                bloomPoolScale
            );
        }
    }

    private void SpawnArea(
        EnemyAttackContext context,
        Vector3 candidate,
        float scale)
    {
        if (!NavMesh.SamplePosition(
                context.Target.position,
                out NavMeshHit centerHit,
                navMeshSampleRadius + maxVerticalDifference,
                NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
        {
            return;
        }

        if (Mathf.Abs(
                hit.position.y - centerHit.position.y) >
            maxVerticalDifference)
        {
            return;
        }

        if (!context.Enemy.Detection.IsPointInLeash(
                hit.position))
        {
            return;
        }

        GameObject instance =
            ObjectPooling.Instance.SpawnFromPool(
                poisonAreaPool,
                hit.position,
                Quaternion.identity
            );

        if (instance == null)
            return;

        EnemyPoisonArea poisonArea =
            instance.GetComponent<EnemyPoisonArea>();

        if (poisonArea == null)
        {
            ObjectPooling.Instance.ReturnToPool(
                poisonAreaPool,
                instance
            );
            return;
        }

        poisonArea.Initialize(
            context.Enemy,
            context.AttackData,
            context.RuntimeDamageMultiplier,
            duration,
            tickInterval,
            tickDamageMultiplier,
            exposurePerTick,
            scale
        );
    }

    private static Vector3 GetHorizontalVelocity(
        Transform target)
    {
        CharacterMovement movement =
            target.GetComponentInParent<CharacterMovement>();

        if (movement?.CC == null)
            return Vector3.zero;

        Vector3 velocity = movement.CC.velocity;
        velocity.y = 0f;
        return velocity;
    }

    private static bool IsValid(
        EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.AttackData != null &&
               context.Target != null;
    }
}
