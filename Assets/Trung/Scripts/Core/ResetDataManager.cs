using UnityEngine;

public class ResetDataManager : MonoBehaviour
{
    public void ResetAllData()
    {
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.ResetCharacterData(); 
        }

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.DeleteSave(); 
            
            // Khởi tạo lại cấu trúc data mới tinh cho tài khoản
            SaveLoadManager.Instance.SaveData = new GameSaveData();
            SaveLoadManager.Instance.SaveData.isTutorialCompleted = false;
            SaveLoadManager.Instance.SaveGame();
        }

        // BẢN VÁ MỚI: Xóa sạch cả tên hiển thị cũ kẹt trong PlayerPrefs tránh lệch pha tài khoản
        if (PlayerPrefs.HasKey("DisplayName"))
        {
            PlayerPrefs.DeleteKey("DisplayName");
            PlayerPrefs.Save();
        }

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdatePowerScore(); 
        }

        // Ép lưu đồng bộ dữ liệu rỗng này lên đám mây lập tức trước khi sút người chơi về màn hình đăng nhập
        if (PlayFabDataManager.Instance != null)
        {
            Debug.Log("[Reset Data] Đang cưỡng chế đồng bộ xóa sạch dữ liệu lên Cloud PlayFab...");
            PlayFabDataManager.Instance.SaveCloud();
        }

        Debug.Log("All Data Reset thành công!");

        // Đưa người chơi quay xe về màn hình Login Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login Scene");
    }
}