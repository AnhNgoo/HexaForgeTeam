using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [Header("Scene Config")]
    [SerializeField] private string gameplaySceneName = "Run Scene";

    [Header("Lobby Spawn Settings")]
    [SerializeField] private Transform lobbySpawnPoint;
    [SerializeField] private GameObject lobbyVisuals;

    [Header("HUD Controller")]
    [SerializeField] private GameObject lobbyHUDMainObject;

    [Header("Run Damage Statistics")]
    private float totalDamageDealt;

    private int pendingGem;
    private int pendingExp;
    private int pendingShards;

    private bool isRunActive = false;

    [SerializeField] private PoolType selectedFinalBossPool = PoolType.EnemyEarthshakerBoss;

    public PoolType SelectedFinalBossPool => selectedFinalBossPool;

    public void ConfigureRun(string sceneName, PoolType finalBossPool)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
            gameplaySceneName = sceneName;

        if (finalBossPool != PoolType.None)
            selectedFinalBossPool = finalBossPool;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public string GetGameplaySceneName() => gameplaySceneName;

    /// <summary>
    /// Hàm gọi từ Player để tích lũy Sát thương tổng đã gây ra
    /// </summary>
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

        ResetDamageData(); // Reset Damage khi bắt đầu Run mới

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

    public void EnterFinalBoss(string sceneName)
    {
        if (!isRunActive || string.IsNullOrWhiteSpace(sceneName))
            return;

        StartCoroutine(EnterFinalBossCoroutine(sceneName));
    }

    private IEnumerator EnterFinalBossCoroutine(string sceneName)
    {
        if (InteractManagerV2.Instance != null)
            InteractManagerV2.Instance.IsBusy = true;

        SafeZoneManager.Instance?.StopForFinalBoss();

        Scene previousRunScene = SceneManager.GetSceneByName(gameplaySceneName);

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        while (!load.isDone)
            yield return null;

        Scene finalBossScene = SceneManager.GetSceneByName(sceneName);

        if (finalBossScene.IsValid()) SceneManager.SetActiveScene(finalBossScene);

        gameplaySceneName = sceneName;

        if (previousRunScene.isLoaded)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(previousRunScene);

            while (unload != null && !unload.isDone)
                yield return null;
        }

        if (InteractManagerV2.Instance != null)
            InteractManagerV2.Instance.IsBusy = false;
    }

    private IEnumerator LoadSceneCoroutine()
    {
        AsyncOperation loadLoading = SceneManager.LoadSceneAsync("Loading Scene", LoadSceneMode.Additive);
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
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(load));
        }

        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;

        Scene runScene = SceneManager.GetSceneByName(gameplaySceneName);
        if (runScene.IsValid())
        {
            SceneManager.SetActiveScene(runScene);
            Debug.Log($"<color=cyan>[RunManager] Active Scene FORCED to: {runScene.name}</color>");
        }

        if (lobbyVisuals != null)
        {
            lobbyVisuals.SetActive(false);
        }

        yield return new WaitForFixedUpdate();

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
                try
                {
                    charBase.gameObject.tag = "Player";
                }
                catch (UnityException)
                {
                    Debug.LogWarning("[RunManager] Hãy tạo Tag 'Player' trong Unity Editor!");
                }
            }

            CharacterController cc = charBase.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Physics.SyncTransforms();
        }

        isRunActive = true;

        yield return new WaitForSeconds(0.4f);

        Scene loadingScene = SceneManager.GetSceneByName("Loading Scene");
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }
    }

    public void ReturnToLobby()
    {
        StartCoroutine(UnloadSceneCoroutine());
    }

    private IEnumerator UnloadSceneCoroutine()
    {
        isRunActive = false;
        Time.timeScale = 1f;

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync("Loading Scene", LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName("LobbyMain Scene");
        }

        Scene runScene = SceneManager.GetSceneByName(gameplaySceneName);
        AsyncOperation unloadRun = null;
        if (runScene.isLoaded)
        {
            unloadRun = SceneManager.UnloadSceneAsync(gameplaySceneName);
        }

        if (LoadingUIManager.Instance != null)
        {
            if (unloadRun != null)
            {
                yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(unloadRun));
            }
            else
            {
                float dummyDuration = Random.Range(2f, 3f);
                float dummyElapsed = 0f;
                while (dummyElapsed < dummyDuration)
                {
                    dummyElapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
        }
        else if (unloadRun != null)
        {
            while (!unloadRun.isDone) yield return null;
        }

        Scene lobbyScene = SceneManager.GetSceneByName("LobbyMain Scene");
        if (lobbyScene.IsValid())
        {
            SceneManager.SetActiveScene(lobbyScene);
        }

        if (lobbyVisuals != null)
        {
            lobbyVisuals.SetActive(true);
        }

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SpawnCharacterInLobby();
        }

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.ResetGold();
        }

        if (lobbyHUDMainObject != null)
        {
            lobbyHUDMainObject.SetActive(true);
            if (LobbyHUDTopBar.Instance != null)
            {
                LobbyHUDTopBar.Instance.ShowFullHUD();
            }
        }

        if (GemManager.Instance != null && pendingGem > 0) GemManager.Instance.AddGem(pendingGem);
        if (AccountLevelManager.Instance != null && pendingExp > 0) AccountLevelManager.Instance.AddExp(pendingExp);
        if (RuneShardManager.Instance != null && pendingShards > 0) RuneShardManager.Instance.AddShards(pendingShards);

        pendingGem = 0; pendingExp = 0; pendingShards = 0;
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateAllStatistics();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }

        if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
        if (RuneEquipUI.Instance != null) RuneEquipUI.Instance.RefreshEquipUI();
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        yield return new WaitForSecondsRealtime(0.2f);

        Scene loadingScene = SceneManager.GetSceneByName("Loading Scene");
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }
    }
}