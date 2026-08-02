using System.Collections.Generic;
using UnityEngine;

public class RunGameplayController : MonoBehaviour
{
    public static RunGameplayController Instance;

    public int MonstersKilled { get; private set; }
    public float TimeElapsed { get; private set; }

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
        ScanAndRegisterEnemies();

        // Phím = test nhanh Win (Thắt cờ thắng)
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            OnSkipRunPressed(true);
        }
        // Phím - test nhanh Loss (Thua)
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            OnSkipRunPressed(false);
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
                enemy.EventManager.OnDead += () => OnEnemyDiedRealtime(enemy);
            }
        }
    }

    private void OnEnemyDiedRealtime(EnemyBase enemy)
    {
        if (enemy == null || enemy.Data == null) return;

        string enemyName = enemy.gameObject.name.ToLower();

        // 1. Phân loại Final Boss
        if (enemy.Data.isBoss && (enemyName.Contains("final") || enemyName.Contains("nightmare") || enemyName.Contains("lord")))
        {
            FinalBossKilled++;
            Debug.Log("<color=purple>[RunGameplay] FINAL BOSS DEFEATED!</color>");
            TriggerEndRun(true); // Hạ xong Final Boss -> Tự động kích hoạt Bảng Thắng Elden Style!
        }
        // 2. Phân loại Boss Thường
        else if (enemy.Data.isBoss)
        {
            BossKilled++;
        }
        // 3. Phân loại Elite (Miniboss)
        else if (enemy.MinibossBehaviour != null || enemyName.Contains("elite"))
        {
            EliteKilled++;
        }
        // 4. Quái Thường
        else
        {
            NormalKilled++;
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
        trackedEnemies.Clear();
    }

    public void TriggerEndRun(bool isVictory)
    {
        StopAllCoroutines();

        var spawners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in spawners)
        {
            if (script != null && script.gameObject.scene.name == "Run Scene" && script != this)
            {
                script.StopAllCoroutines();
                script.CancelInvoke();
            }
        }

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
}