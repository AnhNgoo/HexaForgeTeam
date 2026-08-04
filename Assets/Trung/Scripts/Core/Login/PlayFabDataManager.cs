using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabDataManager : MonoBehaviour
{
    public static PlayFabDataManager Instance;
    private bool needSaveCloud = false;
    private float saveTimer = 0f;
    private bool isSwitchingScene = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        string loginSceneName = GameSceneData.Instance != null ? GameSceneData.Instance.loginScene : "Login Scene";

        if (activeSceneName == loginSceneName)
        {
            Debug.Log("[PlayFabDataManager] Loading Scene mở Additive từ Login. Kích hoạt LoadCloud...");
            LoadCloud();
        }
        else
        {
            Debug.Log($"[PlayFabDataManager] Loading Scene mở Additive từ [{activeSceneName}]. Hoạt động ở chế độ chuyển cảnh thuần túy.");
        }
    }

    #region SAVE CLOUD
    public void SaveCloud()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("SaveLoadManager not found");
            return;
        }

        string json = JsonUtility.ToJson(SaveLoadManager.Instance.SaveData);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerData", json } }
        };

        PlayFabClientAPI.UpdateUserData(request, OnSaveSuccess, OnPlayFabError);
    }

    private void OnSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Cloud Save Success");
    }
    #endregion

    #region LOAD CLOUD
    public void LoadCloud()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnLoadSuccess, OnPlayFabError);
    }

    private void OnLoadSuccess(GetUserDataResult result)
    {
        Debug.Log("[PlayFabDataManager] Cloud Load Success!");

        if (SaveLoadManager.Instance == null)
        {
            SaveLoadManager.Instance = FindFirstObjectByType<SaveLoadManager>();
        }

        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("[PlayFabDataManager] KHÔNG THỂ tìm thấy SaveLoadManager.Instance trong Scene!");
            return;
        }

        string tutorialSceneName = GameSceneData.Instance != null ? GameSceneData.Instance.tutorialScene : "Tutorial Scene";
        string lobbySceneName = GameSceneData.Instance != null ? GameSceneData.Instance.lobbyMainScene : "LobbyMain Scene";

        if (result.Data == null || !result.Data.ContainsKey("PlayerData"))
        {
            Debug.Log("[CLOUD] Tài khoản mới tinh trên Cloud. Thiết lập data mặc định...");

            SaveLoadManager.Instance.SaveData = new GameSaveData();
            SaveLoadManager.Instance.SaveData.isTutorialCompleted = false;
            SaveLoadManager.Instance.SaveGame();

            StartCoroutine(SwitchSceneRoutine(tutorialSceneName));
            return;
        }

        string json = result.Data["PlayerData"].Value;
        Debug.Log($"[CLOUD] Dữ liệu JSON tải về từ Cloud: {json}");

        if (string.IsNullOrEmpty(json))
        {
            SaveLoadManager.Instance.SaveData = new GameSaveData();
            SaveLoadManager.Instance.SaveData.isTutorialCompleted = false;
            SaveLoadManager.Instance.SaveGame();
            StartCoroutine(SwitchSceneRoutine(tutorialSceneName));
            return;
        }

        GameSaveData cloudData = JsonUtility.FromJson<GameSaveData>(json);
        if (cloudData == null)
        {
            Debug.LogError("[CLOUD] Không thể giải mã JSON từ Cloud thành GameSaveData!");
            return;
        }

        SaveLoadManager.Instance.SaveData = cloudData;
        SaveLoadManager.Instance.SaveGame();

        Debug.Log($"[CLOUD] Đồng bộ thành công! Trạng thái isTutorialCompleted hiện tại là: {SaveLoadManager.Instance.SaveData.isTutorialCompleted}");

        if (!SaveLoadManager.Instance.SaveData.isTutorialCompleted)
        {
            Debug.Log($"[CLOUD] Tài khoản chưa hoàn thành Tutorial -> Chuyển hướng tới {tutorialSceneName}");
            StartCoroutine(SwitchSceneRoutine(tutorialSceneName));
        }
        else
        {
            Debug.Log($"[CLOUD] Tài khoản đã hoàn thành Tutorial -> Chuyển hướng tới {lobbySceneName}");
            StartCoroutine(SwitchSceneRoutine(lobbySceneName));
        }
    }
    #endregion

    private System.Collections.IEnumerator SwitchSceneRoutine(string targetSceneName)
    {
        if (isSwitchingScene) yield break;
        isSwitchingScene = true;

        string loadingSceneName = GameSceneData.Instance != null ? GameSceneData.Instance.loadingScene : "Loading Scene";
        string lobbySceneName = GameSceneData.Instance != null ? GameSceneData.Instance.lobbyMainScene : "LobbyMain Scene";

        Debug.Log($"[PlayFabDataManager] Bắt đầu luồng chuyển cảnh mượt mà tới: {targetSceneName}");

        Scene existingLoadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (!existingLoadingScene.isLoaded)
        {
            AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
            while (!loadLoading.isDone) yield return null;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(targetSceneName);
        }

        AsyncOperation loadTarget = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        loadTarget.allowSceneActivation = false;

        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadTarget));
        }
        else
        {
            while (loadTarget.progress < 0.9f) yield return null;
        }

        loadTarget.allowSceneActivation = true;
        while (!loadTarget.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.2f);

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }

        if (targetSceneName == lobbySceneName && UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
        }

        isSwitchingScene = false;
    }

    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void MarkDirty()
    {
        needSaveCloud = true;
    }

    private void Update()
    {
        if (!needSaveCloud) return;

        saveTimer += Time.deltaTime;
        if (saveTimer >= 5f)
        {
            saveTimer = 0f;
            needSaveCloud = false;
            SaveCloud();
        }
    }

    private void OnApplicationQuit()
    {
        if (needSaveCloud)
        {
            SaveCloud();
        }
    }
}