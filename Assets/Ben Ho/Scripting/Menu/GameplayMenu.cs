using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayMenu : MenuBase
{
    public override MenuType menuType => MenuType.GameplayMenu;

    [Header("Player Stats")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Stats Slider")]
    [SerializeField] private Slider slider_HP;
    [SerializeField] private Slider slider_MP;
    [SerializeField] private Slider slider_Stamina;

    [Header("Level")]
    [SerializeField] private TextMeshProUGUI txt_Level;

    [Header("Buttons")]
    [SerializeField] private Button btn_Settings;
    [SerializeField] private Button btn_Inventory;

    protected override void LoadComponent()
    {
        if (slider_HP == null)
            slider_HP = transform.Find("Slider_HP")?.GetComponent<Slider>();

        if (slider_MP == null)
            slider_MP = transform.Find("Slider_MP")?.GetComponent<Slider>();

        if (slider_Stamina == null)
            slider_Stamina = transform.Find("Slider_Stamina")?.GetComponent<Slider>();

        if (txt_Level == null)
            txt_Level = transform.Find("Txt_Level")?.GetComponent<TextMeshProUGUI>();

        if (btn_Settings == null)
            btn_Settings = transform.Find("Btn_Settings")?.GetComponent<Button>();

        if (btn_Inventory == null)
            btn_Inventory = transform.Find("Btn_Inventory")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        Time.timeScale = 1f;

        btn_Settings.onClick.AddListener(OnSettingsButtonClicked);
        btn_Inventory.onClick.AddListener(OnInventoryButtonClicked);

        UpdatePlayerStatsUI();
    }

    public override void Close()
    {
        base.Close();

        btn_Settings.onClick.RemoveListener(OnSettingsButtonClicked);
        btn_Inventory.onClick.RemoveListener(OnInventoryButtonClicked);
    }

    private void Update()
    {
        UpdatePlayerStatsUI();
    }

    private void UpdatePlayerStatsUI()
    {
        if (playerStats == null) return;

        if (slider_HP != null)
        {
            slider_HP.maxValue = playerStats.maxHP;
            slider_HP.value = playerStats.currentHP;
        }

        if (slider_MP != null)
        {
            slider_MP.maxValue = playerStats.maxMP;
            slider_MP.value = playerStats.currentMP;
        }

        if (slider_Stamina != null)
        {
            slider_Stamina.maxValue = playerStats.maxStamina;
            slider_Stamina.value = playerStats.currentStamina;
        }

        if (txt_Level != null)
        {
            txt_Level.text = "Lv. " + playerStats.level;
        }
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("Settings button clicked");

        Time.timeScale = 0f;

        UIManager.Instance.ChangeMenu(MenuType.PauseMenu);
    }

    private void OnInventoryButtonClicked()
    {
        Debug.Log("Inventory button clicked");

        UIManager.Instance.ChangeMenu(MenuType.InventoryMenu);
    }
}