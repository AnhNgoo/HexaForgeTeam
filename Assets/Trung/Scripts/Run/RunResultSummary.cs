using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class RunResultSummary : MonoBehaviour
{
    public static RunResultSummary Instance;

    [Header("UI Canvas Group & Background")]
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private Image bgOverlay;

    [Header("Title Banner (Elden Style)")]
    [SerializeField] private TMP_Text txtTitleBanner;

    [Header("Stats Texts")]
    [SerializeField] private TMP_Text txtTotalDamage;
    [SerializeField] private TMP_Text txtNormalCount;
    [SerializeField] private TMP_Text txtEliteCount;
    [SerializeField] private TMP_Text txtBossCount;
    [SerializeField] private TMP_Text txtFinalBossCount;
    [SerializeField] private TMP_Text txtTotalScore;

    [Header("Rewards Texts")]
    [SerializeField] private TMP_Text txtGemReward;
    [SerializeField] private TMP_Text txtShardReward;
    [SerializeField] private TMP_Text txtExpReward;

    [Header("Action Buttons")]
    [SerializeField] private Button btnReturnLobby;
    [SerializeField] private CanvasGroup buttonCanvasGroup;

    private int calculatedGem;
    private int calculatedExp;
    private int calculatedShards;

    private void Awake() => Instance = this;

    public void DisplaySummary(int normal, int elite, int boss, int finalBoss, bool isVictory)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.LobbyRunResultSummaryMenu);
        }

        if (mainCanvasGroup != null) mainCanvasGroup.alpha = 0f;
        if (buttonCanvasGroup != null) buttonCanvasGroup.alpha = 0f;

        // Lấy tổng damage từ RunGameplayController hoặc RunManager
        float totalDamage = RunGameplayController.Instance != null 
            ? RunGameplayController.Instance.TotalDamageDealt 
            : (RunManager.Instance != null ? RunManager.Instance.GetTotalDamage() : 0);

        int totalKills = normal + elite + boss + finalBoss;

        // CÔNG THỨC TÍNH TỔNG ĐIỂM
        int damageScore = Mathf.RoundToInt(totalDamage);
        int killScore = (normal * 100) + (elite * 300) + (boss * 1000) + (finalBoss * 5000);
        int totalScore = damageScore + killScore;

        // TÍNH PHẦN THƯỞNG
        calculatedGem = (normal * 2) + (elite * 10) + (boss * 50) + (finalBoss * 300);
        calculatedExp = (normal * 10) + (elite * 30) + (boss * 100) + (finalBoss * 500);
        calculatedShards = (normal * 5) + (elite * 20) + (boss * 150) + (finalBoss * 1000);

        // Reset hiển thị ban đầu
        SetupTextInitial(txtTotalDamage, "TOTAL DAMAGE", 0);
        SetupTextInitial(txtNormalCount, "NORMAL ENEMIES", 0);
        SetupTextInitial(txtEliteCount, "ELITE FOES", 0);
        SetupTextInitial(txtBossCount, "BOSS TARGETS", 0);
        SetupTextInitial(txtFinalBossCount, "NIGHTMARE LORD", 0);
        SetupTextInitial(txtTotalScore, "TOTAL SCORE", 0);

        SetupTextInitial(txtGemReward, "CRYSTALS", 0, "+");
        SetupTextInitial(txtShardReward, "RUNE SHARDS", 0, "+");
        SetupTextInitial(txtExpReward, "ACCOUNT EXP", 0, "+");

        if (txtTitleBanner != null)
        {
            if (isVictory)
            {
                txtTitleBanner.text = "NIGHT FELL\n<size=45%><color=#FFD700>VICTORY ACHIEVED</color></size>";
                txtTitleBanner.color = new Color(1f, 0.85f, 0.3f, 1f);
            }
            else
            {
                txtTitleBanner.text = "YOU DIED\n<size=45%><color=#FF3333>NIGHTMARE PREVAILS</color></size>";
                txtTitleBanner.color = new Color(0.9f, 0.2f, 0.2f, 1f);
            }
            txtTitleBanner.transform.localScale = Vector3.one * 2.2f;
        }

        Sequence seq = DOTween.Sequence();

        if (bgOverlay != null)
        {
            bgOverlay.color = new Color(0, 0, 0, 0);
            seq.Append(bgOverlay.DOFade(0.85f, 0.6f));
        }

        if (mainCanvasGroup != null) seq.Join(mainCanvasGroup.DOFade(1f, 0.5f));

        if (txtTitleBanner != null)
        {
            seq.Append(txtTitleBanner.transform.DOScale(1f, 0.45f).SetEase(Ease.OutBack));
            seq.Append(txtTitleBanner.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 2, 0.5f));
        }

        seq.AppendInterval(0.15f);

        // Dập hiển thị các trường chỉ số
        AppendSlamText(seq, txtTotalDamage, "TOTAL DAMAGE", Mathf.RoundToInt(totalDamage), 0.2f);
        AppendSlamText(seq, txtNormalCount, "NORMAL ENEMIES", normal, 0.18f);
        AppendSlamText(seq, txtEliteCount, "ELITE FOES", elite, 0.18f);
        AppendSlamText(seq, txtBossCount, "BOSS TARGETS", boss, 0.18f);
        AppendSlamText(seq, txtFinalBossCount, "NIGHTMARE LORD", finalBoss, 0.18f);
        AppendSlamText(seq, txtTotalScore, "<color=#FFD700>TOTAL SCORE</color>", totalScore, 0.3f);

        seq.AppendInterval(0.15f);

        // Dập phần thưởng
        AppendSlamText(seq, txtGemReward, "CRYSTALS", calculatedGem, 0.18f, "+");
        AppendSlamText(seq, txtShardReward, "RUNE SHARDS", calculatedShards, 0.18f, "+");
        AppendSlamText(seq, txtExpReward, "ACCOUNT EXP", calculatedExp, 0.18f, "+");

        seq.AppendInterval(0.2f);

        if (buttonCanvasGroup != null)
        {
            seq.Append(buttonCanvasGroup.DOFade(1f, 0.4f));
            if (btnReturnLobby != null)
            {
                seq.Append(btnReturnLobby.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 1, 0.5f));
            }
        }

        OnRunEnded(totalKills);

        if (RunManager.Instance != null)
        {
            RunManager.Instance.SetPendingRewards(calculatedGem, calculatedExp, calculatedShards);
        }

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.AddKillProgress(totalKills, boss + finalBoss);
        }
    }

    private void SetupTextInitial(TMP_Text textTarget, string label, int val, string prefix = "")
    {
        if (textTarget == null) return;
        textTarget.text = $"{label}   {prefix}{val}";
        textTarget.transform.localScale = Vector3.zero;
    }

    private void AppendSlamText(Sequence seq, TMP_Text textTarget, string label, int finalVal, float duration, string prefix = "")
    {
        if (textTarget == null) return;

        seq.AppendCallback(() =>
        {
            textTarget.transform.localScale = Vector3.one * 1.4f;
            int currentVal = 0;

            DOTween.To(() => currentVal, x => currentVal = x, finalVal, duration)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() =>
                {
                    textTarget.text = $"{label}   <color=#FFD700>{prefix}{currentVal:N0}</color>";
                });

            textTarget.transform.DOScale(1f, duration).SetEase(Ease.OutBack);
        });

        seq.AppendInterval(duration + 0.04f);
    }

    public void OnConfirmAndReturn()
    {
        if (RunManager.Instance != null)
        {
            RunManager.Instance.ReturnToLobby();
        }
    }

    public void OnRunEnded(int killsInThisRun)
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            var data = SaveLoadManager.Instance.SaveData;
            data.totalRuns += 1;
            data.totalKills += killsInThisRun;

            SaveLoadManager.Instance.SaveGame();

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.UpdateAllStatistics();
            }
        }
    }
}