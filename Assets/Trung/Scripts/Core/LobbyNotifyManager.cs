using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyNotifyManager : MonoBehaviour
{
    public static LobbyNotifyManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject notifyPanelRoot;
    [SerializeField] private TMP_Text notifyText;

    [Header("Settings")]
    [SerializeField] private float totalDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float moveDistance = 60f;

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
            canvasGroup = notifyPanelRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notifyPanelRoot.AddComponent<CanvasGroup>();
            }

            rectTransform = notifyPanelRoot.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalAnchoredPosition = rectTransform.anchoredPosition;
            }

            notifyPanelRoot.SetActive(false);
        }
    }

    public void ShowNotify(string message, Color textColor)
    {
        if (notifyPanelRoot == null || notifyText == null) return;

        if (activeNotifySequence != null)
        {
            activeNotifySequence.Kill(true);
        }

        notifyText.SetTextSafe(message);
        notifyText.color = textColor;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        notifyPanelRoot.SetActive(true);

        activeNotifySequence = DOTween.Sequence();
        activeNotifySequence.SetUpdate(true);

        if (rectTransform != null)
        {
            Vector2 targetPosition = originalAnchoredPosition + new Vector2(0f, moveDistance);
            activeNotifySequence.Append(rectTransform.DOAnchorPos(targetPosition, totalDuration).SetEase(Ease.OutCubic));
        }

        if (canvasGroup != null)
        {
            float holdBeforeFade = totalDuration - fadeOutDuration;
            if (holdBeforeFade > 0f)
            {
                activeNotifySequence.Join(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad).SetDelay(holdBeforeFade));
            }
            else
            {
                activeNotifySequence.Join(canvasGroup.DOFade(0f, totalDuration).SetEase(Ease.InQuad));
            }
        }

        activeNotifySequence.OnComplete(() =>
        {
            notifyPanelRoot.SetActive(false);
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalAnchoredPosition;
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
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