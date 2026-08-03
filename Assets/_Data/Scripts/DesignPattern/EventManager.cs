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
    OnSkill_2 = 12,
    OnActiveSkill_1 = 13,
    OnActiveSkill_2 = 14,
    OnMusicVolumeChangedTest = 15,
    OnBtn_TestEventFromMenuToOther = 16,
    OnTestEventFromOtherToMenu = 17,
    OnShowTutorial = 18,
    OnHideTutorial = 19,
    OnShowPickUpItemPanel = 20,
    OnHidePickUpItemPanel = 21,
    OnAddWeaponToInventory = 22,
    OnSelectItemInInventory = 23,
    OnDeselectItemInInventory = 24,
    OnDiscardItemInInventory = 25,
    OnUpdateDisplayWeapon = 26,
    OnUpdateMaxHealth = 27,
    OnUpdateHealth = 28,
    OnUpdateRecoveryBottle = 29,
    OnUpdateMaxStamina = 30,
    OnUpdateStamina = 31,
    OnUpdateMaxMP = 32,
    OnUpdateMP = 33,
    OnSetImageSkill1 = 34,
    OnSetImageSkill2 = 35,
    OnUpdateCooldownSkill1 = 36,
    OnUpdateCooldownSkill2 = 37,
    OnPlayerSpawned = 38,
    OnPlayerDeath = 39,
    OnLoadingComplete = 40,
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