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

    private int pendingGem;
    private int pendingExp;
    private int pendingShards;

    private bool isRunActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public string GetGameplaySceneName() => gameplaySceneName;

    public void SetPendingRewards(int gem, int exp, int shards)
    {
        pendingGem = gem;
        pendingExp = exp;
        pendingShards = shards;
    }

    public void StartRun()
    {
        if (InteractManagerV2.Instance != null && InteractManagerV2.Instance.IsBusy) return;

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

    private IEnumerator LoadSceneCoroutine()
    {
        // 1. Mở Scene Loading
        AsyncOperation loadLoading = SceneManager.LoadSceneAsync("Loading Scene", LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSeconds(0.1f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(gameplaySceneName);
        }

        // 2. Load Scene Gameplay
        AsyncOperation load = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(load));
        }

        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;

        // --- FIX CỨNG: ÉP RUN SCENE THÀNH ACTIVE SCENE TRƯỚC KHI BẤT KỲ SPAWNER NÀO CHẠY ---
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

        // 3. Chuyển Player sang hẳn Run Scene & Chuẩn hóa Tag "Player"
        CharacterBase charBase = null;
        if (PlayerManager.Instance != null)
        {
            charBase = PlayerManager.Instance.GetComponentInChildren<CharacterBase>();
        }

        if (charBase != null)
        {
            // Ép Player thuộc về Run Scene
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

        // 4. Tắt Scene Loading
        Scene loadingScene = SceneManager.GetSceneByName("Loading Scene");
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }
        EventManager.Notify(GameEvent.OnLoadingComplete);
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

        // Tự động Spawn lại Player tại Sảnh khi kết thúc Run
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.SpawnCharacterInLobby();
            EventManager.Notify(GameEvent.OnLoadingComplete);
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