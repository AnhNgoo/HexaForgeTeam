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
    OnEnableTrailSlashEffect = 5,
    OnDisableTrailSlashEffect = 6,
    OnPlaySlashEffect = 7,
    OnLockTarget = 8,
    OnWallJump = 9,
    OnHealthRecovery = 10,
    OnSkill_1 = 11,
}
public static class EventManager
{
    private static Dictionary<GameEvent, Action<object>> eventDictionary = new Dictionary<GameEvent, Action<object>>();

    public static void Subscribe(GameEvent eventType, Action<object> listener)
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

    public static void Unsubscribe(GameEvent eventType, Action<object> listener)
    {
        if (!eventDictionary.ContainsKey(eventType)) return;

        eventDictionary[eventType] -= listener;
    }

    public static void Notify(GameEvent eventType, object data = null)
    {
        if (!eventDictionary.ContainsKey(eventType)) return;

        eventDictionary[eventType]?.Invoke(data);
    }
}