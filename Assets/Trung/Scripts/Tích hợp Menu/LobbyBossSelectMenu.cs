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
        [TextArea(2, 4)] public string bossDescription;
        public Sprite bossIcon;
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

    [Header("Map Debug Preview (Optional Text in Scene)")]
    [SerializeField] private TMP_Text txtSelectedMapDebug;

    private int selectedBossIndex = 0;
    private string previewedRunMapName = "";

    private void Start()
    {
        if (btnConfirmStartRun != null)
        {
            btnConfirmStartRun.onClick.RemoveAllListeners();
            btnConfirmStartRun.onClick.AddListener(OnConfirmStartRun);
        }

        SetupBossButtons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleForceMap();
        }
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

        RerollPreviewMap();

        SelectBoss(0, isInitialOpen: true);
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

        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
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
            var option = bossOptions[index];
            if (option == null || option.selectButton == null) continue;

            option.selectButton.onClick.RemoveAllListeners();
            option.selectButton.onClick.AddListener(() =>
            {
                SelectBoss(index, isInitialOpen: false);
            });

            UITooltipAutoTrigger tooltipTrigger = option.selectButton.GetComponent<UITooltipAutoTrigger>();
            if (tooltipTrigger == null)
            {
                tooltipTrigger = option.selectButton.gameObject.AddComponent<UITooltipAutoTrigger>();
            }

            tooltipTrigger.SetData(option.bossName, option.bossDescription, option.bossIcon);
        }
    }

    public void RerollPreviewMap()
    {
        previewedRunMapName = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetRandomRunSceneName() 
            : "Run Scene";

        UpdateMapDebugUI();
    }

    private void ToggleForceMap()
    {
        if (GameSceneData.Instance == null) return;

        string map1 = GameSceneData.Instance.GetSceneName(SceneType.RunGameplay);
        string map2 = GameSceneData.Instance.GetSceneName(SceneType.RunGameplay2);

        previewedRunMapName = (previewedRunMapName == map1) ? map2 : map1;
        UpdateMapDebugUI();
    }

    private void UpdateMapDebugUI()
    {
        if (txtSelectedMapDebug != null)
        {
            txtSelectedMapDebug.text = $"Map Target: <color=#00FFFF>{previewedRunMapName}</color>";
        }
        Debug.Log($"<color=#FF7700><b>[LobbyBossSelectMenu]</b> Map Previewed: <b>{previewedRunMapName}</b> (Nhấn 'M' để đổi Map thủ công)</color>");
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

        if (!isInitialOpen)
        {
            RerollPreviewMap();
        }

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

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(previewedRunMapName, selected.bossPoolType);
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

        if (string.IsNullOrEmpty(previewedRunMapName))
        {
            previewedRunMapName = GameSceneData.Instance != null 
                ? GameSceneData.Instance.GetRandomRunSceneName() 
                : "Run Scene";
        }

        Debug.Log($"<color=#00FF00><b>[START RUN]</b> Map: <b>{previewedRunMapName}</b> | Boss: <b>{selected.bossName}</b></color>");

        // Đóng Menu chọn Boss hiện tại
        Close();

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ConfigureRun(previewedRunMapName, selected.bossPoolType);
            RunManager.Instance.StartRun();
        }
    }

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }
}