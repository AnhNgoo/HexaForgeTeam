using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class EventTouch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent onPointerDown;
    public UnityEvent onPointerUp;

    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp?.Invoke();
    }
}
