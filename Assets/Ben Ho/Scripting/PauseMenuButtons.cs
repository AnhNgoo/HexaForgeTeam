using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuButtons : MonoBehaviour
{
    public void ContinueGame()
{
    Time.timeScale = 1f;

    SceneManager.LoadScene("UI Gameplay");
}

    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void NewGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettingFromPauseMenu()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene("UI PauseMenu", LoadSceneMode.Additive);
        SettingReturnData.BackSceneName = "UI PauseMenu";
        SettingReturnData.OpenPauseMenuAfterBack = false;

        SceneManager.LoadScene("UI Setting");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
        SceneManager.LoadScene("UI Menu");
    }
}