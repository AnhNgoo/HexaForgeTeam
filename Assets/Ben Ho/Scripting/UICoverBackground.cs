using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UICoverBackground : MonoBehaviour
{
    [Header("Original background size")]
    public float imageWidth = 1920f;
    public float imageHeight = 1080f;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        ApplyCover();
    }

    private void OnEnable()
    {
        ApplyCover();
    }

    private void Update()
    {
        ApplyCover();
    }

    private void ApplyCover()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (parentRect == null)
            parentRect = transform.parent as RectTransform;

        if (parentRect == null)
            return;

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        if (parentWidth <= 0 || parentHeight <= 0)
            return;

        float imageRatio = imageWidth / imageHeight;
        float parentRatio = parentWidth / parentHeight;

        float targetWidth;
        float targetHeight;

        if (parentRatio > imageRatio)
        {
            targetWidth = parentWidth;
            targetHeight = parentWidth / imageRatio;
        }
        else
        {
            targetHeight = parentHeight;
            targetWidth = parentHeight * imageRatio;
        }

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        rectTransform.localScale = Vector3.one;
    }
}