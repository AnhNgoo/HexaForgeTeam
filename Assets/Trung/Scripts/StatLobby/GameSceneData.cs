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
    CustomRun = 8
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
            }
            return instance;
        }
    }

    [Header("Main Scene Config (Chính)")]
    public string loginScene = "Login Scene";
    public string uiScene = "UIGame";
    public string loadingScene = "Loading Scene";
    public string lobbyMainScene = "LobbyMain Scene";
    public string runGameplayScene = "Run Scene";
    public string tutorialScene = "Tutorial Scene";
    public string finalBossScene = "FinalBoss Scene";

    [Header("Custom Scene List (Mở rộng cho Scene)")]
    [SerializeField] private List<SceneEntry> customScenes = new List<SceneEntry>();

    public string GetSceneName(SceneType type)
    {
        switch (type)
        {
            case SceneType.Login: return loginScene;
            case SceneType.UIGame: return uiScene;
            case SceneType.Loading: return loadingScene;
            case SceneType.LobbyMain: return lobbyMainScene;
            case SceneType.RunGameplay: return runGameplayScene;
            case SceneType.Tutorial: return tutorialScene;
            case SceneType.FinalBoss: return finalBossScene;
            case SceneType.CustomRun:
                if (customScenes != null && customScenes.Count > 0)
                    return customScenes[0].sceneName;
                return runGameplayScene;
            default:
                return runGameplayScene;
        }
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