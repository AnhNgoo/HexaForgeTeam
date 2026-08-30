using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    None = 0,
    Login = 1,
    UIGame = 2,
    Loading = 3,
    LobbyMain = 4,
    RunGameplay = 5,
    Tutorial = 6,
    FinalBoss = 7,
    CustomRun = 8,
    RunGameplay2 = 9
}

[System.Serializable]
public class SceneEntry
{
    public SceneType sceneType;
    public string sceneName;
}

[CreateAssetMenu(fileName = "GameSceneData", menuName = "Config/Game Scene Data")]
public class GameSceneData : ScriptableObject
{
    private static GameSceneData instance;
    public static GameSceneData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameSceneData>("GameSceneData");
                if (instance == null)
                {
                    Debug.LogError("[GameSceneData] Chưa tạo file GameSceneData trong thư mục Assets/Resources/!");
                }
                else
                {
                    instance.CheckAndCacheActivePersonalConfig();
                }
            }
            return instance;
        }
    }

    [Header("Main Scene Config (Dùng Chung Cho Cả Team)")]
    public string loginScene = "Login Scene";
    public string uiScene = "UIGame";
    public string loadingScene = "Loading Scene";
    public string lobbyMainScene = "LobbyMain Scene";
    public string runGameplayScene = "Run Scene";
    public string runGameplayScene2 = "Run Scene 2"; 
    public string tutorialScene = "Tutorial Scene";
    public string finalBossScene = "FinalBoss Scene";

    [Header("Background Music (Nhạc Nền Theo Scene)")]
    [Tooltip("Nhạc nền dùng khi Lobby Main là scene đang hoạt động.")]
    public AudioClip lobbyMainMusic;
    [Tooltip("Nhạc nền dùng chung cho Run Gameplay Scene và Run Gameplay Scene 2.")]
    public AudioClip runGameplayMusic;
    [Tooltip("Nhạc nền dùng khi Final Boss Scene là scene đang hoạt động.")]
    public AudioClip finalBossMusic;

    [Header("Custom Scene List (Mở rộng cho Scene)")]
    [SerializeField] private List<SceneEntry> customScenes = new List<SceneEntry>();

    private SceneConfigSO activePersonalConfig;

    public void CheckAndCacheActivePersonalConfig()
    {
        activePersonalConfig = null;

        #if UNITY_EDITOR
        SceneConfigSO[] allConfigs = Resources.FindObjectsOfTypeAll<SceneConfigSO>();
        if (allConfigs == null || allConfigs.Length == 0)
        {
            allConfigs = Resources.LoadAll<SceneConfigSO>("");
        }

        foreach (var config in allConfigs)
        {
            if (config != null && config.isOverrideMyLocalScene)
            {
                activePersonalConfig = config;
                Debug.Log($"<color=#00FFCC><b>[Scene System]</b> Đã kích hoạt Override Scene Cá Nhân của Dev: <b>[{config.devName}]</b></color>");
                break;
            }
        }
        #endif
    }

    public string GetSceneName(SceneType type)
    {
        CheckAndCacheActivePersonalConfig();

        switch (type)
        {
            case SceneType.Login:
                return (activePersonalConfig != null && !string.IsNullOrEmpty(activePersonalConfig.customLoginScene)) 
                    ? activePersonalConfig.customLoginScene : loginScene;

            case SceneType.UIGame:
                return (activePersonalConfig != null && !string.IsNullOrEmpty(activePersonalConfig.customUiScene)) 
                    ? activePersonalConfig.customUiScene : uiScene;

            case SceneType.Loading:
                return (activePersonalConfig != null && !string.IsNullOrEmpty(activePersonalConfig.customLoadingScene)) 
                    ? activePersonalConfig.customLoadingScene : loadingScene;

            case SceneType.LobbyMain:
                return (activePersonalConfig != null && !string.IsNullOrEmpty(activePersonalConfig.customLobbyScene)) 
                    ? activePersonalConfig.customLobbyScene : lobbyMainScene;

            case SceneType.RunGameplay:
                return (activePersonalConfig != null && !string.IsNullOrEmpty(activePersonalConfig.customRunGameplayScene)) 
                    ? activePersonalConfig.customRunGameplayScene : runGameplayScene;

            case SceneType.RunGameplay2:
                return (activePersonalConfig != null && !string.IsNullOrEmpty(activePersonalConfig.customRunGameplayScene2)) 
                    ? activePersonalConfig.customRunGameplayScene2 : runGameplayScene2;

            case SceneType.Tutorial:
                return (activePersonalConfig != null && !string.IsNullOrEmpty(activePersonalConfig.customTutorialScene)) 
                    ? activePersonalConfig.customTutorialScene : tutorialScene;

            case SceneType.FinalBoss:
                return (activePersonalConfig != null && !string.IsNullOrEmpty(activePersonalConfig.customFinalBossScene)) 
                    ? activePersonalConfig.customFinalBossScene : finalBossScene;

            case SceneType.CustomRun:
                if (customScenes != null && customScenes.Count > 0)
                    return customScenes[0].sceneName;
                return runGameplayScene;

            default:
                return runGameplayScene;
        }
    }

    public string GetRandomRunSceneName()
    {
        string map1 = GetSceneName(SceneType.RunGameplay);
        string map2 = GetSceneName(SceneType.RunGameplay2);
        if (string.IsNullOrWhiteSpace(map2) || map2 == map1)
        {
            return map1;
        }

        int rand = Random.Range(0, 2);
        string selectedMap = (rand == 0) ? map1 : map2;
        Debug.Log($"<color=#FFCC00><b>[Run Random Map]</b> Đã bốc ngẫu nhiên: <b>{selectedMap}</b> (Map index: {rand + 1})</color>");
        return selectedMap;
    }

    public bool TryGetBackgroundMusic(string sceneName, out AudioClip musicClip)
    {
        musicClip = null;

        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        if (sceneName == GetSceneName(SceneType.LobbyMain))
        {
            musicClip = lobbyMainMusic;
            return true;
        }

        if (sceneName == GetSceneName(SceneType.RunGameplay) ||
            sceneName == GetSceneName(SceneType.RunGameplay2))
        {
            musicClip = runGameplayMusic;
            return true;
        }

        if (sceneName == GetSceneName(SceneType.FinalBoss))
        {
            musicClip = finalBossMusic;
            return true;
        }

        return false;
    }

    public bool IsSceneActive(SceneType type)
    {
        string targetName = GetSceneName(type);
        return SceneManager.GetActiveScene().name == targetName;
    }

    public bool IsSceneLoaded(SceneType type)
    {
        string targetName = GetSceneName(type);
        Scene scene = SceneManager.GetSceneByName(targetName);
        return scene.IsValid() && scene.isLoaded;
    }
}
