using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Tab Lines")]
    [SerializeField] private GameObject line_FirstDay;
    [SerializeField] private GameObject line_GreatHunter;
    [SerializeField] private GameObject line_LuckyRoulette;
    [SerializeField] private GameObject line_HeroHunt;
    [SerializeField] private GameObject line_DailySpend;

    [Header("Tab Texts")]
    [SerializeField] private TMP_Text txt_FirstDay;
    [SerializeField] private TMP_Text txt_GreatHunter;
    [SerializeField] private TMP_Text txt_LuckyRoulette;
    [SerializeField] private TMP_Text txt_HeroHunt;
    [SerializeField] private TMP_Text txt_DailySpend;

    [Header("Contents")]
    [SerializeField] private Transform content_1;
    [SerializeField] private Transform content_2;
    [SerializeField] private Transform content_3;
    [SerializeField] private Transform content_4;
    [SerializeField] private Transform content_5;

    private readonly Color selectedColor =
        new Color32(255, 210, 80, 255);

    private readonly Color normalColor =
        Color.white;

    private int currentTab;

    private readonly Dictionary<int, List<ChallengeUI>> challengeMap
        = new();

    protected override void LoadComponent()
    {
        if (btn_Back == null)
            btn_Back = transform.Find("btn_Back")
                ?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {
        BuildChallengeLists();
    }

    private void BuildChallengeLists()
    {
        challengeMap.Clear();

        challengeMap.Add(0, GetChallenges(content_1));
        challengeMap.Add(1, GetChallenges(content_2));
        challengeMap.Add(2, GetChallenges(content_3));
        challengeMap.Add(3, GetChallenges(content_4));
        challengeMap.Add(4, GetChallenges(content_5));
    }

    private List<ChallengeUI> GetChallenges(Transform root)
    {
        List<ChallengeUI> result = new();

        if (root == null)
            return result;

        foreach (Transform child in root)
        {
            if (!child.name.StartsWith("List"))
                continue;

            ChallengeUI ui =
                child.GetComponent<ChallengeUI>();

            if (ui == null)
                ui = child.gameObject.AddComponent<ChallengeUI>();

            result.Add(ui);
        }

        return result;
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
        currentTab = index;

        line_FirstDay.SetActive(index == 0);
        line_GreatHunter.SetActive(index == 1);
        line_LuckyRoulette.SetActive(index == 2);
        line_HeroHunt.SetActive(index == 3);
        line_DailySpend.SetActive(index == 4);

        txt_FirstDay.color =
            index == 0 ? selectedColor : normalColor;

        txt_GreatHunter.color =
            index == 1 ? selectedColor : normalColor;

        txt_LuckyRoulette.color =
            index == 2 ? selectedColor : normalColor;

        txt_HeroHunt.color =
            index == 3 ? selectedColor : normalColor;

        txt_DailySpend.color =
            index == 4 ? selectedColor : normalColor;

        content_1.gameObject.SetActive(index == 0);
        content_2.gameObject.SetActive(index == 1);
        content_3.gameObject.SetActive(index == 2);
        content_4.gameObject.SetActive(index == 3);
        content_5.gameObject.SetActive(index == 4);

        RefreshCurrentTab();
    }

    private void RefreshCurrentTab()
    {
        if (!challengeMap.ContainsKey(currentTab))
            return;

        List<ChallengeUI> challenges =
            challengeMap[currentTab];

        for (int i = 0; i < challenges.Count; i++)
        {
            bool completed = Random.value > 0.5f;
            bool claimed = completed && Random.value > 0.7f;

            UpdateChallengeUI(
                challenges[i],
                completed,
                claimed);
        }
    }

    private void UpdateChallengeUI(
        ChallengeUI ui,
        bool completed,
        bool claimed)
    {
        if (ui == null)
            return;

        if (!completed)
        {
            ui.darkOverlay?.SetActive(true);

            if (ui.claimButton != null)
            {
                ui.claimButton.gameObject.SetActive(true);
                ui.claimButton.interactable = false;
            }

            ui.claimedIcon?.SetActive(false);
        }
        else if (!claimed)
        {
            ui.darkOverlay?.SetActive(false);

            if (ui.claimButton != null)
            {
                ui.claimButton.gameObject.SetActive(true);
                ui.claimButton.interactable = true;

                ui.claimButton.onClick.RemoveAllListeners();
                ui.claimButton.onClick.AddListener(() =>
                {
                    ClaimReward(ui);
                });
            }

            ui.claimedIcon?.SetActive(false);
        }
        else
        {
            ui.darkOverlay?.SetActive(false);

            if (ui.claimButton != null)
            {
                ui.claimButton.gameObject.SetActive(false);
            }

            ui.claimedIcon?.SetActive(true);
        }
    }

    private void ClaimReward(ChallengeUI ui)
    {
        if (ui.claimButton != null)
            ui.claimButton.gameObject.SetActive(false);

        ui.claimedIcon?.SetActive(true);

        Debug.Log("Reward Claimed");
    }
}