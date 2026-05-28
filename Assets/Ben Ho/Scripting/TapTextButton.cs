using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public class TapTextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField] private string sceneName;

    [Header("Animation")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressedScale = 0.97f;
    [SerializeField] private float hoverBrightness = 1.08f;
    [SerializeField] private float pressedBrightness = 0.95f;
    [SerializeField] private float animSpeed = 12f;
    [SerializeField] private bool useUnscaledTime = true;

    private TMP_Text text;
    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private Color baseColor;
    private Color targetColor;
    private bool isHovered;
    private bool isPressed;

    public string SceneName => sceneName;

    public void SetSceneName(string value)
    {
        sceneName = value;
    }

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
        baseColor = text.color;
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
        text.color = Color.Lerp(text.color, targetColor, t);
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
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        SceneManager.LoadScene(sceneName);
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
        text.color = color;
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
