using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LeaderboardItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text powerDetailText;

    [Header("Highlight My Rank")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Color myRankColor = new Color(1f, 0.85f, 0.3f, 0.6f); // Màu Vàng Kim nổi bật cho My Rank
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.4f);  // Màu tối bình thường
    [SerializeField] private GameObject myTagIcon; // (Tùy chọn) Icon / Tag "YOU" nếu có

    private int currentDisplayScore = 0;
    private Tween scoreTween;

    public void Setup(int rank, string playerName, int score, string detailInfo = "", bool isMe = false)
    {
        if (rankText != null) rankText.SetTextSafe($"#{rank}");
        if (playerNameText != null) playerNameText.SetTextSafe(playerName);

        // ===== 1. ĐỔI MÀU HIGHLIGHT NẾU LÀ DÒNG CỦA CHÍNH MÌNH =====
        if (bgImage != null)
        {
            bgImage.color = isMe ? myRankColor : normalColor;
        }

        if (myTagIcon != null)
        {
            myTagIcon.SetActive(isMe);
        }

        // ===== 2. HIỆU ỨNG NHẢY SỐ ĐIỂM TĂNG DẦN =====
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

        // Hiệu ứng nhún nhẹ khi xuất hiện, nếu là bản thân thì nhún mạnh hơn 1 chút
        float punchScale = isMe ? 0.08f : 0.04f;
        transform.DOPunchScale(new Vector3(punchScale, punchScale, 0f), 0.3f, 5, 0.5f);
    }

    private void OnDisable()
    {
        if (scoreTween != null) scoreTween.Kill();
    }
}