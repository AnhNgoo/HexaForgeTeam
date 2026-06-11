using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUIButtons : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string inventorySceneName = "UI Inventory";
    [SerializeField] private string gameplaySceneName = "UI Gameplay";
    [SerializeField] private string pauseMenuSceneName = "UI PauseMenu";

    public void OpenInventory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(inventorySceneName);
    }

    public void BackToGameplay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenPauseMenu()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(pauseMenuSceneName);
    }
}