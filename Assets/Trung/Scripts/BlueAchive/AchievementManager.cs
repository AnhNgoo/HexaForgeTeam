using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AchievementManager :
    MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("UI")]
    [SerializeField]
    private GameObject achievementPanel;

    [SerializeField]
    private Transform contentParent;

    [SerializeField]
    private AchievementCardUI cardPrefab;

    [SerializeField]
    private AchievementToastUI toastUI;

    private List<AchievementData>
        achievements =
        new List<AchievementData>();


    private const string
        Roll10ID =
        "ROLL_10";

    private const string
        Legendary5ID =
        "LEGENDARY_5";

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

        CreateDefaultAchievements();

LoadAchievement();

RefreshUI();
    }

    #region Create

    private void CreateDefaultAchievements()
    {
        if (achievements.Count > 0)
        {
            return;
        }

        achievements.Add(
            new AchievementData(
                Roll10ID,
                "First Gambler",
                "Roll 10 Times",
                10,
                300));

        achievements.Add(
            new AchievementData(
                Legendary5ID,
                "Rune Hunter",
                "Obtain 5 Legendary Runes",
                5,
                1000));
    }

    #endregion

    #region Progress

    public void AddRollProgress(
        int amount)
    {
        AchievementData achievement =
            GetAchievement(
                Roll10ID);

        if (achievement == null)
        {
            return;
        }

        bool wasCompleted =
            achievement.isCompleted;

        achievement.AddProgress(
            amount);

        CheckComplete(
            achievement,
            wasCompleted);

        SaveAchievement();
    }

    public void AddLegendaryProgress(
        int amount)
    {
        AchievementData achievement =
            GetAchievement(
                Legendary5ID);

        if (achievement == null)
        {
            return;
        }

        bool wasCompleted =
            achievement.isCompleted;

        achievement.AddProgress(
            amount);

        CheckComplete(
            achievement,
            wasCompleted);

        SaveAchievement();
    }

    #endregion

    #region Complete

    private void CheckComplete(
        AchievementData achievement,
        bool wasCompleted)
    {
        if (wasCompleted)
        {
            return;
        }

        if (!achievement.isCompleted)
        {
            return;
        }

        if (toastUI != null)
        {
            toastUI.ShowToast(
                "Achievement Unlocked",
                achievement.title);
        }

        RefreshUI();
    }

    #endregion

    #region UI

    public void OpenPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(
                true);
        }

        RefreshUI();
    }

    public void ClosePanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(
                false);
        }
    }

    public void RefreshUI()
    {
        if (contentParent == null)
        {
            return;
        }

        for (int i =
            contentParent.childCount - 1;
            i >= 0;
            i--)
        {
            Destroy(
                contentParent
                .GetChild(i)
                .gameObject);
        }

        for (int i = 0;
            i < achievements.Count;
            i++)
        {
            AchievementCardUI card =
                Instantiate(
                    cardPrefab,
                    contentParent);

            card.Setup(
                achievements[i]);
        }
    }

    #endregion

    #region Get

    private AchievementData
        GetAchievement(
            string achievementID)
    {
        for (int i = 0;
            i < achievements.Count;
            i++)
        {
            if (achievements[i]
                .achievementID ==
                achievementID)
            {
                return
                    achievements[i];
            }
        }

        return null;
    }

    #endregion

    #region Save

    public void SaveAchievement()
{
    SaveLoadManager.Instance
        .SaveData.achievements =
        achievements;

    SaveLoadManager.Instance
        .SaveGame();
}
    public void LoadAchievement()
{
    if (SaveLoadManager.Instance == null)
    {
        return;
    }

    if (SaveLoadManager.Instance
        .SaveData.achievements == null)
    {
        SaveAchievement();

        return;
    }

    if (SaveLoadManager.Instance
        .SaveData.achievements.Count == 0)
    {
        SaveAchievement();

        return;
    }

    achievements =
        SaveLoadManager.Instance
        .SaveData.achievements;
}

    #endregion
}