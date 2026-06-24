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

    private Language selectedLanguage;

    private readonly Color selectedColor =
        new Color32(255, 210, 80, 255);

    private readonly Color normalColor =
        Color.white;

    protected override void LoadComponent()
    {

        if (btn_English == null)
            btn_English = transform.Find("Main/Pop-Up/Button-English")
                ?.GetComponent<Button>();

        if (btn_Vietnamese == null)
            btn_Vietnamese = transform.Find("Main/Pop-Up/Button-Vietnamese")
                ?.GetComponent<Button>();

        if (btn_Confirm == null)
            btn_Confirm = transform.Find("Main/Pop-Up/Button-Orange")
                ?.GetComponent<Button>();

        if (btn_Cancel == null)
            btn_Cancel = transform.Find("Main/Pop-Up/Button-Grey")
                ?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        btn_English.onClick.AddListener(OnEnglishSelected);
        btn_Vietnamese.onClick.AddListener(OnVietnameseSelected);

        btn_Confirm.onClick.AddListener(OnConfirm);
        btn_Cancel.onClick.AddListener(OnCancel);

        selectedLanguage =
            (Language)PlayerPrefs.GetInt(
                "LANGUAGE",
                (int)Language.English);

        RefreshLanguageUI();
    }

    public override void Close()
    {
        base.Close();

        btn_English.onClick.RemoveAllListeners();
        btn_Vietnamese.onClick.RemoveAllListeners();

        btn_Confirm.onClick.RemoveAllListeners();
        btn_Cancel.onClick.RemoveAllListeners();
    }

    private void OnEnglishSelected()
    {
        selectedLanguage = Language.English;
        RefreshLanguageUI();
    }

    private void OnVietnameseSelected()
    {
        selectedLanguage = Language.Vietnamese;
        RefreshLanguageUI();
    }

    private void RefreshLanguageUI()
    {
        if (txt_English != null)
            txt_English.color =
                selectedLanguage == Language.English
                ? selectedColor
                : normalColor;

        if (txt_Vietnamese != null)
            txt_Vietnamese.color =
                selectedLanguage == Language.Vietnamese
                ? selectedColor
                : normalColor;
    }

    private void OnConfirm()
    {
        LocalizationManager.Instance
            .ChangeLanguage(selectedLanguage);

        UIManager.Instance.ChangeMenu(
            MenuType.TitleMenu);
    }

    private void OnCancel()
    {
        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }
}