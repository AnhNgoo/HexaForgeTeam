using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocalizationData",
    menuName = "Localization/Language Data")]
public class LocalizationData : ScriptableObject
{
    public List<LocalizationEntry> entries;
}

[Serializable]
public class LocalizationEntry
{
    public string key;
    [TextArea]
    public string value;
}