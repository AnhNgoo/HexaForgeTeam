using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaMenu : MenuBase
{
    public override MenuType menuType => MenuType.GachaMenu;

    [Header("Gacha Objects")]
    [SerializeField] private GachaManager gachaManager;
    [SerializeField] private GameObject gachaPanel;
    [SerializeField] private GameObject resultPanel;

    [Header("Main Panel Buttons")]
    [SerializeField] private Button btnRollOne;
    [SerializeField] private Button btnRollFive;
    [SerializeField] private Button btnInventory;
    [SerializeField] private Button btnCloseMenu;

    [Header("Result Panel Buttons")]
    [SerializeField] private Button btnCloseResult;
    [SerializeField] private Button btnReroll;

    [Header("Optional")]
    [SerializeField] private TMP_Text txtCurrentGem;
    [SerializeField] private TMP_Text txtMessage;

    protected override void LoadComponent()
    {
        if (gachaManager == null)
        {
            gachaManager =
                GetComponentInChildren<GachaManager>(true);
        }

        if (gachaPanel == null)
        {
            gachaPanel =
                FindDeepChild("GachaPanel")?.gameObject;
        }

        if (resultPanel == null)
        {
            resultPanel =
                FindDeepChild("ResultPanel")?.gameObject;
        }

        if (btnRollOne == null)
        {
            btnRollOne =
                FindDeepChild("Roll1Button")?.GetComponent<Button>();
        }

        if (btnRollFive == null)
        {
            btnRollFive =
                FindDeepChild("Roll5Button")?.GetComponent<Button>();
        }

        if (btnInventory == null)
        {
            btnInventory =
                FindDeepChild("InventoryBtn")?.GetComponent<Button>();
        }

        if (btnReroll == null)
        {
            btnReroll =
                FindDeepChild("ReRollButton")?.GetComponent<Button>();
        }

        // Hai button deu ten CloseButton, nen keo tay trong Inspector.
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        LoadComponentRuntime();

        RemoveEvents();
        AddEvents();

        if (gachaManager != null)
        {
            // RuneCardUI dung static Instance de bao card da lat.
            GachaManager.Instance = gachaManager;
            gachaManager.CloseResultPanel();
        }

        if (gachaPanel != null)
            gachaPanel.SetActive(true);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        ClearMessage();
        RefreshGemUI();
        RefreshButtonState();
    }

    public override void Close()
    {
        RemoveEvents();

        if (gachaManager != null)
            gachaManager.CloseResultPanel();

        if (gachaPanel != null)
            gachaPanel.SetActive(false);

        base.Close();
    }

    private void AddEvents()
    {
        AddButton(btnRollOne, OnRollOneClicked);
        AddButton(btnRollFive, OnRollFiveClicked);
        AddButton(btnInventory, OnInventoryClicked);
        AddButton(btnCloseMenu, OnCloseMenuClicked);
        AddButton(btnCloseResult, OnCloseResultClicked);
        AddButton(btnReroll, OnRerollClicked);
    }

    private void RemoveEvents()
    {
        RemoveButton(btnRollOne, OnRollOneClicked);
        RemoveButton(btnRollFive, OnRollFiveClicked);
        RemoveButton(btnInventory, OnInventoryClicked);
        RemoveButton(btnCloseMenu, OnCloseMenuClicked);
        RemoveButton(btnCloseResult, OnCloseResultClicked);
        RemoveButton(btnReroll, OnRerollClicked);
    }

    private void OnRollOneClicked()
    {
        if (!ValidateGacha())
            return;

        /*
         * Prefab cua Trung co the da gan Roll1 trong OnClick.
         * Neu da co persistent Roll1 thi khong goi them lan nua.
         */
        if (!HasPersistentMethod(btnRollOne, "Roll1"))
            gachaManager.Roll1();

        StartCoroutine(RefreshAfterRoll());
    }

    private void OnRollFiveClicked()
    {
        if (!ValidateGacha())
            return;

        if (!HasPersistentMethod(btnRollFive, "Roll5"))
            gachaManager.Roll5();

        StartCoroutine(RefreshAfterRoll());
    }

    private void OnRerollClicked()
    {
        if (!ValidateGacha())
            return;

        if (!HasPersistentMethod(btnReroll, "ReRoll"))
            gachaManager.ReRoll();

        StartCoroutine(RefreshAfterRoll());
    }

    private void OnCloseResultClicked()
    {
        if (gachaManager == null)
            return;

        if (!HasPersistentMethod(
                btnCloseResult,
                "CloseResultPanel"))
        {
            gachaManager.CloseResultPanel();
        }
    }

    private void OnInventoryClicked()
    {
        if (UIManager.Instance == null)
        {
            ShowMessage("UIManager is missing.");
            return;
        }

        UIManager.Instance.ChangeMenu(
            MenuType.InventoryRuneMenu);
    }

    private void OnCloseMenuClicked()
    {
        /*
         * Khi mo tu Rune NPC, UI Menu duoc load Additive.
         * CloseMenu se unload UI Menu va tra dieu khien cho Lobby.
         */
        if (LobbyUIOverlayManager.Instance != null)
        {
            LobbyUIOverlayManager.Instance.CloseMenu();
            return;
        }

        // Truong hop test rieng scene UI Menu.
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(
                MenuType.GameplayMenu);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator RefreshAfterRoll()
    {
        yield return null;

        RefreshGemUI();
        RefreshButtonState();
    }

    private bool ValidateGacha()
    {
        if (gachaManager == null)
        {
            ShowMessage("GachaManager is missing.");
            return false;
        }

        if (GemManager.Instance == null)
        {
            ShowMessage(
                "GemManager is missing. Open Gacha from Lobby.");
            return false;
        }

        if (RuneInventoryManager.Instance == null)
        {
            ShowMessage(
                "RuneInventoryManager is missing.");
            return false;
        }

        GachaManager.Instance = gachaManager;

        ClearMessage();
        return true;
    }

    private void RefreshGemUI()
    {
        if (txtCurrentGem == null)
            return;

        txtCurrentGem.text =
            GemManager.Instance != null
                ? GemManager.Instance.GetCurrentGem().ToString()
                : "0";
    }

    private void RefreshButtonState()
    {
        bool ready =
            gachaManager != null &&
            GemManager.Instance != null &&
            RuneInventoryManager.Instance != null;

        if (btnRollOne != null)
            btnRollOne.interactable = ready;

        if (btnRollFive != null)
            btnRollFive.interactable = ready;

        if (btnReroll != null)
            btnReroll.interactable = ready;
    }

    private bool HasPersistentMethod(
        Button button,
        string methodName)
    {
        if (button == null)
            return false;

        for (int i = 0;
             i < button.onClick.GetPersistentEventCount();
             i++)
        {
            if (button.onClick.GetPersistentMethodName(i)
                == methodName)
            {
                return true;
            }
        }

        return false;
    }

    private void ShowMessage(string message)
    {
        if (txtMessage != null)
            txtMessage.text = message;

        Debug.LogWarning(message);
    }

    private void ClearMessage()
    {
        if (txtMessage != null)
            txtMessage.text = string.Empty;
    }

    private void AddButton(
        Button button,
        UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    private void RemoveButton(
        Button button,
        UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.RemoveListener(action);
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