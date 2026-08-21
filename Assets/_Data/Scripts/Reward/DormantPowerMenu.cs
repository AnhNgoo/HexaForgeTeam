using UnityEngine;

public class DormantPowerMenu : MenuBase
{
    [SerializeField] private DormantPowerCardUI[] rewardCards;

    private DormantPowerMenuData menuData;
    private bool selectionLocked;

    public override MenuType menuType => MenuType.DormantPowerMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        menuData = data as DormantPowerMenuData;
        selectionLocked = false;

        foreach (DormantPowerCardUI card in rewardCards)
            card?.Hide();

        if (menuData?.Rewards == null)
        {
            Debug.LogWarning("DormantPowerMenu thiếu reward data.");
            return;
        }

        int count = Mathf.Min(
            rewardCards.Length,
            menuData.Rewards.Count);

        for (int i = 0; i < count; i++)
        {
            rewardCards[i].Setup(
                menuData.Rewards[i],
                HandleRewardSelected);
        }
    }

    public override void Close()
    {
        base.Close();
        menuData = null;
        selectionLocked = false;
    }

    private void Update()
    {
        if (InputManager.InputActions != null &&
            InputManager.InputActions.Keyboard.Escape.triggered)
        {
            UIManager.Instance?.ChangeMenu(MenuType.GameplayMenu);
        }
    }

    private void HandleRewardSelected(BossRewardDataSO reward)
    {
        if (selectionLocked || reward == null)
            return;

        selectionLocked = true;

        foreach (DormantPowerCardUI card in rewardCards)
            card?.SetInteractable(false);

        menuData?.OnSelected?.Invoke(reward);
    }

    protected override void LoadComponent()
    {
        if (rewardCards == null || rewardCards.Length == 0)
        {
            rewardCards = GetComponentsInChildren<
                DormantPowerCardUI>(true);
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }
}
