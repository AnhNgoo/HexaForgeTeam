using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

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
    [SerializeField] private TextMeshProUGUI txt_Level;
    [SerializeField] private TextMeshProUGUI txt_Gold;
    [SerializeField] private DOTweenAnimation goldAnimation;
    [SerializeField] private TextMeshProUGUI txt_AmountRecoveryBottle;
    [SerializeField] private DOTweenAnimation recoveryBottleAnimation;

    [Header("Stats Sliders")]
    [SerializeField] private Slider slider_HP;
    [SerializeField] private Slider slider_DelayHP;
    [SerializeField] private Slider slider_MP;
    [SerializeField] private Slider slider_DelayMP;
    [SerializeField] private Slider slider_Stamina;
    [SerializeField] private Slider slider_DelayStamina;

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

    [Header("Skill")]
    [SerializeField] private GameObject panel_Skill;
    [SerializeField] private Image img_Skill1;
    [SerializeField] private Image cooldown_Skill1;
    [SerializeField] private Image img_Skill2;
    [SerializeField] private Image cooldown_Skill2;



    private bool isGoldSubscribed;
    private CancellationTokenSource cooldownSkill1Cts;
    private CancellationTokenSource cooldownSkill2Cts;

    protected override void LoadComponent()
    {
        if (txt_AmountRecoveryBottle == null)
            txt_AmountRecoveryBottle = FindDeepChild("Txt_AmountRecoveryBottle")?.GetComponent<TextMeshProUGUI>();

        if (recoveryBottleAnimation == null)
            recoveryBottleAnimation = FindDeepChild("Txt_AmountRecoveryBottle")?.GetComponent<DOTweenAnimation>();
        if (slider_HP == null)
        {
            slider_HP =
                FindDeepChild("Slider_HP")
                    ?.GetComponent<Slider>();
        }

        if (slider_DelayHP == null)
            slider_DelayHP = FindDeepChild("Slider_DelayHP")?.GetComponent<Slider>();

        if (slider_DelayMP == null)
            slider_DelayMP = FindDeepChild("Slider_DelayMP")?.GetComponent<Slider>();

        if (slider_DelayStamina == null)
            slider_DelayStamina = FindDeepChild("Slider_DelayStamina")?.GetComponent<Slider>();

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

        if (goldAnimation == null)
            goldAnimation = FindDeepChild("Txt_Gold")?.GetComponent<DOTweenAnimation>();

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

        if (panel_Skill == null)
            panel_Skill = FindDeepChild("Panel_Skill")?.gameObject;

        if (img_Skill1 == null)
            img_Skill1 = panel_Skill?.transform.Find("Panel_Skill1/Mask/Img_Skill1")?.GetComponent<Image>();
        if (img_Skill2 == null)
            img_Skill2 = panel_Skill?.transform.Find("Panel_Skill2/Mask/Img_Skill2")?.GetComponent<Image>();
        if (cooldown_Skill1 == null)
            cooldown_Skill1 = panel_Skill?.transform.Find("Panel_Skill1/Mask/Cooldown")?.GetComponent<Image>();
        if (cooldown_Skill2 == null)
            cooldown_Skill2 = panel_Skill?.transform.Find("Panel_Skill2/Mask/Cooldown")?.GetComponent<Image>();
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
        SubscribeGold(); // Đăng ký sự kiện ở awake để đảm bảo luôn nhận được thông báo khi vàng thay đổi cho dù ở menu nào

        //NOTE - Lạy bố, code nó dài quá cái màn hình mới enter xuống cho dễ nhìn chứ cái event chưa được nửa cái màn xuống làm gì
        EventManager.Subscribe(GameEvent.OnShowTutorial, ShowTutorialPanel);
        EventManager.Subscribe(GameEvent.OnHideTutorial, HideTutorialPanel);
        EventManager.Subscribe(GameEvent.OnShowPickUpItemPanel, ShowPickUpItemPanel);
        EventManager.Subscribe(GameEvent.OnHidePickUpItemPanel, HidePickUpItemPanel);
        EventManager.Subscribe(GameEvent.OnUpdateDisplayWeapon, UpdateDisplayWeapon);
        EventManager.Subscribe(GameEvent.OnUpdateMaxHealth, UpdateMaxHealth);
        EventManager.Subscribe(GameEvent.OnUpdateHealth, UpdateHealth);
        EventManager.Subscribe(GameEvent.OnUpdateMaxStamina, UpdateMaxStamina);
        EventManager.Subscribe(GameEvent.OnUpdateStamina, UpdateStamina);
        EventManager.Subscribe(GameEvent.OnUpdateMaxMP, UpdateMaxMP);
        EventManager.Subscribe(GameEvent.OnUpdateMP, UpdateMP);
        EventManager.Subscribe(GameEvent.OnUpdateRecoveryBottle, UpdateRecoveryBottle);
        EventManager.Subscribe(GameEvent.OnSetImageSkill1, SetImageSkill1);
        EventManager.Subscribe(GameEvent.OnSetImageSkill2, SetImageSkill2);
        EventManager.Subscribe(GameEvent.OnUpdateCooldownSkill1, UpdateCooldownSkill1);
        EventManager.Subscribe(GameEvent.OnUpdateCooldownSkill2, UpdateCooldownSkill2);
    }

    private void OnDestroy()
    {
        UnsubscribeGold();

        EventManager.Unsubscribe(GameEvent.OnShowTutorial, ShowTutorialPanel);
        EventManager.Unsubscribe(GameEvent.OnHideTutorial, HideTutorialPanel);
        EventManager.Unsubscribe(GameEvent.OnShowPickUpItemPanel, ShowPickUpItemPanel);
        EventManager.Unsubscribe(GameEvent.OnHidePickUpItemPanel, HidePickUpItemPanel);
        EventManager.Unsubscribe(GameEvent.OnUpdateDisplayWeapon, UpdateDisplayWeapon);
        EventManager.Unsubscribe(GameEvent.OnUpdateMaxHealth, UpdateMaxHealth);
        EventManager.Unsubscribe(GameEvent.OnUpdateHealth, UpdateHealth);
        EventManager.Unsubscribe(GameEvent.OnUpdateMaxStamina, UpdateMaxStamina);
        EventManager.Unsubscribe(GameEvent.OnUpdateStamina, UpdateStamina);
        EventManager.Unsubscribe(GameEvent.OnUpdateMaxMP, UpdateMaxMP);
        EventManager.Unsubscribe(GameEvent.OnUpdateMP, UpdateMP);
        EventManager.Unsubscribe(GameEvent.OnUpdateRecoveryBottle, UpdateRecoveryBottle);
        EventManager.Unsubscribe(GameEvent.OnSetImageSkill1, SetImageSkill1);
        EventManager.Unsubscribe(GameEvent.OnSetImageSkill2, SetImageSkill2);
        EventManager.Unsubscribe(GameEvent.OnUpdateCooldownSkill1, UpdateCooldownSkill1);
        EventManager.Unsubscribe(GameEvent.OnUpdateCooldownSkill2, UpdateCooldownSkill2);
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        LoadComponentRuntime();

        // Souls-like: gameplay continues while menus are open.
        // Do not modify Time.timeScale here.
    }

    public override void Close()
    {
        base.Close();
    }

    private void Update()
    {
        HandleShortcutInput();

        if (InputManager.InputActions.Keyboard.Escape.triggered)
        {
            OpenInventoryMenu();
        }
    }

    private void HandleShortcutInput()
    {
        if (Keyboard.current == null ||
            UIManager.Instance == null)
        {
            return;
        }

        if (UIManager.Instance.CurrentMenuType !=
            MenuType.GameplayMenu)
        {
            return;
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            UIManager.Instance.ChangeMenu(
                MenuType.GameSystemMenu);
        }
    }

    public void OpenInventoryMenu()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.ChangeMenu(
            MenuType.InventoryMenu);
    }

    public void OpenPauseMenu()
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.ChangeMenu(
            MenuType.PauseMenu);
    }

    #region Skill

    private void SetImageSkill2(object obj)
    {
        if (obj is not Sprite skillSprite)
            return;

        if (img_Skill2 != null)
        {
            img_Skill2.sprite = skillSprite;
            cooldown_Skill2.fillAmount = 0f;
        }
    }

    private void SetImageSkill1(object obj)
    {
        if (obj is not Sprite skillSprite)
            return;

        if (img_Skill1 != null)
        {
            img_Skill1.sprite = skillSprite;
            cooldown_Skill1.fillAmount = 0f;
        }
    }

    private void UpdateCooldownSkill1(object obj)
    {
        if (obj is not float cooldown)
            return;

        if (cooldown_Skill1 != null)
        {
            cooldownSkill1Cts = ResetCooldownSource(cooldownSkill1Cts);
            StartCooldownFill(cooldown_Skill1, cooldown, cooldownSkill1Cts).Forget();
        }
    }

    private void UpdateCooldownSkill2(object obj)
    {
        if (obj is not float cooldown)
            return;

        if (cooldown_Skill2 != null)
        {
            cooldownSkill2Cts = ResetCooldownSource(cooldownSkill2Cts);
            StartCooldownFill(cooldown_Skill2, cooldown, cooldownSkill2Cts).Forget();
        }
    }

    private CancellationTokenSource ResetCooldownSource(CancellationTokenSource currentSource)
    {
        currentSource?.Cancel();
        currentSource?.Dispose();
        return new CancellationTokenSource();
    }

    private async UniTaskVoid StartCooldownFill(Image cooldownImage, float cooldown, CancellationTokenSource cooldownCts)
    {
        if (cooldown <= 0f)
        {
            cooldownImage.fillAmount = 0f;
            return;
        }

        float elapsed = 0f;
        cooldownImage.fillAmount = 1f;

        while (elapsed < cooldown)
        {
            if (cooldownCts.IsCancellationRequested || cooldownImage == null)
                return;

            elapsed += Time.unscaledDeltaTime;
            cooldownImage.fillAmount = Mathf.Clamp01(1f - (elapsed / cooldown));
            await UniTask.Yield();
        }

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
        }
    }

    #endregion

    #region Stats

    //SECTION - Health
    private void UpdateMaxHealth(object data)
    {
        if (data is not HealthData healthData)
            return;

        if (slider_HP != null)
        {
            slider_HP.maxValue = healthData.MaxHealth;
            slider_HP.value = healthData.CurrentHealth;

            slider_DelayHP.maxValue = slider_HP.maxValue;
            slider_DelayHP.value = slider_HP.value;
        }
    }
    private void UpdateHealth(object data)
    {
        if (data is not HealthData healthData)
            return;

        if (slider_HP != null)
        {
            slider_HP.DOValue(healthData.CurrentHealth, 0.1f);
            slider_DelayHP.DOValue(healthData.CurrentHealth, 2f);
        }
    }

    //!SECTION stamina
    private void UpdateMaxStamina(object data)
    {
        if (data is not StaminaData staminaData)
        {
            Debug.LogWarning("UpdateMaxStamina requires StaminaData.");
            return;
        }


        if (slider_Stamina != null)
        {
            slider_Stamina.maxValue = staminaData.MaxStamina;
            slider_Stamina.value = staminaData.CurrentStamina;

            slider_DelayStamina.maxValue = slider_Stamina.maxValue;
            slider_DelayStamina.value = slider_Stamina.value;
        }
    }

    private void UpdateStamina(object data)
    {
        if (data is not StaminaData staminaData)
            return;

        if (slider_Stamina != null)
        {
            slider_Stamina.DOValue(staminaData.CurrentStamina, 0.1f);
            slider_DelayStamina.DOValue(staminaData.CurrentStamina, 2f);
        }
    }

    private void UpdateMaxMP(object data)
    {
        if (data is not MPData mpData)
            return;

        if (slider_MP != null)
        {
            slider_MP.maxValue = mpData.MaxMP;
            slider_MP.value = mpData.CurrentMP;

            slider_DelayMP.maxValue = slider_MP.maxValue;
            slider_DelayMP.value = slider_MP.value;
        }
    }

    private void UpdateMP(object data)
    {
        if (data is not MPData mpData)
            return;

        if (slider_MP != null)
        {
            slider_MP.DOValue(mpData.CurrentMP, 0.1f);
            slider_DelayMP.DOValue(mpData.CurrentMP, 2f);
        }
    }
    #endregion

    //SECTION - Recovery Bottle
    private void UpdateRecoveryBottle(object obj)
    {
        if (obj is not int recoveryBottle)
            return;

        if (txt_AmountRecoveryBottle != null)
        {
            txt_AmountRecoveryBottle.text = recoveryBottle.ToString();
            if (recoveryBottleAnimation != null)
            {
                recoveryBottleAnimation.DORestart();
            }
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

            txt_Gold.DOBlendableColor(Color.yellow, 0.5f)
                .OnComplete(() =>
                {
                    txt_Gold.DOBlendableColor(Color.white, 0.5f);
                });
            if (goldAnimation != null)
            {
                goldAnimation.DORestart();
            }
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