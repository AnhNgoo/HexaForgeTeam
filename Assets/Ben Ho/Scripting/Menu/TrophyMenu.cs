using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrophyMenu : MenuBase
{
    public override MenuType menuType => MenuType.TrophyMenu;

    [Header("Buttons")]
    [SerializeField] private Button btn_Back;

    [SerializeField] private Button btn_FirstDay;
    [SerializeField] private Button btn_GreatHunter;
    [SerializeField] private Button btn_LuckyRoulette;
    [SerializeField] private Button btn_HeroHunt;
    [SerializeField] private Button btn_DailySpend;

    [Header("Lines")]
    [SerializeField] private GameObject line_FirstDay;
    [SerializeField] private GameObject line_GreatHunter;
    [SerializeField] private GameObject line_LuckyRoulette;
    [SerializeField] private GameObject line_HeroHunt;
    [SerializeField] private GameObject line_DailySpend;

    [Header("Texts")]
    [SerializeField] private TMP_Text txt_FirstDay;
    [SerializeField] private TMP_Text txt_GreatHunter;
    [SerializeField] private TMP_Text txt_LuckyRoulette;
    [SerializeField] private TMP_Text txt_HeroHunt;
    [SerializeField] private TMP_Text txt_DailySpend;

    [Header("Contents")]
    [SerializeField] private GameObject contentFirstDay;
    [SerializeField] private GameObject contentGreatHunter;
    [SerializeField] private GameObject contentLuckyRoulette;
    [SerializeField] private GameObject contentHeroHunt;
    [SerializeField] private GameObject contentDailySpend;

    [SerializeField] private List<TrophyChallenge> firstDayChallenges;

    [SerializeField] private List<TrophyChallenge> greatHunterChallenges;

    [SerializeField] private List<TrophyChallenge> luckyRouletteChallenges;

    [SerializeField] private List<TrophyChallenge> heroHuntChallenges;

    [SerializeField] private List<TrophyChallenge> dailySpendChallenges;
    
    [SerializeField] private Transform contentRoot;

    private List<ChallengeUI> challengeUIs = new();

    private readonly Color selectedColor =
        new Color32(255, 210, 80, 255);

    private readonly Color normalColor =
        Color.white;

    private int currentTab = 0;

    protected override void LoadComponent()
    {
        if (btn_Back == null)
            btn_Back = transform.Find("btn_Back")?.GetComponent<Button>();
        if(contentRoot == null)
        {
            contentRoot = transform.Find(
                "Main/Content/Left-Content/ScrollRect/Viewport/Content");
        }
    }
    protected override void LoadComponentRuntime()
    {
        challengeUIs.Clear();

        foreach(Transform child in contentRoot)
        {
            ChallengeUI ui =
                child.GetComponent<ChallengeUI>();

            if(ui == null)
                ui = child.gameObject.AddComponent<ChallengeUI>();

            challengeUIs.Add(ui);
        }
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        btn_Back.onClick.AddListener(OnBack);

        btn_FirstDay.onClick.AddListener(() => SelectTab(0));
        btn_GreatHunter.onClick.AddListener(() => SelectTab(1));
        btn_LuckyRoulette.onClick.AddListener(() => SelectTab(2));
        btn_HeroHunt.onClick.AddListener(() => SelectTab(3));
        btn_DailySpend.onClick.AddListener(() => SelectTab(4));

        SelectTab(0);
    }

    public override void Close()
    {
        base.Close();

        btn_Back.onClick.RemoveAllListeners();
        btn_FirstDay.onClick.RemoveAllListeners();
        btn_GreatHunter.onClick.RemoveAllListeners();
        btn_LuckyRoulette.onClick.RemoveAllListeners();
        btn_HeroHunt.onClick.RemoveAllListeners();
        btn_DailySpend.onClick.RemoveAllListeners();
    }

    private void OnBack()
    {
        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }

    private void SelectTab(int index)
    {
        // Line
        line_FirstDay.SetActive(index == 0);
        line_GreatHunter.SetActive(index == 1);
        line_LuckyRoulette.SetActive(index == 2);
        line_HeroHunt.SetActive(index == 3);
        line_DailySpend.SetActive(index == 4);

        // Text color
        txt_FirstDay.color = index == 0 ? selectedColor : normalColor;
        txt_GreatHunter.color = index == 1 ? selectedColor : normalColor;
        txt_LuckyRoulette.color = index == 2 ? selectedColor : normalColor;
        txt_HeroHunt.color = index == 3 ? selectedColor : normalColor;
        txt_DailySpend.color = index == 4 ? selectedColor : normalColor;

        // Content
        contentFirstDay.SetActive(index == 0);
        contentGreatHunter.SetActive(index == 1);
        contentLuckyRoulette.SetActive(index == 2);
        contentHeroHunt.SetActive(index == 3);
        contentDailySpend.SetActive(index == 4);

        currentTab = index;
        RefreshCurrentTab();
    }
    private void RefreshChallengeUI(List<TrophyChallenge> challenges)
    {
        foreach (var challenge in challenges)
        {
            // Chưa hoàn thành
            if (!challenge.isCompleted)
            {
                challenge.darkOverlay.SetActive(true);

                challenge.claimButton.gameObject.SetActive(true);
                challenge.claimButton.interactable = false;

                challenge.claimedIcon.SetActive(false);
            }

            // Đã hoàn thành nhưng chưa nhận
            else if (!challenge.isClaimed)
            {
                challenge.darkOverlay.SetActive(false);

                challenge.claimButton.gameObject.SetActive(true);
                challenge.claimButton.interactable = true;

                challenge.claimedIcon.SetActive(false);
            }

            // Đã nhận thưởng
            else
            {
                challenge.darkOverlay.SetActive(false);

                challenge.claimButton.gameObject.SetActive(false);

                challenge.claimedIcon.SetActive(true);
            }
        }
    }
    private void ClaimReward(TrophyChallenge challenge)
    {
        if (!challenge.isCompleted)
            return;

        if (challenge.isClaimed)
            return;

        challenge.isClaimed = true;

        RefreshCurrentTab();

        Debug.Log("Reward Claimed");
    }
    private void RefreshCurrentTab()
    {
        switch(currentTab)
        {
            case 0:
                RefreshChallengeUI(firstDayChallenges);
                break;

            case 1:
                RefreshChallengeUI(greatHunterChallenges);
                break;

            case 2:
                RefreshChallengeUI(luckyRouletteChallenges);
                break;

            case 3:
                RefreshChallengeUI(heroHuntChallenges);
                break;

            case 4:
                RefreshChallengeUI(dailySpendChallenges);
                break;
        }
    }
    private void UpdateChallengeUI(
    ChallengeUI ui,
    bool completed,
    bool claimed)
    {
        if(!completed)
        {
            ui.darkOverlay?.SetActive(true);

            if(ui.claimButton != null)
                ui.claimButton.interactable = false;

            ui.claimedIcon?.SetActive(false);
        }
        else if(!claimed)
        {
            ui.darkOverlay?.SetActive(false);

            if(ui.claimButton != null)
                ui.claimButton.interactable = true;

            ui.claimedIcon?.SetActive(false);
        }
        else
        {
            ui.darkOverlay?.SetActive(false);

            if(ui.claimButton != null)
                ui.claimButton.gameObject.SetActive(false);

            ui.claimedIcon?.SetActive(true);
        }
    }
}