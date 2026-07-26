using UnityEngine;

public class LobbyRuneInventoryMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyRuneInventoryMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.OpenInventory();
        }
    }

    public override void Close()
    {
        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.CloseInventory();
        }

        gameObject.SetActive(false);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }
    }

    public bool HandleEscapeKey()
    {
        if (RuneRerollUI.Instance != null && RuneRerollUI.Instance.IsPanelActive())
        {
            RuneRerollUI.Instance.ClosePanel();
            return true;
        }

        if (RuneFilterPanel.Instance != null && RuneFilterPanel.Instance.IsPanelActive())
        {
            RuneFilterPanel.Instance.CloseFilterPanel();
            return true;
        }

        if (RuneDetailInfoPanel.Instance != null && RuneDetailInfoPanel.Instance.IsPanelActive())
        {
            RuneDetailInfoPanel.Instance.ClosePanel();
            return true;
        }

        if (RuneInventoryUI.Instance != null)
        {
            if (RuneInventoryUI.Instance.IsFusionActive())
            {
                RuneInventoryUI.Instance.ResetFusionState();
                return true;
            }

            if (RuneInventoryUI.Instance.IsSelectModeActive())
            {
                RuneInventoryUI.Instance.DisableSelectMode();
                return true;
            }

            if (RuneInventoryUI.Instance.IsItemTabActive())
            {
                RuneInventoryUI.Instance.SwitchToRuneTab();
                return true;
            }
        }

        return false;
    }

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }
}