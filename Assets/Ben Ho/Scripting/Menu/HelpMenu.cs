using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpMenu : MenuBase
{
    public override MenuType menuType => MenuType.HelpMenu;

    [Header("Tab Buttons")]
    [SerializeField] private Button btn_Basic;
    [SerializeField] private Button btn_Combat;
    [SerializeField] private Button btn_System;

    [Header("Tab Texts")]
    [SerializeField] private TextMeshProUGUI txt_Basic;
    [SerializeField] private TextMeshProUGUI txt_Combat;
    [SerializeField] private TextMeshProUGUI txt_System;

    [Header("Tab Colors")]
    [SerializeField] private Color selectedTextColor = new Color32(229, 184, 91, 255);
    [SerializeField] private Color normalTextColor = Color.white;

    [Header("Selected Lines")]
    [SerializeField] private GameObject line_Basic;
    [SerializeField] private GameObject line_Combat;
    [SerializeField] private GameObject line_System;

    [Header("Content Panels")]
    [SerializeField] private GameObject main_Basic;
    [SerializeField] private GameObject main_Combat;
    [SerializeField] private GameObject main_System;

    [Header("Back Button")]
    [SerializeField] private Button btn_Back;

    protected override void LoadComponent()
    {
        if (btn_Basic == null)
            btn_Basic = FindDeepChild("btn_basic")?.GetComponent<Button>();

        if (btn_Combat == null)
            btn_Combat = FindDeepChild("btn_combat")?.GetComponent<Button>();

        if (btn_System == null)
            btn_System = FindDeepChild("btn_system")?.GetComponent<Button>();

        if (line_Basic == null && btn_Basic != null)
            line_Basic = btn_Basic.transform.Find("SelectedLine UI")?.gameObject;

        if (line_Combat == null && btn_Combat != null)
            line_Combat = btn_Combat.transform.Find("SelectedLine UI")?.gameObject;

        if (line_System == null && btn_System != null)
            line_System = btn_System.transform.Find("SelectedLine UI")?.gameObject;

        if (main_Basic == null)
            main_Basic = FindDeepChild("Main_Basic")?.gameObject;

        if (main_Combat == null)
            main_Combat = FindDeepChild("Main_Combat")?.gameObject;

        if (main_System == null)
            main_System = FindDeepChild("Main_System")?.gameObject;

        if (btn_Back == null)
            btn_Back = FindDeepChild("btn_Back")?.GetComponent<Button>();
        if (txt_Basic == null && btn_Basic != null)
            txt_Basic = btn_Basic.GetComponentInChildren<TextMeshProUGUI>(true);

        if (txt_Combat == null && btn_Combat != null)
            txt_Combat = btn_Combat.GetComponentInChildren<TextMeshProUGUI>(true);

        if (txt_System == null && btn_System != null)
            txt_System = btn_System.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    protected override void LoadComponentRuntime()
    {
        // Quan trọng: tự tìm component khi game chạy.
        LoadComponent();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        AddEvents();

        // Mặc định hiện Basic Controls.
        ShowBasicControls();
    }

    public override void Close()
    {
        RemoveEvents();
        base.Close();
    }

    private void Update()
    {
        // Game PC: ESC để quay lại.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackButtonClicked();
        }
    }

    private void AddEvents()
    {
        if (btn_Basic != null)
        {
            btn_Basic.onClick.RemoveListener(OnBasicButtonClicked);
            btn_Basic.onClick.AddListener(OnBasicButtonClicked);
        }

        if (btn_Combat != null)
        {
            btn_Combat.onClick.RemoveListener(OnCombatButtonClicked);
            btn_Combat.onClick.AddListener(OnCombatButtonClicked);
        }

        if (btn_System != null)
        {
            btn_System.onClick.RemoveListener(OnSystemButtonClicked);
            btn_System.onClick.AddListener(OnSystemButtonClicked);
        }

        if (btn_Back != null)
        {
            btn_Back.onClick.RemoveListener(OnBackButtonClicked);
            btn_Back.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void RemoveEvents()
    {
        if (btn_Basic != null)
            btn_Basic.onClick.RemoveListener(OnBasicButtonClicked);

        if (btn_Combat != null)
            btn_Combat.onClick.RemoveListener(OnCombatButtonClicked);

        if (btn_System != null)
            btn_System.onClick.RemoveListener(OnSystemButtonClicked);

        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(OnBackButtonClicked);
    }

    private void OnBasicButtonClicked()
    {
        ShowBasicControls();
    }

    private void OnCombatButtonClicked()
    {
        ShowCombatControls();
    }

    private void OnSystemButtonClicked()
    {
        ShowSystemControls();
    }

    private void ShowBasicControls()
    {
        SetTab(main_Basic, line_Basic);
    }

    private void ShowCombatControls()
    {
        SetTab(main_Combat, line_Combat);
    }

    private void ShowSystemControls()
    {
        SetTab(main_System, line_System);
    }

    private void SetTab(GameObject selectedPanel, GameObject selectedLine)
    {
        // Đổi nội dung panel
        if (main_Basic != null)
            main_Basic.SetActive(main_Basic == selectedPanel);

        if (main_Combat != null)
            main_Combat.SetActive(main_Combat == selectedPanel);

        if (main_System != null)
            main_System.SetActive(main_System == selectedPanel);

        // Đổi vạch vàng
        if (line_Basic != null)
            line_Basic.SetActive(line_Basic == selectedLine);

        if (line_Combat != null)
            line_Combat.SetActive(line_Combat == selectedLine);

        if (line_System != null)
            line_System.SetActive(line_System == selectedLine);

        // Đổi màu chữ tab
        SetTabTextColor(
            selectedLine == line_Basic,
            selectedLine == line_Combat,
            selectedLine == line_System
        );
    }
    private void SetTabTextColor(
        bool basicSelected,
        bool combatSelected,
        bool systemSelected
    )
    {
        if (txt_Basic != null)
            txt_Basic.color = basicSelected
                ? selectedTextColor
                : normalTextColor;

        if (txt_Combat != null)
            txt_Combat.color = combatSelected
                ? selectedTextColor
                : normalTextColor;

        if (txt_System != null)
            txt_System.color = systemSelected
                ? selectedTextColor
                : normalTextColor;
    }

    private void OnBackButtonClicked()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy UIManager.");
            return;
        }

        // Quay về PauseMenu thì game vẫn pause.
        if (HelpMenuData.BackMenu == MenuType.PauseMenu)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        UIManager.Instance.ChangeMenu(HelpMenuData.BackMenu);
    }

    private Transform FindDeepChild(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}