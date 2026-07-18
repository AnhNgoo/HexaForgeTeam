using UnityEngine;

public class BossHealthBarDetector : LoadComponents
{
    [Header("Components")]
    [SerializeField] private GameplayBossHealthUI bossHealthUI;
    [SerializeField] private CharacterLockTarget lockTarget;

    [Header("Detection")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("Bán kính quét thô để tìm boss quanh player. Range hiện thật vẫn lấy từ EnemyData.detectRange.")]
    [SerializeField] private float maxScanRange = 50f;

    [Tooltip("Khoảng nới thêm khi đang hiện thanh máu, tránh UI nhấp nháy ở mép detectRange.")]
    [SerializeField] private float hideExtraRange = 5f;

    [Tooltip("Bao lâu quét boss một lần. Không cần quét mỗi frame.")]
    [SerializeField] private float scanInterval = 0.2f;

    private EnemyBase _currentBoss;
    private float _nextScanTime;

    protected override void LoadComponent()
    {
        LoadLockTarget();
        LoadBossHealthUI();
        LoadEnemyLayer();
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    private void Update()
    {
        if (bossHealthUI == null)
            return;

        if (Time.time < _nextScanTime)
            return;

        _nextScanTime = Time.time + scanInterval;

        EnemyBase boss = FindLockedBoss();
        if (boss == null)
            boss = FindNearestBoss();

        if (boss != null)
        {
            _currentBoss = boss;
            bossHealthUI.Show(boss);
            return;
        }

        if (_currentBoss != null && IsValidBossInRange(_currentBoss, hideExtraRange))
        {
            bossHealthUI.Show(_currentBoss);
            return;
        }

        _currentBoss = null;
        bossHealthUI.Hide();
    }

    private void LoadLockTarget()
    {
        if (lockTarget == null)
            lockTarget = GetComponent<CharacterLockTarget>();
    }

    private void LoadBossHealthUI()
    {
        if (bossHealthUI == null)
            bossHealthUI = FindObjectOfType<GameplayBossHealthUI>(true);
    }

    private void LoadEnemyLayer()
    {
        if (enemyLayer.value != 0)
            return;

        int layer = LayerMask.NameToLayer("Enemy");
        if (layer >= 0)
            enemyLayer = 1 << layer;
    }

    private EnemyBase FindLockedBoss()
    {
        if (lockTarget == null || lockTarget.Target == null)
            return null;

        EnemyBase enemy = lockTarget.Target.GetComponentInParent<EnemyBase>();
        return IsValidBossInRange(enemy, hideExtraRange) ? enemy : null;
    }

    private EnemyBase FindNearestBoss()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, maxScanRange, enemyLayer);

        EnemyBase best = null;
        float bestSqrDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (!IsValidBossInRange(enemy, 0f))
                continue;

            float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                best = enemy;
            }
        }

        return best;
    }

    private bool IsValidBossInRange(EnemyBase enemy, float extraRange)
    {
        if (enemy == null || enemy.Data == null || !enemy.Data.isBoss || enemy.Health.CurrentHealth <= 0f)
            return false;

        float bossUiRange = enemy.Data.detectRange + extraRange;
        float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;

        return sqrDistance <= bossUiRange * bossUiRange;
    }
}