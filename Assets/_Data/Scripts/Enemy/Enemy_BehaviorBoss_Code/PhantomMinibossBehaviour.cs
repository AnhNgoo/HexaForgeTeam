using UnityEngine.AI;
using UnityEngine;

public class PhantomMinibossBehaviour : EnemyMinibossBehaviour
{
    private enum BlinkPhase
    {
        None,
        WaitingToWarp,
        WaitingToReappear
    }

    [Header("Arcane Surge")]
    [SerializeField, Range(0f, 1f)] private float surgeHealthRatio = 0.5f;
    [SerializeField] private float attackSpeedMultiplier = 1.25f;

    [Header("Blink")]
    [SerializeField] private float blinkTriggerRange = 4f;
    [SerializeField] private float blinkDistance = 8f;
    [SerializeField] private float blinkCooldown = 6f;
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float disappearDelay = 0.5f;
    [SerializeField] private float reappearDelay = 0.5f;

    [Header("Blink Presentation")]
    [SerializeField] private PoolType blinkVFX;
    [SerializeField] private Renderer[] renderersToHide;

    private bool[] _defaultRendererStates;
    private bool _arcaneSurgeActive;
    private float _nextBlinkTime;
    private float _phaseEndTime;
    private Vector3 _blinkDestination;
    private BlinkPhase _blinkPhase;

    protected override void Awake()
    {
        base.Awake();

        if (renderersToHide == null || renderersToHide.Length == 0)
        {
            renderersToHide = GetComponentsInChildren<Renderer>(true);
        }

        _defaultRendererStates = new bool[renderersToHide.Length];

        for (int i = 0; i < renderersToHide.Length; i++)
        {
            _defaultRendererStates[i] = renderersToHide[i] != null && renderersToHide[i].enabled;
        }
    }

    private void Update()
    {
        if (Enemy == null || Enemy.Health.CurrentHealth <= 0f) return;

        UpdateArcaneSurge();

        if (_blinkPhase != BlinkPhase.None)
        {
            UpdateBlink();
            return;
        }

        TryStartBlink();
    }

    public override float ModifyAttackCooldown(float cooldown)
    {
        if (!_arcaneSurgeActive)
            return cooldown;

        return cooldown / Mathf.Max(1f, attackSpeedMultiplier);
    }

    private void UpdateArcaneSurge()
    {
        if (_arcaneSurgeActive) return;

        float healthRatio = Enemy.Health.CurrentHealth / Enemy.Data.maxHealth;

        if (healthRatio <= surgeHealthRatio)
        {
            _arcaneSurgeActive = true;
            // Có thể thêm hiệu ứng hoặc âm thanh ở đây để báo hiệu Arcane Surge kích hoạt.
        }
    }

    private void TryStartBlink()
    {
        if (Time.time < _nextBlinkTime) return;

        Transform target = Enemy.Detection.CurrentTarget;
        if (target == null) return;

        float distance = Vector3.Distance(Enemy.MyTransform.position, target.position);

        if (distance > blinkTriggerRange) return;

        if (!TryFindBlinkDestination(target, out _blinkDestination))
        {
            _nextBlinkTime = Time.time + 1f;
            return;
        }

        IsActionLocked = true;

        Enemy.Combat.ForceCloseHitbox();
        Enemy.Locomotion.StopMoving();

        PlayBlinkVFX(Enemy.MyTransform.position);
        SetRenderersVisible(false);

        _blinkPhase = BlinkPhase.WaitingToWarp;
        _phaseEndTime = Time.time + disappearDelay;
    }

    private void UpdateBlink()
    {
        Enemy.Locomotion.StopMoving();

        if (Time.time < _phaseEndTime) return;

        if (_blinkPhase == BlinkPhase.WaitingToWarp)
        {
            Enemy.Locomotion.WarpTo(_blinkDestination);
            PlayBlinkVFX(_blinkDestination);

            _blinkPhase = BlinkPhase.WaitingToReappear;
            _phaseEndTime = Time.time + reappearDelay;
            return;
        }

        SetRenderersVisible(true);

        _blinkPhase = BlinkPhase.None;
        IsActionLocked = false;
        _nextBlinkTime = Time.time + blinkCooldown;
    }

    private bool TryFindBlinkDestination(Transform target, out Vector3 destination)
    {
        destination = Enemy.MyTransform.position;

        Vector3 away = Enemy.MyTransform.position - target.position;
        away.y = 0f;

        if (away.sqrMagnitude <= 0.001f)
            away = -Enemy.MyTransform.forward;

        away.Normalize();

        float bestDistance = float.MinValue;
        bool found = false;

        for (int i = 0; i < 8; i++)
        {
            Vector3 direction = Quaternion.Euler(0f, i * 360f / 8f, 0f) * away;
            Vector3 candidate = Enemy.MyTransform.position + direction * blinkDistance;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
                continue;

            float currentY = Enemy.MyTransform.position.y;
            float maxVerticalDifference = 1.5f;

            if (Mathf.Abs(hit.position.y - currentY) > maxVerticalDifference)
                continue;

            if (!Enemy.Detection.IsPointInLeash(hit.position))
                continue;

            float distanceFromPlayer = (hit.position - target.position).sqrMagnitude;

            if (distanceFromPlayer <= bestDistance)
                continue;

            bestDistance = distanceFromPlayer;

            Vector3 finalPosition = hit.position;
            finalPosition.y = Enemy.MyTransform.position.y;

            destination = finalPosition;
            found = true;
        }

        return found;
    }

    private void PlayBlinkVFX(Vector3 position)
    {
        if (blinkVFX == PoolType.None) return;
        ObjectPooling.Instance.SpawnFromPool(blinkVFX, position, Quaternion.identity);
    }

    private void SetRenderersVisible(bool visible)
    {
        for (int i = 0; i < renderersToHide.Length; i++)
        {
            if (renderersToHide[i] == null) continue;

            renderersToHide[i].enabled = visible && _defaultRendererStates[i];
        }
    }

    public override void ResetBehaviour()
    {
        base.ResetBehaviour();
        _arcaneSurgeActive = false;
        _nextBlinkTime = 0f;
        _phaseEndTime = 0f;
        _blinkPhase = BlinkPhase.None;

        if (_defaultRendererStates != null)
            SetRenderersVisible(true);
    }
}
