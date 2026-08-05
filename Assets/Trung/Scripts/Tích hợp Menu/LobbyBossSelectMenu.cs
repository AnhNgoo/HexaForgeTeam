using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyBossSelectMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyBossSelectMenu;

    [Header("Boss Config List")]
    [SerializeField] private List<BossSelectOption> bossOptions = new List<BossSelectOption>();

    [Header("UI Action Buttons")]
    [SerializeField] private Button btnConfirmStartRun;

    private int selectedBossIndex = 0;

    [System.Serializable]
    public class BossSelectOption
    {
        public string bossName;
        public PoolType bossPoolType;
        public string targetSceneName = "Run Scene";
        public Button selectButton;
        public GameObject highlightObject;
    }

    private void Start()
    {
        if (btnConfirmStartRun != null)
        {
            btnConfirmStartRun.onClick.RemoveAllListeners();
            btnConfirmStartRun.onClick.AddListener(OnConfirmStartRun);
        }

        SetupBossButtons();
    }

    public override void Open(object data = null)
    {
        base.Open(data); // Tween mở UI tự động[cite: 68]

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly(); //[cite: 68]
        }

        SelectBoss(0); // Mặc định chọn Boss đầu tiên khi mở Menu
    }

    public override void Close()
    {
        base.Close(); // Tween đóng UI tự động[cite: 68]

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD(); //[cite: 68]
        }
    }

    private void SetupBossButtons()
    {
        for (int i = 0; i < bossOptions.Count; i++)
        {
            int index = i;
            if (bossOptions[i].selectButton != null)
            {
                bossOptions[i].selectButton.onClick.RemoveAllListeners();
                bossOptions[i].selectButton.onClick.AddListener(() => SelectBoss(index));
            }
        }
    }

    public void SelectBoss(int index)
    {
        if (index < 0 || index >= bossOptions.Count) return;

        selectedBossIndex = index;

        // Cập nhật Visual Highlight
        for (int i = 0; i < bossOptions.Count; i++)
        {
            if (bossOptions[i].highlightObject != null)
            {
                bossOptions[i].highlightObject.SetActive(i == selectedBossIndex);
            }
        }

        var selected = bossOptions[selectedBossIndex];

        // Đẩy cấu hình Boss đã chọn vào RunManager theo đúng thiết lập
        if (RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(selected.targetSceneName, selected.bossPoolType); //
        }
    }

    private void OnConfirmStartRun()
    {
        if (bossOptions.Count == 0) return;

        var selected = bossOptions[selectedBossIndex];

        // Khóa cấu hình lần cuối trước khi phát lệnh Start Run
        if (RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(selected.targetSceneName, selected.bossPoolType); //
            RunManager.Instance.StartRun(); //
        }
    }

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }
}