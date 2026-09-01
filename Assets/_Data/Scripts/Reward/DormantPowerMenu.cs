using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DormantPowerMenu : MenuBase
{
    [Header("Detail")]
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailType;
    [SerializeField] private TMP_Text detailRarity;
    [SerializeField] private TMP_Text detailDescription;
    [SerializeField] private Button confirmButton;

    private BossRewardDataSO selectedReward;

    [SerializeField] private DormantPowerCardUI[] rewardCards;

    private DormantPowerMenuData menuData;
    private bool selectionLocked;

    public override MenuType menuType => MenuType.DormantPowerMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        menuData = data as DormantPowerMenuData;
        selectionLocked = false;

        if (rewardCards == null)
            return;

        foreach (DormantPowerCardUI card in rewardCards) card?.Hide();

        if (menuData?.Rewards == null)
        {
            Debug.LogWarning("DormantPowerMenu thiếu reward data.");
            return;
        }

        int count = Mathf.Min(rewardCards.Length, menuData.Rewards.Count);

        for (int i = 0; i < count; i++)
        {
            rewardCards[i]?.Setup(menuData.Rewards[i], FocusReward);
        }

        confirmButton.onClick.RemoveListener(ConfirmReward);
        confirmButton.onClick.AddListener(ConfirmReward);

        if (count > 0)
            FocusReward(menuData.Rewards[0]);
    }

    public override void Close()
    {
        base.Close();
        menuData = null;
        selectionLocked = false;

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmReward);
            confirmButton.interactable = true;
        }

        selectedReward = null;
    }

    private void Update()
    {
        if (UIManager.Instance?.CurrentMenuType != menuType)
            return;

        if (InputManager.InputActions != null && InputManager.InputActions.Keyboard.Escape.triggered)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }
    }

    private void FocusReward(BossRewardDataSO reward)
    {
        if (reward == null)
            return;

        selectedReward = reward;

        foreach (DormantPowerCardUI card in rewardCards)
            card?.SetSelected(card.Reward == reward);

        detailIcon.sprite = reward.DisplayIcon;
        detailIcon.enabled = reward.DisplayIcon != null;
        detailName.text = reward.DisplayName;
        detailType.text = reward.TypeLabel;
        detailRarity.text = reward.RarityLabel;
        detailDescription.text = reward.DisplayDescription;
    }

    private void ConfirmReward()
    {
        if (selectionLocked || selectedReward == null || menuData == null)
        {
            return;
        }

        WeaponInventorySystem inventory = WeaponInventorySystem.Instance;

        bool inventoryFull = selectedReward.RewardType == BossRewardType.Weapon && inventory != null && !inventory.CheckEmptyWeaponSlots();

        selectionLocked = true;
        SetCardsInteractable(false);
        confirmButton.interactable = false;

        bool success = inventoryFull ? menuData.OnWeaponInventoryFull?.Invoke(selectedReward) == true : menuData.OnSelected?.Invoke(selectedReward) == true;

        if (!success)
        {
            selectionLocked = false;
            SetCardsInteractable(true);
            confirmButton.interactable = true;
        }
    }

    private void SetCardsInteractable(bool value)
    {
        if (rewardCards == null)
            return;

        foreach (DormantPowerCardUI card in rewardCards)
            card?.SetInteractable(value);
    }

    protected override void LoadComponent()
    {
        if (rewardCards == null || rewardCards.Length == 0)
            rewardCards = GetComponentsInChildren<DormantPowerCardUI>(true);

        Transform detailPanel = transform.Find("RewardPanel/DetailPanel");

        if (detailPanel == null)
            return;

        if (detailIcon == null)
            detailIcon = detailPanel.Find("DetailIcon")?.GetComponent<Image>();

        if (detailName == null)
            detailName = detailPanel.Find("DetailName")?.GetComponent<TMP_Text>();

        if (detailType == null)
            detailType = detailPanel.Find("DetailType")?.GetComponent<TMP_Text>();

        if (detailRarity == null)
            detailRarity = detailPanel.Find("DetailRarity")?.GetComponent<TMP_Text>();

        if (detailDescription == null)
            detailDescription =
                detailPanel.Find("DetailDescription")?.GetComponent<TMP_Text>();

        if (confirmButton == null)
            confirmButton = detailPanel.Find("ConfirmButton")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }
}
