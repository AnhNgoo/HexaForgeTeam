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

    private HashSet<EnemyBase> trackedEnemies = new HashSet<EnemyBase>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        ResetStats();
    }

    private void Update()
    {
        TimeElapsed += Time.deltaTime;
        ScanAndRegisterEnemies();

        // Phím = test nhanh Win
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            OnSkipRunPressed(true);
        }
        // Phím - test nhanh Loss
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            OnSkipRunPressed(false);
        }
    }

    public void RegisterPlayerDamage(float damage)
    {
        if (damage <= 0) return;
        TotalDamageDealt += damage;
        if (RunManager.Instance != null)
        {
            RunManager.Instance.RegisterDamage(damage);
        }
    }

    private void ScanAndRegisterEnemies()
    {
        EnemyBase[] activeEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && !trackedEnemies.Contains(enemy) && enemy.EventManager != null)
            {
                trackedEnemies.Add(enemy);

                enemy.EventManager.OnTakeDamage += (damage) => OnEnemyTookDamageRealtime(damage);
                enemy.EventManager.OnDead += () => OnEnemyDiedRealtime(enemy);
            }
        }
    }

    private void OnEnemyTookDamageRealtime(float damage)
    {
        RegisterPlayerDamage(damage);
    }

    private void OnEnemyDiedRealtime(EnemyBase enemy)
    {
        if (enemy == null || enemy.Data == null) return;

        if (!enemy.Data.isBoss)
        {
            NormalKilled++;
        }
        else
        {
            switch (enemy.Data.bossCategory)
            {
                case EnemyBossCategory.Miniboss:
                    EliteKilled++;
                    break;

                case EnemyBossCategory.TwilightTerror:
                    BossKilled++;
                    break;

                case EnemyBossCategory.FinalBoss:
                    FinalBossKilled++;
                    Debug.Log("<color=purple>[RunGameplay] FINAL BOSS DEFEATED! KÍCH HOẠT PANEL WIN!</color>");
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
        trackedEnemies.Clear();
    }

    public void TriggerEndRun(bool isVictory)
    {
        StopAllCoroutines();

        MonstersKilled = NormalKilled + EliteKilled + BossKilled + FinalBossKilled;

        if (RunResultSummary.Instance != null)
        {
            RunResultSummary.Instance.DisplaySummary(NormalKilled, EliteKilled, BossKilled, FinalBossKilled, isVictory);
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
            Debug.Log("<color=yellow>[Cheat/Skip] Chuyển thẳng vào Final Boss Map!</color>");
            RunManager.Instance.EnterFinalBoss();
        }
        else
        {
            Debug.LogWarning("Chỉ có thể bấm Skip khi đang chạy game (Play Mode)!");
        }
    }
}