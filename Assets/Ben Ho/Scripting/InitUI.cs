using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class InitUI : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.InitUI();
        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
        EventManager.Subscribe(GameEvent.OnMusicVolumeChangedTest, OnMusicVolumeChanged);
        EventManager.Subscribe(GameEvent.OnBtn_TestEventFromMenuToOther, OnTestEvent);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnMusicVolumeChangedTest, OnMusicVolumeChanged);
        EventManager.Unsubscribe(GameEvent.OnBtn_TestEventFromMenuToOther, OnTestEvent);
    }

// *NOTE - Nhận event từ menu
    private void OnTestEvent(object obj)
    {
        Debug.Log("Test Event Triggered");
    }

    // *NOTE - Gọi event từ nơi khác vào menu
    [Button("Trigger Test Event From Other To Menu")]
    private void OnTestEventFromOtherToMenu()
    {
        EventManager.Notify(GameEvent.OnTestEventFromOtherToMenu);
    }
    private void OnMusicVolumeChanged(object obj)
    {
        if (obj is float volume)
        {
            Debug.Log($"Music Volume Changed: {volume}");
        }
    }
}
