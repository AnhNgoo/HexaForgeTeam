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
    private SettingsDescriptionEntry entry;

    public void Configure(
        SettingsDescriptionPanel targetPanel,
        SettingsDescriptionEntry targetEntry)
    {
        panel = targetPanel;
        entry = targetEntry;
    }

    public void OnSelect(BaseEventData eventData) => ShowDescription();

    public void OnPointerEnter(PointerEventData eventData) => ShowDescription();

    public void OnPointerClick(PointerEventData eventData) => ShowDescription();

    public void OnSubmit(BaseEventData eventData) => ShowDescription();

    private void ShowDescription()
    {
        if (panel != null && entry != null)
            panel.ShowEntry(entry);
    }
}