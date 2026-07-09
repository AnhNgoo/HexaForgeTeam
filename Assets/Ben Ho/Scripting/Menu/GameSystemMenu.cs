using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum GameSystemTab
{
    Map = 0,
    Inventory = 1,
    PlayerState = 2,
    System = 3
}

[Serializable]
public class GameSystemTabItem
{
    public GameSystemTab tab;
    public Toggle toggle;
    public GameObject background;
    public GameObject backgroundActive;
    public Image icon;
}

public class GameSystemMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.GameSystemMenu;

    [Header("Tab Menu Items")]
    [SerializeField]
    private GameSystemTabItem[] tabItems;

    [Header("Content Panels")]
    [SerializeField] private GameObject mapPanel;

    [SerializeField]
    private InventoryMenu inventoryMenu;

    [SerializeField]
    private GameObject playerStatePanel;

    [SerializeField]
    private GameObject systemPanel;

    [Header("Buttons")]
    [SerializeField] private Button btnClose;

    [Header("Opening")]
    [SerializeField] private GameSystemTab defaultTab =
        GameSystemTab.Map;

    [Header("Icon Colors")]
    [SerializeField] private Color normalIconColor =
        Color.white;

    [SerializeField] private Color selectedIconColor =
        new Color(1f, 0.78f, 0.25f, 1f);

    private GameSystemTab currentTab;
    [Header("Toggle Group")]
    [SerializeField] private ToggleGroup tabToggleGroup;

    private UnityAction<bool>[] tabActions;

    private int openedFrame;
    private bool eventsAdded;
    private bool cursorCaptured;

    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        openedFrame = Time.frameCount;

        CaptureCursor();
        AddEvents();

        GameSystemTab openingTab = defaultTab;

        if (data is GameSystemTab requestedTab)
            openingTab = requestedTab;

        SelectTab(openingTab);

        // Không thay đổi Time.timeScale.
    }

    public override void Close()
    {
        RemoveEvents();
        CloseAllContent();
        RestoreCursor();

        base.Close();
    }

    private void Update()
    {
        if (Time.frameCount == openedFrame)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.iKey.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseToGameplay();
        }
    }

    public void SelectTab(GameSystemTab tab)
    {
        currentTab = tab;

        bool showMap =
            tab == GameSystemTab.Map;

        bool showInventory =
            tab == GameSystemTab.Inventory;

        bool showPlayerState =
            tab == GameSystemTab.PlayerState;

        bool showSystem =
            tab == GameSystemTab.System;

        SetActive(mapPanel, showMap);
        SetInventoryActive(showInventory);
        SetActive(playerStatePanel, showPlayerState);
        SetActive(systemPanel, showSystem);

        RefreshTabVisuals();
    }

    private void SetInventoryActive(bool active)
    {
        if (inventoryMenu == null)
            return;

        if (active)
        {
            if (!inventoryMenu.gameObject.activeSelf)
                inventoryMenu.Open();
        }
        else
        {
            if (inventoryMenu.gameObject.activeSelf)
                inventoryMenu.Close();
        }
    }

    private void RefreshTabVisuals()
    {
        if (tabItems == null)
            return;

        foreach (GameSystemTabItem item in tabItems)
        {
            if (item == null)
                continue;

            bool selected =
                item.tab == currentTab;

            if (item.toggle != null)
            {
                item.toggle.SetIsOnWithoutNotify(
                    selected);
            }

            SetActive(
                item.background,
                !selected);

            SetActive(
                item.backgroundActive,
                selected);

            if (item.icon != null)
            {
                item.icon.color = selected
                    ? selectedIconColor
                    : normalIconColor;
            }
        }
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        if (tabItems != null)
        {
            tabActions =
                new UnityAction<bool>[tabItems.Length];

            for (int i = 0;
                i < tabItems.Length;
                i++)
            {
                GameSystemTabItem item =
                    tabItems[i];

                if (item == null ||
                    item.toggle == null)
                {
                    continue;
                }

                if (tabToggleGroup != null)
                {
                    item.toggle.group =
                        tabToggleGroup;
                }

                GameSystemTab tab = item.tab;

                tabActions[i] =
                    isOn => OnTabToggleChanged(
                        tab,
                        isOn);

                item.toggle.onValueChanged.AddListener(
                    tabActions[i]);
            }
        }

        if (btnClose != null)
        {
            btnClose.onClick.AddListener(
                CloseToGameplay);
        }

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        if (tabItems != null &&
            tabActions != null)
        {
            int count = Mathf.Min(
                tabItems.Length,
                tabActions.Length);

            for (int i = 0; i < count; i++)
            {
                if (tabItems[i] == null ||
                    tabItems[i].toggle == null ||
                    tabActions[i] == null)
                {
                    continue;
                }

                tabItems[i].toggle.onValueChanged
                    .RemoveListener(tabActions[i]);
            }
        }

        if (btnClose != null)
        {
            btnClose.onClick.RemoveListener(
                CloseToGameplay);
        }

        tabActions = null;
        eventsAdded = false;
    }

    private void CloseAllContent()
    {
        SetActive(mapPanel, false);
        SetInventoryActive(false);
        SetActive(playerStatePanel, false);
        SetActive(systemPanel, false);
    }

    public void CloseToGameplay()
    {
        if (UIManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        UIManager.Instance.ChangeMenu(
            MenuType.GameplayMenu);
    }

    private void CaptureCursor()
    {
        if (cursorCaptured)
            return;

        previousCursorLock =
            Cursor.lockState;

        previousCursorVisible =
            Cursor.visible;

        cursorCaptured = true;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }

    private void RestoreCursor()
    {
        if (!cursorCaptured)
            return;

        Cursor.lockState =
            previousCursorLock;

        Cursor.visible =
            previousCursorVisible;

        cursorCaptured = false;
    }

    private void SetActive(
        GameObject target,
        bool value)
    {
        if (target != null)
            target.SetActive(value);
    }

    private void OnDestroy()
    {
        RemoveEvents();
        RestoreCursor();
    }

    private void OnTabToggleChanged(GameSystemTab tab, bool isOn)
    {
        if (!isOn)
            return;

        SelectTab(tab);
    }
}