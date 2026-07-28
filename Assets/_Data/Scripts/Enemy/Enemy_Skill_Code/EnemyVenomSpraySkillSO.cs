using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyVenomSpraySkill",
    menuName = "Enemy/Skills/Venom Spray")]
public class EnemyVenomSpraySkillSO : EnemyAttackSkillSO
{
    [Header("Sweep")]
    [SerializeField, Min(1)] private int cloudCount = 3;
    [SerializeField, Range(0f, 120f)]
    private float totalSpreadAngle = 60f;
    [SerializeField] private float cloudDistance = 4f;
    [SerializeField] private float intervalBetweenClouds = 0.12f;
    [SerializeField] private float phase2IntervalMultiplier = 0.8f;

    [Header("Placement")]
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float maxVerticalDifference = 1.5f;
    [SerializeField] private float groundOffset = 0.1f;

    [Header("Poison")]
    [SerializeField] private float tickInterval = 0.75f;
    [SerializeField] private float tickDamageMultiplier = 0.2f;
    [SerializeField] private float exposurePerTick = 10f;
    [SerializeField] private float cloudScale = 1f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        SweepAsync(context).Forget();
    }

    private async UniTaskVoid SweepAsync(
        EnemyAttackContext context)
    {
        VenomousQueenBossBehaviour queen =
            context.Enemy.GetComponent<
                VenomousQueenBossBehaviour>();

        float interval =
            queen != null && queen.IsPhase2Active
                ? intervalBetweenClouds *
                  phase2IntervalMultiplier
                : intervalBetweenClouds;

        for (int i = 0; i < cloudCount; i++)
        {
            if (!IsCasterAlive(context))
                return;

            Vector3 centerDirection =
                context.Target.position -
                context.Enemy.MyTransform.position;

            centerDirection.y = 0f;

            if (centerDirection.sqrMagnitude <= 0.001f)
            {
                centerDirection =
                    context.Enemy.MyTransform.forward;
            }

            centerDirection.Normalize();

            float t = cloudCount == 1
                ? 0.5f
                : i / (cloudCount - 1f);

            float angle = Mathf.Lerp(
                -totalSpreadAngle * 0.5f,
                totalSpreadAngle * 0.5f,
                t
            );

            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f) *
                centerDirection;

            SpawnCloud(context, direction);

            if (i < cloudCount - 1)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(
                        interval)
                );
            }
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
                context.Enemy.MyTransform.position,
                out NavMeshHit originHit,
                navMeshSampleRadius + maxVerticalDifference,
                NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(
                rawPosition,
                out NavMeshHit hit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
        {
            return;
        }

        if (Mathf.Abs(
                hit.position.y - originHit.position.y) >
            maxVerticalDifference)
        {
            return;
        }

        if (!context.Enemy.Detection.IsPointInLeash(
                hit.position))
        {
            return;
        }

        Vector3 position =
            hit.position + Vector3.up * groundOffset;

        GameObject instance =
            ObjectPooling.Instance.SpawnFromPool(
                context.AttackData.projectilePoolType,
                position,
                Quaternion.LookRotation(
                    direction,
                    Vector3.up)
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
}
