using UnityEngine;

public class LobbyCharacterMenu : MenuBase
{
    [SerializeField] private GameObject characterPanelRoot; // Kéo thả Panel chọn nhân vật vào đây

    public override MenuType menuType =>
        MenuType.LobbyCharacterMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        // Ẩn cụm Cấp độ tài khoản, chỉ giữ lại cụm Tiền tệ khi mở bảng Nhân Vật
        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        if (characterPanelRoot != null)
        {
            characterPanelRoot.SetActive(true);
        }
    }

    public override void Close()
    {
        if (characterPanelRoot != null)
        {
            characterPanelRoot.SetActive(false);
        }

        base.Close();

        // Hiện lại đầy đủ cả Cấp độ lẫn Tiền tệ khi quay về sảnh trống
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