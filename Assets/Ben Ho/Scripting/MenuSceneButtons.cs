using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneButtons : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string loadingSceneName = "UI Loading";
    [SerializeField] private string gameplaySceneName = "UI Gameplay";
    [SerializeField] private string settingSceneName = "UI Setting";
    [SerializeField] private string languageSceneName = "UI Language";
    [SerializeField] private string trophySceneName = "UI Trophy";
    [SerializeField] private string helpSceneName = "UI Help";
    [SerializeField] private string creditsSceneName = "UI Credits";
    [SerializeField] private string menuSceneName = "UI Menu";

    public void PlayGame()
    {
        LoadThroughLoading(gameplaySceneName);
    }

    public void OpenSettingFromMainMenu()
    {
        SettingReturnData.BackSceneName = "UI Menu";
        SettingReturnData.OpenPauseMenuAfterBack = false;

        SceneManager.LoadScene("UI Setting");
    }

    public void OpenLanguage()
    {
        LoadDirect(languageSceneName);
    }

    public void OpenTrophy()
    {
        LoadDirect(trophySceneName);
    }

    public void OpenCredits()
    {
        LoadDirect(creditsSceneName);
    }

    public void OpenHelp()
    {
        LoadDirect(helpSceneName);
    }

    public void BackToMenu()
    {
        LoadDirect(menuSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }

    public void LoadDirect(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadThroughLoading(string targetSceneName)
    {
        Time.timeScale = 1f;
        LoadingData.TargetSceneName = targetSceneName;
        SceneManager.LoadScene(loadingSceneName);
    }
}