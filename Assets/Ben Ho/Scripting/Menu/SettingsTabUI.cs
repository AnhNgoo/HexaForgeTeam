using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SettingsTabUI
{
    public Button btnAudio;
    public Button btnGraphics;
    public Button btnController;

    public TMP_Text txtAudio;
    public TMP_Text txtGraphics;
    public TMP_Text txtController;

    public GameObject lineAudio;
    public GameObject lineGraphics;
    public GameObject lineController;

    private static readonly Color SelectedColor =
        new Color(0.92549f, 0.80392f, 0.62353f, 1f);

    public void SetSelected(MenuType menuType)
    {
        SetTab(
            txtAudio,
            lineAudio,
            menuType == MenuType.SettingMenu);

        SetTab(
            txtGraphics,
            lineGraphics,
            menuType == MenuType.GraphicsMenu);

        SetTab(
            txtController,
            lineController,
            menuType == MenuType.ControllerMenu);
    }

    private void SetTab(
        TMP_Text text,
        GameObject line,
        bool selected)
    {
        if (text != null)
            text.color = selected ? SelectedColor : Color.white;

        if (line != null)
            line.SetActive(selected);
    }
}