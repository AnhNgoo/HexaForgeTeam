using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public string loadingSceneName = "UI Loading";

    public void LoadSceneThroughLoading(string targetSceneName)
    {
        Time.timeScale = 1f;

        LoadingData.TargetSceneName = targetSceneName;
        SceneManager.LoadScene(loadingSceneName);
    }
}