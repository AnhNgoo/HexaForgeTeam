using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUIManager : MonoBehaviour
{
    public static LoadingUIManager Instance;

    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text destinationText;
    [SerializeField] private TMP_Text loadingTipText;

    [Header("Config")]
    [SerializeField][Range(0.1f, 3f)] private float fillSpeed = 0.8f;

    [Header("Loading Tips")]
    [SerializeField]
    private List<string> loadingTips = new List<string>()
    {
        "Sharpening weapons... Ready for the next battle.",
        "Lyra is memorizing ancient spells.",
        "Kael claims that dodging is easier than blocking. Do not trust him.",
        "Transmuting rune affixes costs a lot of Shards. Think twice!",
        "Every rune dropped from the dungeon has unique powers.",
        "Cleaning up previous monsters' debris to make room for you...",
        "Equipping matching elemental runes unlocks powerful bonuses.",
        "Prepare yourself! The Nightmare Lord awaits ahead!"
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        ShowRandomTip();
    }

    public void SetDestinationName(string sceneName)
    {
        if (destinationText == null) return;

        string formattedName = "Unknown Zone";
        GameSceneData data = GameSceneData.Instance;

        string lobbySceneName = data != null ? data.GetSceneName(SceneType.LobbyMain) : "LobbyMain Scene";
        string runSceneName = data != null ? data.GetSceneName(SceneType.RunGameplay) : "Run Scene";
        string bossSceneName = data != null ? data.GetSceneName(SceneType.FinalBoss) : "FinalBoss Scene";
        string tutorialSceneName = data != null ? data.GetSceneName(SceneType.Tutorial) : "Tutorial Scene";

        if (sceneName == lobbySceneName)
        {
            formattedName = "TRAVELING TO: HEROES' LOBBY";
        }
        else if (sceneName == runSceneName || sceneName.Contains("Run"))
        {
            formattedName = "ENTERING: THE DEEP DUNGEON";
        }
        else if (sceneName == bossSceneName || sceneName.Contains("Boss") || sceneName.Contains("Arena"))
        {
            formattedName = "APPROACHING: NIGHTMARE LORD'S ARENA";
        }
        else if (sceneName == tutorialSceneName)
        {
            formattedName = "ENTERING: TRIAL GROUNDS";
        }
        else
        {
            formattedName = $"TRAVELING TO: {sceneName.Replace(" Scene", "").Replace("Game", "").ToUpper()}";
        }

        destinationText.SetTextSafe(formattedName);
    }

    public void ShowRandomTip()
    {
        if (loadingTipText == null || loadingTips == null || loadingTips.Count == 0) return;

        int randomIndex = Random.Range(0, loadingTips.Count);
        loadingTipText.SetTextSafe($"<color=#FFA500>TIP:</color> {loadingTips[randomIndex]}");
    }

    /// <summary>
    /// Giữ màn hình loading trong khoảng 5-7 giây mượt mà
    /// </summary>
    public IEnumerator TrackProgressRoutine(AsyncOperation targetSceneLoad, bool hasEventNotify = true, float minDuration = 6.0f)
    {
        LoadTrace.Mark($"TrackProgressRoutine started | " + $"MinDuration={minDuration:F2}s | " + $"OperationNull={targetSceneLoad == null}");
        if (progressSlider == null) yield break;

        float targetDuration = (minDuration <= 0f) ? Random.Range(5.0f, 7.0f) : minDuration;
        float elapsedTime = 0f;
        progressSlider.value = 0f;

        while (elapsedTime < targetDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float timeRatio = Mathf.Clamp01(elapsedTime / targetDuration);

            float targetProgress = 1f;
            if (targetSceneLoad != null)
            {
                targetProgress = Mathf.Clamp01(targetSceneLoad.progress / 0.9f);
            }

            float visualProgress = Mathf.Min(timeRatio, targetProgress);
            progressSlider.value = Mathf.MoveTowards(progressSlider.value, visualProgress, Time.unscaledDeltaTime * fillSpeed);

            yield return null;
        }

        LoadTrace.Mark($"Visual minimum duration completed | " + $"SceneProgress={targetSceneLoad?.progress ?? 1f:F2}");

        while (progressSlider.value < 1f)
        {
            progressSlider.value = Mathf.MoveTowards(progressSlider.value, 1f, Time.unscaledDeltaTime * fillSpeed);
            yield return null;
        }

        LoadTrace.Mark("Loading slider reached 100%");

        yield return new WaitForSecondsRealtime(0.3f);

        if (hasEventNotify)
        {
            EventManager.Notify(GameEvent.OnLoadingComplete);
        }
    }
}