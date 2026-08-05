using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MapType
{
    None = 0,
    Lobby = 1,
    Run = 2,
    Boss = 3,
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private MenuType startingMenu = MenuType.GameplayMenu;
    [SerializeField] private MapType currentMapType = MapType.None;
    public MapType MapType => currentMapType;

    protected override void Awake()
    {
        base.Awake();
        EventManager.Subscribe(GameEvent.OnLoadingComplete, OnLoadingComplete);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnLoadingComplete, OnLoadingComplete);
    }

    private void OpenMenuAfterLoadingComplete()
    {
        if (currentMapType == MapType.Lobby)
        {
            bool isTutorialDone = false;
            if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
            {
                isTutorialDone = SaveLoadManager.Instance.SaveData.isTutorialCompleted;
            }

            if (!isTutorialDone)
            {
                StartCoroutine(DelayedOpenTutorialRoutine());
            }
            else
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
                }
            }
        }
        else if (currentMapType == MapType.Run || currentMapType == MapType.Boss)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
            }
        }
    }

    private IEnumerator DelayedOpenTutorialRoutine()
    {
        yield return null;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.LobbyTutorialMenu);
        }
    }

    public void SetMapType(MapType mapType)
    {
        currentMapType = mapType;
    }

    public void GetMapType()
    {
        string mapTypeName = SceneManager.GetActiveScene().name;
        if (mapTypeName.Contains("Lobby"))
        {
            currentMapType = MapType.Lobby;
        }
        else if (mapTypeName.Contains("Run"))
        {
            currentMapType = MapType.Run;
        }
        else if (mapTypeName.Contains("Boss"))
        {
            currentMapType = MapType.Boss;
        }
        else
        {
            currentMapType = MapType.None;
        }
    }

    private void OnLoadingComplete(object obj)
    {
        GetMapType();
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitUI();
        }

        OpenMenuAfterLoadingComplete();
        Debug.Log($"[GameManager] OnLoadingComplete: Current MapType is {currentMapType}");
    }
}