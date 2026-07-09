using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMenu : MenuBase
{
    public override MenuType menuType => MenuType.CharacterMenu;

    [Header("Character System")]
    [SerializeField] private CharacterSelectUI characterSelectUI;
    [SerializeField] private CharacterPreviewManager previewManager;

    [Header("UI Objects")]
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnClose;
    [SerializeField] private TMP_Text txtStatus;

    [Header("Navigation")]
    [SerializeField] private MenuType fallbackMenu =
        MenuType.GameplayMenu;

    private CharacterType originalCharacter;
    private bool hasOriginalCharacter;
    private Coroutine initializeRoutine;

    protected override void LoadComponent()
    {
        if (characterPanel == null)
        {
            characterPanel =
                FindDeepChild("CharacterPanel")?.gameObject;
        }

        if (characterSelectUI == null)
        {
            characterSelectUI =
                GetComponentInChildren<CharacterSelectUI>(true);
        }

        if (btnConfirm == null)
        {
            btnConfirm =
                FindDeepChild("Confirmbtn")?.GetComponent<Button>();
        }

        /*
         * Trong hierarchy cua ban nut dong la CloseButton_1.
         * Nen keo truc tiep nut nay vao btnClose.
         */
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        LoadComponentRuntime();

        if (characterPanel != null)
            characterPanel.SetActive(true);

        RemoveEvents();
        AddEvents();

        hasOriginalCharacter = false;
        SetStatus(string.Empty);

        if (initializeRoutine != null)
            StopCoroutine(initializeRoutine);

        initializeRoutine =
            StartCoroutine(InitializeCharacterUI());
    }

    public override void Close()
    {
        if (initializeRoutine != null)
        {
            StopCoroutine(initializeRoutine);
            initializeRoutine = null;
        }

        RemoveEvents();

        if (characterPanel != null)
            characterPanel.SetActive(false);

        base.Close();
    }

    private IEnumerator InitializeCharacterUI()
    {
        float timeout = 5f;

        while (CharacterManager.Instance == null &&
               timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        initializeRoutine = null;

        if (CharacterManager.Instance == null)
        {
            SetStatus("CharacterManager is missing.");

            if (btnConfirm != null)
                btnConfirm.interactable = false;

            yield break;
        }

        /*
         * CharacterManager trong SetupLobby la manager chinh.
         * Khong can keo CharacterManager con cua UI vao script nay.
         */
        if (AccountLevelManager.Instance != null)
        {
            CharacterManager.Instance
                .CheckUnlockCharacter();
        }

        originalCharacter =
            CharacterManager.Instance.GetSelectedCharacter();

        hasOriginalCharacter = true;

        if (characterSelectUI != null)
            characterSelectUI.RefreshUI();

        RefreshLobbyPreview();

        if (btnConfirm != null)
            btnConfirm.interactable = true;
    }

    private void AddEvents()
    {
        if (btnConfirm != null)
            btnConfirm.onClick.AddListener(OnConfirmClicked);

        if (btnClose != null)
            btnClose.onClick.AddListener(OnCancelClicked);
    }

    private void RemoveEvents()
    {
        if (btnConfirm != null)
            btnConfirm.onClick.RemoveListener(OnConfirmClicked);

        if (btnClose != null)
            btnClose.onClick.RemoveListener(OnCancelClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnCancelClicked();
    }

    private void OnConfirmClicked()
    {
        if (CharacterManager.Instance == null)
        {
            SetStatus("CharacterManager is missing.");
            return;
        }

        CharacterType selected =
            CharacterManager.Instance.GetSelectedCharacter();

        SetStatus("Selected: " + selected);

        RefreshLobbyPreview();
        CloseToLobby();
    }

    private void OnCancelClicked()
    {
        /*
         * CharacterSelectUI cua Trung luu ngay khi bam avatar.
         * Khi bam X hoac Escape, khoi phuc nhan vat ban dau.
         */
        if (hasOriginalCharacter &&
            CharacterManager.Instance != null)
        {
            CharacterManager.Instance.SelectCharacter(
                originalCharacter);

            if (characterSelectUI != null)
                characterSelectUI.RefreshUI();

            RefreshLobbyPreview();
        }

        CloseToLobby();
    }

    private void RefreshLobbyPreview()
    {
        if (previewManager == null)
        {
            previewManager =
                FindFirstObjectByType<CharacterPreviewManager>();
        }

        if (previewManager != null)
            previewManager.RefreshPreview();
    }

    private void CloseToLobby()
    {
        if (LobbyUIOverlayManager.Instance != null)
        {
            LobbyUIOverlayManager.Instance.CloseMenu();
            return;
        }

        if (UIManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        MenuType backMenu =
            UIManager.Instance.PreviousMenuType;

        if (backMenu == MenuType.None ||
            backMenu == MenuType.CharacterMenu)
        {
            backMenu = fallbackMenu;
        }

        UIManager.Instance.ChangeMenu(backMenu);
    }

    private void SetStatus(string message)
    {
        if (txtStatus != null)
            txtStatus.text = message;
    }

    private Transform FindDeepChild(string childName)
    {
        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}