using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountLevelUI : LoadComponents
{
    public static AccountLevelUI Instance;

    [SerializeField]
    private TMP_Text LevelText;

    [SerializeField]
    private TMP_Text ExpText;

    [SerializeField]
    private Slider ExpBar;

    private void Awake()
    {
        Instance = this;
    }

    public void Refresh(
        int level,
        int currentExp,
        int requiredExp)
    {
        if (LevelText != null)
        {
            LevelText.text =
                level.ToString();
        }

        if (ExpText != null)
        {
            ExpText.text =
                $"{currentExp}/{requiredExp}";
        }

        if (ExpBar != null)
        {
            ExpBar.value =
                (float)currentExp /
                requiredExp;
        }
    }
    protected override void LoadComponent()
{
    if (LevelText == null)
    {
        LevelText =
            transform.Find(nameof(LevelText))
            ?.GetComponent<TMP_Text>();
    }

    if (ExpText == null)
    {
        ExpText =
            transform.Find(nameof(ExpText))
            ?.GetComponent<TMP_Text>();
    }

    if (ExpBar == null)
    {
        ExpBar =
            transform.Find(nameof(ExpBar))
            ?.GetComponent<Slider>();
    }
}

protected override void LoadComponentRuntime()
{
}
}