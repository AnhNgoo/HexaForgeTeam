using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// Cộng dồn tổng sát thương thực tế Player đã gây ra
    /// </summary>
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

                // 1. ĐĂNG KÝ THEO DÕI SÁT THƯƠNG REAL-TIME
                enemy.EventManager.OnTakeDamage += (damage) => OnEnemyTookDamageRealtime(damage);

                // 2. ĐĂNG KÝ THEO DÕI QUÁI CHẾT
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

        // PHÂN LOẠI QUÁI CHUẨN XÁC DỰA TRÊN ENEMYDATA
        if (!enemy.Data.isBoss)
        {
            // Quái Thường (Is Boss KHÔNG ĐƯỢC TÍCH)
            NormalKilled++;
        }
        else
        {
            // Quái Boss (Is Boss ĐƯỢC TÍCH) -> Phân loại theo Enum bossCategory
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

                    // KÍCH HOẠT PANEL WIN KHI TIÊU DIỆT FINAL BOSS
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

        // Bật màn hình tổng kết Summary
        if (RunResultSummary.Instance != null)
        {
            RunResultSummary.Instance.DisplaySummary(NormalKilled, EliteKilled, BossKilled, FinalBossKilled, isVictory);
        }
    }

    public void OnSkipRunPressed(bool forceVictory = false)
    {
        TriggerEndRun(forceVictory);
    }
}