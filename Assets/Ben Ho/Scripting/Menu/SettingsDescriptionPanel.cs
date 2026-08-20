using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SettingsDescriptionEntry
{
    [Tooltip("Kéo GameObject của một dòng setting vào đây.")]
    public GameObject target;

    public string itemName;

    [TextArea(2, 5)]
    public string description;
}

public sealed class SettingsDescriptionPanel : MonoBehaviour
{
    [Header("Description UI")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Description Data")]
    [SerializeField] private int defaultEntryIndex;
    [SerializeField] private SettingsDescriptionEntry[] entries;

    private void Reset()
    {
        ResolveTextReferences();
    }

    private void Awake()
    {
        ResolveTextReferences();
    }

    private void OnEnable()
    {
        ResolveTextReferences();
        BindEntries();
        ShowDefaultEntry();
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

            // Bắt sự kiện khi người chơi bấm vào cả dòng.
            ConfigureTarget(entry.target, entry);

            // Đồng thời bắt Slider, Toggle, Button, Dropdown nằm trong dòng.
            Selectable[] selectables =
                entry.target.GetComponentsInChildren<Selectable>(true);

            foreach (Selectable selectable in selectables)
            {
                if (selectable != null)
                    ConfigureTarget(selectable.gameObject, entry);
            }
        }
    }

    private void ConfigureTarget(
        GameObject target,
        SettingsDescriptionEntry entry)
    {
        SettingDescriptionTarget listener =
            target.GetComponent<SettingDescriptionTarget>();

        if (listener == null)
            listener = target.AddComponent<SettingDescriptionTarget>();

        listener.Configure(
            this,
            entry.itemName,
            entry.description);
    }

    private void ShowDefaultEntry()
    {
        if (entries == null || entries.Length == 0)
            return;

        int index = Mathf.Clamp(
            defaultEntryIndex,
            0,
            entries.Length - 1);

        SettingsDescriptionEntry entry = entries[index];

        if (entry != null)
            Show(entry.itemName, entry.description);
    }

    public void Show(string itemName, string description)
    {
        if (itemNameText != null)
            itemNameText.text = itemName;

        if (descriptionText != null)
            descriptionText.text = description;
    }
}