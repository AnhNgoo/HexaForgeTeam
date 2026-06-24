using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageMenu : MenuBase
{
    public override MenuType menuType => MenuType.LanguageMenu;

    [Header("Buttons")]
    [SerializeField] private Button btn_English;
    [SerializeField] private Button btn_Vietnamese;
    [SerializeField] private Button btn_Confirm;
    [SerializeField] private Button btn_Cancel;

    [Header("Texts")]
    [SerializeField] private TMP_Text txt_English;
    [SerializeField] private TMP_Text txt_Vietnamese;

    private int selectedLanguage;

    private readonly Color selectedColor =
        new Color32(255, 210, 80, 255);

    private readonly Color normalColor =
        Color.white;

    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        selectedLanguage =
            PlayerPrefs.GetInt("LANGUAGE", 0);

        RefreshUI();

        btn_English.onClick.AddListener(OnEnglishClicked);
        btn_Vietnamese.onClick.AddListener(OnVietnameseClicked);

        btn_Confirm.onClick.AddListener(OnConfirmClicked);
        btn_Cancel.onClick.AddListener(OnCancelClicked);
    }

    public override void Close()
    {
        base.Close();

        btn_English.onClick.RemoveAllListeners();
        btn_Vietnamese.onClick.RemoveAllListeners();

        btn_Confirm.onClick.RemoveAllListeners();
        btn_Cancel.onClick.RemoveAllListeners();
    }

    private void OnEnglishClicked()
    {
        selectedLanguage = 0;
        RefreshUI();
    }

    private void OnVietnameseClicked()
    {
        selectedLanguage = 1;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (txt_English != null)
            txt_English.color =
                selectedLanguage == 0
                ? selectedColor
                : normalColor;

        if (txt_Vietnamese != null)
            txt_Vietnamese.color =
                selectedLanguage == 1
                ? selectedColor
                : normalColor;
    }

    private void OnConfirmClicked()
    {


        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }

    private void OnCancelClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }
}