using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotifyUI : LoadComponents
{
    [SerializeField] private TextMeshProUGUI txt_Notify;
    protected override void LoadComponent()
    {
        if (txt_Notify == null)
            txt_Notify = GetComponentInChildren<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void SetDescription(string description)
    {
        if (txt_Notify != null)
            txt_Notify.text = description;
    }
}
