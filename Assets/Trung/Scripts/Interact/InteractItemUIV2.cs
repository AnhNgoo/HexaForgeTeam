using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractItemUIV2 : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text interactText;

    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private Image highlightImage;

    [Header("Color")]
    [SerializeField]
    private Color normalTextColor = Color.white;

    [SerializeField]
    private Color selectedTextColor = Color.yellow;

    [SerializeField]
    private Color normalIconColor = Color.white;

    [SerializeField]
    private Color selectedIconColor = Color.yellow;

    public void Setup(
        InteractV2 interact,
        bool selected)
    {
        if (interact == null)
        {
            gameObject.SetActive(false);

            return;
        }

        if (interactText != null)
        {
            interactText.SetTextSafe(
                interact.InteractText);

            interactText.color =
                selected
                ? selectedTextColor
                : normalTextColor;
        }

        if (iconImage != null)
        {
            iconImage.sprite =
                interact.InteractIcon;

            iconImage.enabled =
                interact.InteractIcon != null;

            iconImage.color =
                selected
                ? selectedIconColor
                : normalIconColor;
        }

        if (highlightImage != null)
        {
            highlightImage.gameObject
                .SetActive(selected);
        }
    }
}