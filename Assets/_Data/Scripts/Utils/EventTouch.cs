using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EventTouch : Selectable, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent onPointerDown;
    public UnityEvent onPointerUp;

    protected override void Awake()
    {
        base.Awake();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
    }
#endif

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable()) return;

        base.OnPointerDown(eventData);
        onPointerDown?.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!IsInteractable()) return;

        onPointerUp?.Invoke();
    }

    public void SetInteractable(bool value)
    {
        interactable = value;

        if (!value)
        {
            DoStateTransition(SelectionState.Disabled, false);
        }
        else
        {
            DoStateTransition(SelectionState.Normal, false);
        }
    }
}