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
    [SerializeField][Range(0.1f, 2f)] private float fillSpeed = 1.5f; // Tốc độ trượt Slider giả lập

    [Header("Loading Tips (English - No Font Errors)")]
    [SerializeField]
    private List<string> loadingTips = new List<string>()
    {
        "Sharpening Ares's axe... Hopefully it does not break this time.",
        "Lyra is memorizing three more ancient spells.",
        "Kael claims that dodging is easier than blocking. Do not trust him.",
        "The ultimate relic contains +99,999 to all stats... Have you found it yet?",
        "Transmuting rune affixes (Reroll) costs a lot of Shards. Think twice!",
        "If you lose the battle, it is definitely your keyboard's fault.",
        "Every rune dropped from the dungeon has a soul. Do not dismantle them recklessly.",
        "Cleaning up previous monsters' debris to make room for you...",
        "Did you know? Equipping matching elemental runes unlocks powerful bonuses.",
        "Origin of Creation is the legendary rune that proves your absolute conquest.",
        "The dungeon portals are unstable. Enter at your own risk!",
        "Dismantling high-rarity runes refunds both Gems and Rune Shards."
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
        string tutorialSceneName = data != null ? data.GetSceneName(SceneType.Tutorial) : "Tutorial Scene";

        if (sceneName == lobbySceneName)
        {
            formattedName = "TRAVELING TO: HEROES' LOBBY";
        }
        else if (sceneName == runSceneName)
        {
            formattedName = "ENTERING: THE DEEP DUNGEON";
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
    /// Coroutine running smooth slider simulation and forcing a minimum load time of 5-7 seconds
    /// </summary>
    public IEnumerator TrackProgressRoutine(AsyncOperation targetSceneLoad, bool hasEventNotify = true)
    {
        if (progressSlider == null || targetSceneLoad == null) yield break;

        // Ép buộc thời gian chờ ngẫu nhiên từ 5 đến 7 giây thực tế
        float minLoadingDuration = Random.Range(5f, 7f);
        float elapsedTime = 0f;

        progressSlider.value = 0f;

        // Sử dụng thang thời gian Unscaled (bất chấp Time.timeScale = 0 khi Pause game)
        while (elapsedTime < minLoadingDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            // Tính toán % tiến trình dựa trên thời gian trôi qua thực tế so với mốc thời gian tối thiểu
            float timeRatio = Mathf.Clamp01(elapsedTime / minLoadingDuration);

            // Tiến trình thực tế của Unity (nạp ngầm đạt tối đa 0.9f trước khi kích hoạt scene)
            float unityProgress = Mathf.Clamp01(targetSceneLoad.progress / 0.9f);

            // Giá trị hiển thị lên Slider sẽ là giá trị nhỏ nhất giữa Tiến trình thời gian và Tiến trình nạp của Unity.
            // Điều này đảm bảo: Slider chỉ đạt 100% khi CẢ HAI điều kiện (Đủ 5-7s VÀ Unity nạp xong) đều thỏa mãn!
            float finalVisualProgress = Mathf.Min(timeRatio, unityProgress);

            // Nội suy tuyến tính để Slider trượt êm mượt nhất có thể
            progressSlider.value = Mathf.MoveTowards(progressSlider.value, finalVisualProgress, Time.unscaledDeltaTime * fillSpeed);

            yield return null;
        }

        // Đảm bảo slider chạm mốc 100% hoàn hảo ở cuối hành trình
        while (progressSlider.value < 1f)
        {
            progressSlider.value = Mathf.MoveTowards(progressSlider.value, 1f, Time.unscaledDeltaTime * fillSpeed);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.2f); // Chờ thêm một chút ngắn để người chơi cảm nhận sự hoàn tất mượt mà

        if (hasEventNotify)
            EventManager.Notify(GameEvent.OnLoadingComplete);
    }
}