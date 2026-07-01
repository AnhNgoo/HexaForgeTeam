using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    [SerializeField] private LocalizationData english;
    [SerializeField] private LocalizationData vietnamese;

    private Dictionary<string, string> currentDict =
        new Dictionary<string, string>();

    private void Awake()
    {
        Instance = this;

        LoadLanguage(Language.English);
    }

    public void LoadLanguage(Language language)
    {
        currentDict.Clear();

        LocalizationData data =
            language == Language.English
            ? english
            : vietnamese;

        foreach (var entry in data.entries)
        {
            currentDict[entry.key] = entry.value;
        }
    }

    public string Get(string key)
    {
        if (currentDict.TryGetValue(key, out string value))
            return value;

        return key;
    }
}