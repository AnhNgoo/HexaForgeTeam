using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelMenuData
{
    public int level;
    public float progress;
    public string information;
    public MenuType returnMenu = MenuType.GameplayMenu;
    public Action onConfirm;
    public Action onCancel;
}

public class LevelMenu : MenuBase
{
    public override MenuType menuType => MenuType.LevelMenu;

    [Header("Level Up UI")]
    [SerializeField] private TMP_Text txtLevel;
    [SerializeField] private TMP_Text txtInformation;
    [SerializeField] private Slider sliderProgress;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnCancel;

    private LevelMenuData currentData;

    protected override void LoadComponent()
    {
        if (sliderProgress == null)
        {
            sliderProgress =
                FindDeepChild("Progress-3")?.GetComponent<Slider>();
        }

        if (btnConfirm == null)
        {
            btnConfirm =
                FindDeepChild("Confirm-Button")?.GetComponent<Button>();
        }

        if (btnCancel == null)
        {
            btnCancel =
                FindDeepChild("Cancel-Button")?.GetComponent<Button>();
        }

        if (txtLevel == null)
        {
            Transform level = FindDeepChild("Level");

            if (level != null)
                txtLevel = level.GetComponentInChildren<TMP_Text>(true);
        }

        if (txtInformation == null)
        {
            Transform info = FindDeepChild("Info");

            if (info != null)
                txtInformation = info.GetComponentInChildren<TMP_Text>(true);
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        currentData = data as LevelMenuData;

        if (currentData == null)
        {
            currentData = new LevelMenuData
            {
                level = PlayerPrefs.GetInt("AccountLevel", 1),
                progress = 0f,
                information = "+ Character Stats",
                returnMenu = MenuType.GameplayMenu
            };
        }

        RefreshUI();
        AddEvents();
    }

    public override void Close()
    {
        RemoveEvents();
        currentData = null;
        base.Close();
    }

    private void RefreshUI()
    {
        if (txtLevel != null)
            txtLevel.text = $"Lvl {currentData.level}";

        if (txtInformation != null)
            txtInformation.text = currentData.information;

        if (sliderProgress != null)
            sliderProgress.SetValueWithoutNotify(
                Mathf.Clamp01(currentData.progress));
    }

    private void AddEvents()
    {
        if (btnConfirm != null)
            btnConfirm.onClick.AddListener(Confirm);

        if (btnCancel != null)
            btnCancel.onClick.AddListener(Cancel);
    }

    private void RemoveEvents()
    {
        if (btnConfirm != null)
            btnConfirm.onClick.RemoveListener(Confirm);

        if (btnCancel != null)
            btnCancel.onClick.RemoveListener(Cancel);
    }

    private void Confirm()
    {
        MenuType returnMenu = currentData.returnMenu;
        currentData.onConfirm?.Invoke();
        UIManager.Instance.ChangeMenu(returnMenu);
    }

    private void Cancel()
    {
        MenuType returnMenu = currentData.returnMenu;
        currentData.onCancel?.Invoke();
        UIManager.Instance.ChangeMenu(returnMenu);
    }

    private Transform FindDeepChild(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}