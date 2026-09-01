using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

public enum MapType
{
    None = 0,
    Lobby = 1,
    Run = 2,
    Boss = 3,
    Tutorial = 4,
}

public class GameManager : Singleton<GameManager>
{
    [Header("Current Map Status")]
    [SerializeField] private MapType currentMapType = MapType.None;
    public MapType MapType => currentMapType;

    [Header("Runtime Resolved Scene Names")]
    [SerializeField] private string currentActiveSceneName = "";
    [SerializeField] private string resolvedLobbyScene = "";
    [SerializeField] private string resolvedRunScene1 = "";
    [SerializeField] private string resolvedRunScene2 = "";
    [SerializeField] private string resolvedBossScene = "";
    [SerializeField] private string resolvedTutorialScene = "";

    protected override void Awake()
    {
        base.Awake();
        EventManager.Subscribe(GameEvent.OnLoadingComplete, OnLoadingComplete);
        EventManager.Subscribe(GameEvent.OnPlayerSpawned, HandlePlayerSpawned);
    }

    private void Start()
    {
        OnLoadingComplete(null);
    }

    protected virtual void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnLoadingComplete, OnLoadingComplete);
        EventManager.Unsubscribe(GameEvent.OnPlayerSpawned, HandlePlayerSpawned);
    }

    public void StartRun()
    {
        PlayerManager.Instance.CurrentCharacterBase.CharacterInput.LockInput = true;
    }
    private void OpenMenuAfterLoadingComplete()
    {
        if (UIManager.Instance == null) return;

        if (currentMapType == MapType.Lobby)
        {
            InitInLobby();
        }
        else if (currentMapType == MapType.Run || currentMapType == MapType.Tutorial)
        {
            InitInRun();
        }
        else if (currentMapType == MapType.Boss)
        {
            InitInBoss();
        }
        else
        {
            // Fallback an toàn: Nếu là màn chơi bất kỳ khác ngoài Lobby thì luôn bật GameplayMenu
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }
    }

    private void InitInLobby()
    {
        Time.timeScale = 1f;

        UIManager.Instance?.ChangeMenu(
            MenuType.DefaultLobbyInputMenu
        );

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.ForceUnlockState();
        }

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(true);
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }

        PlayerManager.Instance?.SpawnCharacterInLobby();
        PlayerManager.Instance.CurrentCharacterBase.CharacterInput.LockInput = false;
    }

    private void InitInRun()
    {
        Time.timeScale = 1f;

        UIManager.Instance?.ChangeMenu(MenuType.GameplayMenu);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(false);
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.ForceUnlockState();
        }

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SetMaxRespawnAttempts(maxAttempts: 0, limitRespawnAttempts: false);
        }
        PlayerManager.Instance?.CurrentCharacterBase?.CharacterSkill?.LockUseSkill(lockSkill1: false, lockSkill2: false);
    }

    private void InitInBoss()
    {
        UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);

        // Nếu ở map run nhân vật chưa chết lần nào khi bo cuối thì nhân vật sẽ được respawn ở map boss + thêm 1 lần
        if (PlayerManager.Instance.CurrentRespawnAttempts == PlayerManager.Instance.MaxRespawnAttemptsInFinalSafeZone)
            PlayerManager.Instance.SetMaxRespawnAttempts(maxAttempts: PlayerManager.Instance.MaxRespawnAttemptsInBoss + PlayerManager.Instance.BonusRespawnAttemptsInBoss, limitRespawnAttempts: true);
        else
            PlayerManager.Instance.SetMaxRespawnAttempts(maxAttempts: PlayerManager.Instance.MaxRespawnAttemptsInBoss, limitRespawnAttempts: true);

        PlayerManager.Instance?.CurrentCharacterBase?.CharacterSkill?.LockUseSkill(lockSkill1: false, lockSkill2: false);
    }

    private void HandlePlayerSpawned(object data)
    {
        if (data is not Transform playerTransform ||
            !playerTransform.TryGetComponent(
                out CharacterBase character))
        {
            return;
        }

        bool lockSkills =
            currentMapType == MapType.Lobby;

        character.CharacterSkill?.LockUseSkill(
            lockSkills,
            lockSkills
        );
    }

    public void SetMapType(MapType mapType)
    {
        currentMapType = mapType;
    }

    public void GetMapType()
    {
        GameSceneData sceneData = GameSceneData.Instance;

        if (sceneData != null)
        {
            resolvedLobbyScene = sceneData.GetSceneName(SceneType.LobbyMain);
            resolvedRunScene1 = sceneData.GetSceneName(SceneType.RunGameplay);
            resolvedRunScene2 = sceneData.GetSceneName(SceneType.RunGameplay2);
            resolvedBossScene = sceneData.GetSceneName(SceneType.FinalBoss);
            resolvedTutorialScene = sceneData.GetSceneName(SceneType.Tutorial);
        }

        // 1. Kiểm tra trạng thái nạp của các Scene trong SceneManager
        bool isBossLoaded = IsSceneLoaded(resolvedBossScene);
        bool isRun1Loaded = IsSceneLoaded(resolvedRunScene1);
        bool isRun2Loaded = !string.IsNullOrEmpty(resolvedRunScene2) && IsSceneLoaded(resolvedRunScene2);
        bool isTutorialLoaded = IsSceneLoaded(resolvedTutorialScene);
        bool isLobbyLoaded = IsSceneLoaded(resolvedLobbyScene);

        // 2. Phân định MapType: Nếu có bất kỳ scene Run/Boss/Tutorial nào đang nạp -> Run/Boss/Tutorial
        if (isBossLoaded)
        {
            currentMapType = MapType.Boss;
        }
        else if (isRun1Loaded || isRun2Loaded)
        {
            currentMapType = MapType.Run;
        }
        else if (isTutorialLoaded)
        {
            currentMapType = MapType.Tutorial;
        }
        else if (isLobbyLoaded)
        {
            currentMapType = MapType.Lobby;
            Scene lobbyScene = SceneManager.GetSceneByName(resolvedLobbyScene);
            if (lobbyScene.IsValid() && lobbyScene.isLoaded && SceneManager.GetActiveScene() != lobbyScene)
            {
                SceneManager.SetActiveScene(lobbyScene);
            }
        }
        else
        {
            // Fallback dựa trên Active Scene Name
            string activeName = SceneManager.GetActiveScene().name;
            if (activeName.Equals(resolvedLobbyScene, StringComparison.OrdinalIgnoreCase) || activeName.IndexOf("Lobby", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                currentMapType = MapType.Lobby;
            }
            else if (activeName.Equals(resolvedBossScene, StringComparison.OrdinalIgnoreCase) || activeName.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0 || activeName.IndexOf("Arena", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                currentMapType = MapType.Boss;
            }
            else if (activeName.Equals(resolvedTutorialScene, StringComparison.OrdinalIgnoreCase) || activeName.IndexOf("Tutorial", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                currentMapType = MapType.Tutorial;
            }
            else
            {
                currentMapType = MapType.Run;
            }
        }

        currentActiveSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"<color=#00FFCC><b>[GameManager]</b> Active Scene: '{currentActiveSceneName}' | Current MapType: <b>[{currentMapType}]</b></color>");
    }

    private bool IsSceneLoaded(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }


    private void OnLoadingComplete(object obj)
    {
        GetMapType();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitUI();
        }

        OpenMenuAfterLoadingComplete();
    }
}