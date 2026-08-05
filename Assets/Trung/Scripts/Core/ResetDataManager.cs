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
            
            SaveLoadManager.Instance.SaveData = new GameSaveData();
            SaveLoadManager.Instance.SaveData.isTutorialCompleted = false;
            SaveLoadManager.Instance.SaveGame();
        }

        if (PlayerPrefs.HasKey("DisplayName"))
        {
            PlayerPrefs.DeleteKey("DisplayName");
        }

        if (PlayerPrefs.HasKey("SelectedCharacter"))
        {
            PlayerPrefs.DeleteKey("SelectedCharacter");
        }

        PlayerPrefs.Save();

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdatePowerScore(); 
        }

        if (PlayFabDataManager.Instance != null)
        {
            Debug.Log("[Reset Data] Đang cưỡng chế đồng bộ xóa sạch dữ liệu lên Cloud PlayFab...");
            PlayFabDataManager.Instance.SaveCloud();
        }

        Debug.Log("All Data Reset thành công!");

        UnityEngine.SceneManagement.SceneManager.LoadScene("Login Scene");
    }
}