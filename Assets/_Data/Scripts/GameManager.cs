using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MapType
{
    None = 0,
    Lobby = 1,
    Run = 2,
    Boss = 3,
    Tutorial = 4
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

    private void Start()
    {
        OnLoadingComplete(null);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnLoadingComplete, OnLoadingComplete);
    }

    private void OpenMenuAfterLoadingComplete()
    {
        if (currentMapType == MapType.Lobby)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ChangeMenu(
                    MenuType.DefaultLobbyInputMenu
                );
            }
        }
        else if (currentMapType == MapType.Run || currentMapType == MapType.Boss || currentMapType == MapType.Tutorial)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ChangeMenu(
                    MenuType.GameplayMenu
                );
            }
        }
    }

    public void SetMapType(MapType mapType)
    {
        currentMapType = mapType;
    }

    public void GetMapType()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        GameSceneData sceneData = GameSceneData.Instance;

        if (sceneData == null)
        {
            currentMapType = MapType.None;
            return;
        }

        if (sceneName == sceneData.lobbyMainScene)
            currentMapType = MapType.Lobby;
        else if (sceneName == sceneData.runGameplayScene)
            currentMapType = MapType.Run;
        else if (sceneName == sceneData.finalBossScene)
            currentMapType = MapType.Boss;
        else if (sceneName == sceneData.tutorialScene)
            currentMapType = MapType.Tutorial;
        else
            currentMapType = MapType.None;
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