using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapLocationMarkerUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text txtName;
    [SerializeField] private GameObject selectedFrame;

    private MapLocationData location;
    private WorldMapPanel owner;
    private bool selected;

    public void Setup(MapLocationData newLocation, WorldMapPanel newOwner)
    {
        location = newLocation;
        owner = newOwner;

        if (iconImage != null)
            iconImage.sprite = location.icon;

        if (txtName != null)
            txtName.text = location.locationName;

        SetSelected(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null)
            owner.ShowInfo(location);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected && owner != null)
            owner.HideInfo();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selected = !selected;
        SetSelected(selected);

        if (owner == null)
            return;

        if (selected)
            owner.ShowInfo(location);
        else
            owner.HideInfo();
    }

    private void SetSelected(bool value)
    {
        selected = value;

        if (selectedFrame != null)
            selectedFrame.SetActive(value);
    }
}