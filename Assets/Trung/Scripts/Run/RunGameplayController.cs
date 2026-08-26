using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

public class RunGameplayController : MonoBehaviour
{
    public static RunGameplayController Instance;

    public int MonstersKilled { get; private set; }
    public float TimeElapsed { get; private set; }
    public float TotalDamageDealt { get; private set; }

    public int NormalKilled { get; private set; }
    public int EliteKilled { get; private set; }
    public int BossKilled { get; private set; }
    public int FinalBossKilled { get; private set; }

    public bool IsFinalBossDefeated => FinalBossKilled > 0;

    private readonly HashSet<EnemyBase> trackedEnemies = new HashSet<EnemyBase>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDisable()
    {
        UnregisterAllTrackedEnemies();
    }

    private void OnDestroy()
    {
        UnregisterAllTrackedEnemies();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        TimeElapsed += Time.deltaTime;
        ScanAndRegisterEnemies();

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            OnSkipRunPressed(true);
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            OnSkipRunPressed(false);
        }
    }

    public void RegisterPlayerDamage(float damage)
    {
        if (this == null || damage <= 0) return;
        TotalDamageDealt += damage;
        if (RunManager.Instance != null)
        {
            RunManager.Instance.RegisterDamage(damage);
        }
    }

    private void ScanAndRegisterEnemies()
    {
        if (this == null) return;

        EnemyBase[] activeEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && !trackedEnemies.Contains(enemy) && enemy.EventManager != null)
            {
                trackedEnemies.Add(enemy);
                enemy.EventManager.OnTakeDamage += RegisterPlayerDamage;
                enemy.EventManager.OnDead += () => OnEnemyDiedHandler(enemy);
            }
        }
    }

    private void OnEnemyDiedHandler(EnemyBase enemy)
    {
        if (this == null || !gameObject.activeInHierarchy || enemy == null) return;

        if (trackedEnemies.Contains(enemy))
        {
            trackedEnemies.Remove(enemy);
        }

        OnEnemyDiedRealtime(enemy);
    }

    private void UnregisterAllTrackedEnemies()
    {
        foreach (var enemy in trackedEnemies)
        {
            if (enemy != null && enemy.EventManager != null)
            {
                enemy.EventManager.OnTakeDamage -= RegisterPlayerDamage;
            }
        }
        trackedEnemies.Clear();
    }

    private void OnEnemyDiedRealtime(EnemyBase enemy)
    {
        if (this == null || enemy == null || enemy.Data == null) return;

        if (!enemy.Data.isBoss)
        {
            NormalKilled++;
            if (RunManager.Instance != null) RunManager.Instance.AddKillCount(1, 0, 0, 0);
        }
        else
        {
            switch (enemy.Data.bossCategory)
            {
                case EnemyBossCategory.Miniboss:
                    EliteKilled++;
                    if (RunManager.Instance != null) RunManager.Instance.AddKillCount(0, 1, 0, 0);
                    break;

                case EnemyBossCategory.TwilightTerror:
                    BossKilled++;
                    if (RunManager.Instance != null) RunManager.Instance.AddKillCount(0, 0, 1, 0);
                    break;

                case EnemyBossCategory.FinalBoss:
                    FinalBossKilled++;
                    if (RunManager.Instance != null) RunManager.Instance.AddKillCount(0, 0, 0, 1);
                    TriggerEndRun(true);
                    break;
            }
        }

        MonstersKilled = NormalKilled + EliteKilled + BossKilled + FinalBossKilled;
    }

    public void ResetStats()
    {
        NormalKilled = 0;
        EliteKilled = 0;
        BossKilled = 0;
        FinalBossKilled = 0;
        MonstersKilled = 0;
        TotalDamageDealt = 0f;
        TimeElapsed = 0f;
        UnregisterAllTrackedEnemies();
    }

    public void TriggerEndRun(bool isVictory)
    {
        if (this == null) return;
        StopAllCoroutines();

        int normal = NormalKilled;
        int elite = EliteKilled;
        int boss = BossKilled;
        int finalBoss = FinalBossKilled;

        if (RunManager.Instance != null)
        {
            normal = Mathf.Max(normal, RunManager.Instance.TotalNormalKilled);
            elite = Mathf.Max(elite, RunManager.Instance.TotalEliteKilled);
            boss = Mathf.Max(boss, RunManager.Instance.TotalBossKilled);
            finalBoss = Mathf.Max(finalBoss, RunManager.Instance.TotalFinalBossKilled);
        }

        if (RunResultSummary.Instance != null)
        {
            RunResultSummary.Instance.DisplaySummary(normal, elite, boss, finalBoss, isVictory);
        }
    }

    public void OnSkipRunPressed(bool forceVictory = false)
    {
        TriggerEndRun(forceVictory);
    }

#if UNITY_EDITOR
    [Button("⚡ SKIP TO FINAL BOSS MAP", ButtonSizes.Large)]
    [GUIColor(1f, 0.8f, 0.2f)]
#endif
    public void SkipToFinalBoss()
    {
        if (Application.isPlaying && RunManager.Instance != null)
        {
            RunManager.Instance.EnterFinalBoss();
        }
    }
}