using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField]
    private string key;

    private TMP_Text text;

    private static readonly List<LocalizedText> all =
        new List<LocalizedText>();

    private void Awake()
    {
        text = GetComponent<TMP_Text>();

        if (!all.Contains(this))
            all.Add(this);

        Refresh();
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    public void Refresh()
    {
        if (LocalizationManager.Instance == null)
            return;

        text.text =
            LocalizationManager.Instance.GetText(key);
    }

    public static void RefreshAll()
    {
        foreach (var item in all)
            item.Refresh();
    }
}