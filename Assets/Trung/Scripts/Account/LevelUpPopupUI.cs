using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LevelUpPopupUI : LoadComponents
{
    public static LevelUpPopupUI Instance;

    [Header("Panel Root & Animation Containers")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private RectTransform popupContainer;
    [SerializeField] private CanvasGroup bgOverlayCanvasGroup;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelNoticeText;
    [SerializeField] private TMP_Text bonusStatsText;

    [Header("Reward Display")]
    [SerializeField] private CostDisplayUI rewardDisplayUI;

    [Header("Audio SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelUpSFX;

    [Header("Auto Hide Settings")]
    [SerializeField] private float autoHideDelay = 2.5f;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

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

    public void Show(string title, int oldLevel, int newLevel, List<CostData> rewards, string bonusText)
    {
        if (levelUpPanel == null) return;

        CancelInvoke(nameof(Hide));

        if (titleText != null) titleText.SetTextSafe(title);
        if (levelNoticeText != null) levelNoticeText.SetTextSafe($"LEVEL {oldLevel} -> <color=#00FFCC>LEVEL {newLevel}</color>");
        if (bonusStatsText != null) bonusStatsText.SetTextSafe(bonusText);

        if (rewardDisplayUI != null && rewards != null)
        {
            rewardDisplayUI.SetupCost(rewards);
        }

        levelUpPanel.SetActive(true);

        // Phát âm thanh thăng cấp
        PlaySFX(levelUpSFX);

        if (bgOverlayCanvasGroup != null)
        {
            bgOverlayCanvasGroup.DOKill();
            bgOverlayCanvasGroup.alpha = 0f;
            bgOverlayCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
        }

        if (popupContainer != null)
        {
            popupContainer.DOKill();
            popupContainer.transform.localScale = Vector3.one * 0.7f;
            popupContainer.transform.DOScale(Vector3.one, 0.35f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    Invoke(nameof(Hide), autoHideDelay);
                });
        }
        else
        {
            Invoke(nameof(Hide), autoHideDelay);
        }
    }

    public void Show(string title, string reward)
    {
        CancelInvoke(nameof(Hide));

        if (titleText != null) titleText.SetTextSafe(title);
        if (bonusStatsText != null) bonusStatsText.SetTextSafe(reward);

        if (levelUpPanel != null) levelUpPanel.SetActive(true);

        PlaySFX(levelUpSFX);

        Invoke(nameof(Hide), autoHideDelay);
    }

    public void Hide()
    {
        CancelInvoke(nameof(Hide));

        if (levelUpPanel == null || !levelUpPanel.activeSelf) return;

        if (bgOverlayCanvasGroup != null)
        {
            bgOverlayCanvasGroup.DOKill();
            bgOverlayCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        }

        if (popupContainer != null)
        {
            popupContainer.DOKill();
            popupContainer.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    levelUpPanel.SetActive(false);
                });
        }
        else
        {
            levelUpPanel.SetActive(false);
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    protected override void LoadComponent()
    {
        if (levelUpPanel == null)
        {
            levelUpPanel = transform.Find("LevelUpPanel")?.gameObject ?? gameObject;
        }

        if (titleText == null)
        {
            titleText = transform.Find("TitleText")?.GetComponent<TMP_Text>();
        }

        if (bonusStatsText == null)
        {
            bonusStatsText = transform.Find("BonusStatsText")?.GetComponent<TMP_Text>();
        }

        if (rewardDisplayUI == null)
        {
            rewardDisplayUI = GetComponentInChildren<CostDisplayUI>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    protected override void LoadComponentRuntime()
    {
    }
}