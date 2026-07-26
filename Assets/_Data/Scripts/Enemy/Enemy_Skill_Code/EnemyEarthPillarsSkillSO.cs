using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyEarthPillarsSkill",
    menuName = "Enemy/Skills/Earth Pillars")]
public class EnemyEarthPillarsSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private PoolType telegraphPool;
    [SerializeField] private PoolType pillarPool;

    [SerializeField] private int pillarCount = 5;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float warningDuration = 0.7f;
    [SerializeField] private float spawnInterval = 0.12f;
    [SerializeField] private float pillarLifetime = 1f;
    [SerializeField] private float navMeshSampleRadius = 1.5f;

    [Header("Cataclysm")]
    [SerializeField] private int cataclysmWaves = 3;
    [SerializeField] private int cataclysmPillarsPerWave = 8;
    [SerializeField] private float cataclysmRadius = 10f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context?.Enemy == null || context.Target == null)
            return;

        CastPatternAsync(context, 1, pillarCount, radius).Forget();
    }

    public UniTask CastCataclysmAsync(EnemyAttackContext context)
    {
        return CastPatternAsync(
            context,
            cataclysmWaves,
            cataclysmPillarsPerWave,
            cataclysmRadius
        );
    }

    private async UniTask CastPatternAsync(
        EnemyAttackContext context,
        int waves,
        int countPerWave,
        float maximumRadius)
    {
        for (int wave = 0; wave < waves; wave++)
        {
            float waveRadius =
                maximumRadius * (wave + 1f) / waves;

            List<Vector3> positions = BuildPositions(
                context.Target.position,
                countPerWave,
                waveRadius
            );

            List<GameObject> warnings = new();

            foreach (Vector3 position in positions)
            {
                GameObject warning =
                    ObjectPooling.Instance.SpawnFromPool(
                        telegraphPool,
                        position,
                        Quaternion.identity
                    );

                if (warning != null)
                    warnings.Add(warning);
            }

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(warningDuration)
            );

            foreach (GameObject warning in warnings)
            {
                if (warning != null && warning.activeInHierarchy)
                    ObjectPooling.Instance.ReturnToPool(
                        telegraphPool,
                        warning
                    );
            }

            foreach (Vector3 position in positions)
            {
                SpawnPillar(context, position);

                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(spawnInterval)
                );
            }
        }
    }

    private List<Vector3> BuildPositions(
        Vector3 center,
        int count,
        float spawnRadius)
    {
        List<Vector3> positions = new(count);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset =
                Random.insideUnitCircle * spawnRadius;

            Vector3 candidate =
                center + new Vector3(offset.x, 0f, offset.y);

            if (NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleRadius,
                    NavMesh.AllAreas))
            {
                positions.Add(hit.position);
            }
        }

        return positions;
    }

    private void SpawnPillar(
        EnemyAttackContext context,
        Vector3 position)
    {
        GameObject pillar = ObjectPooling.Instance.SpawnFromPool(
            pillarPool,
            position,
            Quaternion.identity
        );

        if (pillar == null)
            return;

        EnemyHitbox hitbox =
            pillar.GetComponentInChildren<EnemyHitbox>(true);

        hitbox?.Initialize(context.Enemy, context.AttackData, context.RuntimeDamageMultiplier);
        hitbox?.EnableHitBox();

        ReturnPillarAsync(pillar, hitbox).Forget();
    }

    private async UniTaskVoid ReturnPillarAsync(
        GameObject pillar,
        EnemyHitbox hitbox)
    {
        await UniTask.Delay(
            System.TimeSpan.FromSeconds(pillarLifetime)
        );

        hitbox?.DisableHitBox();

        if (pillar != null && pillar.activeInHierarchy)
            ObjectPooling.Instance.ReturnToPool(pillarPool, pillar);
    }
}