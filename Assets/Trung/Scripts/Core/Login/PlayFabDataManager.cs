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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ Manager này sống xuyên suốt game để dùng cho các lần chuyển map sau
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // KHÓA BẢO VỆ: Chỉ kéo mây nếu chúng ta thực sự đi lên từ màn hình Đăng Nhập đầu game
        // Nếu không, đây chỉ là màn hình chuyển cảnh Loading Additive giữa các ải sảnh/phụ bản!
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName == "Login Scene")
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

    // Trường hợp 1: Tài khoản hoàn toàn mới chưa có dữ liệu trên mây
    if (result.Data == null || !result.Data.ContainsKey("PlayerData"))
    {
        Debug.Log("[CLOUD] Tài khoản mới tinh trên Cloud. Thiết lập data mặc định...");
        
        SaveLoadManager.Instance.SaveData = new GameSaveData();
        SaveLoadManager.Instance.SaveData.isTutorialCompleted = false; 
        SaveLoadManager.Instance.SaveGame();

        StartCoroutine(SwitchSceneRoutine("Tutorial Scene")); 
        return;
    }

    // Trường hợp 2: Có dữ liệu trên mây
    string json = result.Data["PlayerData"].Value;
    Debug.Log($"[CLOUD] Dữ liệu JSON tải về từ Cloud: {json}");

    if (string.IsNullOrEmpty(json))
    {
        // Phòng hờ chuỗi trống
        SaveLoadManager.Instance.SaveData = new GameSaveData();
        SaveLoadManager.Instance.SaveData.isTutorialCompleted = false;
        SaveLoadManager.Instance.SaveGame();
        StartCoroutine(SwitchSceneRoutine("Tutorial Scene"));
        return;
    }

    GameSaveData cloudData = JsonUtility.FromJson<GameSaveData>(json);
    if (cloudData == null)
    {
        Debug.LogError("[CLOUD] Không thể giải mã JSON từ Cloud thành GameSaveData!");
        return;
    }

    // ĐỒNG BỘ DỮ LIỆU: Đảm bảo dữ liệu local nhận chính xác dữ liệu mây
    SaveLoadManager.Instance.SaveData = cloudData;
    SaveLoadManager.Instance.SaveGame(); // Lưu ngay xuống thiết bị cục bộ

    Debug.Log($"[CLOUD] Đồng bộ thành công! Trạng thái isTutorialCompleted hiện tại là: {SaveLoadManager.Instance.SaveData.isTutorialCompleted}");

    // CHUYỂN SCENE DỰA TRÊN TRẠNG THÁI ĐÃ ĐỒNG BỘ
    if (!SaveLoadManager.Instance.SaveData.isTutorialCompleted)
    {
        Debug.Log("[CLOUD] Tài khoản chưa hoàn thành Tutorial -> Chuyển hướng tới Tutorial Scene");
        StartCoroutine(SwitchSceneRoutine("Tutorial Scene"));
    }
    else
    {
        Debug.Log("[CLOUD] Tài khoản đã hoàn thành Tutorial -> Chuyển hướng tới LobbyMain Scene");
        StartCoroutine(SwitchSceneRoutine("LobbyMain Scene"));
    }
}
    #endregion

    private System.Collections.IEnumerator SwitchSceneRoutine(string targetSceneName)
    {
        Debug.Log($"[PlayFabDataManager] Bắt đầu luồng chuyển cảnh mượt mà tới: {targetSceneName}");

        // Bước 1: Nạp "Loading Scene" ở chế độ ADDITIVE để che mắt người chơi trước
        // (Lúc này Login Scene vẫn đang mở ở dưới nền)
        AsyncOperation loadLoading = SceneManager.LoadSceneAsync("Loading Scene", LoadSceneMode.Additive);
        while (!loadLoading.isDone)
        {
            yield return null;
        }

        // Chờ 1 nhịp rất ngắn để UI của Loading Scene (bao gồm LoadingUIManager) kịp khởi tạo Instance
        yield return new WaitForSeconds(0.1f);

        // Bước 2: Thiết lập thông tin bản đồ đích lên UI Loading
        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(targetSceneName);
        }

        // Bước 3: Nạp Scene đích (Lobby hoặc Tutorial) ở chế độ SINGLE
        // Chế độ SINGLE sẽ tự động dọn dẹp sạch sẽ Login Scene cũ để giải phóng RAM
        AsyncOperation loadTarget = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        
        // Ngăn không cho Scene đích tự kích hoạt ngay lập tức để Slider kịp chạy từ 0% -> 100%
        loadTarget.allowSceneActivation = false;

        // Cho LoadingUIManager theo dõi tiến độ nạp thực tế
        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadTarget));
        }
        else
        {
            // Nếu không tìm thấy UI Manager (đề phòng lỗi kéo thả), tự động đợi nạp xong
            while (loadTarget.progress < 0.9f)
            {
                yield return null;
            }
        }

        // Bước 4: Kích hoạt Scene đích thực sự hoạt động
        loadTarget.allowSceneActivation = true;
        while (!loadTarget.isDone)
        {
            yield return null;
        }

        Debug.Log($"[PlayFabDataManager] Đã nạp thành công Scene đích: {targetSceneName}");

        // Chờ thêm 0.2 giây để các hệ thống trong Scene đích (như UIManager, Player) chạy xong hàm Start/Awake của họ
        yield return new WaitForSeconds(0.2f);

        // Bước 5: Giải phóng (Unload) màn hình Loading Scene (Additive) để hiển thị sảnh chính sòng phẳng
        Scene loadingScene = SceneManager.GetSceneByName("Loading Scene");
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone)
            {
                yield return null;
            }
            Debug.Log("[PlayFabDataManager] Đã giải phóng Loading Scene thành công. Sảnh chính đã sẵn sàng!");
        }
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