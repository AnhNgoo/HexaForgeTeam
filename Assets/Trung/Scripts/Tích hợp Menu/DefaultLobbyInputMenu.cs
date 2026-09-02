using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DefaultLobbyInputMenu : MenuBase
{
    public override MenuType menuType => MenuType.DefaultLobbyInputMenu;

    [Header("0. Skippable Tutorial Quests (Gán QuestSO vào đây)")]
    [Tooltip("Kéo toàn bộ các QuestSO của phần Tutorial/chỉ dẫn cần skip vào danh sách này")]
    [SerializeField] private List<QuestSO> skippableTutorialQuests = new List<QuestSO>();

    [Header("1. Side Banner (Thông báo góc màn hình)")]
    [SerializeField] private GameObject skipBannerRoot;
    [SerializeField] private Button btnOpenConfirmPopup;

    [Header("2. Confirm Popup (Bảng xác nhận Skip)")]
    [SerializeField] private GameObject confirmPopupRoot;
    [SerializeField] private TMP_Text txtConfirmMessage;
    [SerializeField] private Button btnConfirmSkip;
    [SerializeField] private Button btnCancelSkip;

    [Header("3. Hotkeys")]
    [SerializeField] private KeyCode questHotkey = KeyCode.J;
    [SerializeField] private KeyCode skipTutorialHotkey = KeyCode.Tab;

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }

    private void Start()
    {
        if (btnOpenConfirmPopup != null)
        {
            btnOpenConfirmPopup.onClick.RemoveAllListeners();
            btnOpenConfirmPopup.onClick.AddListener(ShowConfirmPopup);
        }

        if (btnConfirmSkip != null)
        {
            btnConfirmSkip.onClick.RemoveAllListeners();
            btnConfirmSkip.onClick.AddListener(OnConfirmSkip);
        }

        if (btnCancelSkip != null)
        {
            btnCancelSkip.onClick.RemoveAllListeners();
            btnCancelSkip.onClick.AddListener(HideConfirmPopup);
        }

        if (confirmPopupRoot != null)
        {
            confirmPopupRoot.SetActive(false);
        }

        RefreshTutorialBannerState();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += RefreshTutorialBannerState;
        }

        RefreshTutorialBannerState();
    }

    public override void Close()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshTutorialBannerState;
        }

        if (confirmPopupRoot != null)
        {
            confirmPopupRoot.SetActive(false);
        }

        base.Close();
    }

    private void Update()
    {
        // 1. Phím J mở LobbyQuestMenu (chỉ mở khi popup xác nhận đang đóng)
        if (Input.GetKeyDown(questHotkey))
        {
            if (confirmPopupRoot == null || !confirmPopupRoot.activeSelf)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ChangeMenu(MenuType.LobbyQuestMenu);
                }
            }
        }

        // 2. Phím Tab mở / đóng popup xác nhận Skip Tutorial
        if (Input.GetKeyDown(skipTutorialHotkey))
        {
            bool isAllDone = IsTutorialFinished();
            if (!isAllDone)
            {
                if (confirmPopupRoot != null && confirmPopupRoot.activeSelf)
                {
                    HideConfirmPopup();
                }
                else
                {
                    ShowConfirmPopup();
                }
            }
        }

        // 3. Phím ESC để tắt popup nếu đang mở
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (confirmPopupRoot != null && confirmPopupRoot.activeSelf)
            {
                HideConfirmPopup();
            }
        }
    }

    private bool IsTutorialFinished()
    {
        if (QuestManager.Instance == null) return false;
        return QuestManager.Instance.AreSpecificQuestsCompleted(skippableTutorialQuests);
    }

    public void RefreshTutorialBannerState()
    {
        bool isAllDone = IsTutorialFinished();

        if (skipBannerRoot != null)
        {
            skipBannerRoot.SetActive(!isAllDone);
        }

        if (isAllDone && confirmPopupRoot != null && confirmPopupRoot.activeSelf)
        {
            HideConfirmPopup();
        }
    }

        public void ShowConfirmPopup()
    {
        if (confirmPopupRoot != null)
        {
            confirmPopupRoot.SetActive(true);
            confirmPopupRoot.transform.localScale = Vector3.one * 0.85f;
            confirmPopupRoot.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        if (txtConfirmMessage != null)
        {
            txtConfirmMessage.text = SettingsLocalizationData.IsVietnamesePublic()
                ? "Bạn có chắc muốn <b><color=#FFCC00>Bỏ qua Hướng dẫn</color></b> không?\n\nTất cả các cơ chế cơ bản sẽ được mở khóa hoàn toàn và toàn bộ phần thưởng cột mốc sẽ được nhận ngay lập tức."
                : "Are you sure you want to <b><color=#FFCC00>Skip Tutorial</color></b>?\n\nAll basic mechanics will be fully unlocked and all milestone rewards will be claimed immediately.";
        }

        // Hiện con trỏ chuột và mở khóa để click nút
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = true;
        }
    }

    public void HideConfirmPopup()
    {
        if (confirmPopupRoot != null)
        {
            confirmPopupRoot.transform.DOScale(0.85f, 0.15f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                confirmPopupRoot.SetActive(false);
            });
        }

        // Khóa lại con trỏ chuột khi quay về gameplay bình thường ở sảnh
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }
    }

    private void OnConfirmSkip()
    {
        HideConfirmPopup();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SkipAndClaimSpecificQuests(skippableTutorialQuests);
        }

        RefreshTutorialBannerState();
    }
}