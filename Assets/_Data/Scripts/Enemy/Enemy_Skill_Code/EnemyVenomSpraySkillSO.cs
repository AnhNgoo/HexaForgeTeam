using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyVenomSpraySkill",
    menuName = "Enemy/Skills/Venom Spray")]
public class EnemyVenomSpraySkillSO : EnemyAttackSkillSO
{
    [SerializeField, Min(1)] private int cloudCount = 3;
    [SerializeField, Range(0f, 90f)] private float totalSpreadAngle = 50f;
    [SerializeField] private float cloudDistance = 4f;
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float groundOffset = 0.1f;

    [Header("Poison")]
    [SerializeField] private float tickInterval = 0.75f;
    [SerializeField] private float tickDamageMultiplier = 0.2f;
    [SerializeField] private float exposurePerTick = 10f;
    [SerializeField] private float cloudScale = 1f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context?.Enemy == null ||
            context.AttackData == null ||
            context.Target == null)
        {
            return;
        }

        Vector3 centerDirection =
            context.Target.position - context.Enemy.MyTransform.position;

        centerDirection.y = 0f;

        if (centerDirection.sqrMagnitude <= 0.001f)
            centerDirection = context.Enemy.MyTransform.forward;

        centerDirection.Normalize();

        for (int i = 0; i < cloudCount; i++)
        {
            float t = cloudCount == 1
                ? 0.5f
                : i / (cloudCount - 1f);

            float angle = Mathf.Lerp(
                -totalSpreadAngle * 0.5f,
                totalSpreadAngle * 0.5f,
                t
            );

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) * centerDirection;

            SpawnCloud(context, direction);
        }
    }

    private void SpawnCloud(
        EnemyAttackContext context,
        Vector3 direction)
    {
        Vector3 rawPosition =
            context.Enemy.MyTransform.position +
            direction * cloudDistance;

        if (!NavMesh.SamplePosition(
                rawPosition,
                out NavMeshHit hit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
        {
            return;
        }

        Vector3 position =
            hit.position + Vector3.up * groundOffset;

        GameObject instance =
            ObjectPooling.Instance.SpawnFromPool(
                context.AttackData.projectilePoolType,
                position,
                Quaternion.LookRotation(direction, Vector3.up)
            );

        if (instance == null)
            return;

        EnemyPoisonArea poisonArea =
            instance.GetComponent<EnemyPoisonArea>();

        if (poisonArea == null)
        {
            ObjectPooling.Instance.ReturnToPool(
                context.AttackData.projectilePoolType,
                instance
            );

            return;
        }

        poisonArea.Initialize(
            context.Enemy,
            context.AttackData,
            context.RuntimeDamageMultiplier,
            context.AttackData.projectileLifetime,
            tickInterval,
            tickDamageMultiplier,
            exposurePerTick,
            cloudScale
        );
    }
}