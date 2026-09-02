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
    private static readonly string[] TipsEN = new string[]
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

private static readonly string[] TipsVI = new string[]
{
    "Đang mài sắc vũ khí... Lưỡi thép sẽ chạm bóng tối trong thử thách phía trước.",
    "Lyra đang kết dệt những phù văn Arcane cổ xưa. Đừng làm gián đoạn lời chú của cô ấy.",
    "Kael khẳng định né đòn dễ hơn đỡ đòn. Hãy học thật kỹ nhịp điệu của anh ấy.",
    "Ares tụ hội cơn cuồng nộ. Khi máu thấp, đòn đánh của anh ta càng thêm chí mạng.",
    "Elara chưa từng trượt mục tiêu từ trong bóng tối. Giữ khoảng cách và thả diều.",
    "Chuyển hóa phụ tố rune tiêu tốn những Mảnh quý giá. Hãy hoạch định lối build cuối game thật khôn ngoan!",
    "Mọi rune rơi ra từ hầm ngục đều ẩn chứa sức mạnh nguyên tố tiềm tàng.",
    "Trang bị các rune cùng nguyên tố sẽ đánh thức những nội tại cộng hưởng đáng gờm.",
    "Bậc Cược càng cao càng khiến kẻ địch mạnh lên khủng khiếp, nhưng phần thưởng Gem cũng khổng lồ.",
    "Chết trong lượt Cược bậc cao sẽ mất trắng tiền cược. Bị áp đảo thì hãy rút lui!",
    "Quản lý thể lực là chìa khóa: chạy nhanh và né đòn bừa bãi sẽ khiến bạn không còn gì để phòng thủ.",
    "Kẻ địch tinh anh có lớp siêu giáp lì lợm. Hãy phá thế thủ của chúng trước khi tung combo.",
    "Đánh bại The Earthshaker sẽ mở khóa lãnh địa cấm của The DarkMage.",
    "Trùng lặp gacha sẽ tự động chuyển hóa thành Mảnh và Tinh thể quý giá.",
    "Để ý các vạch chỉ báo đỏ trên mặt đất để né những chiêu AoE hủy diệt của boss.",
    "Năng lượng không hồi phục tức thì. Hãy uống thuốc hoặc canh hồi chiêu thật cẩn thận.",
    "Trang bị Rune phòng thủ có thể biến pháp sư mỏng manh thành chiến pháp sư kiên cường.",
    "Boss sẽ rơi vào trạng thái cuồng nộ khi máu thấp. Hãy dành kỹ năng cuối cho giai đoạn cuối!",
    "Vùng an toàn thu hẹp dần ở những tầng sâu. Hãy ở trong ranh giới để sống sót.",
    "Vàng kiếm trong hầm ngục chỉ là tạm thời, nhưng Gem và Mảnh thì còn mãi mãi.",
    "Thanh tẩy những bàn thờ bị tha hóa để nhận phước lành tạm thời trước khi đối đầu Boss lãnh địa."
};

private void ShowRandomTip()
{
    if (txt_LoadingTip == null) return;

    bool vi = SettingsLocalizationData.IsVietnamesePublic();
    int i = Random.Range(0, TipsEN.Length);

    string label = vi ? "MẸO" : "TIP";
    string tip   = vi ? TipsVI[i] : TipsEN[i];

    txt_LoadingTip.text = $"<color=#FFA500><b>{label}:</b></color> {tip}";
}

    private Coroutine loadingCoroutine;

    // ✅ Helper dịch
    private string T(string text) => SettingsLocalizationData.Translate(text);

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

        txt_Destination.text = T(formattedName);   // ✅ DỊCH
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
            int percent = Mathf.RoundToInt(value * 100);
            txt_Loading.text = T($"Loading... {percent}%");   // ✅ DỊCH (regex "Đang tải... X%")
        }
    }
}