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
    [SerializeField] private RectTransform handleTransform;
    [SerializeField] private TMP_Text destinationText;
    [SerializeField] private TMP_Text loadingTipText;

    [Header("Config")]
    [SerializeField][Range(0.1f, 3f)] private float fillSpeed = 0.8f;
    [SerializeField] private float handleRotationSpeed = 360f;

    [Header("Loading Tips Database")]
    [SerializeField]
    private List<string> loadingTips = new List<string>()
    {
        "Sharpening weapons... Steel meets darkness in the trials ahead.",
        "Lyra is weaving ancient Arcane glyphs. Do not interrupt her incantations.",
        "Kael claims that dodging is easier than blocking. Learn his rhythm well.",
        "Ares channels brute rage. When low on health, his strikes become deadlier.",
        "Elara never misses a target from the shadows. Keep your distance and kite.",
        "Transmuting rune affixes costs precious Shards. Plan your endgame build wisely!",
        "Every rune dropped from the dungeon harbors latent elemental power.",
        "Equipping matching elemental runes awakens formidable synergy passives.",
        "Higher Wager tiers drastically empower enemies but yield massive bonus Gems.",
        "Dying in a high-tier Wager run costs your entire bet. Retreat when overwhelmed!",
        "Stamina management is key: sprinting and dodging recklessly leaves you defenseless.",
        "Elite foes have relentless super-armor. Break their guard before committing combos.",
        "Defeating The Earthshaker unlocks the forbidden domain of The DarkMage.",
        "Gacha duplicates are automatically converted into valuable Shards and Crystals.",
        "Pay attention to the red indicator markers on the ground to dodge devastating boss AoEs.",
        "Mana does not replenish instantly. Drink potions or manage spell cooldowns carefully.",
        "Equipping defensive Runes can turn fragile spellcasters into resilient battle-mages.",
        "Bosses enter an enrage state at low HP. Save your ultimate skills for the final phase!",
        "Safe zones shrink progressively in deep runs. Stay within the perimeter to survive.",
        "Gold earned inside dungeons is temporary, but Gems and Shards remain forever.",
        "Cleanse corrupted altars to gain temporary blessings before facing the domain Boss."
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

    private void Update()
    {
        RotateLoadingHandle();
    }

    private void RotateLoadingHandle()
    {
        if (handleTransform != null)
        {
            handleTransform.Rotate(0f, 0f, -handleRotationSpeed * Time.unscaledDeltaTime);
        }
    }

    public void SetDestinationName(string sceneName)
    {
        if (destinationText == null) return;

        string formattedName = "TRAVELING THROUGH THE VOID";
        GameSceneData data = GameSceneData.Instance;

        string lobbySceneName = data != null ? data.GetSceneName(SceneType.LobbyMain) : "LobbyMain Scene";
        string runSceneName1 = data != null ? data.GetSceneName(SceneType.RunGameplay) : "Run Scene";
        string runSceneName2 = data != null ? data.GetSceneName(SceneType.RunGameplay2) : "Run Scene 2";
        string bossSceneName = data != null ? data.GetSceneName(SceneType.FinalBoss) : "FinalBoss Scene";
        string tutorialSceneName = data != null ? data.GetSceneName(SceneType.Tutorial) : "Tutorial Scene";
        string uiSceneName = data != null ? data.GetSceneName(SceneType.UIGame) : "UIGame";

        if (sceneName == lobbySceneName || sceneName.Contains("Lobby"))
        {
            formattedName = "RETURNING TO: HEROES' SANCTUARY";
        }
        else if (sceneName == runSceneName1 || sceneName == runSceneName2 || sceneName.Contains("Run"))
        {
            formattedName = "DESCENDING INTO: THE ABYSSAL DUNGEON";
        }
        else if (sceneName == bossSceneName || sceneName.Contains("Boss") || sceneName.Contains("Arena"))
        {
            formattedName = "APPROACHING: NIGHTMARE LORD'S THRONE";
        }
        else if (sceneName == tutorialSceneName || sceneName.Contains("Tutorial"))
        {
            formattedName = "ENTERING: TRIAL OF ASCENSION";
        }
        else if (sceneName == uiSceneName || sceneName.Contains("Login"))
        {
            formattedName = "CONNECTING TO: ASTRAL GATEWAY";
        }
        else
        {
            string cleanName = sceneName.Replace(" Scene", "").Replace("Game", "").ToUpper();
            formattedName = $"JOURNEYING TO: {cleanName}";
        }

        destinationText.SetTextSafe(formattedName);
    }

    public void ShowRandomTip()
    {
        if (loadingTipText == null || loadingTips == null || loadingTips.Count == 0) return;

        int randomIndex = Random.Range(0, loadingTips.Count);
        loadingTipText.SetTextSafe($"<color=#FFA500><b>TIP:</b></color> {loadingTips[randomIndex]}");
    }

    public IEnumerator TrackProgressRoutine(AsyncOperation targetSceneLoad, bool hasEventNotify = true, float minDuration = 6.0f)
    {
        LoadTrace.Mark($"TrackProgressRoutine started | MinDuration={minDuration:F2}s | OperationNull={targetSceneLoad == null}");
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

        LoadTrace.Mark($"Visual minimum duration completed | SceneProgress={targetSceneLoad?.progress ?? 1f:F2}");

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