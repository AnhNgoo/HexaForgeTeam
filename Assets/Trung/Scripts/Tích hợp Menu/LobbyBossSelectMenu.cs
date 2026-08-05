using UnityEngine;
using UnityEngine.UI;

public class LobbyBossSelectMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyBossSelectMenu;

    [Header("Confirm Action Button")]
    [SerializeField] private Button btnConfirmStartRun;

    private void Start()
    {
        if (btnConfirmStartRun != null)
        {
            btnConfirmStartRun.onClick.RemoveAllListeners();
            btnConfirmStartRun.onClick.AddListener(OnConfirmStartRun);
        }
    }

    public override void Open(object data = null)
    {
        base.Open(data); // Tween mở UI tự động từ MenuBase[cite: 9]

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }
    }

    public override void Close()
    {
        base.Close(); // Tween đóng UI tự động từ MenuBase[cite: 9]

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }
    }

    private void OnConfirmStartRun()
    {
        // Kích hoạt Start Run vào màn chơi[cite: 11]
        if (RunManager.Instance != null)
        {
            RunManager.Instance.StartRun();
        }
    }

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }
}