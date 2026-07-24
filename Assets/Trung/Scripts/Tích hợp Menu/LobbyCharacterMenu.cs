using UnityEngine;

public class LobbyCharacterMenu : MenuBase
{
    [SerializeField] private GameObject characterPanelRoot;

    public override MenuType menuType => MenuType.LobbyCharacterMenu;

    public override void Open(object data = null)
    {
        base.Open(data);
        if (LobbyHUDTopBar.Instance != null) LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        if (characterPanelRoot != null) characterPanelRoot.SetActive(true);
    }

    public override void Close()
    {
        if (characterPanelRoot != null) characterPanelRoot.SetActive(false);
        gameObject.SetActive(false);
        if (LobbyHUDTopBar.Instance != null) LobbyHUDTopBar.Instance.ShowFullHUD();
    }

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }
}