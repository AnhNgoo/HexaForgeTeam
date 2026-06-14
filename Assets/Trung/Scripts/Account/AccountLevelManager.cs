using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountLevelManager :
    MonoBehaviour
{
    public static AccountLevelManager Instance;

    [Header("UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Slider expBar;

    [Header("Level Up UI")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rewardText;

    private AccountLevelData accountData =
        new AccountLevelData();

    private string savePath;

    private const int MAX_LEVEL = 30;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);

            return;
        }

        savePath =
            Application.persistentDataPath +
            "/AccountLevel.json";

        LoadData();
    }

    private void Start()
    {
        UpdateUI();

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    public void AddExp(
        int amount)
    {
        if (accountData.level >= MAX_LEVEL)
        {
            return;
        }

        accountData.currentExp += amount;

        while (accountData.level < MAX_LEVEL &&
               accountData.currentExp >=
               GetRequiredExp(
                   accountData.level))
        {
            accountData.currentExp -=
                GetRequiredExp(
                    accountData.level);

            LevelUp();
        }

        SaveData();

        UpdateUI();
    }

    private void LevelUp()
    {
        int oldLevel =
            accountData.level;

        accountData.level++;
        string unlockText = "";
        if (accountData.level == 5)
{
    unlockText =
        "\nUnlocked Character: Lyra";
}

if (accountData.level == 15)
{
    unlockText =
        "\nUnlocked Character: Ares";
}

if (accountData.level == 25)
{
    unlockText =
        "\nUnlocked Character: Elara";
}
    if (CharacterManager.Instance != null)
{
    CharacterManager.Instance
        .CheckUnlockCharacter();
}
        if (LobbyStatManager.Instance != null)
{
    LobbyStatManager.Instance
        .RecalculateStats();
}

        int gemReward =
    100 +
    accountData.level * 50;

        
        GemManager.Instance
            .AddGem(gemReward);

        ShowLevelUpPopup(
    oldLevel,
    accountData.level,
    gemReward,
    unlockText);
    }

    private int GetRequiredExp(
        int level)
    {
        return 100 +
               ((level - 1) * 50);
    }

    private void UpdateUI()
    {
        if (levelText != null)
        {
            levelText.text =
                accountData.level.ToString();
        }

        int requiredExp =
            GetRequiredExp(
                accountData.level);

        if (expText != null)
        {
            expText.text =
                $"{accountData.currentExp}/{requiredExp}";
        }

        if (expBar != null)
        {
            expBar.value =
                (float)accountData.currentExp /
                requiredExp;
        }
    }

    private void ShowLevelUpPopup(
    int oldLevel,
    int newLevel,
    int gemReward,
    string unlockText)
{
    if (levelUpPanel == null)
    {
        return;
    }

    levelUpPanel.SetActive(true);

    if (titleText != null)
    {
        titleText.text =
            "LEVEL UP";
    }

    if (rewardText != null)
    {
        rewardText.text =
            $"Lv {oldLevel} → Lv {newLevel}\n" +
            $"+{gemReward} Gems\n" +
            $"+10 HP\n" +
            $"+1 ATK" +
            unlockText;
    }

    Invoke(
        nameof(CloseLevelUpPanel),
        3f);
}

    public void CloseLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    public int GetLevel()
    {
        return accountData.level;
    }

    public int GetHPBonus()
    {
        return (accountData.level - 1) * 10;
    }

    public int GetATKBonus()
    {
        return accountData.level - 1;
    }

    private void SaveData()
    {
        SaveLoadManager.Instance
    .SaveData.accountLevel =
    accountData.level;

SaveLoadManager.Instance
    .SaveData.accountExp =
    accountData.currentExp;

SaveLoadManager.Instance
    .SaveGame();
    }

    private void LoadData()
    {
        accountData.level =
    SaveLoadManager.Instance
    .SaveData.accountLevel;

accountData.currentExp =
    SaveLoadManager.Instance
    .SaveData.accountExp;
    }
}