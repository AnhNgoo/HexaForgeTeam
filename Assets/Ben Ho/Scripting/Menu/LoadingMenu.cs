using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadingMenu : MenuBase
{
    public override MenuType menuType => MenuType.LoadingMenu;

    [Header("Loading UI References")]
    [SerializeField] private Slider slider_Loading;
    [SerializeField] private Image img_Fill;
    [SerializeField] private TextMeshProUGUI txt_Loading;
    [SerializeField] private RectTransform handleTransform;
    [SerializeField] private TextMeshProUGUI txt_Destination;
    [SerializeField] private TextMeshProUGUI txt_LoadingTip;

    [Header("Config")]
    [SerializeField] private float loadingTime = 3.5f;
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

    private Coroutine loadingCoroutine;

    protected override void LoadComponent()
    {
        if (slider_Loading == null)
            slider_Loading = GetComponentInChildren<Slider>();

        if (txt_Loading == null)
            txt_Loading = transform.Find("LoadingText")?.GetComponent<TextMeshProUGUI>();

        if (handleTransform == null && slider_Loading != null && slider_Loading.handleRect != null)
        {
            if (slider_Loading.handleRect.childCount > 0)
                handleTransform = slider_Loading.handleRect.GetChild(0) as RectTransform;
            else
                handleTransform = slider_Loading.handleRect;
        }
    }

    protected override void LoadComponentRuntime() { }

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

    public override void Open(object data = null)
    {
        base.Open(data);

        Time.timeScale = 1f;

        ShowRandomTip();
        SetDestinationInfo();

        if (loadingCoroutine != null)
            StopCoroutine(loadingCoroutine);

        loadingCoroutine = StartCoroutine(LoadingRoutine());
    }

    public override void Close()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        base.Close();
    }

    private void SetDestinationInfo()
    {
        if (txt_Destination == null) return;

        string target = LoadingData.TargetSceneName;
        string formattedName = "TRAVELING THROUGH THE VOID";
        GameSceneData data = GameSceneData.Instance;

        string lobbySceneName = data != null ? data.GetSceneName(SceneType.LobbyMain) : "LobbyMain Scene";
        string runSceneName1 = data != null ? data.GetSceneName(SceneType.RunGameplay) : "Run Scene";
        string runSceneName2 = data != null ? data.GetSceneName(SceneType.RunGameplay2) : "Run Scene 2";
        string bossSceneName = data != null ? data.GetSceneName(SceneType.FinalBoss) : "FinalBoss Scene";
        string tutorialSceneName = data != null ? data.GetSceneName(SceneType.Tutorial) : "Tutorial Scene";
        string uiSceneName = data != null ? data.GetSceneName(SceneType.UIGame) : "UIGame";

        if (string.IsNullOrEmpty(target))
        {
            formattedName = $"ACCESSING: {LoadingData.TargetMenu.ToString().ToUpper()}";
        }
        else if (target == lobbySceneName || target.Contains("Lobby"))
        {
            formattedName = "RETURNING TO: HEROES' SANCTUARY";
        }
        else if (target == runSceneName1 || target == runSceneName2 || target.Contains("Run"))
        {
            formattedName = "DESCENDING INTO: THE ABYSSAL DUNGEON";
        }
        else if (target == bossSceneName || target.Contains("Boss") || target.Contains("Arena"))
        {
            formattedName = "APPROACHING: NIGHTMARE LORD'S THRONE";
        }
        else if (target == tutorialSceneName || target.Contains("Tutorial"))
        {
            formattedName = "ENTERING: TRIAL OF ASCENSION";
        }
        else if (target == uiSceneName || target.Contains("Login"))
        {
            formattedName = "CONNECTING TO: ASTRAL GATEWAY";
        }
        else
        {
            formattedName = $"JOURNEYING TO: {target.Replace(" Scene", "").Replace("Game", "").ToUpper()}";
        }

        txt_Destination.text = formattedName;
    }

    private void ShowRandomTip()
    {
        if (txt_LoadingTip == null || loadingTips == null || loadingTips.Count == 0) return;

        int randomIndex = Random.Range(0, loadingTips.Count);
        txt_LoadingTip.text = $"<color=#FFA500><b>TIP:</b></color> {loadingTips[randomIndex]}";
    }

    private IEnumerator LoadingRoutine()
    {
        LoadTrace.Mark($"LoadingMenu delay started: {loadingTime:F2}s");
        float timer = 0f;

        SetProgress(0f);

        while (timer < loadingTime)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / loadingTime);
            SetProgress(progress);

            yield return null;
        }

        SetProgress(1f);

        yield return new WaitForSecondsRealtime(0.25f);

        if (!string.IsNullOrEmpty(LoadingData.TargetSceneName))
        {
            string sceneName = LoadingData.TargetSceneName;
            LoadingData.TargetSceneName = "";

            LoadTrace.Mark($"SceneManager.LoadScene begin: {sceneName}");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            LoadTrace.End($"SceneManager.LoadScene returned: {sceneName}");
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ChangeMenu(LoadingData.TargetMenu);
            }
        }
    }

    private void SetProgress(float value)
    {
        if (slider_Loading != null)
            slider_Loading.value = value;

        if (img_Fill != null)
            img_Fill.fillAmount = value;

        if (txt_Loading != null)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            txt_Loading.text = $"Loading... {percent}%";
        }
    }
}