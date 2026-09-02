using UnityEngine;
using UnityEngine.Localization.Settings;

public static class LanguageBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedLanguage()
    {
        int lang = PlayerPrefs.GetInt("LANGUAGE", 0);
        string code = lang == 0 ? "en" : "vi-VN";

        Debug.Log($"[LanguageBootstrap] Applying saved language: {code} (PlayerPrefs LANGUAGE = {lang})");

        // Nếu locale đã sẵn sàng thì set ngay lập tức
        TrySet(code);

        // Nếu locale load async thì set bù ngay khi xong
        LocalizationSettings.InitializationOperation.Completed += _ => TrySet(code);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureAutoTranslateUI()
    {
        if (Object.FindObjectOfType<AutoTranslateUI>() == null)
        {
            var go = new GameObject("AutoTranslateUI");
            go.AddComponent<AutoTranslateUI>();
        }
    }

    private static void TrySet(string code)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale != null && LocalizationSettings.SelectedLocale != locale)
        {
            LocalizationSettings.SelectedLocale = locale;
            Debug.Log($"[LanguageBootstrap] Locale set to: {locale.Identifier.Code}");
        }
    }
}