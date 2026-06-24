using TMPro;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    private static readonly List<LocalizedText>
        Instances = new();

    [SerializeField]
    private string key;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent =
            GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (!Instances.Contains(this))
            Instances.Add(this);

        Refresh();
    }

    private void OnDisable()
    {
        Instances.Remove(this);
    }

    public void Refresh()
    {
        if (LocalizationManager.Instance == null)
            return;
    }

    public static void RefreshAll()
    {
        foreach (var item in Instances)
        {
            item.Refresh();
        }
    }
}