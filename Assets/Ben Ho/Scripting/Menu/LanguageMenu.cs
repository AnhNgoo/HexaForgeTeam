using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Localization.Settings;

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

    private int selectedLanguage = 0;
    
    [SerializeField] private Image englishFrame;
    [SerializeField] private Image vietnameseFrame;

    private readonly Color selectedColor = new Color32(255, 210, 80, 255);
    private readonly Color normalColor = Color.white;

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }

    public override void Open(object data = null)
    {
        base.Open(data);

        selectedLanguage = PlayerPrefs.GetInt("LANGUAGE", 0);
        
        // Cập nhật UI ngay khi mở
        UpdateButtonState();
        RefreshUI();

        btn_English.onClick.AddListener(SelectEnglish);
        btn_Vietnamese.onClick.AddListener(SelectVietnamese);
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

    private void SelectEnglish()
    {
        selectedLanguage = 0;
        UpdateButtonState();
        RefreshUI();
    }

    private void SelectVietnamese()
    {
        selectedLanguage = 1;
        UpdateButtonState();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (txt_English != null)
            txt_English.color = selectedLanguage == 0 ? selectedColor : normalColor;

        if (txt_Vietnamese != null)
            txt_Vietnamese.color = selectedLanguage == 1 ? selectedColor : normalColor;
    }

    private void OnConfirmClicked()
    {
        // QUAN TRỌNG: Lưu ngôn ngữ
        PlayerPrefs.SetInt("LANGUAGE", selectedLanguage);
        PlayerPrefs.Save();
        
        string localeCode = selectedLanguage == 0 ? "en" : "vi-VN";
        StartCoroutine(ApplyLanguageAndClose(localeCode));
    }

    private IEnumerator ApplyLanguageAndClose(string localeCode)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        if (locale == null)
        {
            Debug.LogError($"Không tìm thấy locale: {localeCode}");
            yield break;
        }

        LocalizationSettings.SelectedLocale = locale;
        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }

    private void OnCancelClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }

    private void UpdateButtonState()
    {
        if (englishFrame != null)
            englishFrame.enabled = selectedLanguage == 0;
            
        if (vietnameseFrame != null)
            vietnameseFrame.enabled = selectedLanguage == 1;
    }
}