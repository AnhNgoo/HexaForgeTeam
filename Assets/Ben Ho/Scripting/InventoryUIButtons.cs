using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryUIButtons : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "UI Gameplay";
    [SerializeField] private string settingSceneName = "UI Setting";
    public void BackToGameplay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSetting()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(settingSceneName);
    }
}
