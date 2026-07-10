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
    public override MenuType menuType => MenuType.GameSystemMenu;

    [Header("Tabs")]
    [SerializeField] private ToggleGroup tabToggleGroup;
    [SerializeField] private GameSystemTabItem[] tabItems;

    [Header("Panels")]
    [SerializeField] private WorldMapPanel mapPanel;
    [SerializeField] private InventoryMenu inventoryMenu;
    [SerializeField] private PlayerStatePanel playerStatePanel;
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button btnClose;

    [Header("Default")]
    [SerializeField] private GameSystemTab defaultTab = GameSystemTab.Map;

    [Header("Visual")]
    [SerializeField] private Color normalIconColor = Color.white;
    [SerializeField] private Color selectedIconColor = new Color(1f, 0.78f, 0.25f, 1f);

    private GameSystemTab currentTab;
    private UnityAction<bool>[] tabActions;
    private bool eventsAdded;
    private int openedFrame;

    private bool cursorCaptured;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }

    public override void Open(object data = null)
    {
        base.Open(data);

        openedFrame = Time.frameCount;
        CaptureCursor();
        AddEvents();

        GameSystemTab tab = data is GameSystemTab requestedTab
            ? requestedTab
            : defaultTab;

        SelectTab(tab);
    }

    public override void Close()
    {
        RemoveEvents();
        CloseAllPanels();
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

        SetMapActive(tab == GameSystemTab.Map);
        SetInventoryActive(tab == GameSystemTab.Inventory);
        SetPlayerStateActive(tab == GameSystemTab.PlayerState);
        SetSystemActive(tab == GameSystemTab.System);

        RefreshTabs();
    }

    private void SetMapActive(bool active)
    {
        if (mapPanel == null)
            return;

        if (active)
            mapPanel.Open();
        else
            mapPanel.Close();
    }

    private void SetInventoryActive(bool active)
    {
        if (inventoryMenu == null)
            return;

        if (active)
            inventoryMenu.Open();
        else
            inventoryMenu.Close();
    }

    private void SetPlayerStateActive(bool active)
    {
        if (playerStatePanel == null)
            return;

        if (active)
            playerStatePanel.Open();
        else
            playerStatePanel.Close();
    }

    private void SetSystemActive(bool active)
    {
        if (systemSettingsPanel == null)
            return;

        if (active)
            systemSettingsPanel.Open();
        else
            systemSettingsPanel.Close();
    }

    private void RefreshTabs()
    {
        if (tabItems == null)
            return;

        foreach (GameSystemTabItem item in tabItems)
        {
            if (item == null)
                continue;

            bool selected = item.tab == currentTab;

            if (item.toggle != null)
                item.toggle.SetIsOnWithoutNotify(selected);

            if (item.background != null)
                item.background.SetActive(!selected);

            if (item.backgroundActive != null)
                item.backgroundActive.SetActive(selected);

            if (item.icon != null)
                item.icon.color = selected ? selectedIconColor : normalIconColor;
        }
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        if (tabItems != null)
        {
            tabActions = new UnityAction<bool>[tabItems.Length];

            for (int i = 0; i < tabItems.Length; i++)
            {
                GameSystemTabItem item = tabItems[i];

                if (item == null || item.toggle == null)
                    continue;

                if (tabToggleGroup != null)
                    item.toggle.group = tabToggleGroup;

                GameSystemTab tab = item.tab;
                tabActions[i] = isOn => OnTabChanged(tab, isOn);

                item.toggle.onValueChanged.AddListener(tabActions[i]);
            }
        }

        if (btnClose != null)
            btnClose.onClick.AddListener(CloseToGameplay);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        if (tabItems != null && tabActions != null)
        {
            int count = Mathf.Min(tabItems.Length, tabActions.Length);

            for (int i = 0; i < count; i++)
            {
                if (tabItems[i]?.toggle != null && tabActions[i] != null)
                    tabItems[i].toggle.onValueChanged.RemoveListener(tabActions[i]);
            }
        }

        if (btnClose != null)
            btnClose.onClick.RemoveListener(CloseToGameplay);

        tabActions = null;
        eventsAdded = false;
    }

    private void OnTabChanged(GameSystemTab tab, bool isOn)
    {
        if (!isOn)
            return;

        SelectTab(tab);
    }

    private void CloseAllPanels()
    {
        SetMapActive(false);
        SetInventoryActive(false);
        SetPlayerStateActive(false);
        SetSystemActive(false);
    }

    public void CloseToGameplay()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        else
            gameObject.SetActive(false);
    }

    private void CaptureCursor()
    {
        if (cursorCaptured)
            return;

        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cursorCaptured = true;
    }

    private void RestoreCursor()
    {
        if (!cursorCaptured)
            return;

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;

        cursorCaptured = false;
    }

    private void OnDestroy()
    {
        RemoveEvents();
        RestoreCursor();
    }
}