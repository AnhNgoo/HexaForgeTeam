using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // YÊU CẦU: Đã Import DOTween vào Project

public class AccountLevelUI : MonoBehaviour
{
    public static AccountLevelUI Instance;

        [Header("UI References")]
        [SerializeField] private TMP_Text LevelText;
        [SerializeField] private TMP_Text UserNameText;
        [SerializeField] private TMP_Text ExpText;
        [SerializeField] private Slider ExpBar;

        // Biến cục bộ lưu trữ giá trị EXP hiển thị chạy động để đếm số mượt
        private float animatedCurrentExp;
        private int cachedRequiredExp;
        private Tween activeExpTween;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Hàm làm mới hiển thị cấp độ và thanh kinh nghiệm chạy động mượt mà bằng DOTween
        /// </summary>
        public void Refresh(int level, int currentExp, int requiredExp)
        {
            if (LevelText != null)
            {
                LevelText.text = level.ToString();
            }

            if (requiredExp <= 0) return;

            cachedRequiredExp = requiredExp;

            // Tính toán tỷ lệ phần trăm fill đích đến
            float targetValue = (float)currentExp / requiredExp;

            // Xóa hiệu ứng đang chạy dở trước đó để tránh xung đột đè Tween khi click liên tục
            if (activeExpTween != null) activeExpTween.Kill();

            // Khởi tạo chuỗi chuyển động mượt mà kéo dài 0.5 giây
            Sequence expSequence = DOTween.Sequence();

            if (ExpBar != null)
            {
                // 1. Ép thanh Slider trượt tịnh tiến mượt mà từ vị trí cũ sang vị trí mới
                expSequence.Join(ExpBar.DOValue(targetValue, 0.5f).SetEase(Ease.OutQuad));
            }

            if (ExpText != null)
            {
                // 2. Tạo hiệu ứng đếm số chạy chữ tăng dần (từ giá trị animatedCurrentExp cũ -> currentExp mới)
                expSequence.Join(DOTween.To(() => animatedCurrentExp, x => animatedCurrentExp = x, currentExp, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnUpdate(() =>
                    {
                        // Cập nhật văn bản trong lúc số đang chạy cuộn động
                        ExpText.text = $"{(int)animatedCurrentExp:N0} / {cachedRequiredExp:N0}";
                    }));
            }

            activeExpTween = expSequence;
        }

        public void SetUserName(string userName)
        {
            if (UserNameText != null)
            {
                UserNameText.text = userName;
            }
        }
    }