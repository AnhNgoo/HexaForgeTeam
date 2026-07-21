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

    [Header("Phase 2")]
    [SerializeField] private int extraPoolCount = 3;
    [SerializeField] private float extraPoolRadius = 3f;
    [SerializeField] private float extraPoolScale = 0.65f;

    [Header("Venom Bloom")]
    [SerializeField] private int bloomPoolCount = 9;
    [SerializeField] private float bloomRadius = 9f;
    [SerializeField] private float bloomPoolScale = 0.85f;

    [Header("Placement")]
    [SerializeField] private float navMeshSampleRadius = 1.5f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        Vector3 center = context.Target.position;

        SpawnArea(context, center, 1f);

        VenomousQueenBossBehaviour queen =
            context.Enemy.GetComponent<VenomousQueenBossBehaviour>();

        if (queen == null || !queen.IsPhase2Active)
            return;

        SpawnRandomAreas(
            context,
            center,
            extraPoolCount,
            extraPoolRadius,
            extraPoolScale
        );
    }

    public void CastBloom(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        Vector3 center = context.Target.position;

        SpawnArea(context, center, 1f);

        SpawnRandomAreas(
            context,
            center,
            bloomPoolCount,
            bloomRadius,
            bloomPoolScale
        );
    }

    private void SpawnRandomAreas(
        EnemyAttackContext context,
        Vector3 center,
        int count,
        float radius,
        float scale)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;

            Vector3 candidate = center +
                new Vector3(offset.x, 0f, offset.y);

            SpawnArea(context, candidate, scale);
        }
    }

    private void SpawnArea(
        EnemyAttackContext context,
        Vector3 candidate,
        float scale)
    {
        if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
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

    private static bool IsValid(EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.AttackData != null &&
               context.Target != null;
    }
}