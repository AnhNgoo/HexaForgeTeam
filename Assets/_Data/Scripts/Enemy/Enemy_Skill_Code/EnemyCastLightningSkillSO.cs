using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCastLightningSkillSO", menuName = "Enemy/Skills/CastLightning")]
public class EnemyCastLightningSkillSO : EnemyAttackSkillSO
{

    [SerializeField] private PoolType telegraphPool;
    [SerializeField] private PoolType lightningPool;

    [SerializeField] private int minColumns = 3; // Cột nhỏ nhất để sét đánh xuống.
    [SerializeField] private int maxColumns = 5; // Cột lớn nhất để sét đánh xuống.
    [SerializeField] private float strikeRadius = 4f; // Bán kính để sét đánh xuống, tính từ vị trí trung tâm của mục tiêu.
    [SerializeField] private float minColumnDistance = 1.5f; // Khoảng cách tối thiểu giữa các cột sét để tránh chồng chéo.
    [SerializeField] private int maxPositionAttempts = 20; // Số lần thử để tìm vị trí hợp lệ cho các cột sét.
    [SerializeField] private float warningDuration = 1f; // Thời gian cảnh báo trước khi sét đánh xuống.
    [SerializeField] private float intervalBetweenColumns = 0.12f; // Thời gian giữa các cột sét đánh xuống.
    [SerializeField] private float navMeshSampleRadius = 2f; // Bán kính để kiểm tra vị trí hợp lệ trên NavMesh.
    [SerializeField] private float lightningGroundDuration = 0.5f; // Thời gian sét nằm trên đất trước khi tắt hitbox và trả pool.
    [SerializeField] private Vector3 lightningRotation = new(-90f, 0f, 0f); // Góc xoay của sét khi spawn, để sét hướng xuống đất.
    [SerializeField] private float lightningSpawnHeight = 10f; // Chiều cao spawn của sét so với mặt đất, để sét rơi xuống từ trên cao.
    [SerializeField] private float lightningFallDuration = 0.3f; // Thời gian sét rơi từ spawn xuống mặt đất.
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

        List<Vector3> positions = BuildStrikePositions(center);

        foreach (Vector3 position in positions)
        {
            ObjectPooling.Instance.SpawnFromPool(telegraphPool, position, Quaternion.identity);
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(warningDuration));

        foreach (Vector3 position in positions)
        {
            StrikeAsync(context, position).Forget();

            await UniTask.Delay(System.TimeSpan.FromSeconds(intervalBetweenColumns));
        }
    }

    private List<Vector3> BuildStrikePositions(Vector3 center)
    {
        int count = Random.Range(minColumns, maxColumns + 1);
        List<Vector3> positions = new List<Vector3>(count);

        TryAddStrikePosition(center, positions);

        // ponytail: random sampling is enough here; use fixed combat slots only if overlap becomes noticeable.
        for (int i = 0; i < maxPositionAttempts && positions.Count < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * strikeRadius;
            Vector3 rawPosition = center + new Vector3(offset.x, 0f, offset.y);

            TryAddStrikePosition(rawPosition, positions);
        }

        return positions;
    }

    private bool TryAddStrikePosition(Vector3 rawPosition, List<Vector3> positions)
    {
        if (!NavMesh.SamplePosition(rawPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            return false;

        float minDistanceSqr = minColumnDistance * minColumnDistance;

        foreach (Vector3 position in positions)
        {
            if ((position - hit.position).sqrMagnitude < minDistanceSqr)
                return false;
        }

        positions.Add(hit.position);
        return true;
    }

    private async UniTaskVoid FinishLightningAsync(EnemyHitbox hitbox, GameObject lightning)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(lightningGroundDuration));

        if (hitbox != null)
            hitbox.DisableHitBox();

        if (lightning != null && lightning.activeInHierarchy)
            ObjectPooling.Instance.ReturnToPool(lightningPool, lightning);
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

        // Khởi tạo hitbox với thông tin từ context và kích hoạt nó.
        hitbox.Initialize(context.Enemy, context.AttackData, context.RuntimeDamageMultiplier);
        hitbox.EnableHitBox();

        await MoveLightningDownAsync(lightning, spawnPosition, landingPosition);

        if (lightning == null || !lightning.activeInHierarchy)
            return;

        FinishLightningAsync(hitbox, lightning).Forget();
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
