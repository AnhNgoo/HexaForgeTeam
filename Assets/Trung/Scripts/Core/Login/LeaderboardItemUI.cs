using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class LeaderboardItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text powerDetailText;

    [Header("Highlight My Rank")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Color myRankColor = new Color(1f, 0.85f, 0.3f, 0.6f);
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.4f);
    [SerializeField] private GameObject myTagIcon;

    private int currentDisplayScore = 0;
    private Tween scoreTween;
    private string cachedPlayerName;
    private string cachedDetailInfo;

    public void Setup(int rank, string playerName, int score, string detailInfo = "", bool isMe = false)
    {
        cachedPlayerName = playerName;
        cachedDetailInfo = detailInfo;

        if (rankText != null) rankText.SetTextSafe($"#{rank}");
        if (playerNameText != null) playerNameText.SetTextSafe(playerName);

        if (bgImage != null)
        {
            bgImage.color = isMe ? myRankColor : normalColor;
        }

        if (myTagIcon != null)
        {
            myTagIcon.SetActive(isMe);
        }

        if (scoreTween != null) scoreTween.Kill();

        scoreTween = DOVirtual.Int(currentDisplayScore, score, 0.6f, (val) =>
        {
            currentDisplayScore = val;
            if (scoreText != null) scoreText.SetTextSafe($"{val:N0}");
        }).SetEase(Ease.OutCubic);

        if (powerDetailText != null)
        {
            if (!string.IsNullOrEmpty(detailInfo))
            {
                powerDetailText.gameObject.SetActive(true);
                powerDetailText.SetTextSafe(detailInfo);
            }
            else
            {
                powerDetailText.gameObject.SetActive(false);
            }
        }

        float punchScale = isMe ? 0.08f : 0.04f;
        transform.DOPunchScale(new Vector3(punchScale, punchScale, 0f), 0.3f, 5, 0.5f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance == null) return;

        string title = string.IsNullOrEmpty(cachedPlayerName) ? "Player Stat" : cachedPlayerName;
        string detail = string.IsNullOrEmpty(cachedDetailInfo) ? $"Combat Power: {currentDisplayScore:N0}" : cachedDetailInfo;

        UITooltipPanel.Instance.ShowTooltip(title, detail);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        if (scoreTween != null) scoreTween.Kill();
        if (UITooltipPanel.Instance != null) UITooltipPanel.Instance.HideTooltip();
    }
}