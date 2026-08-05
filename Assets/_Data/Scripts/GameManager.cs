using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Enum để xác định nhân vật đang ở map nào, ví dụ: Lobby, Run, Boss, v.v...
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

    // void Start()
    // {
    //     UIManager.Instance.InitUI();
    //     GetMapType();
    //     OpenMenuAfterLoadingComplete();
    // }

    private void OpenMenuAfterLoadingComplete()
    {
        if (currentMapType == MapType.Lobby)
        {
            UIManager.Instance.ChangeMenu(MenuType.LobbyTutorialMenu);
        }
        else if (currentMapType == MapType.Run)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }
        else if (currentMapType == MapType.Boss)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
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
        UIManager.Instance.InitUI();
        OpenMenuAfterLoadingComplete();
        Debug.Log($"[GameManager] OnLoadingComplete: Current MapType is {currentMapType}");
    }
}