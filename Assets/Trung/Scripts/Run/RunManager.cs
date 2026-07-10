using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [Header("Scene Config")]
    [Tooltip("Bạn có thể đổi tên Scene hầm ngục tùy ý ở đây để quản lý sau này")]
    [SerializeField] private string gameplaySceneName = "Run Scene"; // Tên Scene gameplay hầm ngục

    [Header("Lobby Spawn Settings")]
    [SerializeField] private Transform playerTransform; // Kéo thả Player ở sảnh vào đây
    [SerializeField] private Transform lobbySpawnPoint;   // Kéo thả Object vị trí Spawn Point ở sảnh vào đây
    [SerializeField] private GameObject lobbyVisuals;    // Nhóm đồ họa/NPC sảnh để tắt/bật cho nhẹ máy

    // Biến lưu trữ phần thưởng tạm thời chờ mang về sảnh mới cộng thực tế
    private int pendingGem;
    private int pendingExp;
    private int pendingShards; // MỚI: Biến tạm ngậm số Shards mang về từ map chiến đấu

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public string GetGameplaySceneName() => gameplaySceneName;

    // CẬP NHẬT HÀM NHẬN THƯỞNG: Thêm tham số nhận lượng Shards tích lũy
    public void SetPendingRewards(int gem, int exp, int shards)
    {
        pendingGem = gem;
        pendingExp = exp;
        pendingShards = shards; // Ghi nhận mảnh ngọc tạm tính
    }

    // Gọi khi tương tác vào Cổng dịch chuyển
    public void StartRun()
    {
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = true; // Khóa tương tác sảnh không cho bấm bậy
        }

        if (lobbyVisuals != null) lobbyVisuals.SetActive(false); // Ẩn môi trường sảnh

        StartCoroutine(LoadGameplaySceneCoroutine());
    }

    private IEnumerator LoadGameplaySceneCoroutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone) yield return null;
        
        Debug.Log($"[RunManager] Đã tải xong hầm ngục: {gameplaySceneName}");

        // Tìm điểm Spawn Point nằm bên trong Scene Dungeon vừa tải xong và dịch chuyển Player tới đó
        GameObject runSpawnPoint = GameObject.FindWithTag("RunSpawnPoint");
        if (runSpawnPoint != null && playerTransform != null)
        {
            playerTransform.position = runSpawnPoint.transform.position;
            playerTransform.rotation = runSpawnPoint.transform.rotation;
            Debug.Log("[RunManager] Đã dịch chuyển Player tới Điểm Xuất Phát của Dungeon!");
        }

        // ================================================================
        // BƯỚC SỬA LỖI: Mở khóa hệ thống tương tác để vào map mới có thể mở rương
        // ================================================================
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false; // Thả xích hệ thống tương tác
            InteractManagerV2.Instance.ForceRefresh(); // Ép hệ thống quét và nhận diện lại cái rương mới
        }
    }

    // Hàm này được gọi từ nút xác nhận ở bảng tổng kết phụ bản để quay xe về nhà
    public void ReturnToLobby()
    {
        StartCoroutine(UnloadGameplaySceneCoroutine());
    }

    private IEnumerator UnloadGameplaySceneCoroutine()
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(gameplaySceneName);
        while (!asyncUnload.isDone) yield return null;

        // 1. Hiện lại toàn bộ môi trường và giao diện sảnh chính
        if (lobbyVisuals != null) lobbyVisuals.SetActive(true);

        // 2. Đưa Player về đúng vị trí Spawn Point sảnh ban đầu tránh bị kẹt
        if (playerTransform != null && lobbySpawnPoint != null)
        {
            playerTransform.position = lobbySpawnPoint.position;
            playerTransform.rotation = lobbySpawnPoint.rotation;
            Debug.Log("[RunManager] Đã đưa Player về vị trí Spawn Point sảnh!");
        }

        // 3. TIẾN HÀNH CỘNG THƯỞNG VÀ ÉP LÀM TƯƠI UI SẢNH CHÍNH
        // Thực hiện cộng tiền thực tế và ép hiển thị nhảy số trên màn hình sảnh
        if (GemManager.Instance != null && pendingGem > 0)
        {
            GemManager.Instance.AddGem(pendingGem);
        }

        // Thực hiện cộng EXP và ép thanh tiến trình Level sảnh chạy/hiện Popup cấp mới
        if (AccountLevelManager.Instance != null && pendingExp > 0)
        {
            AccountLevelManager.Instance.AddExp(pendingExp);
        }

        // MỚI: Cộng Mảnh Cổ Tự thực tế vào ví tài khoản sảnh chính khi dọn sạch map phụ bản
        if (RuneShardManager.Instance != null && pendingShards > 0)
        {
            RuneShardManager.Instance.AddShards(pendingShards);
        }

        // Hoàn tất cộng quà, làm sạch bộ nhớ tạm hoàn toàn
        pendingGem = 0;
        pendingExp = 0;
        pendingShards = 0; // Làm sạch bộ nhớ tạm Mảnh ngọc

        // 4. Giải phóng trạng thái và mở khóa hệ thống tương tác sảnh
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }

        // Làm tươi hòm đồ để cập nhật viên Ngọc mới nhận được (nếu có)
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.RefreshInventory();
        }

        Debug.Log("[RunManager] Đã hoàn tất xử lý đồng bộ và cộng thưởng trực tiếp lên UI sảnh!");
    }
    
}