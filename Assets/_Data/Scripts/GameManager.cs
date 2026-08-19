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

    protected virtual void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnLoadingComplete, OnLoadingComplete);
    }

    private void OpenMenuAfterLoadingComplete()
    {
        if (UIManager.Instance == null) return;

        if (currentMapType == MapType.Lobby)
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
        }
        else if (currentMapType == MapType.Run || currentMapType == MapType.Boss || currentMapType == MapType.Tutorial)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }
        else
        {
            // Fallback an toàn: Nếu là màn chơi bất kỳ khác ngoài Lobby thì luôn bật GameplayMenu
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
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

        // Lấy tên scene cá nhân đã override qua GetSceneName(...)
        string expectedLobbyScene = sceneData.GetSceneName(SceneType.LobbyMain);
        string expectedRunScene = sceneData.GetSceneName(SceneType.RunGameplay);
        string expectedBossScene = sceneData.GetSceneName(SceneType.FinalBoss);
        string expectedTutorialScene = sceneData.GetSceneName(SceneType.Tutorial);

        // 1. So khớp chính xác tên Scene đã cấu hình
        if (sceneName == expectedLobbyScene)
        {
            currentMapType = MapType.Lobby;
        }
        else if (sceneName == expectedRunScene)
        {
            currentMapType = MapType.Run;
        }
        else if (sceneName == expectedBossScene)
        {
            currentMapType = MapType.Boss;
        }
        else if (sceneName == expectedTutorialScene)
        {
            currentMapType = MapType.Tutorial;
        }
        // 2. Fallback đối chiếu từ khóa an toàn nếu cấu hình override bị lệch
        else if (sceneName.IndexOf("Lobby", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            currentMapType = MapType.Lobby;
        }
        else if (sceneName.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0 || sceneName.IndexOf("Final", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            currentMapType = MapType.Boss;
        }
        else if (sceneName.IndexOf("Run", StringComparison.OrdinalIgnoreCase) >= 0 || sceneName.IndexOf("Gameplay", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            currentMapType = MapType.Run;
        }
        else if (sceneName.IndexOf("Tutorial", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            currentMapType = MapType.Tutorial;
        }
        else
        {
            currentMapType = MapType.None;
        }
            
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