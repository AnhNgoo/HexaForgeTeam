using UnityEngine;
using UnityEngine.EventSystems;

public sealed class SettingDescriptionTarget :
    MonoBehaviour,
    ISelectHandler,
    IPointerEnterHandler,
    IPointerClickHandler,
    ISubmitHandler
{
    private SettingsDescriptionPanel panel;
    private string itemName;
    private string description;

    public void Configure(
        SettingsDescriptionPanel targetPanel,
        string targetItemName,
        string targetDescription)
    {
        panel = targetPanel;
        itemName = targetItemName;
        description = targetDescription;
    }

    public void OnSelect(BaseEventData eventData)
    {
        ShowDescription();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowDescription();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ShowDescription();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        ShowDescription();
    }

    private void ShowDescription()
    {
        if (panel != null)
            panel.Show(itemName, description);
    }
}