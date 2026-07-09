using UnityEngine;

public class PortalRunInteract : MonoBehaviour
{
    // Hàm này sẽ tự động được gọi nhờ lệnh SendMessage("OnInteract") từ file InteractV2.cs của bạn!
    public void OnInteract()
    {
        // 1. Kiểm tra xem hệ thống quản lý Run có tồn tại không
        if (RunManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy RunManager trong Scene sảnh!");
            return;
        }

        // 2. Không cần mở Dialogue rườm rà, kích hoạt thẳng tính năng Start Run luôn!
        RunManager.Instance.StartRun();
    }
}