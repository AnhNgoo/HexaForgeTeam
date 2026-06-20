using Cysharp.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCastLightningSkillSO", menuName = "Enemy/Skills/CastLightning")]
public class EnemyCastLightningSkillSO : EnemyAttackSkillSO
{

    [SerializeField] private PoolType telegraphPool;
    [SerializeField] private PoolType lightningPool;

    [SerializeField] private float collumnSpacing = 2f;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private float intervalBetweenColumns = 0.12f;
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float hitboxActiveDuration = 0.15f;
    [SerializeField] private Vector3 lightningRotation = new(-90f, 0f, 0f);
    [SerializeField] private float lightningSpawnHeight = 10f;
    [SerializeField] private float lightningFallDuration = 0.3f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        base.OnAttackImpact(context);
        if (context?.Enemy == null || context.Target == null || context.AttackData == null) return;

        CastAsync(context).Forget();
    }

    private async UniTaskVoid CastAsync(EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        Vector3 center = context.Target.position;

        Vector3 direction = center - enemy.MyTransform.position;
        direction.y = 0f;

        Vector3 right = direction.sqrMagnitude > 0.01f ? Vector3.Cross(Vector3.up, direction.normalized) : enemy.MyTransform.right;

        Vector3[] positions = { center, center + right * collumnSpacing, center - right * collumnSpacing };

        for (int i = 0; i < positions.Length; i++)
        {
            if (NavMesh.SamplePosition(positions[i], out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                positions[i] = hit.position;
            }
            ObjectPooling.Instance.SpawnFromPool(telegraphPool, positions[i], Quaternion.identity);
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(warningDuration));

        foreach (Vector3 position in positions)
        {
            StrikeAsync(context, position).Forget();

            await UniTask.Delay(System.TimeSpan.FromSeconds(intervalBetweenColumns));
        }
    }

    private async UniTaskVoid DisableHitboxAsync(EnemyHitbox hitbox)
    {
        await UniTask.Delay(
            System.TimeSpan.FromSeconds(hitboxActiveDuration)
        );

        if (hitbox != null)
            hitbox.DisableHitBox();
    }

    private async UniTaskVoid StrikeAsync(EnemyAttackContext context, Vector3 landingPosition)
    {
        Vector3 spawnPosition = landingPosition + Vector3.up * lightningSpawnHeight;

        GameObject lightning = ObjectPooling.Instance.SpawnFromPool(lightningPool, spawnPosition, Quaternion.Euler(lightningRotation));

        if (lightning == null)
        {
            Debug.LogWarning("[CastLightning] Không lấy được object từ pool.");
            return;
        }

        EnemyHitbox hitbox =
            lightning.GetComponentInChildren<EnemyHitbox>(true);

        if (hitbox == null)
        {
            Debug.LogWarning("[CastLightning] Prefab thiếu EnemyHitbox.");
            return;
        }

        // Không gây damage trong lúc đang rơi.
        hitbox.DisableHitBox();

        await MoveLightningDownAsync(lightning, spawnPosition, landingPosition);

        if (lightning == null || !lightning.activeInHierarchy)
            return;

        // Chỉ gây damage khi sét chạm đất.
        hitbox.Initialize(context.Enemy, context.AttackData);
        hitbox.EnableHitBox();

        DisableHitboxAsync(hitbox).Forget();
    }

    private async UniTask MoveLightningDownAsync(GameObject lightning, Vector3 startPosition, Vector3 landingPosition)
    {
        float elapsed = 0f;

        while (elapsed < lightningFallDuration)
        {
            if (lightning == null || !lightning.activeInHierarchy)
                return;

            elapsed += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsed / lightningFallDuration);

            float curvedTime = fallCurve.Evaluate(normalizedTime);

            lightning.transform.position = Vector3.Lerp(startPosition, landingPosition, curvedTime);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        if (lightning != null && lightning.activeInHierarchy)
            lightning.transform.position = landingPosition;
    }
}
