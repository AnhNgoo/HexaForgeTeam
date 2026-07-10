public class LobbyInventoryMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.LobbyInventoryMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.OpenInventory();
        }
    }

    public override void Close()
    {
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.CloseInventory();
        }

        base.Close();
    }

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }
}