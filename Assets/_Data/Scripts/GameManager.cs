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

        // BẮT BỘC DÙNG GetSceneName(...) ĐỂ SO SÁNH CHUẨN TÊN SCENE LOBBY CÁ NHÂN (VD: LobbyMainGameTrung)
        string expectedLobbyScene = sceneData.GetSceneName(SceneType.LobbyMain);
        string expectedRunScene = sceneData.GetSceneName(SceneType.RunGameplay);
        string expectedBossScene = sceneData.GetSceneName(SceneType.FinalBoss);
        string expectedTutorialScene = sceneData.GetSceneName(SceneType.Tutorial);

        if (sceneName == expectedLobbyScene)
            currentMapType = MapType.Lobby;
        else if (sceneName == expectedRunScene)
            currentMapType = MapType.Run;
        else if (sceneName == expectedBossScene)
            currentMapType = MapType.Boss;
        else if (sceneName == expectedTutorialScene)
            currentMapType = MapType.Tutorial;
        else
            currentMapType = MapType.None;
            
        Debug.Log($"<color=#00FFCC>[GameManager] Active Scene Name: '{sceneName}' -> Target MapType evaluated: {currentMapType}</color>");
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