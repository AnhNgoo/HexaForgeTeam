using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUIController : MonoBehaviour
{
    public GameObject pauseMenuPanel;

    private void Start()
    {
        if (SettingReturnData.OpenPauseMenuAfterBack)
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;

            SettingReturnData.OpenPauseMenuAfterBack = false;
        }
    }

     [Header("Scene Name")]
    public string pauseMenuSceneName = "UI Pause Menu";

    public void OpenPauseMenu()
    {
        Time.timeScale = 0f;

        if (!SceneManager.GetSceneByName(pauseMenuSceneName).isLoaded)
        {
            SceneManager.LoadScene(pauseMenuSceneName, LoadSceneMode.Additive);
        }
    }

    public void ClosePauseMenu()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}