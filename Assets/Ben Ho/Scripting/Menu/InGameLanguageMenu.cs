using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Localization.Settings;

public class InGameLanguageMenu : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button btn_English;
    [SerializeField] private Button btn_Vietnamese;
    [SerializeField] private Button btn_Confirm;
    [SerializeField] private Button btn_Cancel;

    [Header("Texts")]
    [SerializeField] private TMP_Text txt_English;
    [SerializeField] private TMP_Text txt_Vietnamese;

    [Header("Selection Frames")]
    [SerializeField] private Image englishFrame;
    [SerializeField] private Image vietnameseFrame;

    [Header("Settings")]
    [SerializeField] private SystemSettingPage cancelReturnPage = SystemSettingPage.Audio;
    [SerializeField] private bool returnAfterConfirm = false;
    [SerializeField] private SystemSettingPage confirmReturnPage = SystemSettingPage.Audio;

    private int selectedLanguage;
    private bool eventsAdded;

    private readonly Color selectedColor = new Color32(255, 210, 80, 255);
    private readonly Color normalColor = Color.white;

    private void OnEnable()
    {
        AddEvents();

        // Đồng bộ lại lựa chọn hiện tại mỗi khi mở page
        selectedLanguage = PlayerPrefs.GetInt("LANGUAGE", 0);
        UpdateButtonState();
        RefreshUI();
    }

    private void OnDisable()
    {
        RemoveEvents();
    }

    private void AddEvents()
    {
        if (eventsAdded) return;

        if (btn_English != null)    btn_English.onClick.AddListener(SelectEnglish);
        if (btn_Vietnamese != null) btn_Vietnamese.onClick.AddListener(SelectVietnamese);
        if (btn_Confirm != null)    btn_Confirm.onClick.AddListener(OnConfirmClicked);
        if (btn_Cancel != null)     btn_Cancel.onClick.AddListener(OnCancelClicked);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded) return;

        if (btn_English != null)    btn_English.onClick.RemoveListener(SelectEnglish);
        if (btn_Vietnamese != null) btn_Vietnamese.onClick.RemoveListener(SelectVietnamese);
        if (btn_Confirm != null)    btn_Confirm.onClick.RemoveListener(OnConfirmClicked);
        if (btn_Cancel != null)     btn_Cancel.onClick.RemoveListener(OnCancelClicked);

        eventsAdded = false;
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

    private void UpdateButtonState()
    {
        if (englishFrame != null)
            englishFrame.enabled = selectedLanguage == 0;

        if (vietnameseFrame != null)
            vietnameseFrame.enabled = selectedLanguage == 1;
    }

    private void OnConfirmClicked()
    {
        PlayerPrefs.SetInt("LANGUAGE", selectedLanguage);
        PlayerPrefs.Save();

        string localeCode = selectedLanguage == 0 ? "en" : "vi-VN";
        StartCoroutine(ApplyLanguage(localeCode));
    }

    private IEnumerator ApplyLanguage(string localeCode)
    {
        yield return LocalizationSettings.InitializationOperation;

        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (locale == null)
        {
            Debug.LogError($"Không tìm thấy locale: {localeCode}");
            yield break;
        }

        // KHÁC với TitleMenu: đổi ngôn ngữ ngay trong game, KHÔNG ChangeMenu(TitleMenu)
        LocalizationSettings.SelectedLocale = locale;

        if (returnAfterConfirm && systemSettingsPanel != null)
            systemSettingsPanel.ShowPage(confirmReturnPage);
    }

    private void OnCancelClicked()
    {
        // Không lưu, quay về page chỉ định (giống ExitMenu.Cancel)
        if (systemSettingsPanel != null)
            systemSettingsPanel.ShowPage(cancelReturnPage);
    }
}