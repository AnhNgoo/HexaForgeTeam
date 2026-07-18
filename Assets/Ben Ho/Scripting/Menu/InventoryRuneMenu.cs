using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryRuneMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.InventoryRuneMenu;

    [Header("Rune UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private RuneInventoryUI inventoryUI;
    [SerializeField] private RuneEquipUI runeEquipUI;

    [Header("Buttons")]
    [SerializeField] private Button btnClose;

    [Header("Navigation")]
    [SerializeField] private MenuType standaloneFallback =
        MenuType.GameplayMenu;

    private RuneInventoryUI previousInventoryUI;
    private Coroutine openRoutine;
    private bool openedFromGacha;

    protected override void LoadComponent()
    {
        if (inventoryPanel == null)
        {
            Transform panel =
                FindDeepChild("InventoryPanel");

            if (panel != null)
                inventoryPanel = panel.gameObject;
        }

        if (inventoryUI == null)
        {
            inventoryUI =
                GetComponentInChildren<RuneInventoryUI>(true);
        }

        if (runeEquipUI == null)
        {
            runeEquipUI =
                GetComponentInChildren<RuneEquipUI>(true);
        }

        if (btnClose == null)
        {
            Transform close =
                FindDeepChild("CloseButton");

            if (close != null)
                btnClose = close.GetComponent<Button>();
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        LoadComponentRuntime();

        openedFromGacha =
            UIManager.Instance != null &&
            UIManager.Instance.PreviousMenuType ==
            MenuType.GachaMenu;

        BindInventoryUI();

        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(
                OnCloseClicked);

            btnClose.onClick.AddListener(
                OnCloseClicked);
        }

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine =
            StartCoroutine(OpenAfterStart());
    }

    private IEnumerator OpenAfterStart()
    {
        // InventoryUI.Start() đang tự tắt InventoryPanel.
        // Chờ Start chạy xong rồi mới mở lại.
        yield return null;

        if (!gameObject.activeInHierarchy)
            yield break;

        if (RuneInventoryManager.Instance == null)
        {
            Debug.LogError(
                "RuneInventoryManager is missing. " +
                "Open UI Menu additively from Lobby.");

            yield break;
        }

        RuneInventoryUI.Instance = inventoryUI;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);

        if (inventoryUI != null)
            inventoryUI.OpenInventory();

        if (runeEquipUI != null)
            runeEquipUI.RefreshEquipUI();

        openRoutine = null;
    }

    public override void Close()
    {
        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(
                OnCloseClicked);
        }

        if (inventoryUI != null)
            inventoryUI.CloseInventory();

        RestoreInventoryUI();

        base.Close();
    }

    private void BindInventoryUI()
    {
        if (inventoryUI == null)
            return;

        previousInventoryUI =
            RuneInventoryUI.Instance != inventoryUI
                ? RuneInventoryUI.Instance
                : null;

        // RuneCardUI sử dụng InventoryUI.Instance.
        RuneInventoryUI.Instance = inventoryUI;
    }

    private void RestoreInventoryUI()
    {
        if (RuneInventoryUI.Instance == inventoryUI)
        {
            RuneInventoryUI.Instance =
                previousInventoryUI;
        }

        previousInventoryUI = null;
    }

    private void OnCloseClicked()
    {
        if (openedFromGacha &&
            UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(
                MenuType.GachaMenu);

            return;
        }

        if (LobbyUIOverlayManager.Instance != null)
        {
            LobbyUIOverlayManager.Instance.CloseMenu();
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(
                standaloneFallback);

            return;
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        RestoreInventoryUI();
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