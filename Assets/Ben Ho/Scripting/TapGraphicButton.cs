using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Graphic))]
public class TapGraphicButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressedScale = 0.97f;
    [SerializeField] private float hoverBrightness = 1.08f;
    [SerializeField] private float pressedBrightness = 0.95f;
    [SerializeField] private float animSpeed = 12f;
    [SerializeField] private bool useUnscaledTime = true;

    private Graphic graphic;
    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private Color baseColor;
    private Color targetColor;
    private bool isHovered;
    private bool isPressed;
    private Action onClick;

    public void SetOnClick(Action action)
    {
        onClick = action;
    }

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
        baseColor = graphic.color;
        targetScale = baseScale;
        targetColor = baseColor;
    }

    private void OnEnable()
    {
        ApplyImmediate(baseScale, baseColor);
    }

    private void OnDisable()
    {
        ApplyImmediate(baseScale, baseColor);
    }

    private void Update()
    {
        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float t = 1f - Mathf.Exp(-animSpeed * delta);
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, t);
        graphic.color = Color.Lerp(graphic.color, targetColor, t);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (!isPressed)
            SetHoverState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (!isPressed)
            SetNormalState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        SetPressedState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if (isHovered)
            SetHoverState();
        else
            SetNormalState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    private void SetNormalState()
    {
        targetScale = baseScale;
        targetColor = baseColor;
    }

    private void SetHoverState()
    {
        targetScale = baseScale * hoverScale;
        targetColor = ApplyBrightness(baseColor, hoverBrightness);
    }

    private void SetPressedState()
    {
        targetScale = baseScale * pressedScale;
        targetColor = ApplyBrightness(baseColor, pressedBrightness);
    }

    private void ApplyImmediate(Vector3 scale, Color color)
    {
        rectTransform.localScale = scale;
        graphic.color = color;
        targetScale = scale;
        targetColor = color;
    }

    private static Color ApplyBrightness(Color color, float brightness)
    {
        return new Color(
            Mathf.Clamp01(color.r * brightness),
            Mathf.Clamp01(color.g * brightness),
            Mathf.Clamp01(color.b * brightness),
            color.a);
    }
}
