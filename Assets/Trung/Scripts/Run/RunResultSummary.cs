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
    [SerializeField] private RectTransform statsBoardRect;
    [SerializeField] private RectTransform rewardsBoardRect;

    [Header("Title Banner")]
    [SerializeField] private TMP_Text txtTitleBanner;

    [Header("Hero Display")]
    [SerializeField] private Image imgHeroArtwork;
    [SerializeField] private TMP_Text txtHeroName;
    [SerializeField] private CanvasGroup heroCanvasGroup;

    [Header("Hero Visual Assets")]
    [SerializeField] private Sprite kaelArtwork;
    [SerializeField] private Sprite lyraArtwork;
    [SerializeField] private Sprite aresArtwork;
    [SerializeField] private Sprite elaraArtwork;

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
    private Sequence runningSequence;
    private Tween heroIdleTween;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        KillAllAnimations();
    }

    private void OnDestroy()
    {
        KillAllAnimations();
    }

    private void KillAllAnimations()
    {
        if (runningSequence != null && runningSequence.IsActive()) runningSequence.Kill();
        if (heroIdleTween != null && heroIdleTween.IsActive()) heroIdleTween.Kill();
    }

    public void DisplaySummary(int normal, int elite, int boss, int finalBoss, bool isVictory)
    {
        KillAllAnimations();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.LobbyRunResultSummaryMenu);
        }

        if (mainCanvasGroup != null) mainCanvasGroup.alpha = 0f;
        if (buttonCanvasGroup != null) buttonCanvasGroup.alpha = 0f;
        if (heroCanvasGroup != null) heroCanvasGroup.alpha = 0f;

        float totalDamage = RunGameplayController.Instance != null 
            ? RunGameplayController.Instance.TotalDamageDealt 
            : (RunManager.Instance != null ? RunManager.Instance.GetTotalDamage() : 0);

        int totalKills = normal + elite + boss + finalBoss;

        int damageScore = Mathf.RoundToInt(totalDamage);
        int killScore = (normal * 100) + (elite * 300) + (boss * 1000) + (finalBoss * 5000);
        int totalScore = damageScore + killScore;

        // --- TÍNH TOÁN THƯỞNG / PHẠT THEO MỐC CƯỢC ---
        int wager = RunManager.Instance != null ? RunManager.Instance.CurrentWagerAmount : 0;
        float multiplier = RunManager.Instance != null ? RunManager.Instance.CurrentRewardMultiplier : 1.0f;

        int baseGemReward = (normal * 2) + (elite * 10) + (boss * 50) + (finalBoss * 300);

        if (isVictory)
        {
            int multipliedReward = Mathf.RoundToInt(baseGemReward * multiplier);
            calculatedGem = wager + multipliedReward; // Hoàn tiền cược gốc + thưởng thắng trận theo mốc
            if (RunManager.Instance != null) RunManager.Instance.MarkRunVictory(true);
        }
        else
        {
            // Thua / Chết trận: Mất trắng 100% tiền cược gốc
            calculatedGem = Mathf.Max(0, baseGemReward - wager);
            if (RunManager.Instance != null) RunManager.Instance.MarkRunVictory(false);
        }

        calculatedExp = Mathf.RoundToInt(((normal * 10) + (elite * 30) + (boss * 100) + (finalBoss * 500)) * (isVictory ? multiplier : 0.5f));
        calculatedShards = isVictory ? Mathf.RoundToInt(((normal * 5) + (elite * 20) + (boss * 150) + (finalBoss * 1000)) * multiplier) : 0;
        // ----------------------------------------------

        SetupHeroDisplay();

        SetupTextInitial(txtTotalDamage, "<color=#B0C4DE>TOTAL DAMAGE</color>", "#FFA07A");
        SetupTextInitial(txtNormalCount, "<color=#DCDCDC>NORMAL FOES</color>", "#FFFFFF");
        SetupTextInitial(txtEliteCount, "<color=#87CEFA>ELITE FOES</color>", "#00FFFF");
        SetupTextInitial(txtBossCount, "<color=#FFA500>BOSS TARGETS</color>", "#FF8C00");
        SetupTextInitial(txtFinalBossCount, "<color=#FF4500>NIGHTMARE LORD</color>", "#FF1493");
        SetupTextInitial(txtTotalScore, "<color=#FFD700>TOTAL SCORE</color>", "#FFD700");

        SetupTextInitial(txtGemReward, isVictory ? "<color=#00FFFF>CRYSTALS (WON)</color>" : "<color=#FF5555>CRYSTALS (LOST WAGER)</color>", isVictory ? "#00FFFF" : "#FF5555", "+");
        SetupTextInitial(txtShardReward, "<color=#BA55D3>RUNE SHARDS</color>", "#EE82EE", "+");
        SetupTextInitial(txtExpReward, "<color=#32CD32>ACCOUNT EXP</color>", "#7FFF00", "+");

        if (txtTitleBanner != null)
        {
            if (isVictory)
            {
                txtTitleBanner.text = "<size=130%><color=#FFE066> NIGHT FELL </color></size>\n<size=60%><color=#FFD700>— VICTORY ACHIEVED (x" + multiplier + ") —</color></size>";
            }
            else
            {
                txtTitleBanner.text = "<size=130%><color=#FF3333> YOU DIED </color></size>\n<size=60%><color=#FF6B6B>— WAGER GEMS LOST —</color></size>";
            }
            txtTitleBanner.transform.localScale = Vector3.one * 2.2f;
            txtTitleBanner.alpha = 0f;
        }

        if (statsBoardRect != null)
        {
            statsBoardRect.localScale = Vector3.one * 0.9f;
            statsBoardRect.DOKill();
        }
        if (rewardsBoardRect != null)
        {
            rewardsBoardRect.localScale = Vector3.one * 0.9f;
            rewardsBoardRect.DOKill();
        }

        runningSequence = DOTween.Sequence();

        if (bgOverlay != null)
        {
            bgOverlay.color = new Color(0, 0, 0, 0);
            runningSequence.Append(bgOverlay.DOFade(0.88f, 0.45f));
        }

        if (mainCanvasGroup != null)
        {
            runningSequence.Join(mainCanvasGroup.DOFade(1f, 0.4f));
        }

        if (txtTitleBanner != null)
        {
            runningSequence.Append(txtTitleBanner.DOFade(1f, 0.25f));
            runningSequence.Join(txtTitleBanner.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            runningSequence.Append(txtTitleBanner.transform.DOPunchScale(Vector3.one * 0.15f, 0.35f, 3, 0.6f));
        }

        if (heroCanvasGroup != null && imgHeroArtwork != null)
        {
            imgHeroArtwork.transform.localScale = Vector3.one * 0.7f;
            runningSequence.Append(heroCanvasGroup.DOFade(1f, 0.35f));
            runningSequence.Join(imgHeroArtwork.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

            if (txtHeroName != null)
            {
                txtHeroName.transform.localScale = Vector3.zero;
                runningSequence.Append(txtHeroName.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            }

            runningSequence.AppendCallback(StartHeroFloatingAnimation);
        }

        if (statsBoardRect != null)
        {
            runningSequence.Append(statsBoardRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        }

        runningSequence.AppendInterval(0.08f);

        AppendJuicyText(runningSequence, txtTotalDamage, "<color=#B0C4DE>TOTAL DAMAGE</color>", Mathf.RoundToInt(totalDamage), 0.5f, "#FFA07A");
        AppendJuicyText(runningSequence, txtNormalCount, "<color=#DCDCDC>NORMAL FOES</color>", normal, 0.35f, "#FFFFFF");
        AppendJuicyText(runningSequence, txtEliteCount, "<color=#87CEFA>ELITE FOES</color>", elite, 0.35f, "#00FFFF");
        AppendJuicyText(runningSequence, txtBossCount, "<color=#FFA500>BOSS TARGETS</color>", boss, 0.35f, "#FF8C00");
        AppendJuicyText(runningSequence, txtFinalBossCount, "<color=#FF4500>NIGHTMARE LORD</color>", finalBoss, 0.4f, "#FF1493");
        AppendJuicyText(runningSequence, txtTotalScore, "<size=110%><color=#FFD700>TOTAL SCORE</color></size>", totalScore, 0.65f, "#FFD700", "", true);

        if (rewardsBoardRect != null)
        {
            runningSequence.Append(rewardsBoardRect.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        }

        runningSequence.AppendInterval(0.08f);

        AppendJuicyText(runningSequence, txtGemReward, isVictory ? "<color=#00FFFF>CRYSTALS</color>" : "<color=#FF5555>CRYSTALS</color>", calculatedGem, 0.38f, isVictory ? "#00FFFF" : "#FF5555", "+");
        AppendJuicyText(runningSequence, txtShardReward, "<color=#BA55D3>RUNE SHARDS</color>", calculatedShards, 0.38f, "#EE82EE", "+");
        AppendJuicyText(runningSequence, txtExpReward, "<color=#32CD32>ACCOUNT EXP</color>", calculatedExp, 0.38f, "#7FFF00", "+");

        runningSequence.AppendInterval(0.12f);

        if (buttonCanvasGroup != null)
        {
            runningSequence.Append(buttonCanvasGroup.DOFade(1f, 0.35f));
            if (btnReturnLobby != null)
            {
                btnReturnLobby.transform.localScale = Vector3.one * 0.8f;
                runningSequence.Join(btnReturnLobby.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
                runningSequence.Append(btnReturnLobby.transform.DOPunchScale(Vector3.one * 0.12f, 0.4f, 2, 0.5f));
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

    private void StartHeroFloatingAnimation()
    {
        if (imgHeroArtwork == null) return;

        heroIdleTween = imgHeroArtwork.transform.DOLocalMoveY(imgHeroArtwork.transform.localPosition.y + 10f, 1.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void SetupHeroDisplay()
    {
        CharacterType selectedType = CharacterType.Kael;

        // 1. Lấy từ CharacterManager nếu có
        if (CharacterManager.Instance != null)
        {
            selectedType = CharacterManager.Instance.GetSelectedCharacter();
        }
        // 2. Fallback lấy từ PlayerPrefs
        else
        {
            selectedType = (CharacterType)PlayerPrefs.GetInt("SELECTED_CHARACTER", 0);
        }

        Sprite targetSprite = kaelArtwork;
        string heroTitle = "<color=#FFA500><b>KAEL</b></color>";

        switch (selectedType)
        {
            case CharacterType.Kael:
                targetSprite = kaelArtwork;
                heroTitle = "<color=#FFA500><b>KAEL</b></color>";
                break;
            case CharacterType.Lyra:
                targetSprite = lyraArtwork;
                heroTitle = "<color=#9966FF><b>LYRA</b></color>";
                break;
            case CharacterType.Ares:
                targetSprite = aresArtwork;
                heroTitle = "<color=#FF4444><b>ARES</b></color>";
                break;
            case CharacterType.Elara:
                targetSprite = elaraArtwork;
                heroTitle = "<color=#33FF88><b>ELARA</b></color>";
                break;
        }

        if (imgHeroArtwork != null)
        {
            imgHeroArtwork.sprite = targetSprite;
            imgHeroArtwork.preserveAspect = true;
            imgHeroArtwork.gameObject.SetActive(targetSprite != null);
        }

        if (txtHeroName != null)
        {
            txtHeroName.text = heroTitle;
        }
    }

    private void SetupTextInitial(TMP_Text textTarget, string label, string hexColor, string prefix = "")
    {
        if (textTarget == null) return;
        textTarget.text = $"{label}   <color={hexColor}>{prefix}0</color>";
        textTarget.transform.localScale = Vector3.zero;
    }

    private void AppendJuicyText(Sequence seq, TMP_Text textTarget, string label, int finalVal, float duration, string hexColor, string prefix = "", bool isGrandTotal = false)
    {
        if (textTarget == null) return;

        seq.AppendCallback(() =>
        {
            textTarget.transform.localScale = Vector3.one * (isGrandTotal ? 1.4f : 1.25f);
            textTarget.transform.DOScale(1f, duration).SetEase(Ease.OutBack);

            int currentVal = 0;
            DOTween.To(() => currentVal, x => currentVal = x, finalVal, duration)
                .SetEase(isGrandTotal ? Ease.OutExpo : Ease.OutQuad)
                .OnUpdate(() =>
                {
                    textTarget.text = $"{label}   <color={hexColor}><b>{prefix}{currentVal:N0}</b></color>";
                })
                .OnComplete(() =>
                {
                    textTarget.text = $"{label}   <color={hexColor}><b>{prefix}{finalVal:N0}</b></color>";
                    textTarget.transform.DOPunchScale(Vector3.one * (isGrandTotal ? 0.2f : 0.1f), 0.2f, 2, 0.5f);
                });
        });

        seq.AppendInterval(duration * 0.45f);
    }

    public void OnConfirmAndReturn()
    {
        KillAllAnimations();
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

            if (RunGameplayController.Instance != null && RunGameplayController.Instance.IsFinalBossDefeated)
            {
                if (RunManager.Instance != null && RunManager.Instance.SelectedFinalBossPool == PoolType.EnemyEarthshakerBoss)
                {
                    bool isUnlocked = PlayerPrefs.GetInt("UNLOCKED_BOSS_DARKMAGE", 0) == 1;
                    if (!isUnlocked)
                    {
                        PlayerPrefs.SetInt("UNLOCKED_BOSS_DARKMAGE", 1);
                        PlayerPrefs.Save();

                        if (LobbyNotifyManager.Instance != null)
                        {
                            LobbyNotifyManager.Instance.ShowNotify("Unlocked New Boss: The DarkMage!", Color.yellow);
                        }
                    }
                }
            }

            SaveLoadManager.Instance.SaveGame();

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.UpdateAllStatistics();
            }
        }
    }
}