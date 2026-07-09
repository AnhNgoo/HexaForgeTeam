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
        // Giả lập số liệu trận đấu
        MonstersKilled = Random.Range(20, 60);
        TimeElapsed = Random.Range(80f, 200f);
        Score = MonstersKilled * 120 + Random.Range(100, 500);

        Debug.Log($"[Gameplay] Run Skipped! Simulating stats -> Kills: {MonstersKilled} | Score: {Score}");

        // Kích hoạt bảng hiển thị kết quả
        if (RunResultSummary.Instance != null)
        {
            RunResultSummary.Instance.DisplaySummary(MonstersKilled, Score);
        }
    }
}