using UnityEngine;
using Sirenix.OdinInspector;
using PlayFab;

public class ResetDataManager : MonoBehaviour
{
    [Button("Reset All Data")]
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

        PlayerPrefs.SetInt("IsAutoLoginActive", 0);
        PlayerPrefs.DeleteKey("LastAccountUser");
        PlayerPrefs.DeleteKey("LastAccountPass");
        PlayerPrefs.SetInt("UNLOCKED_BOSS_DARKMAGE", 0);

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

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PlayFabClientAPI.ForgetAllCredentials();
            if (PlayFabSettings.staticPlayer != null)
            {
                PlayFabSettings.staticPlayer.PlayFabId = null;
            }
            Debug.Log("[Reset Data] Đã Logout PlayFab thành công!");
        }

        Debug.Log("All Data Reset thành công!");

        string targetLoginScene = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.Login) 
            : "Login Scene";

        UnityEngine.SceneManagement.SceneManager.LoadScene(targetLoginScene);
    }
}