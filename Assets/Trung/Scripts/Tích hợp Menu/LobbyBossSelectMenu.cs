using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LobbyBossSelectMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyBossSelectMenu;

    [System.Serializable]
    public class BossSelectOption
    {
        public string bossName;
        public PoolType bossPoolType;
        public Button selectButton;
        public GameObject highlightObject;

        [Header("Lock State")]
        public GameObject lockOverlay;
        public TMP_Text lockMessageText;
    }

    [Header("Boss Config List")]
    [SerializeField] private List<BossSelectOption> bossOptions = new List<BossSelectOption>();

    [Header("UI Action Buttons")]
    [SerializeField] private Button btnConfirmStartRun;

    private int selectedBossIndex = 0;

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
        base.Open(data);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        SetupBossButtons();
        RefreshBossLockStates();

        // Không tự động đè lựa chọn trước đó về Earthshaker mỗi khi menu mở lại.
        int initialIndex = 0;
        if (RunManager.Instance != null)
        {
            int savedIndex = bossOptions.FindIndex(option =>
                option != null &&
                option.bossPoolType == RunManager.Instance.SelectedFinalBossPool);

            if (savedIndex >= 0)
            {
                initialIndex = savedIndex;
            }
        }

        SelectBoss(initialIndex, isInitialOpen: true);
    }

    public override void Close()
    {
        base.Close();

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
        }
    }

    public void RefreshBossLockStates()
    {
        bool isBoss2Unlocked = PlayerPrefs.GetInt("UNLOCKED_BOSS_DARKMAGE", 0) == 1;

        for (int i = 0; i < bossOptions.Count; i++)
        {
            var option = bossOptions[i];
            if (option == null) continue;

            bool isLocked = (option.bossPoolType == PoolType.EnemyDarkMageBoss) && !isBoss2Unlocked;

            if (option.lockOverlay != null)
            {
                option.lockOverlay.SetActive(isLocked);
            }

            if (option.selectButton != null)
            {
                option.selectButton.interactable = !isLocked;
            }

            if (option.lockMessageText != null)
            {
                option.lockMessageText.gameObject.SetActive(isLocked);
            }
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

        var targetOption = bossOptions[index];
        bool isBoss2Unlocked = PlayerPrefs.GetInt("UNLOCKED_BOSS_DARKMAGE", 0) == 1;
        if (targetOption.bossPoolType == PoolType.EnemyDarkMageBoss && !isBoss2Unlocked)
        {
            if (LobbyNotifyManager.Instance != null && !isInitialOpen)
            {
                LobbyNotifyManager.Instance.ShowNotify("Defeat The Earthshaker first to unlock!", Color.red);
            }
            return;
        }

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
        string targetRunScene = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.RunGameplay) 
            : "Run Scene";

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(targetRunScene, selected.bossPoolType);
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

        // Đóng Menu chọn Boss hiện tại
        Close();

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(targetRunScene, selected.bossPoolType);
            RunManager.Instance.StartRun();
        }
        else
        {
            Debug.LogError("[LobbyBossSelectMenu] Không tìm thấy RunManager để lưu boss đã chọn.");
        }
    }

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }
}
