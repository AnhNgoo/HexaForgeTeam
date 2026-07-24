using UnityEngine;

public class LobbyGachaMenu : MenuBase
{
    [SerializeField] private GameObject gachaPanelRoot; 

    public override MenuType menuType =>
        MenuType.LobbyGachaMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        if (gachaPanelRoot != null)
        {
            gachaPanelRoot.SetActive(true);
        }
    }

    public override void Close()
    {
        if (gachaPanelRoot != null)
        {
            gachaPanelRoot.SetActive(false);
        }

        gameObject.SetActive(false);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }
    }

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }
}