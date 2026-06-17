using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaMenu : MenuBase
{
    public override MenuType menuType => MenuType.GachaMenu;

    [Header("Buttons")]
    [SerializeField] private Button btn_Cancel;
    [SerializeField] private Button btn_Confirm;
    [SerializeField] private Button btn_RollOne;
    [SerializeField] private Button btn_RollFive;

    [Header("Reward Display")]
    [SerializeField] private Image img_Reward;
    [SerializeField] private TextMeshProUGUI txt_RewardName;
    [SerializeField] private TextMeshProUGUI txt_Coin;

    [Header("Gacha Setting")]
    [SerializeField] private int rollOneCost = 500;
    [SerializeField] private int rollFiveCost = 2500;
    [SerializeField] private int playerCoin = 5000;

    [Header("Reward Data")]
    [SerializeField] private GachaReward[] rewards;

    private int selectedRollCount = 1;
    private int selectedCost = 500;

    protected override void LoadComponent()
    {
        if (btn_Cancel == null)
            btn_Cancel = FindDeepChild("Btn_Cancel")?.GetComponent<Button>();

        if (btn_Confirm == null)
            btn_Confirm = FindDeepChild("Btn_Confirm")?.GetComponent<Button>();

        if (btn_RollOne == null)
            btn_RollOne = FindDeepChild("Btn_RollOne")?.GetComponent<Button>();

        if (btn_RollFive == null)
            btn_RollFive = FindDeepChild("Btn_RollFive")?.GetComponent<Button>();

        if (img_Reward == null)
            img_Reward = FindDeepChild("Image_Reward")?.GetComponent<Image>();

        if (txt_RewardName == null)
            txt_RewardName = FindDeepChild("Txt_RewardName")?.GetComponent<TextMeshProUGUI>();

        if (txt_Coin == null)
            txt_Coin = FindDeepChild("Txt_Coin")?.GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        selectedRollCount = 1;
        selectedCost = rollOneCost;

        AddEvents();
        UpdateCoinUI();
        ClearRewardUI();
    }

    public override void Close()
    {
        RemoveEvents();

        base.Close();
    }

    private void AddEvents()
    {
        if (btn_Cancel != null)
            btn_Cancel.onClick.AddListener(OnCancelButtonClicked);

        if (btn_Confirm != null)
            btn_Confirm.onClick.AddListener(OnConfirmButtonClicked);

        if (btn_RollOne != null)
            btn_RollOne.onClick.AddListener(OnRollOneButtonClicked);

        if (btn_RollFive != null)
            btn_RollFive.onClick.AddListener(OnRollFiveButtonClicked);
    }

    private void RemoveEvents()
    {
        if (btn_Cancel != null)
            btn_Cancel.onClick.RemoveListener(OnCancelButtonClicked);

        if (btn_Confirm != null)
            btn_Confirm.onClick.RemoveListener(OnConfirmButtonClicked);

        if (btn_RollOne != null)
            btn_RollOne.onClick.RemoveListener(OnRollOneButtonClicked);

        if (btn_RollFive != null)
            btn_RollFive.onClick.RemoveListener(OnRollFiveButtonClicked);
    }

    private void OnRollOneButtonClicked()
    {
        selectedRollCount = 1;
        selectedCost = rollOneCost;

        Debug.Log("Selected roll x1");
    }

    private void OnRollFiveButtonClicked()
    {
        selectedRollCount = 5;
        selectedCost = rollFiveCost;

        Debug.Log("Selected roll x5");
    }

    private void OnConfirmButtonClicked()
{
    if (playerCoin < selectedCost)
    {
        Debug.Log("Không đủ coin để gacha.");
        return;
    }

    playerCoin -= selectedCost;
    UpdateCoinUI();

    for (int i = 0; i < selectedRollCount; i++)
    {
        GachaReward reward = GetRandomReward();

        if (reward != null && reward.gemData != null)
        {
            // if (InventoryGem.Instance != null)
            // {
            //     InventoryGem.Instance.AddGem(reward.gemData, reward.amount);
            // }

            ShowReward(reward);

            Debug.Log("Gacha nhận được: " + reward.gemData.gemName + " x" + reward.amount);
        }
    }
}

    private void OnCancelButtonClicked()
    {
        Debug.Log("Cancel gacha");

        UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
    }

    private GachaReward GetRandomReward()
    {
        if (rewards == null || rewards.Length == 0)
        {
            Debug.LogWarning("Chưa gán reward cho GachaMenu.");
            return null;
        }

        int totalRate = 0;

        for (int i = 0; i < rewards.Length; i++)
        {
            totalRate += rewards[i].rate;
        }

        int randomValue = Random.Range(0, totalRate);
        int currentRate = 0;

        for (int i = 0; i < rewards.Length; i++)
        {
            currentRate += rewards[i].rate;

            if (randomValue < currentRate)
            {
                return rewards[i];
            }
        }

        return rewards[0];
    }

    private void ShowReward(GachaReward reward)
    {
        if (reward == null || reward.gemData == null) return;

        if (img_Reward != null)
        {
            img_Reward.sprite = reward.gemData.gemIcon;
            img_Reward.enabled = reward.gemData.gemIcon != null;
        }

        if (txt_RewardName != null)
        {
            txt_RewardName.text = reward.gemData.gemName + " x" + reward.amount;
        }
    }

    private void ClearRewardUI()
    {
        if (img_Reward != null)
        {
            img_Reward.sprite = null;
            img_Reward.enabled = false;
        }

        if (txt_RewardName != null)
        {
            txt_RewardName.text = "";
        }
    }

    private void UpdateCoinUI()
    {
        if (txt_Coin != null)
        {
            txt_Coin.text = playerCoin.ToString();
        }
    }

    private Transform FindDeepChild(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}

[System.Serializable]
    public class GachaReward
    {
        public GemData gemData;
        public int amount = 1;

        [Range(1, 100)]
        public int rate = 10;
    }