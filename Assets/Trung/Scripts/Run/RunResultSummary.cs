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

    [Header("Title Summary Slide Effect")]
    [SerializeField] private RectTransform titleSummaryRect;
    [SerializeField] private float titleStartX = 95f;
    [SerializeField] private float titleEndX = 507f;
    [SerializeField] private float titleSlideDuration = 0.45f;

    [Header("Divider Line Effect (Gach ngang)")]
    [SerializeField] private RectTransform dividerLineRect;

    [Header("Rank Rating UI")]
    [SerializeField] private RectTransform rankContainerRect;
    [SerializeField] private TMP_Text txtRankGrade;

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

    [Header("Audio SFX Clips")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxWin;
    [SerializeField] private AudioClip sfxLose;
    [SerializeField] private AudioClip sfxTextCount;
    [SerializeField] private AudioClip sfxDividerSlash;
    [SerializeField] private AudioClip sfxRankStamp;

    private int calculatedGem;
    private int calculatedExp;
    private int calculatedShards;
    private Sequence runningSequence;
    private Tween heroIdleTween;

    private void Awake()
    {
        Instance = this;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
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

        int wager = RunManager.Instance != null ? RunManager.Instance.CurrentWagerAmount : 0;
        float multiplier = RunManager.Instance != null ? RunManager.Instance.CurrentRewardMultiplier : 1.0f;

        // BẢNG THƯỞNG CÂN BẰNG: Normal = 1, Elite = 4, Boss = 25, FinalBoss = 120
        int baseGemReward = (normal * 1) + (elite * 4) + (boss * 25) + (finalBoss * 120);

        if (isVictory)
        {
            // Thắng: Hoàn tiền cược gốc + (Thưởng x Hệ số Wager)
            int multipliedReward = Mathf.RoundToInt(baseGemReward * multiplier);
            calculatedGem = wager + multipliedReward;
            if (RunManager.Instance != null) RunManager.Instance.MarkRunVictory(true);
        }
        else
        {
            // Thua: Mất toàn bộ Wager đã đặt, chỉ vớt vát được 25% số Gem nhặt từ quái
            calculatedGem = Mathf.RoundToInt(baseGemReward * 0.25f);
            if (RunManager.Instance != null) RunManager.Instance.MarkRunVictory(false);
        }

        // EXP CÂN BẰNG THEO LỘ TRÌNH 30 LEVEL
        calculatedExp = Mathf.RoundToInt(((normal * 8) + (elite * 25) + (boss * 80) + (finalBoss * 300)) * (isVictory ? multiplier : 0.4f));

        // SHARDS: Chỉ rớt khi chiến thắng, số lượng chuẩn hóa: Normal = 2, Elite = 8, Boss = 40, FinalBoss = 150
        calculatedShards = isVictory ? Mathf.RoundToInt(((normal * 2) + (elite * 8) + (boss * 40) + (finalBoss * 150)) * multiplier) : 0;

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
            txtTitleBanner.text = isVictory
                ? $"<size=130%><color=#FFE066> NIGHT FELL </color></size>\n<size=60%><color=#FFD700>— VICTORY ACHIEVED (x{multiplier:F1}) —</color></size>"
                : "<size=130%><color=#FF3333> YOU DIED </color></size>\n<size=60%><color=#FF6B6B>— WAGER GEMS LOST —</color></size>";
            txtTitleBanner.transform.localScale = Vector3.one * 1.8f;
            txtTitleBanner.alpha = 0f;
        }

        if (titleSummaryRect != null)
        {
            Vector2 pos = titleSummaryRect.anchoredPosition;
            pos.x = titleStartX;
            titleSummaryRect.anchoredPosition = pos;
            titleSummaryRect.localScale = Vector3.one;
        }

        if (dividerLineRect != null)
        {
            dividerLineRect.localScale = new Vector3(0f, 1f, 1f);
            dividerLineRect.gameObject.SetActive(false);
        }

        if (rankContainerRect != null)
        {
            rankContainerRect.localScale = Vector3.zero;
        }

        string rankGrade = EvaluateRank(totalScore, isVictory);
        string rankColorHex = GetRankColorHex(rankGrade);

        if (txtRankGrade != null)
        {
            txtRankGrade.text = $"<color={rankColorHex}>{rankGrade}</color>";
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

        runningSequence.AppendCallback(() =>
        {
            PlaySFX(isVictory ? sfxWin : sfxLose);
        });

        if (bgOverlay != null)
        {
            bgOverlay.color = new Color(0, 0, 0, 0);
            runningSequence.Append(bgOverlay.DOFade(0.9f, 0.4f));
        }

        if (mainCanvasGroup != null)
        {
            runningSequence.Join(mainCanvasGroup.DOFade(1f, 0.35f));
        }

        if (txtTitleBanner != null)
        {
            runningSequence.Append(txtTitleBanner.DOFade(1f, 0.2f));
            runningSequence.Join(txtTitleBanner.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
            runningSequence.Append(txtTitleBanner.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 5, 0.6f));
        }

        if (heroCanvasGroup != null && imgHeroArtwork != null)
        {
            imgHeroArtwork.transform.localScale = Vector3.one;
            runningSequence.Append(heroCanvasGroup.DOFade(1f, 0.3f));

            if (txtHeroName != null)
            {
                txtHeroName.transform.localScale = Vector3.zero;
                runningSequence.Append(txtHeroName.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
            }

            runningSequence.AppendCallback(StartHeroFloatingAnimation);
        }

        if (statsBoardRect != null)
        {
            runningSequence.Append(statsBoardRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        }

        runningSequence.AppendInterval(0.05f);

        AppendJuicyText(runningSequence, txtTotalDamage, "<color=#B0C4DE>TOTAL DAMAGE</color>", Mathf.RoundToInt(totalDamage), 0.4f, "#FFA07A");
        AppendJuicyText(runningSequence, txtNormalCount, "<color=#DCDCDC>NORMAL FOES</color>", normal, 0.3f, "#FFFFFF");
        AppendJuicyText(runningSequence, txtEliteCount, "<color=#87CEFA>ELITE FOES</color>", elite, 0.3f, "#00FFFF");
        AppendJuicyText(runningSequence, txtBossCount, "<color=#FFA500>BOSS TARGETS</color>", boss, 0.3f, "#FF8C00");
        AppendJuicyText(runningSequence, txtFinalBossCount, "<color=#FF4500>NIGHTMARE LORD</color>", finalBoss, 0.35f, "#FF1493");
        AppendJuicyText(runningSequence, txtTotalScore, "<size=110%><color=#FFD700>TOTAL SCORE</color></size>", totalScore, 0.5f, "#FFD700", "", true);

        if (dividerLineRect != null)
        {
            runningSequence.AppendInterval(0.1f);
            runningSequence.AppendCallback(() =>
            {
                dividerLineRect.gameObject.SetActive(true);
                dividerLineRect.localScale = new Vector3(0f, 1f, 1f);
                PlaySFX(sfxDividerSlash);
            });
            runningSequence.Append(dividerLineRect.DOScaleX(1f, 0.35f).SetEase(Ease.OutCubic));
            runningSequence.Append(dividerLineRect.DOPunchScale(new Vector3(0f, 0.3f, 0f), 0.2f, 4, 0.5f));
        }

        if (rewardsBoardRect != null)
        {
            runningSequence.Append(rewardsBoardRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        }

        runningSequence.AppendInterval(0.05f);

        AppendJuicyText(runningSequence, txtGemReward, isVictory ? "<color=#00FFFF>CRYSTALS</color>" : "<color=#FF5555>CRYSTALS</color>", calculatedGem, 0.35f, isVictory ? "#00FFFF" : "#FF5555", "+");
        AppendJuicyText(runningSequence, txtShardReward, "<color=#BA55D3>RUNE SHARDS</color>", calculatedShards, 0.35f, "#EE82EE", "+");
        AppendJuicyText(runningSequence, txtExpReward, "<color=#32CD32>ACCOUNT EXP</color>", calculatedExp, 0.35f, "#7FFF00", "+");

        runningSequence.AppendInterval(0.15f);

        if (titleSummaryRect != null)
        {
            runningSequence.Append(titleSummaryRect.DOAnchorPosX(titleEndX, titleSlideDuration).SetEase(Ease.OutBack));
            runningSequence.Join(titleSummaryRect.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 4, 0.5f));
        }

        if (rankContainerRect != null)
        {
            runningSequence.AppendCallback(() =>
            {
                rankContainerRect.localScale = Vector3.one * 3.5f;
                rankContainerRect.DOScale(1f, 0.22f).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    PlaySFX(sfxRankStamp);
                    rankContainerRect.DOPunchScale(Vector3.one * 0.5f, 0.35f, 8, 0.8f);
                    if (Camera.main != null)
                    {
                        Camera.main.transform.DOShakePosition(0.25f, 15f, 30);
                    }
                });
            });
            runningSequence.AppendInterval(0.5f);
        }

        if (buttonCanvasGroup != null)
        {
            runningSequence.Append(buttonCanvasGroup.DOFade(1f, 0.3f));
            if (btnReturnLobby != null)
            {
                btnReturnLobby.transform.localScale = Vector3.one * 0.75f;
                runningSequence.Join(btnReturnLobby.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
                runningSequence.Append(btnReturnLobby.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 3, 0.5f));
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

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private string EvaluateRank(int score, bool isVictory)
    {
        if (!isVictory)
        {
            if (score >= 25000) return "C";
            if (score >= 12000) return "D";
            return "F";
        }

        if (score >= 45000) return "S";
        if (score >= 30000) return "A";
        if (score >= 18000) return "B";
        if (score >= 10000) return "C";
        return "D";
    }

    private string GetRankColorHex(string rank)
    {
        switch (rank)
        {
            case "S": return "#FFD700";
            case "A": return "#FF3366";
            case "B": return "#B366FF";
            case "C": return "#00E5FF";
            case "D": return "#55FF55";
            case "F": return "#A0A0A0";
            default: return "#FFFFFF";
        }
    }

    private void StartHeroFloatingAnimation()
    {
        if (imgHeroArtwork == null) return;

        heroIdleTween = imgHeroArtwork.transform.DOLocalMoveY(imgHeroArtwork.transform.localPosition.y + 8f, 1.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void SetupHeroDisplay()
    {
        CharacterType selectedType = CharacterType.Kael;

        if (CharacterManager.Instance != null)
        {
            selectedType = CharacterManager.Instance.GetSelectedCharacter();
        }
        else
        {
            string savedChar = PlayerPrefs.GetString("SelectedCharacter", "Kael");
            if (!System.Enum.TryParse(savedChar, out selectedType))
            {
                selectedType = CharacterType.Kael;
            }
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
            textTarget.transform.localScale = Vector3.one * (isGrandTotal ? 1.35f : 1.2f);
            textTarget.transform.DOScale(1f, duration).SetEase(Ease.OutBack);
            PlaySFX(sfxTextCount);

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
                    textTarget.transform.DOPunchScale(Vector3.one * (isGrandTotal ? 0.2f : 0.1f), 0.18f, 3, 0.5f);
                });
        });

        seq.AppendInterval(duration * 0.4f);
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