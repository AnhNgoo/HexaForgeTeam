using UnityEngine;

public class LobbyGachaMenu : MenuBase
{
    [SerializeField] private GameObject gachaPanelRoot; // Kéo thả Gacha Panel lớn vào đây nếu cần điều khiển chủ động

    public override MenuType menuType =>
        MenuType.LobbyGachaMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        // Ẩn cụm Cấp độ tài khoản, chỉ giữ lại cụm Tiền tệ khi mở Gacha
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