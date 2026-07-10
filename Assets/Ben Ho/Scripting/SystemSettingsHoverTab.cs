using UnityEngine;
using UnityEngine.EventSystems;

public class SystemSettingsHoverTab :
    MonoBehaviour,
    IPointerEnterHandler,
    ISelectHandler
{
    [SerializeField]
    private SystemSettingsPanel settingsPanel;

    [SerializeField]
    private SystemSettingPage page;

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        OpenPage();
    }

    public void OnSelect(
        BaseEventData eventData)
    {
        OpenPage();
    }

    private void OpenPage()
    {
        if (settingsPanel != null)
            settingsPanel.ShowPage(page);
    }
}