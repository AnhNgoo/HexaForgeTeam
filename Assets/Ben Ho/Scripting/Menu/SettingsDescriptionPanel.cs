using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public sealed class SettingsDescriptionPanel : MonoBehaviour
{
    [Header("Description UI")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Description Data")]
    [SerializeField] private int defaultEntryIndex;
    [SerializeField] private SettingsDescriptionEntry[] entries;

    private SettingsDescriptionEntry lastEntry;

    private void Reset() => ResolveTextReferences();

    private void OnValidate() => NormalizeEntryTargets();

    private void Awake()
    {
        ResolveTextReferences();
        NormalizeEntryTargets();
    }

    private void OnEnable()
    {
        ResolveTextReferences();
        NormalizeEntryTargets();
        BindEntries();
        ShowDefaultEntry();

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        // Đổi ngôn ngữ → tự dịch lại dòng đang hiển thị
        if (lastEntry != null)
            ShowEntry(lastEntry);
        else
            ShowDefaultEntry();
    }

    private void NormalizeEntryTargets()
    {
        if (!UsesContentSliderTargets() || entries == null)
            return;

        foreach (SettingsDescriptionEntry entry in entries)
        {
            if (entry == null || entry.target == null)
                continue;

            Transform contentTarget = FindContentTarget(entry.target.transform);

            if (contentTarget != null)
                entry.target = contentTarget.gameObject;
        }
    }

    private bool UsesContentSliderTargets()
    {
        return gameObject.name.StartsWith("SettingMenu") ||
               gameObject.name.StartsWith("GraphicsMenu") ||
               gameObject.name.StartsWith("ControllerMenu");
    }

    private static Transform FindContentTarget(Transform target)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.name == "Content-Slider" ||
                current.name == "Content-Selection" ||
                current.name == "Content-Checkbox")
                return current;

            current = current.parent;
        }

        return null;
    }

    private void ResolveTextReferences()
    {
        if (itemNameText == null)
            itemNameText = FindTextInside("Item-Name");

        if (descriptionText == null)
            descriptionText = FindTextInside("Description");
    }

    private TMP_Text FindTextInside(string objectName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name != objectName)
                continue;

            TMP_Text text = child.GetComponent<TMP_Text>();

            if (text == null)
                text = child.GetComponentInChildren<TMP_Text>(true);

            return text;
        }

        return null;
    }

    private void BindEntries()
    {
        if (entries == null)
            return;

        foreach (SettingsDescriptionEntry entry in entries)
        {
            if (entry == null || entry.target == null)
                continue;

            ConfigureTarget(entry.target, entry);

            Selectable[] selectables =
                entry.target.GetComponentsInChildren<Selectable>(true);

            foreach (Selectable selectable in selectables)
            {
                if (selectable != null)
                    ConfigureTarget(selectable.gameObject, entry);
            }
        }
    }

    private void ConfigureTarget(GameObject target, SettingsDescriptionEntry entry)
    {
        SettingDescriptionTarget listener =
            target.GetComponent<SettingDescriptionTarget>();

        if (listener == null)
            listener = target.AddComponent<SettingDescriptionTarget>();

        listener.Configure(this, entry);
    }

    private void ShowDefaultEntry()
    {
        if (entries == null || entries.Length == 0)
            return;

        int index = Mathf.Clamp(defaultEntryIndex, 0, entries.Length - 1);

        if (entries[index] != null)
            ShowEntry(entries[index]);
    }

    public void ShowEntry(SettingsDescriptionEntry entry)
    {
        if (entry == null)
            return;

        lastEntry = entry;

        // ✅ TỰ ĐỘNG DỊCH theo ngôn ngữ hiện tại
        if (itemNameText != null)
            itemNameText.text = SettingsLocalizationData.Translate(entry.itemName);

        if (descriptionText != null)
            descriptionText.text = SettingsLocalizationData.Translate(entry.description);
    }

    public void Show(string itemName, string description)
    {
        if (itemNameText != null)
            itemNameText.text = SettingsLocalizationData.Translate(itemName);

        if (descriptionText != null)
            descriptionText.text = SettingsLocalizationData.Translate(description);
    }
}