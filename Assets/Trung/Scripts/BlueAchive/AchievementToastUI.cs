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

    [Header("Audio SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toastSFX;

    [Header("Slide Animation Settings")]
    [SerializeField] private float slideDistanceX = 450f; // Khoảng cách trượt từ bên phải vào

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

                // Phát âm thanh khi Toast xuất hiện
                PlaySFX(toastSFX);

                // Setup vị trí bắt đầu lệch về bên phải (Right -> Left)
                if (rectTransform != null)
                {
                    rectTransform.DOKill();
                    rectTransform.anchoredPosition = originalPosition + new Vector2(slideDistanceX, 0f);
                    rectTransform.DOAnchorPos(originalPosition, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
                }

                if (canvasGroup != null)
                {
                    canvasGroup.DOKill();
                    canvasGroup.alpha = 0f;
                    canvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
                }

                VisualRoot.transform.DOKill();
                VisualRoot.transform.localScale = Vector3.one * 0.9f;
                VisualRoot.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);

                yield return new WaitForSecondsRealtime(showDuration);

                // Trượt tiếp sang trái hoặc thu về khi biến mất
                if (rectTransform != null)
                {
                    rectTransform.DOAnchorPos(originalPosition - new Vector2(60f, 0f), 0.25f).SetEase(Ease.InQuad).SetUpdate(true);
                }

                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InQuad).SetUpdate(true);
                }

                yield return new WaitForSecondsRealtime(0.26f);
                VisualRoot.SetActive(false);
            }
        }

        isShowing = false;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}