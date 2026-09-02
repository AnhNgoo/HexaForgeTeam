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
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        group.alpha = 1f;
        transform.localScale = Vector3.one;
    }

    protected virtual void AnimateClose(System.Action onComplete)
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 0f;
        }
        transform.localScale = Vector3.one;
        onComplete?.Invoke();
    }

    protected virtual Transform FindDeepChild(string childName)
    {
        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}