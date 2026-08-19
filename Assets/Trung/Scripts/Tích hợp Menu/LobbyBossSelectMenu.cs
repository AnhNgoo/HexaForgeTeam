using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
        base.Open(data); // Tween mở UI

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        SetupBossButtons(); // Re-bind sự kiện click để đảm bảo không bị rụng listener
        SelectBoss(0, isInitialOpen: true); // Mặc định chọn Boss 1 khi mở Menu (không bắn Notify)
    }

    public override void Close()
    {
        base.Close(); // Tween đóng UI

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
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
                bossOptions[i].selectButton.onClick.AddListener(() =>
                {
                    SelectBoss(index, isInitialOpen: false);
                });
            }
        }
    }

    public void SelectBoss(int index, bool isInitialOpen = false)
    {
        if (bossOptions == null || bossOptions.Count == 0) return;
        if (index < 0 || index >= bossOptions.Count) return;

        selectedBossIndex = index;

        for (int i = 0; i < bossOptions.Count; i++)
        {
            var option = bossOptions[i];
            bool isSelected = (i == selectedBossIndex);

            if (option.highlightObject != null)
            {
                option.highlightObject.SetActive(isSelected);

                if (isSelected && !isInitialOpen)
                {
                    option.highlightObject.transform.DOKill(true);
                    option.highlightObject.transform.localScale = Vector3.one;
                    option.highlightObject.transform.DOPunchScale(new Vector3(0.12f, 0.12f, 0f), 0.25f, 5);
                }
            }

            if (isSelected && option.selectButton != null && !isInitialOpen)
            {
                option.selectButton.transform.DOKill(true);
                option.selectButton.transform.localScale = Vector3.one;
                option.selectButton.transform.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), 0.2f, 4);
            }
        }

        var selected = bossOptions[selectedBossIndex];

        // LẤY ĐÚNG TÊN SCENE DỰA TRÊN SCENE CONFIG CÁ NHÂN (VD: RunGameTrung)
        string targetRunScene = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.RunGameplay) 
            : "Run Scene";

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(targetRunScene, selected.bossPoolType);
            Debug.Log($"<color=green>[LobbyBossSelectMenu] Target Scene Configured: {targetRunScene} | Boss: {selected.bossName}</color>");
        }

        if (!isInitialOpen && LobbyNotifyManager.Instance != null)
        {
            string notifyText = string.IsNullOrEmpty(selected.bossName) ? $"Selected Boss {index + 1}" : $"Target: {selected.bossName}";
            LobbyNotifyManager.Instance.ShowNotify(notifyText, Color.cyan);
        }
    }

    private void OnConfirmStartRun()
    {
        if (bossOptions.Count == 0) return;

        if (btnConfirmStartRun != null)
        {
            btnConfirmStartRun.transform.DOKill(true);
            btnConfirmStartRun.transform.localScale = Vector3.one;
            btnConfirmStartRun.transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.25f, 5);
        }

        var selected = bossOptions[selectedBossIndex];

        string targetRunScene = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.RunGameplay) 
            : "Run Scene";

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(targetRunScene, selected.bossPoolType);
            RunManager.Instance.StartRun();
        }
    }


    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }
}