using UnityEngine;

public class RunGameplayController : MonoBehaviour
{
    public static RunGameplayController Instance;

    public int MonstersKilled { get; private set; }
    public float TimeElapsed { get; private set; }
    public int Score { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Hàm gắn vào nút SKIP RUN
    public void OnSkipRunPressed()
    {
        // 1. NGĂN CHẶN CÔNG TRÌNH TIẾP TỤC SPAWN: Tắt toàn bộ Coroutine đang chạy ngầm trong Controller này
        StopAllCoroutines();

        // 2. Tìm và tắt các bộ sinh bản đồ/quái vật tự động (Spawners/Generators) trong Run Scene
        // Điều này ngăn chặn việc chúng tiếp tục gọi Instantiate() sinh thêm rác sau khi đã bấm Skip
        var spawners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in spawners)
        {
            if (script != null && script.gameObject.scene.name == "Run Scene")
            {
                // Tắt các hàm tự động gọi ngầm (Invoke) hoặc Coroutine trên toàn bộ script thuộc Run Scene
                script.StopAllCoroutines();
                script.CancelInvoke();
            }
        }

        // 3. Giả lập số liệu trận đấu
        MonstersKilled = Random.Range(20, 60);
        TimeElapsed = Random.Range(80f, 200f);
        Score = MonstersKilled * 120 + Random.Range(100, 500);

        Debug.Log($"[Gameplay] Run Skipped! Simulating stats -> Kills: {MonstersKilled} | Score: {Score}");

        // 4. Kích hoạt bảng hiển thị kết quả
        if (RunResultSummary.Instance != null)
        {
            RunResultSummary.Instance.DisplaySummary(MonstersKilled, Score);
        }
    }
}