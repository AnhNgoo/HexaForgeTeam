using TMPro;
using UnityEngine;

public class LevelUpPopupUI : LoadComponents
{
    public static LevelUpPopupUI Instance;

    [SerializeField]
    private GameObject LevelUpPanel;

    [SerializeField]
    private TMP_Text TitleText;

    [SerializeField]
    private TMP_Text RewardText;

    private void Awake()
    {
        Instance = this;

        if (LevelUpPanel != null)
        {
            LevelUpPanel.SetActive(false);
        }
    }

    public void Show(
        string title,
        string reward)
    {
        if (LevelUpPanel != null)
        {
            LevelUpPanel.SetActive(true);
        }

        if (TitleText != null)
        {
            TitleText.SetTextSafe(
                title);
        }

        if (RewardText != null)
        {
            RewardText.SetTextSafe(
                reward);
        }

        CancelInvoke();

        Invoke(
            nameof(Hide),
            3f);
    }

    public void Hide()
    {
        if (LevelUpPanel != null)
        {
            LevelUpPanel.SetActive(false);
        }
    }
    protected override void LoadComponent()
{
    if (LevelUpPanel == null)
    {
        LevelUpPanel =
            transform.Find(nameof(LevelUpPanel))
            ?.gameObject;
    }

    if (TitleText == null)
    {
        TitleText =
            transform.Find(nameof(TitleText))
            ?.GetComponent<TMP_Text>();
    }

    if (RewardText == null)
    {
        RewardText =
            transform.Find(nameof(RewardText))
            ?.GetComponent<TMP_Text>();
    }
}

protected override void LoadComponentRuntime()
{
}
}