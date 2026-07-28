using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class AchievementToastUI : MonoBehaviour
{
    [SerializeField] private GameObject VisualRoot;
    [SerializeField] private TMP_Text TitleText;
    [SerializeField] private TMP_Text DescriptionText;
    [SerializeField] private float showDuration = 3f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Queue<(string title, string description)> toastQueue = new Queue<(string, string)>();
    private bool isShowing = false;

    private void Awake()
    {
        if (VisualRoot != null)
        {
            canvasGroup = VisualRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = VisualRoot.AddComponent<CanvasGroup>();

            rectTransform = VisualRoot.GetComponent<RectTransform>();
            if (rectTransform != null) originalPosition = rectTransform.anchoredPosition;

            VisualRoot.SetActive(false);
        }
    }

    public void ShowToast(string title, string description)
    {
        toastQueue.Enqueue((title, description));
        if (!isShowing)
        {
            StartCoroutine(ProcessToastQueue());
        }
    }

    private IEnumerator ProcessToastQueue()
    {
        isShowing = true;

        while (toastQueue.Count > 0)
        {
            var current = toastQueue.Dequeue();

            if (TitleText != null) TitleText.SetTextSafe(current.title);
            if (DescriptionText != null) DescriptionText.SetTextSafe(current.description);

            if (VisualRoot != null)
            {
                VisualRoot.SetActive(true);

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = originalPosition + new Vector2(0f, 100f);
                    rectTransform.DOAnchorPos(originalPosition, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
                }

                VisualRoot.transform.localScale = Vector3.one * 0.8f;
                VisualRoot.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);

                yield return new WaitForSecondsRealtime(showDuration);

                if (rectTransform != null)
                {
                    rectTransform.DOAnchorPos(originalPosition + new Vector2(0f, 60f), 0.3f).SetEase(Ease.InQuad).SetUpdate(true);
                }

                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad).SetUpdate(true);
                }

                yield return new WaitForSecondsRealtime(0.35f);
                VisualRoot.SetActive(false);
            }
        }

        isShowing = false;
    }
}