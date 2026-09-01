using UnityEngine.Localization.Settings;

public static class LocalizationText
{
    public const string UITable = "UI_Common";

    public static string Get(string key, string fallback)
    {
        if (string.IsNullOrWhiteSpace(key))
            return fallback;

        if (!LocalizationSettings.InitializationOperation.IsDone)
            return fallback;

        string value = LocalizationSettings.StringDatabase
            .GetLocalizedString(UITable, key);

        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value;
    }
}