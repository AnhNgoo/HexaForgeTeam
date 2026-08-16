using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

public abstract class MenuBase : LoadComponents
{
    [ShowInInspector] public abstract MenuType menuType { get; }

    public virtual void Open(object data = null)
    {
        gameObject.SetActive(true);

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = true;
            if (InteractUIV2.Instance != null)
            {
                InteractUIV2.Instance.Hide();
            }
        }

        AnimateOpen();
    }

    public virtual void Close()
    {
        AnimateClose(() =>
        {
            gameObject.SetActive(false);

            if (InteractManagerV2.Instance != null)
            {
                InteractManagerV2.Instance.SetCooldown(0.2f);
                InteractManagerV2.Instance.IsBusy = false;
                InteractManagerV2.Instance.ForceRefresh();
            }
        });
    }

    protected virtual void AnimateOpen()
    {
        DOTween.Kill(transform);
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        // Đảm bảo hiển thị ngay lập tức trước khi chạy Tween
        group.alpha = 1f;
        transform.localScale = Vector3.one;

        // Reset vị trí và hiệu ứng Scale nảy nhẹ
        group.DOKill();
        group.alpha = 0f;
        transform.localScale = Vector3.one * 0.95f;

        group.DOFade(1f, 0.2f).SetUpdate(true);
        transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    protected virtual void AnimateClose(System.Action onComplete)
    {
        DOTween.Kill(transform);
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.DOKill();
            group.DOFade(0f, 0.15f).SetUpdate(true);
            transform.DOScale(Vector3.one * 0.95f, 0.15f).SetEase(Ease.InQuad).SetUpdate(true).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }
}