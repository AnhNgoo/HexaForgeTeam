using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(
    fileName = "EnemyShadowRainSkill",
    menuName = "Enemy/Skills/Shadow Rain")]
public class EnemyShadowRainSkillSO : EnemyAttackSkillSO
{
    [Header("Pools")]
    [SerializeField] private PoolType telegraphPool;
    [SerializeField] private PoolType strikePool;

    [Header("Shadow Lanes")]
    [SerializeField, Min(1)] private int strikeCount = 9;
    [SerializeField, Min(1)] private int laneCount = 3;
    [SerializeField] private float laneLength = 10f;
    [SerializeField] private float laneSpacing = 2.4f;
    [SerializeField] private float predictionTime = 0.35f;
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float maxVerticalDifference = 1.5f;

    [Header("Timing")]
    [SerializeField] private float warningDuration = 0.65f;
    [SerializeField] private float intervalBetweenStrikes = 0.08f;
    [SerializeField] private float spawnHeight = 9f;
    [SerializeField] private float fallDuration = 0.4f;
    [SerializeField] private float groundDuration = 0.25f;
    [SerializeField]
    private AnimationCurve fallCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsCasterAlive(context) || context.Target == null)
            return;

        CastRainAsync(context, 1, 0f).Forget();
    }

    public async UniTask CastRainAsync(
        EnemyAttackContext context,
        int waves,
        float waveInterval)
    {
        for (int wave = 0; wave < waves; wave++)
        {
            if (!IsCasterAlive(context) ||
                context.Target == null)
            {
                return;
            }

            await CastWaveAsync(context, wave);

            if (wave < waves - 1 && waveInterval > 0f)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(waveInterval)
                );
            }
        }
    }

    private async UniTask CastWaveAsync(
        EnemyAttackContext context,
        int waveIndex)
    {
        List<Vector3> positions =
            BuildLanePositions(context, waveIndex);

        List<GameObject> telegraphs =
            new List<GameObject>(positions.Count);

        foreach (Vector3 position in positions)
        {
            GameObject telegraph =
                ObjectPooling.Instance.SpawnFromPool(
                    telegraphPool,
                    position,
                    Quaternion.identity
                );

            telegraphs.Add(telegraph);
        }

        await UniTask.Delay(
            System.TimeSpan.FromSeconds(warningDuration)
        );

        for (int i = 0; i < positions.Count; i++)
        {
            if (!IsCasterAlive(context))
            {
                ReturnTelegraphs(telegraphs, i);
                return;
            }

            ReturnTelegraph(telegraphs[i]);
            DropStrikeAsync(context, positions[i]).Forget();

            if (i < positions.Count - 1)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(
                        intervalBetweenStrikes)
                );
            }
        }
    }

    private List<Vector3> BuildLanePositions(
        EnemyAttackContext context,
        int waveIndex)
    {
        List<Vector3> positions =
            new List<Vector3>(strikeCount);

        Vector3 center = context.Target.position;
        Vector3 movementDirection =
            GetTargetMovementDirection(context);

        center +=
            movementDirection * GetTargetSpeed(context) *
            predictionTime;

        Vector3 right =
            Vector3.Cross(Vector3.up, movementDirection).normalized;

        if (!NavMesh.SamplePosition(
                center,
                out NavMeshHit centerHit,
                navMeshSampleRadius,
                NavMesh.AllAreas))
        {
            return positions;
        }

        float waveRotation = waveIndex * 25f;
        movementDirection =
            Quaternion.Euler(0f, waveRotation, 0f) *
            movementDirection;
        right = Vector3.Cross(
            Vector3.up,
            movementDirection
        ).normalized;

        for (int i = 0; i < strikeCount; i++)
        {
            float alongT = strikeCount == 1
                ? 0.5f
                : i / (strikeCount - 1f);

            int lane = i % laneCount;
            float laneOffset =
                (lane - (laneCount - 1) * 0.5f) *
                laneSpacing;

            float lengthOffset =
                Mathf.Lerp(
                    -laneLength * 0.5f,
                    laneLength * 0.5f,
                    alongT
                );

            Vector3 candidate =
                centerHit.position +
                movementDirection * lengthOffset +
                right * laneOffset;

            if (!NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    navMeshSampleRadius,
                    NavMesh.AllAreas))
            {
                continue;
            }

            if (Mathf.Abs(
                    hit.position.y - centerHit.position.y) >
                maxVerticalDifference)
            {
                continue;
            }

            if (!context.Enemy.Detection.IsPointInLeash(
                    hit.position))
            {
                continue;
            }

            positions.Add(hit.position);
        }

        return positions;
    }

    private static Vector3 GetTargetMovementDirection(
        EnemyAttackContext context)
    {
        CharacterMovement movement =
            context.Target.GetComponentInParent<CharacterMovement>();

        Vector3 direction =
            movement?.CC != null
                ? movement.CC.velocity
                : Vector3.zero;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.1f)
        {
            direction =
                context.Target.position -
                context.Enemy.MyTransform.position;
            direction.y = 0f;
        }

        return direction.sqrMagnitude > 0.001f
            ? direction.normalized
            : context.Enemy.MyTransform.forward;
    }

    private static float GetTargetSpeed(
        EnemyAttackContext context)
    {
        CharacterMovement movement =
            context.Target.GetComponentInParent<CharacterMovement>();

        if (movement?.CC == null)
            return 0f;

        Vector3 velocity = movement.CC.velocity;
        velocity.y = 0f;
        return velocity.magnitude;
    }

    private async UniTaskVoid DropStrikeAsync(
        EnemyAttackContext context,
        Vector3 landingPosition)
    {
        Vector3 spawnPosition =
            landingPosition + Vector3.up * spawnHeight;

        GameObject strike =
            ObjectPooling.Instance.SpawnFromPool(
                strikePool,
                spawnPosition,
                Quaternion.identity
            );

        if (strike == null)
            return;

        EnemyHitbox hitbox =
            strike.GetComponentInChildren<EnemyHitbox>(true);

        hitbox?.Initialize(
            context.Enemy,
            context.AttackData,
            context.RuntimeDamageMultiplier
        );

        hitbox?.EnableHitBox();

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            if (!IsCasterAlive(context) ||
                strike == null ||
                !strike.activeInHierarchy)
            {
                ReturnStrike(strike, hitbox);
                return;
            }

            elapsed += Time.deltaTime;
            float normalized =
                Mathf.Clamp01(elapsed / fallDuration);

            strike.transform.position = Vector3.Lerp(
                spawnPosition,
                landingPosition,
                fallCurve.Evaluate(normalized)
            );

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        if (strike == null || !strike.activeInHierarchy)
            return;

        strike.transform.position = landingPosition;

        await UniTask.Delay(
            System.TimeSpan.FromSeconds(groundDuration)
        );

        ReturnStrike(strike, hitbox);
    }

    private void ReturnStrike(
        GameObject strike,
        EnemyHitbox hitbox)
    {
        hitbox?.DisableHitBox();

        if (strike != null &&
            strike.activeInHierarchy &&
            ObjectPooling.Instance != null)
        {
            ObjectPooling.Instance.ReturnToPool(
                strikePool,
                strike
            );
        }
    }

    private void ReturnTelegraph(GameObject telegraph)
    {
        if (telegraph != null &&
            telegraph.activeInHierarchy &&
            ObjectPooling.Instance != null)
        {
            ObjectPooling.Instance.ReturnToPool(
                telegraphPool,
                telegraph
            );
        }
    }

    private void ReturnTelegraphs(
        List<GameObject> telegraphs,
        int startIndex)
    {
        for (int i = startIndex; i < telegraphs.Count; i++)
            ReturnTelegraph(telegraphs[i]);
    }

    private static bool IsCasterAlive(
        EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.Enemy.gameObject.activeInHierarchy &&
               context.Enemy.Health.CurrentHealth > 0f;
    }
}
