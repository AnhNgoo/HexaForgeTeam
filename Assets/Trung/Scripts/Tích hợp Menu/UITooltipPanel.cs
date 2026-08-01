using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UITooltipPanel : MonoBehaviour
{
    public static UITooltipPanel Instance { get; private set; }

    [Header("UI Canvas Group & Root")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private RectTransform containerRect;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;

    [Header("Offset Settings")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(20f, -20f);

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

        if (tooltipRoot != null)
        {
            if (canvasGroup == null) canvasGroup = tooltipRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = tooltipRoot.AddComponent<CanvasGroup>();
            
            tooltipRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (tooltipRoot != null && tooltipRoot.activeInHierarchy)
        {
            UpdatePositionAndPivot();
        }
    }

    public void ShowTooltip(string title, string description, Sprite icon = null)
    {
        if (tooltipRoot == null) return;

        // 1. Gán nội dung
        if (titleText != null)
        {
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
            titleText.SetTextSafe(title);
        }

        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(description));
            descriptionText.SetTextSafe(description);
        }

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(icon != null);
            if (icon != null) iconImage.sprite = icon;
        }

        // 2. Ép Unity UI tính toán lại kích thước Khung Nền vừa vặn với độ dài chữ mới ngay lập tức
        if (containerRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }

        // 3. Cập nhật vị trí bám chuột & Bật Panel
        UpdatePositionAndPivot();
        tooltipRoot.SetActive(true);

        // 4. Hiệu ứng Scale & Fade mượt mà bằng DOTween
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.15f).SetUpdate(true);
        }

        if (containerRect != null)
        {
            containerRect.DOKill();
            containerRect.localScale = Vector3.one * 0.85f;
            containerRect.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    public void HideTooltip()
    {
        if (tooltipRoot == null || !tooltipRoot.activeInHierarchy) return;

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.1f).SetUpdate(true).OnComplete(() =>
            {
                tooltipRoot.SetActive(false);
            });
        }
        else
        {
            tooltipRoot.SetActive(false);
        }
    }

    private void UpdatePositionAndPivot()
    {
        if (containerRect == null) return;

        Vector2 mousePos = Input.mousePosition;

        // Tính toán Pivot tự động lật mép nếu Tooltip tiến sát cạnh màn hình
        float pivotX = (mousePos.x + containerRect.rect.width + cursorOffset.x > Screen.width) ? 1f : 0f;
        float pivotY = (mousePos.y - containerRect.rect.height + cursorOffset.y < 0) ? 0f : 1f;

        containerRect.pivot = new Vector2(pivotX, pivotY);

        // Điều chỉnh Offset hướng theo Pivot
        float offsetX = pivotX == 1f ? -cursorOffset.x : cursorOffset.x;
        float offsetY = pivotY == 1f ? -cursorOffset.y : cursorOffset.y;

        containerRect.position = mousePos + new Vector2(offsetX, offsetY);
    }
}