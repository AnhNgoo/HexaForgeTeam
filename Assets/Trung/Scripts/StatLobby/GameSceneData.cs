using System.Collections.Generic;
using UnityEngine;

public enum SceneType
{
    None = 0,
    Login = 1,
    Loading = 2,
    LobbyMain = 3,
    RunGameplay = 4,
    Tutorial = 5,
    FinalBoss = 6,
    CustomRun = 7
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
}