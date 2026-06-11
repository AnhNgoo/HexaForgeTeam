using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string fallbackSceneName = "GameScene";

    [Header("Loading Time")]
    [SerializeField] private float minimumLoadingTime = 2f;

    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;

    private IEnumerator Start()
    {
        string targetScene = string.IsNullOrEmpty(LoadingData.TargetSceneName)
            ? fallbackSceneName
            : LoadingData.TargetSceneName;

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);

        if (operation == null)
        {
            Debug.LogError("Không load được scene: " + targetScene);
            yield break;
        }

        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            timer += Time.unscaledDeltaTime;

            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(timer / minimumLoadingTime);
            float displayProgress = Mathf.Min(realProgress, timeProgress);

            UpdateUI(displayProgress);

            if (operation.progress >= 0.9f && timer >= minimumLoadingTime)
            {
                UpdateUI(1f);
                yield return new WaitForSecondsRealtime(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private void UpdateUI(float progress)
    {
        if (fillImage != null)
            fillImage.fillAmount = progress;

        if (progressSlider != null)
            progressSlider.value = progress;

        if (progressText != null)
            progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }
}