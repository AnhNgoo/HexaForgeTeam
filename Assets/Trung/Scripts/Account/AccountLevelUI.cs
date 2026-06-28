using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountLevelUI : LoadComponents
{
    public static AccountLevelUI Instance;

    [SerializeField]
    private TMP_Text LevelText;
    [SerializeField]
private TMP_Text UserNameText;

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
    public void SetUserName(
    string userName)
{
    if (UserNameText != null)
    {
        UserNameText.text = userName;
    }
}
    protected override void LoadComponent()
{
    if (UserNameText == null)
{
    UserNameText =
        transform.Find(nameof(UserNameText))
        ?.GetComponent<TMP_Text>();
}
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