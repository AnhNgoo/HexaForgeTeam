using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [Header("Scene Config")]
    [SerializeField] private string gameplaySceneName = "Run Scene"; 

    [Header("Lobby Spawn Settings")]
    [SerializeField] private Transform playerTransform; 
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
        }

        if (lobbyVisuals != null)
        {
            lobbyVisuals.SetActive(false);
        }

        yield return new WaitForFixedUpdate();

        GameObject spawnPoint = GameObject.FindWithTag("RunSpawnPoint");
        if (spawnPoint == null)
        {
            spawnPoint = GameObject.Find("RunSpawnPoint");
        }

        if (spawnPoint != null && playerTransform != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerTransform.position = spawnPoint.transform.position;
            playerTransform.rotation = spawnPoint.transform.rotation;

            yield return null;
            if (cc != null) cc.enabled = true;
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
                float dummyDuration = Random.Range(5f, 6f);
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

        if (lobbySpawnPoint != null && playerTransform != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerTransform.position = lobbySpawnPoint.position;
            playerTransform.rotation = lobbySpawnPoint.rotation;

            Physics.SyncTransforms();

            yield return null;

            if (cc != null) cc.enabled = true;
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