using System.Collections.Generic;
using UnityEngine;

public class RunGameplayController : MonoBehaviour
{
    public static RunGameplayController Instance;

    public int MonstersKilled { get; private set; }
    public float TimeElapsed { get; private set; }
    public int Score { get; private set; }

    public int NormalKilled { get; private set; }
    public int EliteKilled { get; private set; }
    public int BossKilled { get; private set; }

    // Danh sách lưu vết các Enemy đã được bắt sự kiện OnDead
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
        // 1. Tự động quét và đăng ký sự kiện chết trực tiếp từ EnemyBase mà không cần sửa code Enemy
        ScanAndRegisterEnemies();

        // 2. Nhấn phím = để kích hoạt kết thúc hầm ngục nhanh
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            OnSkipRunPressed();
        }
    }

    /// <summary>
    /// Tự động lắng nghe Enemy trong Run Scene khi chúng được spawn ra từ Pool/Camp
    /// </summary>
    private void ScanAndRegisterEnemies()
    {
        EnemyBase[] activeEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && !trackedEnemies.Contains(enemy) && enemy.EventManager != null)
            {
                trackedEnemies.Add(enemy);

                // Lắng nghe trực tiếp sự kiện OnDead từ EnemyEventManager
                enemy.EventManager.OnDead += () => OnEnemyDiedRealtime(enemy);
            }
        }
    }

    /// <summary>
    /// Xử lý phân loại chính xác khi 1 quái chết
    /// </summary>
    private void OnEnemyDiedRealtime(EnemyBase enemy)
    {
        if (enemy == null || enemy.Data == null) return;

        // Báo loại quái chính xác 100% dựa vào dữ liệu ScriptableObject của Enemy
        if (enemy.Data.isBoss)
        {
            BossKilled++;
        }
        else if (enemy.MinibossBehaviour != null) // Nếu có component Miniboss
        {
            EliteKilled++;
        }
        else
        {
            NormalKilled++;
        }

        MonstersKilled = NormalKilled + EliteKilled + BossKilled;
        Debug.Log($"<color=green>[RunGameplay] Enemy Killed: {enemy.gameObject.name} | Normal: {NormalKilled}, Elite: {EliteKilled}, Boss: {BossKilled}</color>");
    }

    public void ResetStats()
    {
        NormalKilled = 0;
        EliteKilled = 0;
        BossKilled = 0;
        MonstersKilled = 0;
        trackedEnemies.Clear();
    }

    public void OnSkipRunPressed()
    {
        StopAllCoroutines();

        // Tắt toàn bộ các bộ Spawner quái trong hầm ngục để ngừng đẻ quái khi bấm kết thúc
        var spawners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in spawners)
        {
            if (script != null && script.gameObject.scene.name == "Run Scene" && script != this)
            {
                script.StopAllCoroutines();
                script.CancelInvoke();
            }
        }

        // Cập nhật tổng quái thu được từ lượt chơi
        MonstersKilled = NormalKilled + EliteKilled + BossKilled;
        TimeElapsed = Random.Range(80f, 200f);

        // Đổ dữ liệu chuẩn xác sang RunResultSummary
        if (RunResultSummary.Instance != null)
        {
            RunResultSummary.Instance.DisplaySummary(NormalKilled, EliteKilled, BossKilled);
        }
    }
}