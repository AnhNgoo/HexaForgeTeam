
using UnityEngine;

public class AccountLevelManager :
    MonoBehaviour
{
    public static AccountLevelManager Instance;

    [Header("UI")]

    private AccountLevelData accountData =
        new AccountLevelData();


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

        LoadData();
    }

    private void Start()
{
    UpdateUI();

    string displayName =
        PlayerPrefs.GetString(
            "DisplayName",
            "Unknown");

    if (AccountLevelUI.Instance != null)
    {
        AccountLevelUI.Instance
            .SetUserName(
                displayName);
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
    if (AccountLevelUI.Instance == null)
    {
        return;
    }

    AccountLevelUI.Instance
        .Refresh(
            accountData.level,
            accountData.currentExp,
            GetRequiredExp(
                accountData.level));
}

    private void ShowLevelUpPopup(
    int oldLevel,
    int newLevel,
    int gemReward,
    string unlockText)
{
    if (LevelUpPopupUI.Instance == null)
    {
        return;
    }

    string reward =
        $"Lv {oldLevel} → Lv {newLevel}\n" +
        $"+{gemReward} Gems\n" +
        $"+10 HP\n" +
        $"+1 ATK" +
        unlockText;

    LevelUpPopupUI.Instance
        .Show(
            "LEVEL UP",
            reward);
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
        SaveLoadManager.Instance.SaveData.accountLevel = accountData.level;
        SaveLoadManager.Instance.SaveData.accountExp = accountData.currentExp;
        SaveLoadManager.Instance.SaveGame();

        // Gộp chung 1 block if và chỉ dùng MarkDirty() để hệ thống tự động lưu sau 5s, tránh spam API
        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.MarkDirty();
            
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.UpdatePowerScore();
            }
        }
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
    public void ResetLevelData()
{
    accountData =
        new AccountLevelData();

    UpdateUI();

    SaveData();
}

}