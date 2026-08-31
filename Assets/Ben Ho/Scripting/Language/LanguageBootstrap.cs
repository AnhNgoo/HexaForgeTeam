using UnityEngine;
using UnityEngine.Localization.Settings;

public static class LanguageBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedLanguage()
    {
        int lang = PlayerPrefs.GetInt("LANGUAGE", 0);
        string code = lang == 0 ? "en" : "vi-VN";

        // Nếu locale đã sẵn sàng thì set ngay lập tức
        TrySet(code);

        // Nếu locale load async thì set bù ngay khi xong (trước khi UI nào kịp hiện sai)
        LocalizationSettings.InitializationOperation.Completed += _ => TrySet(code);
    }

    private static void TrySet(string code)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale != null && LocalizationSettings.SelectedLocale != locale)
            LocalizationSettings.SelectedLocale = locale;
    }
}