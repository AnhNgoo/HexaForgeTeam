using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

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

    [SerializeField] private PoolType selectedFinalBossPool = PoolType.EnemyEarthshakerBoss;

    public PoolType SelectedFinalBossPool => selectedFinalBossPool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
                ? GameSceneData.Instance.GetSceneName(SceneType.RunGameplay)
                : "Run Scene";
        }

        ResetDamageData();

        // ẨN TRIỆT ĐỂ LOBBY TOPBAR VÀ CHUYỂN MENU
        HideLobbyHUD();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }

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

        // ẨN TRIỆT ĐỂ LOBBY TOPBAR KHI VÀO BOSS
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

    private IEnumerator EnterFinalBossCoroutine(string sceneName)
    {
        if (InteractManagerV2.Instance != null)
            InteractManagerV2.Instance.IsBusy = true;

        SafeZoneManager.Instance?.StopForFinalBoss();

        Scene previousRunScene = SceneManager.GetSceneByName(gameplaySceneName);

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!load.isDone) yield return null;

        Scene finalBossScene = SceneManager.GetSceneByName(sceneName);
        if (!finalBossScene.IsValid() || !finalBossScene.isLoaded)
        {
            if (InteractManagerV2.Instance != null) InteractManagerV2.Instance.IsBusy = false;
            yield break;
        }

        SceneManager.SetActiveScene(finalBossScene);

        // Bảo đảm Lobby HUD không bị kích hoạt lại
        HideLobbyHUD();

        // Chuyển Player sang Scene Boss
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerObject.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(playerObject, finalBossScene);
        }

        // Chuyển RunGameplayController sang Scene Boss
        if (RunGameplayController.Instance != null)
        {
            GameObject runController = RunGameplayController.Instance.gameObject;
            runController.transform.SetParent(null, true);
            if (runController.scene != finalBossScene)
            {
                SceneManager.MoveGameObjectToScene(runController, finalBossScene);
            }
        }

        gameplaySceneName = sceneName;

        // Dỡ bỏ Run Scene cũ
        if (previousRunScene.IsValid() && previousRunScene.isLoaded)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(previousRunScene);
            while (unload != null && !unload.isDone) yield return null;
        }

        FinalBossEncounterDirector director = FindFirstObjectByType<FinalBossEncounterDirector>();
        if (director != null) director.StartEncounter();

        // Cập nhật lại UI Gameplay
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }



        if (InteractManagerV2.Instance != null) InteractManagerV2.Instance.IsBusy = false;
    }

    private IEnumerator LoadSceneCoroutine()
    {
        string loadingSceneName = GameSceneData.Instance != null
            ? GameSceneData.Instance.GetSceneName(SceneType.Loading)
            : "Loading Scene";

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSeconds(0.1f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(gameplaySceneName);
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(load, false));
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

        yield return new WaitForSeconds(0.4f);

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
        StartCoroutine(UnloadSceneCoroutine());
    }

    private IEnumerator UnloadSceneCoroutine()
    {
        isRunActive = false;
        Time.timeScale = 1f;

        string loadingSceneName = GameSceneData.Instance != null
            ? GameSceneData.Instance.GetSceneName(SceneType.Loading)
            : "Loading Scene";

        string targetLobbyScene = GameSceneData.Instance != null
            ? GameSceneData.Instance.GetSceneName(SceneType.LobbyMain)
            : "LobbyMain Scene";

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(targetLobbyScene);
        }

        Scene runScene = SceneManager.GetSceneByName(gameplaySceneName);
        AsyncOperation unloadRun = null;
        if (runScene.IsValid() && runScene.isLoaded)
        {
            unloadRun = SceneManager.UnloadSceneAsync(gameplaySceneName);
        }

        if (LoadingUIManager.Instance != null && unloadRun != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(unloadRun));
        }
        else if (unloadRun != null)
        {
            while (!unloadRun.isDone) yield return null;
        }

        Scene lobbyScene = SceneManager.GetSceneByName(targetLobbyScene);
        if (lobbyScene.IsValid())
        {
            SceneManager.SetActiveScene(lobbyScene);
        }

        if (lobbyVisuals != null)
        {
            lobbyVisuals.SetActive(true);
        }

        // if (PlayerManager.Instance != null)
        // {
        //     PlayerManager.Instance.SpawnCharacterInLobby();
        // }

        if (GoldManager.Instance != null) GoldManager.Instance.ResetGold();

        // BẬT LẠI LOBBY HUD KHI VỀ SẢNH
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

        if (UIManager.Instance != null) UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }

        gameplaySceneName = "";

        yield return new WaitForSecondsRealtime(0.2f);

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }

    }
}