using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    // Giữ lựa chọn boss trong suốt phiên chơi, kể cả khi một RunManager khác
    // được tạo lại trong quá trình chuyển scene additive.
    private static PoolType sessionSelectedFinalBossPool = PoolType.None;

    [Header("Current Dynamic Target Scene Name")]
    [SerializeField] private string gameplaySceneName = "";

    [Header("Lobby Spawn Settings")]
    [SerializeField] private Transform lobbySpawnPoint;
    [SerializeField] private GameObject lobbyVisuals;

    [Header("HUD Controller")]
    [SerializeField] private GameObject lobbyHUDMainObject;

    private float totalDamageDealt;
    private int pendingGem;
    private int pendingExp;
    private int pendingShards;
    private bool isRunActive = false;

    public bool IsRunActive => isRunActive;

    [SerializeField] private PoolType selectedFinalBossPool = PoolType.EnemyEarthshakerBoss;

    public PoolType SelectedFinalBossPool => selectedFinalBossPool;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        Instance = null;
        sessionSelectedFinalBossPool = PoolType.None;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (sessionSelectedFinalBossPool != PoolType.None)
            {
                selectedFinalBossPool = sessionSelectedFinalBossPool;
            }
            else
            {
                sessionSelectedFinalBossPool = selectedFinalBossPool;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static PoolType ResolveSelectedFinalBossPool(PoolType fallback)
    {
        if (Instance != null && Instance.selectedFinalBossPool != PoolType.None)
        {
            sessionSelectedFinalBossPool = Instance.selectedFinalBossPool;
            return Instance.selectedFinalBossPool;
        }

        return sessionSelectedFinalBossPool != PoolType.None
            ? sessionSelectedFinalBossPool
            : fallback;
    }

    public void ConfigureRun(string sceneName, PoolType finalBossPool)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            gameplaySceneName = sceneName;
        }

        if (finalBossPool != PoolType.None)
        {
            selectedFinalBossPool = finalBossPool;
            sessionSelectedFinalBossPool = finalBossPool;
            Debug.Log($"[RunManager] Final Boss đã chọn: {selectedFinalBossPool} ({(int)selectedFinalBossPool})");
        }
    }

    public string GetGameplaySceneName() => gameplaySceneName;

    public void RegisterDamage(float amount)
    {
        if (amount <= 0) return;
        totalDamageDealt += amount;
    }

    public float GetTotalDamage() => totalDamageDealt;

    public void ResetDamageData()
    {
        totalDamageDealt = 0f;
    }

    public void SetPendingRewards(int gem, int exp, int shards)
    {
        pendingGem = gem;
        pendingExp = exp;
        pendingShards = shards;
    }

    public void StartRun()
    {
        if (InteractManagerV2.Instance != null && InteractManagerV2.Instance.IsBusy) return;

        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            gameplaySceneName = GameSceneData.Instance != null
                ? GameSceneData.Instance.GetRandomRunSceneName()
                : "Run Scene";
        }

        ResetDamageData();
        if (RunGameplayController.Instance != null)
        {
            RunGameplayController.Instance.ResetStats();
        }

        HideLobbyHUD();

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = true;
        }

        StartCoroutine(LoadSceneCoroutine());
    }

    public void EnterFinalBoss(string overrideSceneName = "")
    {
        if (!isRunActive) return;

        string targetBossScene = overrideSceneName;

        if (string.IsNullOrWhiteSpace(targetBossScene))
        {
            targetBossScene = GameSceneData.Instance != null
                ? GameSceneData.Instance.GetSceneName(SceneType.FinalBoss)
                : "FinalBoss Scene";
        }

        HideLobbyHUD();
        StartCoroutine(EnterFinalBossCoroutine(targetBossScene));
    }

    private void HideLobbyHUD()
    {
        if (lobbyHUDMainObject != null)
        {
            lobbyHUDMainObject.SetActive(false);
        }

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(false);
        }
    }

    private IEnumerator EnterFinalBossCoroutine(string bossSceneName)
    {
        if (InteractManagerV2.Instance != null)
            InteractManagerV2.Instance.IsBusy = true;

        if (SafeZoneManager.Instance != null)
        {
            SafeZoneManager.Instance.StopAllCoroutines();
            SafeZoneManager.Instance.StopForFinalBoss();
        }

        string loadingSceneName = GameSceneData.Instance != null
            ? GameSceneData.Instance.GetSceneName(SceneType.Loading)
            : "Loading Scene";

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.05f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(bossSceneName);
        }

        AsyncOperation loadBoss = SceneManager.LoadSceneAsync(bossSceneName, LoadSceneMode.Additive);
        loadBoss.allowSceneActivation = false;

        float duration = Random.Range(5.0f, 7.0f);
        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadBoss, false, duration));
        }

        loadBoss.allowSceneActivation = true;
        while (!loadBoss.isDone) yield return null;

        Scene finalBossScene = SceneManager.GetSceneByName(bossSceneName);
        if (!finalBossScene.IsValid() || !finalBossScene.isLoaded)
        {
            if (InteractManagerV2.Instance != null) InteractManagerV2.Instance.IsBusy = false;
            yield break;
        }

        SceneManager.SetActiveScene(finalBossScene);
        HideLobbyHUD();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerObject.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(playerObject, finalBossScene);
        }

        yield return StartCoroutine(UnloadAllOldGameplayScenes(bossSceneName));

        gameplaySceneName = bossSceneName;

        FinalBossEncounterDirector director = FindFirstObjectByType<FinalBossEncounterDirector>();
        if (director != null) director.StartEncounter();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMapType(MapType.Boss);
        }

        yield return new WaitForSecondsRealtime(0.2f);

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }

        if (InteractManagerV2.Instance != null) InteractManagerV2.Instance.IsBusy = false;
    }

    private IEnumerator LoadSceneCoroutine()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
        }

        yield return StartCoroutine(UnloadAllOldGameplayScenes());

        string loadingSceneName = GameSceneData.Instance != null
            ? GameSceneData.Instance.GetSceneName(SceneType.Loading)
            : "Loading Scene";

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.05f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(gameplaySceneName);
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        float duration = Random.Range(5.0f, 7.0f);
        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(load, false, duration));
        }

        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;

        Scene runScene = SceneManager.GetSceneByName(gameplaySceneName);
        if (runScene.IsValid())
        {
            SceneManager.SetActiveScene(runScene);
            EventManager.Notify(GameEvent.OnLoadingComplete);
        }

        if (lobbyVisuals != null)
        {
            lobbyVisuals.SetActive(false);
        }

        HideLobbyHUD();

        yield return new WaitForFixedUpdate();

        CharacterController playerController = null;
        CharacterBase charBase = null;
        if (PlayerManager.Instance != null)
        {
            charBase = PlayerManager.Instance.GetComponentInChildren<CharacterBase>();
        }

        if (charBase != null)
        {
            if (runScene.IsValid() && charBase.gameObject.scene != runScene)
            {
                SceneManager.MoveGameObjectToScene(charBase.gameObject, runScene);
            }

            if (!charBase.gameObject.CompareTag("Player"))
            {
                charBase.gameObject.tag = "Player";
            }

            playerController = charBase.CharacterMovement?.CC ?? charBase.GetComponent<CharacterController>();
            if (playerController != null) playerController.enabled = false;
        }

        isRunActive = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMapType(MapType.Run);
        }

        yield return new WaitForSecondsRealtime(0.2f);

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }

        if (playerController != null) playerController.enabled = true;

        Physics.SyncTransforms();
    }

    public void ReturnToLobby()
    {
        EventManager.Notify(GameEvent.OnReturnToLobby);
        StartCoroutine(UnloadSceneCoroutine());
    }

    private IEnumerator UnloadSceneCoroutine()
    {
        isRunActive = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
        }

        string loadingSceneName = GameSceneData.Instance != null
            ? GameSceneData.Instance.GetSceneName(SceneType.Loading)
            : "Loading Scene";

        string targetLobbyScene = GameSceneData.Instance != null
            ? GameSceneData.Instance.GetSceneName(SceneType.LobbyMain)
            : "LobbyMain Scene";

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.05f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(targetLobbyScene);
        }

        yield return StartCoroutine(UnloadAllOldGameplayScenes());

        if (RunGameplayController.Instance != null)
        {
            Destroy(RunGameplayController.Instance.gameObject);
        }

        float duration = Random.Range(5.0f, 7.0f);
        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(null, false, duration));
        }

        Scene lobbyScene = SceneManager.GetSceneByName(targetLobbyScene);
        if (lobbyScene.IsValid())
        {
            SceneManager.SetActiveScene(lobbyScene);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMapType(MapType.Lobby);
        }

        if (lobbyVisuals != null)
        {
            lobbyVisuals.SetActive(true);
        }

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SpawnCharacterInLobby();
        }

        if (GoldManager.Instance != null) GoldManager.Instance.ResetGold();

        if (lobbyHUDMainObject != null)
        {
            lobbyHUDMainObject.SetActive(true);
        }
        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.gameObject.SetActive(true);
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }

        if (GemManager.Instance != null && pendingGem > 0) GemManager.Instance.AddGem(pendingGem);
        if (AccountLevelManager.Instance != null && pendingExp > 0) AccountLevelManager.Instance.AddExp(pendingExp);
        if (RuneShardManager.Instance != null && pendingShards > 0) RuneShardManager.Instance.AddShards(pendingShards);

        pendingGem = 0; pendingExp = 0; pendingShards = 0;

        if (LeaderboardManager.Instance != null) LeaderboardManager.Instance.UpdateAllStatistics();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
        }

        gameplaySceneName = "";

        yield return new WaitForSecondsRealtime(0.15f);

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.ForceUnlockState();
        }
    }

    private IEnumerator UnloadAllOldGameplayScenes(string keepSceneName = "")
    {
        string lobbySceneName = GameSceneData.Instance != null ? GameSceneData.Instance.GetSceneName(SceneType.LobbyMain) : "LobbyMain Scene";
        string loadingSceneName = GameSceneData.Instance != null ? GameSceneData.Instance.GetSceneName(SceneType.Loading) : "Loading Scene";

        List<Scene> scenesToClean = new List<Scene>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != lobbySceneName && s.name != loadingSceneName && s.name != keepSceneName)
            {
                scenesToClean.Add(s);
            }
        }

        foreach (Scene s in scenesToClean)
        {
            if (s.IsValid() && s.isLoaded)
            {
                AsyncOperation un = SceneManager.UnloadSceneAsync(s);
                while (un != null && !un.isDone) yield return null;
            }
        }
    }
}
