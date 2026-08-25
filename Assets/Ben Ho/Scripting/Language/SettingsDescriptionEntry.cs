using System;
using UnityEngine;

[Serializable]
public class SettingsDescriptionEntry
{
    [Tooltip("Kéo GameObject của một dòng setting vào đây.")]
    public GameObject target;

    [Header("Localization Keys")]
    [Tooltip("Key cho tên setting (vd: ui.settings.master_volume)")]
    public string itemNameKey;

    [Tooltip("Key cho mô tả (vd: ui.settings.master_volume_desc)")]
    public string descriptionKey;

    [Header("Fallback (hiển thị nếu chưa điền key)")]
    public string itemName;

    [TextArea(2, 5)]
    public string description;
}