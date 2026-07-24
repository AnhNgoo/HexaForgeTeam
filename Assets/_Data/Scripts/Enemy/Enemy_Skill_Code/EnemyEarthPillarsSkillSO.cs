using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyPillarPattern
{
    Random,
    FaultLine,
    RingWithGap
}

[CreateAssetMenu(
    fileName = "EnemyEarthPillarsSkill",
    menuName = "Enemy/Skills/Earth Pillars")]
public class EnemyEarthPillarsSkillSO : EnemyAttackSkillSO
{
    [Header("Pools")]
    [SerializeField] private PoolType telegraphPool;
    [SerializeField] private PoolType pillarPool;

    [Header("Pattern")]
    [SerializeField] private EnemyPillarPattern pattern;
    [SerializeField] private int pillarCount = 6;
    [SerializeField] private float radius = 7f;
    [SerializeField] private float minimumStartDistance = 2f;
    [SerializeField] private float faultLineWidth = 1.2f;
    [SerializeField] private float faultLineWaveAngle = 18f;
    [SerializeField] private float ringGapDegrees = 65f;
    [SerializeField] private float ringRotationPerWave = 45f;

    [Header("Timing")]
    [SerializeField] private float warningDuration = 0.7f;
    [SerializeField] private float spawnInterval = 0.12f;
    [SerializeField] private float waveInterval = 0.2f;
    [SerializeField] private float pillarLifetime = 1f;

    [Header("Placement")]
    [SerializeField] private float navMeshSampleRadius = 1.5f;
    [SerializeField] private float maxVerticalDifference = 1.5f;

    [Header("Cataclysm / Venom Bloom")]
    [SerializeField] private int cataclysmWaves = 3;
    [SerializeField] private int cataclysmPillarsPerWave = 8;
    [SerializeField] private float cataclysmRadius = 10f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        int waves = IsPhase2(context) ? 2 : 1;

        CastPatternAsync(
            context,
            waves,
            pillarCount,
            radius
        ).Forget();
    }

    public UniTask CastCataclysmAsync(
        EnemyAttackContext context)
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
            if (!IsCasterAlive(context))
                return;

            float waveRadius =
                maximumRadius * (wave + 1f) / waves;

            List<Vector3> positions = BuildPositions(
                context,
                countPerWave,
                waveRadius,
                wave
            );

            List<GameObject> warnings =
                new List<GameObject>(positions.Count);

            foreach (Vector3 position in positions)
            {
                warnings.Add(
                    ObjectPooling.Instance.SpawnFromPool(
                        telegraphPool,
                        position,
                        Quaternion.identity
                    )
                );
            }

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(warningDuration)
            );

            for (int i = 0; i < positions.Count; i++)
            {
                if (!IsCasterAlive(context))
                {
                    ReturnWarnings(warnings, i);
                    return;
                }

                ReturnWarning(warnings[i]);
                SpawnPillar(context, positions[i]);

                if (i < positions.Count - 1)
                {
                    await UniTask.Delay(
                        System.TimeSpan.FromSeconds(
                            spawnInterval)
                    );
                }
            }

            if (wave < waves - 1)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(waveInterval)
                );
            }
        }
    }

    private List<Vector3> BuildPositions(
        EnemyAttackContext context,
        int count,
        float spawnRadius,
        int waveIndex)
    {
        return pattern switch
        {
            EnemyPillarPattern.FaultLine =>
                BuildFaultLine(
                    context,
                    count,
                    spawnRadius,
                    waveIndex
                ),

            EnemyPillarPattern.RingWithGap =>
                BuildRingWithGap(
                    context,
                    count,
                    spawnRadius,
                    waveIndex
                ),

            _ => BuildRandom(
                context,
                count,
                spawnRadius
            )
        };
    }

    private List<Vector3> BuildFaultLine(
        EnemyAttackContext context,
        int count,
        float lineLength,
        int waveIndex)
    {
        List<Vector3> positions =
            new List<Vector3>(count);

        if (!TryGetNavCenter(
                context.Enemy.MyTransform.position,
                out NavMeshHit originHit))
        {
            return positions;
        }

        Vector3 forward =
            context.Target.position - originHit.position;
        forward.y = 0f;

        if (forward.sqrMagnitude <= 0.01f)
            forward = context.Enemy.MyTransform.forward;

        float signedWave =
            waveIndex == 0
                ? 0f
                : (waveIndex % 2 == 0 ? -1f : 1f) *
                  faultLineWaveAngle *
                  ((waveIndex + 1) / 2f);

        forward =
            Quaternion.Euler(0f, signedWave, 0f) *
            forward.normalized;

        Vector3 right =
            Vector3.Cross(Vector3.up, forward).normalized;

        for (int i = 0; i < count; i++)
        {
            float t = count == 1
                ? 1f
                : i / (count - 1f);

            float distance = Mathf.Lerp(
                minimumStartDistance,
                lineLength,
                t
            );

            float side =
                i % 2 == 0 ? -0.5f : 0.5f;

            Vector3 candidate =
                originHit.position +
                forward * distance +
                right * side * faultLineWidth;

            TryAddPosition(
                context.Enemy,
                positions,
                candidate,
                originHit.position.y
            );
        }

        return positions;
    }

    private List<Vector3> BuildRingWithGap(
        EnemyAttackContext context,
        int count,
        float ringRadius,
        int waveIndex)
    {
        List<Vector3> positions =
            new List<Vector3>(count);

        if (!TryGetNavCenter(
                context.Target.position,
                out NavMeshHit centerHit))
        {
            return positions;
        }

        Vector3 gapDirection =
            context.Target.position -
            context.Enemy.MyTransform.position;
        gapDirection.y = 0f;

        if (gapDirection.sqrMagnitude <= 0.01f)
            gapDirection = context.Enemy.MyTransform.forward;

        float baseAngle =
            Mathf.Atan2(
                gapDirection.z,
                gapDirection.x
            ) * Mathf.Rad2Deg;

        baseAngle += waveIndex * ringRotationPerWave;

        float usableDegrees =
            Mathf.Max(0f, 360f - ringGapDegrees);

        for (int i = 0; i < count; i++)
        {
            float t = count == 1
                ? 0.5f
                : i / (count - 1f);

            float angle =
                baseAngle +
                ringGapDegrees * 0.5f +
                Mathf.Lerp(0f, usableDegrees, t);

            float radians = angle * Mathf.Deg2Rad;

            Vector3 candidate =
                centerHit.position +
                new Vector3(
                    Mathf.Cos(radians),
                    0f,
                    Mathf.Sin(radians)
                ) * ringRadius;

            TryAddPosition(
                context.Enemy,
                positions,
                candidate,
                centerHit.position.y
            );
        }

        return positions;
    }

    private List<Vector3> BuildRandom(
        EnemyAttackContext context,
        int count,
        float spawnRadius)
    {
        List<Vector3> positions =
            new List<Vector3>(count);

        if (!TryGetNavCenter(
                context.Target.position,
                out NavMeshHit centerHit))
        {
            return positions;
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 offset =
                Random.insideUnitCircle * spawnRadius;

            Vector3 candidate =
                centerHit.position +
                new Vector3(offset.x, 0f, offset.y);

            TryAddPosition(
                context.Enemy,
                positions,
                candidate,
                centerHit.position.y
            );
        }

        return positions;
    }

    private bool TryGetNavCenter(
        Vector3 position,
        out NavMeshHit hit)
    {
        return NavMesh.SamplePosition(
            position,
            out hit,
            navMeshSampleRadius + maxVerticalDifference,
            NavMesh.AllAreas
        );
    }

    private void TryAddPosition(
        EnemyBase enemy,
        List<Vector3> positions,
        Vector3 candidate,
        float referenceY)
    {
        if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
        {
            return;
        }

        if (Mathf.Abs(hit.position.y - referenceY) >
            maxVerticalDifference)
        {
            return;
        }

        if (!enemy.Detection.IsPointInLeash(hit.position))
            return;

        positions.Add(hit.position);
    }

    private void SpawnPillar(
        EnemyAttackContext context,
        Vector3 position)
    {
        GameObject pillar =
            ObjectPooling.Instance.SpawnFromPool(
                pillarPool,
                position,
                Quaternion.identity
            );

        if (pillar == null)
            return;

        EnemyHitbox hitbox =
            pillar.GetComponentInChildren<EnemyHitbox>(true);

        hitbox?.Initialize(
            context.Enemy,
            context.AttackData,
            context.RuntimeDamageMultiplier
        );

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

        if (pillar != null &&
            pillar.activeInHierarchy &&
            ObjectPooling.Instance != null)
        {
            ObjectPooling.Instance.ReturnToPool(
                pillarPool,
                pillar
            );
        }
    }

    private void ReturnWarning(GameObject warning)
    {
        if (warning != null &&
            warning.activeInHierarchy &&
            ObjectPooling.Instance != null)
        {
            ObjectPooling.Instance.ReturnToPool(
                telegraphPool,
                warning
            );
        }
    }

    private void ReturnWarnings(
        List<GameObject> warnings,
        int startIndex)
    {
        for (int i = startIndex; i < warnings.Count; i++)
            ReturnWarning(warnings[i]);
    }

    private static bool IsPhase2(
        EnemyAttackContext context)
    {
        BruteBossBehaviour brute =
            context.Enemy.GetComponent<BruteBossBehaviour>();

        if (brute != null)
            return brute.IsPhase2Active;

        VenomousQueenBossBehaviour queen =
            context.Enemy.GetComponent<
                VenomousQueenBossBehaviour>();

        return queen != null && queen.IsPhase2Active;
    }

    private static bool IsValid(
        EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.Target != null &&
               context.AttackData != null;
    }

    private static bool IsCasterAlive(
        EnemyAttackContext context)
    {
        return IsValid(context) &&
               context.Enemy.gameObject.activeInHierarchy &&
               context.Enemy.Health.CurrentHealth > 0f;
    }
}
