using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class YouDiedRespawnMenu : MenuBase
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI txt_YouDied;
    [SerializeField] private Color introColor = new Color(0, 0, 0, 1f); // Đen màn hình
    [SerializeField] private Color outroColor = new Color(0, 0, 0, 0f); // Trong suốt màn hình
    public override MenuType menuType => MenuType.YouDiedRespawnMenu;

    protected override void LoadComponent()
    {
        if (background == null)
            background = GetComponent<Image>();
        if (txt_YouDied == null)
            txt_YouDied = transform.Find("Txt_YouDied")?.GetComponent<TextMeshProUGUI>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        if (data is bool isRespawn && isRespawn)
            DisplayYouDiedMenu();
    }

    private async void DisplayYouDiedMenu()
    {
        txt_YouDied.DOFade(0f, 0f); // Ẩn text "You Died" ban đầu
        background.
                DOColor(introColor, 1f).
                SetEase(Ease.OutBack).
                onComplete = () =>
                {
                    txt_YouDied.DOFade(1f, 1f).SetEase(Ease.OutBack).onComplete = () =>
                    {
                        UniTask.Delay(1000).ContinueWith(() =>
                        {
                            txt_YouDied.DOFade(0f, 1f).SetEase(Ease.InBack);
                            background.DOColor(outroColor, 1f).SetEase(Ease.InBack).onComplete = () =>
                            {
                                UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
                            };
                        });
                    };
                };

        //NOTE - Tổng 4 giây: 1 giây fade in background + 1 giây fade in text + 1 giây delay + 1 giây fade out background
    }
}
