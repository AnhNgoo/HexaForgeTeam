using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // YÊU CẦU: Cần Import DOTween vào Project để kích hoạt hiệu ứng bay mờ liên tục

public class LobbyNotifyManager : MonoBehaviour
{
    public static LobbyNotifyManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject notifyPanelRoot; // Kéo thả Object Panel thông báo vào đây
    [SerializeField] private TMP_Text notifyText;         // Kéo thả Text hiển thị thông báo vào đây

    [Header("Settings")]
    [SerializeField] private float totalDuration = 2.0f;   // Tổng thời gian thông báo tồn tại và bay (giây)
    [SerializeField] private float fadeOutDuration = 0.5f; // Thời gian thực hiện mờ dần ở cuối hành trình (giây)
    [SerializeField] private float moveDistance = 60f;     // Tổng khoảng cách trôi lên phía trên (pixels)

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    private Sequence activeNotifySequence;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (notifyPanelRoot != null)
        {
            // Tự động kiểm tra hoặc thêm Component CanvasGroup để quản lý Alpha mờ dần
            canvasGroup = notifyPanelRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notifyPanelRoot.AddComponent<CanvasGroup>();
            }

            // Lấy RectTransform để tính toán tọa độ tịnh tiến đi lên
            rectTransform = notifyPanelRoot.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalAnchoredPosition = rectTransform.anchoredPosition;
            }

            // Ẩn bảng khi mới khởi tạo game
            notifyPanelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Hàm công khai toàn cục dùng để nổ thông báo bay trôi mượt mà ngay từ đầu
    /// </summary>
    public void ShowNotify(string message, Color? textColor = null)
    {
        if (notifyPanelRoot == null || notifyText == null || rectTransform == null || canvasGroup == null)
        {
            Debug.LogWarning("<color=red><b>[NOTIFY ERROR]</b> Chưa kéo thả đủ Component vào LobbyNotifyManager Inspector!</color>");
            return;
        }

        // 1. Nếu có một hiệu ứng thông báo trước đó đang bay dở, ép hủy ngay để làm mới nhịp nổ đồ
        if (activeNotifySequence != null)
        {
            activeNotifySequence.Kill();
        }

        // 2. Thiết lập nội dung tĩnh và đưa Panel về trạng thái gốc ban đầu
        notifyText.text = message;
        notifyText.color = textColor ?? Color.white;
        
        rectTransform.anchoredPosition = originalAnchoredPosition; // Đưa về vị trí xuất phát ban đầu
        canvasGroup.alpha = 1f;                                    // Đưa độ mờ về đậm nhất
        rectTransform.localScale = Vector3.one;                    // Trả lại kích thước chuẩn
        notifyPanelRoot.SetActive(true);

        // 3. Xây dựng chuỗi chuyển động Sequence trôi liên tục từ đầu hành trình
        activeNotifySequence = DOTween.Sequence();

        // [NHÁNH 1: DI CHUYỂN] - Ép bảng thông báo bắt đầu trôi lên trục Y liên tục từ 0s đến hết totalDuration
        Vector2 targetPosition = originalAnchoredPosition + new Vector2(0f, moveDistance);
        activeNotifySequence.Append(rectTransform.DOAnchorPos(targetPosition, totalDuration).SetEase(Ease.OutCubic));

        // [NHÁNH 2: LÀM MỜ DẦN] - Đợi một khoảng thời gian trước khi bắt đầu Fade Out ở cuối hành trình bay
        float holdBeforeFade = totalDuration - fadeOutDuration;
        if (holdBeforeFade > 0f)
        {
            // Thao tác chèn hiệu ứng mờ dần vào đúng mốc thời gian cuối hành trình (bằng lệnh Join kết hợp Delay)
            activeNotifySequence.Join(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad).SetDelay(holdBeforeFade));
        }
        else
        {
            // Trường hợp cấu hình đặc biệt nếu fadeOutDuration lớn hơn hoặc bằng tổng thời gian, mờ dần từ đầu luôn
            activeNotifySequence.Join(canvasGroup.DOFade(0f, totalDuration).SetEase(Ease.InQuad));
        }

        // Sau khi hoàn thành trọn vẹn quãng đường trôi và mờ hẳn -> Tắt Object giải phóng tài nguyên
        activeNotifySequence.OnComplete(() =>
        {
            notifyPanelRoot.SetActive(false);
            rectTransform.anchoredPosition = originalAnchoredPosition;
            canvasGroup.alpha = 1f;
            activeNotifySequence = null;
        });
    }

    private void OnDestroy()
    {
        if (activeNotifySequence != null)
        {
            activeNotifySequence.Kill();
        }
    }
}