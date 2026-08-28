using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonHoverUnderline : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    [SerializeField] private GameObject selectedLine;
    [SerializeField] private Color highlightedColor = new Color(1f, 0.94f, 0.75f, 1f);
    [SerializeField] private float fadeDuration = 0.08f;

    private Button button;
    private bool pointerInside;
    private bool selected;

    public void Configure()
    {
        CacheReferences();
        ConfigureButtonColors();
        RefreshLine();
    }

    private void Awake()
    {
        Configure();
    }

    private void OnEnable()
    {
        pointerInside = false;
        selected = EventSystem.current != null &&
                   EventSystem.current.currentSelectedGameObject == gameObject;
        RefreshLine();
    }

    private void OnDisable()
    {
        pointerInside = false;
        selected = false;

        if (selectedLine != null)
            selectedLine.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        RefreshLine();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        RefreshLine();
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
        RefreshLine();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        RefreshLine();
    }

    private void CacheReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (selectedLine != null)
            return;

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child != transform && child.name == "Selected_line")
            {
                selectedLine = child.gameObject;
                break;
            }
        }
    }

    private void ConfigureButtonColors()
    {
        if (button == null || button.transition != Selectable.Transition.ColorTint)
            return;

        ColorBlock colors = button.colors;
        colors.highlightedColor = highlightedColor;
        colors.selectedColor = highlightedColor;
        colors.fadeDuration = fadeDuration;
        button.colors = colors;
    }

    private void RefreshLine()
    {
        if (selectedLine != null)
            selectedLine.SetActive(pointerInside || selected);
    }
}
