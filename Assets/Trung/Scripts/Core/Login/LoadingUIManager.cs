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

    // ✅ TIPS 2 NGÔN NGỮ — hardcode, KHÔNG [SerializeField] → scene không thể ghi đè
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

    // ✅ Helper dịch
    private string T(string text) => SettingsLocalizationData.Translate(text);

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

        destinationText.SetTextSafe(T(formattedName));   // ✅ DỊCH
    }

    public void ShowRandomTip()
    {
        if (loadingTipText == null || TipsEN.Length == 0) return;

        // ✅ Chọn ngôn ngữ trực tiếp — không phụ thuộc Entries, không phụ thuộc scene
        bool vi = SettingsLocalizationData.IsVietnamesePublic();
        int i = Random.Range(0, TipsEN.Length);

        string label = vi ? "MẸO" : "TIP";
        string tip = vi ? TipsVI[i] : TipsEN[i];

        loadingTipText.SetTextSafe($"<color=#FFA500><b>{label}:</b></color> {tip}");
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