using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class TutorialPanel
{
    public TutorialType tutorialType;
    public GameObject panel;
}

public class GameplayMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.GameplayMenu;

    [Header("Player Stats")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Stats Sliders")]
    [SerializeField] private Slider slider_HP;
    [SerializeField] private Slider slider_MP;
    [SerializeField] private Slider slider_Stamina;

    [Header("Level And Gold")]
    [SerializeField] private TextMeshProUGUI txt_Level;
    [SerializeField] private TextMeshProUGUI txt_Gold;

    [Header("Shortcuts")]
    [SerializeField]
    private MenuType inventoryMenuType =
        MenuType.InventoryMenu;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TutorialPanel[] tutorialPanels;

    [Header("Pick Up Item Panel")]
    [SerializeField] private GameObject pickUpItemPanel;
    [SerializeField] private TextMeshProUGUI text_Keyboard;
    [SerializeField] private TextMeshProUGUI text_Description;

    [Header("Display Item")]
    [SerializeField] private DisplayItem displayWeapon;


    private bool isGoldSubscribed;

    protected override void LoadComponent()
    {
        if (slider_HP == null)
        {
            slider_HP =
                FindDeepChild("Slider_HP")
                    ?.GetComponent<Slider>();
        }

        if (slider_MP == null)
        {
            slider_MP =
                FindDeepChild("Slider_MP")
                    ?.GetComponent<Slider>();
        }

        if (slider_Stamina == null)
        {
            slider_Stamina =
                FindDeepChild("Slider_Stamina")
                    ?.GetComponent<Slider>();
        }

        if (txt_Level == null)
        {
            txt_Level =
                FindDeepChild("Txt_Level")
                    ?.GetComponent<TextMeshProUGUI>();
        }

        if (txt_Gold == null)
        {
            txt_Gold =
                FindDeepChild("Txt_Gold")
                    ?.GetComponent<TextMeshProUGUI>();
        }

        if (tutorialPanel == null)
        {
            tutorialPanel =
                FindDeepChild("Panel_Tutorial")
                    ?.gameObject;
        }

        if (pickUpItemPanel == null)
        {
            pickUpItemPanel =
                FindDeepChild("Panel_PickUpItem")
                    ?.gameObject;
        }

        if (text_Keyboard == null)
        {
            text_Keyboard =
                FindDeepChild("Text_Keyboard")
                    ?.GetComponent<TextMeshProUGUI>();
        }

        if (text_Description == null)
        {
            text_Description =
                FindDeepChild("Text_Description")
                    ?.GetComponent<TextMeshProUGUI>();
        }

        if (displayWeapon == null)
        {
            displayWeapon =
                FindDeepChild("DisplayWeapon")
                    ?.GetComponent<DisplayItem>();
        }
    }

    protected override void LoadComponentRuntime()
    {
        if (playerStats == null)
        {
            playerStats =
                FindObjectOfType<PlayerStats>();
        }
    }

    //NOTE - Sub gold quá nhiều nơi, bị double subscribe
    private void OnEnable()
    {
        SubscribeGold();
    }

    private void OnDisable()
    {
        UnsubscribeGold();
    }

    protected override void Awake()
    {
        base.Awake();
        HidePickUpItemPanel(null);
        HideTutorialPanel(null);

        EventManager.Subscribe(
            GameEvent.OnShowTutorial,
            ShowTutorialPanel);

        EventManager.Subscribe(
            GameEvent.OnHideTutorial,
            HideTutorialPanel);

        EventManager.Subscribe(
            GameEvent.OnShowPickUpItemPanel,
            ShowPickUpItemPanel);

        EventManager.Subscribe(
            GameEvent.OnHidePickUpItemPanel,
            HidePickUpItemPanel);

        EventManager.Subscribe(GameEvent.OnUpdateDisplayWeapon, UpdateDisplayWeapon);
    }

    private void OnDestroy()
    {
        UnsubscribeGold();

        EventManager.Unsubscribe(
            GameEvent.OnShowTutorial,
            ShowTutorialPanel);

        EventManager.Unsubscribe(
            GameEvent.OnHideTutorial,
            HideTutorialPanel);

        EventManager.Unsubscribe(
            GameEvent.OnShowPickUpItemPanel,
            ShowPickUpItemPanel);

        EventManager.Unsubscribe(
            GameEvent.OnHidePickUpItemPanel,
            HidePickUpItemPanel);

        EventManager.Unsubscribe(GameEvent.OnUpdateDisplayWeapon, UpdateDisplayWeapon);
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        LoadComponentRuntime();
        SubscribeGold();
        UpdatePlayerStatsUI();

        // Souls-like: gameplay continues while menus are open.
        // Do not modify Time.timeScale here.
    }

    public override void Close()
    {
        UnsubscribeGold();
        base.Close();
    }

    private void Update()
    {
        UpdatePlayerStatsUI();
        HandleShortcutInput();

        if (InputManager.InputActions.Keyboard.Escape.triggered)
        {
            OpenInventoryMenu();
        }
    }

    private void HandleShortcutInput()
    {
        // GameSystemInputRouter handles M/I/P/ESC.
    }

    public void OpenInventoryMenu()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.ChangeMenu(
            inventoryMenuType);
    }

    public void OpenPauseMenu()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.ChangeMenu(
            MenuType.PauseMenu);
    }

    public void OnInventoryButtonClicked()
    {
        OpenInventoryMenu();
    }

    public void OnSettingsButtonClicked()
    {
        OpenPauseMenu();
    }

    private void UpdatePlayerStatsUI()
    {
        if (playerStats == null)
        {
            playerStats =
                FindObjectOfType<PlayerStats>();

            if (playerStats == null)
                return;
        }

        if (slider_HP != null)
        {
            slider_HP.maxValue =
                playerStats.maxHP;

            slider_HP.value =
                playerStats.currentHP;
        }

        if (slider_MP != null)
        {
            slider_MP.maxValue =
                playerStats.maxMP;

            slider_MP.value =
                playerStats.currentMP;
        }

        if (slider_Stamina != null)
        {
            slider_Stamina.maxValue =
                playerStats.maxStamina;

            slider_Stamina.value =
                playerStats.currentStamina;
        }

        if (txt_Level != null)
        {
            txt_Level.text =
                "Lv. " + playerStats.level;
        }
    }

    #region Gold

    private void SubscribeGold()
    {
        if (isGoldSubscribed)
            return;

        if (GoldManager.Instance == null)
            return;

        GoldManager.Instance.OnGoldChanged +=
            UpdateGoldUI;

        isGoldSubscribed = true;

        UpdateGoldUI(
            GoldManager.Instance.CurrentGold);
    }

    private void UnsubscribeGold()
    {
        if (!isGoldSubscribed)
            return;

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged -=
                UpdateGoldUI;
        }

        isGoldSubscribed = false;
    }

    private void UpdateGoldUI(int gold)
    {
        if (txt_Gold != null)
        {
            txt_Gold.text =
                gold.ToString();
        }
    }

    #endregion

    #region Tutorial

    private void ShowTutorialPanel(object data)
    {
        if (!(data is TutorialType tutorialType))
        {
            Debug.LogWarning(
                "ShowTutorialPanel requires TutorialType.");

            return;
        }

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        if (tutorialPanels == null)
            return;

        foreach (TutorialPanel item in tutorialPanels)
        {
            if (item == null ||
                item.panel == null)
            {
                continue;
            }

            item.panel.SetActive(
                item.tutorialType == tutorialType);
        }
    }

    private void HideTutorialPanel(object data)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (tutorialPanels == null)
            return;

        foreach (TutorialPanel item in tutorialPanels)
        {
            if (item != null &&
                item.panel != null)
            {
                item.panel.SetActive(false);
            }
        }
    }

    #endregion

    #region Pick Up Item

    private void ShowPickUpItemPanel(object data)
    {
        if (!(data is string interactionName))
        {
            Debug.LogWarning(
                "ShowPickUpItemPanel requires string.");

            return;
        }

        if (pickUpItemPanel == null)
            return;

        if (text_Keyboard != null)
        {
            text_Keyboard.text =
                InputManager.InputActions.Keyboard
                    .Interact
                    .GetBindingDisplayString();
        }

        if (text_Description != null)
        {
            text_Description.text =
                interactionName;
        }

        pickUpItemPanel.SetActive(true);
    }

    private void HidePickUpItemPanel(object data)
    {
        if (pickUpItemPanel != null)
        {
            pickUpItemPanel.SetActive(false);
        }
    }

    #endregion

    #region Display Item

    private void UpdateDisplayWeapon(object obj)
    {
        if (obj is WeaponData weaponData)
        {
            displayWeapon.SetDisplayItem(weaponData);
        }
        else
        {
            displayWeapon.SetDisplayItem(null);
        }
    }

    #endregion
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