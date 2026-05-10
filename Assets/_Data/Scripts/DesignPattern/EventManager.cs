using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEvent
{
    None = 0,
    OnMovement = 1,
    OnDodge = 2,
    OnJump = 3,
    OnAttack = 4,
    OnSlashEffect = 5,
}
public class EventManager : Singleton<EventManager>
{
    private Dictionary<GameEvent, Action<object>> eventDictionary = new Dictionary<GameEvent, Action<object>>();

    public void Subscribe(GameEvent eventType, Action<object> listener)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            eventDictionary[eventType] += listener;
        }
        else
        {
            eventDictionary.Add(eventType, listener);
        }
    }

    public void Unsubscribe(GameEvent eventType, Action<object> listener)
    {
        if (!eventDictionary.ContainsKey(eventType)) return;

        eventDictionary[eventType] -= listener;
    }

    public void Notify(GameEvent eventType, object data = null)
    {
        if (!eventDictionary.ContainsKey(eventType)) return;

        eventDictionary[eventType]?.Invoke(data);
    }
}