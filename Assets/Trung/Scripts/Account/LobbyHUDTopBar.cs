using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LobbyHUDTopBar : MonoBehaviour
{
    public static LobbyHUDTopBar Instance;

    [Header("Visual Groups (Dùng để bật/ẩn nhanh theo cụm nếu cần)")]
    [SerializeField] private GameObject levelGroup;
    [SerializeField] private GameObject currencyGroup;

    [Header("Gem, Rune Shard & Ticket UI")]
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text runeShardText;
    [SerializeField] private TMP_Text gachaTicketText;

    [Header("Account Level UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Slider expBar;
    [SerializeField] private string sceneRunName = "Run Scene";

    private float animatedCurrentExp;
    private int cachedRequiredExp;
    private Tween activeExpTween;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Start()
    {
        RefreshLayoutByScene();
        SetupTooltips();

        if (AccountLevelManager.Instance != null)
        {
            int currentLv = AccountLevelManager.Instance.GetLevel();
            if (levelText != null) levelText.SetTextSafe(currentLv.ToString());
        }
    }

    private void SetupTooltips()
    {
        // 1. Tooltip cho Gem
        if (gemText != null)
        {
            GameObject gemRoot = gemText.transform.parent != null ? gemText.transform.parent.gameObject : gemText.gameObject;
            AddTooltip(gemRoot, "Gems", "Premium realm currency. Used for summoning Runes, rerolling affixes, and unlocking rare shop items.");
        }

        // 2. Tooltip cho Rune Shards
        if (runeShardText != null)
        {
            GameObject shardRoot = runeShardText.transform.parent != null ? runeShardText.transform.parent.gameObject : runeShardText.gameObject;
            AddTooltip(shardRoot, "Rune Shards", "Ancient essence gained from dismantling duplicate or obsolete runes. Used for Rune Fusion and Sub-Stat Rerolling.");
        }

        // 3. Tooltip cho Gacha Ticket
        if (gachaTicketText != null)
        {
            GameObject ticketRoot = gachaTicketText.transform.parent != null ? gachaTicketText.transform.parent.gameObject : gachaTicketText.gameObject;
            AddTooltip(ticketRoot, "Summon Ticket", "A rare astral summon ticket. Can be consumed in the Gacha Chamber instead of Gems to summon powerful Runes.");
        }

        // 4. Tooltip cho Thanh EXP / Level
        if (levelGroup != null)
        {
            AddTooltip(levelGroup, "Account Level", "Your overall adventurer progression. Leveling up grants stat enhancements and unlocks deeper dungeon tiers.");
        }
    }

    private void AddTooltip(GameObject targetObj, string title, string description, Sprite icon = null)
    {
        if (targetObj == null) return;
        var trigger = targetObj.GetComponent<UITooltipAutoTrigger>() ?? targetObj.AddComponent<UITooltipAutoTrigger>();
        trigger.SetData(title, description, icon);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshLayoutByScene();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        RefreshLayoutByScene();
    }

    public void RefreshLayoutByScene()
    {
        bool isRunLoaded = GameSceneData.Instance != null && GameSceneData.Instance.IsSceneLoaded(SceneType.RunGameplay);

        if (isRunLoaded)
        {
            if (levelGroup != null) levelGroup.SetActive(false);
            if (currencyGroup != null) currencyGroup.SetActive(false);
        }
        else
        {
            if (levelGroup != null) levelGroup.SetActive(true);
            if (currencyGroup != null) currencyGroup.SetActive(true);
            RefreshCurrencyUI();
        }
    }

    public void RefreshCurrencyUI()
    {
        if (gemText != null && GemManager.Instance != null)
        {
            gemText.SetTextSafe(GemManager.Instance.GetCurrentGem().ToString("N0"));
        }

        if (runeShardText != null && SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            runeShardText.SetTextSafe(SaveLoadManager.Instance.SaveData.runeShards.ToString("N0"));
        }

        if (gachaTicketText != null && InventoryItemManager.Instance != null)
        {
            int tickets = InventoryItemManager.Instance.GetItemQuantity("GACHA_TICKET_01");
            gachaTicketText.SetTextSafe(tickets.ToString("N0"));
        }

        if (userNameText != null)
        {
            userNameText.SetTextSafe(PlayerPrefs.GetString("DisplayName", "Unknown"));
        }
    }

    public void RefreshLevelUI(int level, int currentExp, int requiredExp)
    {
        if (levelText != null) levelText.SetTextSafe(level.ToString());
        if (requiredExp <= 0) return;

        cachedRequiredExp = requiredExp;
        float targetValue = (float)currentExp / requiredExp;

        if (activeExpTween != null) activeExpTween.Kill();
        Sequence expSequence = DOTween.Sequence();

        if (expBar != null)
        {
            expSequence.Join(expBar.DOValue(targetValue, 0.5f).SetEase(Ease.OutQuad));
        }

        if (expText != null)
        {
            expSequence.Join(DOTween.To(() => animatedCurrentExp, x => animatedCurrentExp = x, currentExp, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() =>
                {
                    expText.SetTextSafe($"{(int)animatedCurrentExp:N0} / {cachedRequiredExp:N0}");
                }));
        }
        activeExpTween = expSequence;
    }

    public void ShowFullHUD()
    {
        RefreshLayoutByScene();
    }

    public void ShowCurrencyOnly()
    {
        string runSceneName = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.RunGameplay) 
            : sceneRunName;

        Scene runScene = SceneManager.GetSceneByName(runSceneName);
        if (!runScene.isLoaded)
        {
            if (levelGroup != null) levelGroup.SetActive(false);
            if (currencyGroup != null) currencyGroup.SetActive(true);
            RefreshCurrencyUI();
        }
    }
}