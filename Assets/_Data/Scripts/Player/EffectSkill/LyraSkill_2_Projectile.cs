using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class LyraSkill_2_Projectile : LoadComponents, IPoolable
{
    [SerializeField] private PoolType poolType;

    [Header("Arc Phase")]
    [SerializeField] private float arcDuration = 0.5f;    // Tổng thời gian bay cong (cố định, không phụ thuộc khoảng cách)
    [SerializeField] private float arcHeight = 4f;         // Độ cao đỉnh parabol (world units)

    // X = 0..1 (tiến trình thời gian), Y = 0..1 (tỉ lệ tiến về phía target theo chiều ngang)
    // Mặc định: đi nhanh đầu, chậm giữa, nhanh cuối
    [SerializeField]
    private AnimationCurve moveCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(0.5f, 0.5f, 0.8f, 0.8f),
        new Keyframe(1f, 1f, 2f, 0f)
    );

    // X = 0..1 (tiến trình thời gian), Y = 0..1..0 (tỉ lệ độ cao arc)
    // Mặc định: lên nhanh → chậm ở đỉnh → xuống nhanh
    [SerializeField]
    private AnimationCurve arcCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 3f),    // xuất phát, tangent out cao = lên nhanh
        new Keyframe(0.45f, 1f, 0f, 0f),    // đỉnh, tangent = 0 = chậm ở đỉnh
        new Keyframe(1f, 0f, -3f, 0f)    // về 0, tangent in âm = xuống nhanh
    );

    [Header("Chase Phase (sau arc)")]
    [SerializeField] private float chaseSpeed = 14f;   // Tốc độ bay thẳng đến enemy sau khi arc xong

    // ── Runtime ──────────────────────────────────────────
    private enum Phase { Arc, Chase }
    private Phase phase;
    private float arcTimer;

    private Vector3 arcStartPos;      // vị trí đạn lúc spawn
    private Vector3 arcTargetPos;     // vị trí enemy lúc bắn (snapshot) — dùng cho arc
    private Transform targetTransform;
    private PoolType hitEffect;
    private CharacterBase characterBase;
    private EnemyBase targetEnemy;
    private Rigidbody rb;
    private bool isLaunched;

    public PoolType PoolType => poolType;
    public event Action<Transform> OnEnemyDied;

    protected override void LoadComponent()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    // ─────────────────────────────────────────────────────
    private void FixedUpdate()
    {
        if (!isLaunched) return;

        if (targetTransform == null || (targetEnemy != null && targetEnemy.Health.CurrentHealth <= 0f))
        {
            ObjectPooling.Instance.SpawnFromPool(hitEffect, transform.position, Quaternion.identity);
            OnEnemyDied?.Invoke(targetTransform);
            ObjectPooling.Instance.ReturnToPool(poolType, gameObject);

            return;
        }

        if (phase == Phase.Arc)
            UpdateArc();
        else
            UpdateChase();
    }

    // ── Arc: toàn bộ ngang + dọc đều theo timer, không phụ thuộc distance ──
    private void UpdateArc()
    {
        arcTimer += Time.deltaTime;
        float t = arcDuration > 0f ? Mathf.Clamp01(arcTimer / arcDuration) : 1f;

        // Ngang: lerp từ spawn → vị trí enemy lúc bắn, theo moveCurve
        float moveT = moveCurve.Evaluate(t);
        Vector3 flatPos = Vector3.LerpUnclamped(
            new Vector3(arcStartPos.x, 0f, arcStartPos.z),
            new Vector3(arcTargetPos.x, 0f, arcTargetPos.z),
            moveT
        );

        // Dọc: arc lên rồi xuống về tầm enemy lúc bắn, theo arcCurve
        float baseY = Mathf.Lerp(arcStartPos.y, arcTargetPos.y, t);
        float heightY = arcHeight * arcCurve.Evaluate(t);

        Vector3 prevPos = rb.position;
        Vector3 nextPos = new Vector3(flatPos.x, baseY + heightY, flatPos.z);
        rb.MovePosition(nextPos);

        // Quay mặt theo hướng di chuyển thực
        Vector3 delta = nextPos - prevPos;
        if (delta.sqrMagnitude > 0.0001f)
            rb.MoveRotation(Quaternion.LookRotation(delta.normalized, Vector3.up));

        // Chuyển sang Chase khi arc xong
        if (t >= 1f)
        {
            phase = Phase.Chase;
        }
    }

    // ── Chase: bay thẳng đến vị trí enemy real-time với tốc độ cố định ──
    private void UpdateChase()
    {
        Vector3 currentPos = rb.position;
        Vector3 targetPos = targetTransform.position;
        float step = chaseSpeed * Time.fixedDeltaTime;
        float dist = Vector3.Distance(currentPos, targetPos);

        if (dist <= step)
        {
            rb.MovePosition(targetPos);
            ObjectPooling.Instance.ReturnToPool(poolType, gameObject);
            return;
        }

        Vector3 dir = (targetPos - currentPos).normalized;
        rb.MovePosition(currentPos + dir * step);
        rb.MoveRotation(Quaternion.LookRotation(dir, Vector3.up));
    }

    // ─────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (!isLaunched) return;

        if (other.TryGetComponent(out EnemyBase enemy))
        {
            float damage = characterBase.CharacterData.stats.damage;
            float poisonDamage = characterBase.CharacterData.stats.poisonDamage;
            enemy.DamageReceiver?.TakeHit(damage, poisonDamage, transform);
        }

        if (hitEffect != PoolType.None)
            ObjectPooling.Instance.SpawnFromPool(
                hitEffect, other.ClosestPoint(transform.position), Quaternion.identity);

        ObjectPooling.Instance.ReturnToPool(poolType, gameObject);
    }

    // ── Initialize (Transform — đuổi theo enemy di chuyển) ──
    public void Initialize(CharacterBase characterBase, Transform targetTransform, PoolType hitEffect = PoolType.None)
    {
        this.characterBase = characterBase;
        this.targetTransform = targetTransform;
        this.hitEffect = hitEffect;

        if (targetTransform.TryGetComponent(out EnemyBase enemy))
        {
            targetEnemy = enemy;
        }

        ResetRigidBodyState();
        arcStartPos = transform.position;
        arcTargetPos = targetTransform != null ? targetTransform.position : transform.position + transform.forward;

        phase = Phase.Arc;
        arcTimer = 0f;
        isLaunched = true;

        FaceTarget(arcTargetPos);
    }

    // ── Initialize (Vector3 — target tĩnh) ──
    public void Initialize(CharacterBase characterBase, Vector3 startPos, Vector3 targetPos, PoolType hitEffect = PoolType.None)
    {
        this.characterBase = characterBase;
        this.hitEffect = hitEffect;

        // Wrap target tĩnh thành Transform để Chase phase vẫn dùng được
        var go = new GameObject("_StaticTarget");
        go.transform.position = targetPos;
        this.targetTransform = go.transform;

        transform.position = startPos;
        ResetRigidBodyState();
        arcStartPos = startPos;
        arcTargetPos = targetPos;

        phase = Phase.Arc;
        arcTimer = 0f;
        isLaunched = true;

        FaceTarget(arcTargetPos);
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            transform.forward = dir.normalized;
    }

    private void ResetRigidBodyState()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = transform.position;
        rb.rotation = transform.rotation;
    }

    // ── Pool callbacks ──
    public void OnSpawnFromPool()
    {
        isLaunched = false;
        arcTimer = 0f;
        phase = Phase.Arc;
        targetTransform = null;
        ResetRigidBodyState();
    }

    public void OnReturnToPool()
    {
        isLaunched = false;
        arcTimer = 0f;
        phase = Phase.Arc;
        targetTransform = null;
        ResetRigidBodyState();
        OnEnemyDied = null; // Hủy đăng ký sự kiện khi trả về pool
    }
}