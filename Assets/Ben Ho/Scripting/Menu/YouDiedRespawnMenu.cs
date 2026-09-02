using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;

public class YouDiedRespawnMenu : MenuBase
{
    [Header("You Died UI Anim")]
    [SerializeField] private RectTransform deathRoot;
    [SerializeField] private Image panel_1;
    [SerializeField] private Image panel_2;
    [SerializeField] private TextMeshProUGUI txt_YouDied;

    [SerializeField] private float fadeInDuration = 0.8f; // Thời gian fade in của các UI elements
    [SerializeField] private float bounceDuration = 3f; // Thời gian để text "You Died" bounce lên và xuống
    [SerializeField] private float scaleToBlackDuration = 3f; // Thời gian để scale các panel và text "You Died" ra full màn hình và chuyển sang màu đen
    [SerializeField] private float blackoutHoldDuration = 0.6f; // Thời gian giữ màn hình đen trước khi fade out
    [SerializeField] private float fadeOutDuration = 0.9f; //  Thời gian fade out của các UI elements
    [SerializeField] private float bounceHeight = 18f; //  Chiều cao mà text "You Died" sẽ bounce lên và xuống

    [Header("Bounce Style")]
    [SerializeField] private float panel1BounceHeight = 24f;
    [SerializeField] private float panel2BounceHeight = 18f;
    [SerializeField] private float textBounceHeight = 30f;

    [SerializeField] private Color introColor = new Color(0f, 0f, 0f, 1f); // Đen màn hình
    [SerializeField] private Color outroColor = new Color(0f, 0f, 0f, 0f); // Trong suốt màn hình

    public override MenuType menuType => MenuType.YouDiedRespawnMenu;

    protected override void LoadComponent()
    {
        if (panel_1 == null)
            panel_1 = transform.Find("Panel_1").GetComponent<Image>();
        if (panel_2 == null)
            panel_2 = transform.Find("Panel_1/Panel_2").GetComponent<Image>();
        if (txt_YouDied == null)
            txt_YouDied = transform.Find("Panel_1/Txt_YouDied").GetComponent<TextMeshProUGUI>();
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

    [Button("Test You Died Menu")]
    private void DisplayYouDiedMenu()
    {
        if (panel_1 == null || panel_2 == null || txt_YouDied == null)
        {
            UIManager.Instance?.ChangeMenu(MenuType.GameplayMenu);
            return;
        }

        //  Kill bât kỳ animation nào đang chạy để tránh xung đột
        DOTween.Kill(panel_1);
        DOTween.Kill(panel_2);
        DOTween.Kill(txt_YouDied);
        DOTween.Kill(panel_1.rectTransform);
        DOTween.Kill(panel_2.rectTransform);
        DOTween.Kill(txt_YouDied.rectTransform);
        if (deathRoot != null)
            DOTween.Kill(deathRoot);

        //  Thiết lập trạng thái ban đầu cho các UI elements
        var resetColor = Color.white;
        resetColor.a = 0f;

        //  Reset trạng thái ban đầu cho các UI elements
        panel_1.color = resetColor;
        panel_2.color = resetColor;
        txt_YouDied.color = new Color(txt_YouDied.color.r, txt_YouDied.color.g, txt_YouDied.color.b, 0f);

        //  Reset scale và vị trí ban đầu
        panel_1.transform.localScale = Vector3.one;
        panel_2.transform.localScale = Vector3.one;
        txt_YouDied.rectTransform.localScale = Vector3.one;

        var panel1StartPos = panel_1.rectTransform.anchoredPosition;
        var panel2StartPos = panel_2.rectTransform.anchoredPosition;
        var textStartPos = txt_YouDied.rectTransform.anchoredPosition;

        var panel1Up = new Vector2(panel1StartPos.x, panel1StartPos.y + panel1BounceHeight);
        var panel2Up = new Vector2(panel2StartPos.x, panel2StartPos.y + panel2BounceHeight);
        var textUp = new Vector2(textStartPos.x, textStartPos.y + textBounceHeight);

        Sequence sequence = DOTween.Sequence();
        sequence.SetAutoKill(true);

        //  Thiết lập các animation cho các UI elements
        sequence.Append(panel_1.DOFade(1f, fadeInDuration).SetEase(Ease.OutSine));
        sequence.Join(panel_2.DOFade(1f, fadeInDuration).SetEase(Ease.OutSine));
        sequence.Join(txt_YouDied.DOFade(1f, fadeInDuration).SetEase(Ease.OutSine));

        sequence.AppendInterval(0.2f);

        // Chỉ nhún theo trục Y, nhưng mỗi đối tượng có biên độ và Ease khác nhau.
        sequence.Append(panel_1.rectTransform.DOAnchorPosY(panel1StartPos.y + panel1BounceHeight, bounceDuration * 0.5f).SetEase(Ease.OutBack));
        sequence.Join(panel_2.rectTransform.DOAnchorPosY(panel2StartPos.y + panel2BounceHeight, bounceDuration * 0.5f).SetEase(Ease.OutCirc));
        sequence.Join(txt_YouDied.rectTransform.DOAnchorPosY(textStartPos.y + textBounceHeight, bounceDuration * 0.5f).SetEase(Ease.OutQuad));

        sequence.Append(panel_1.rectTransform.DOAnchorPosY(panel1StartPos.y, bounceDuration * 0.5f).SetEase(Ease.InBack));
        sequence.Join(panel_2.rectTransform.DOAnchorPosY(panel2StartPos.y, bounceDuration * 0.5f).SetEase(Ease.InCirc));
        sequence.Join(txt_YouDied.rectTransform.DOAnchorPosY(textStartPos.y, bounceDuration * 0.5f).SetEase(Ease.InQuad));

        sequence.Append(panel_1.transform.DOScale(Vector3.one * 100f, scaleToBlackDuration).SetEase(Ease.OutBack));
        sequence.Join(panel_2.transform.DOScale(Vector3.one * 100f, scaleToBlackDuration).SetEase(Ease.OutBack));
        sequence.Join(panel_1.DOColor(introColor, scaleToBlackDuration).SetEase(Ease.InOutSine));
        sequence.Join(panel_2.DOColor(introColor, scaleToBlackDuration).SetEase(Ease.InOutSine));

        sequence.AppendInterval(blackoutHoldDuration);

        sequence.Append(panel_1.DOFade(0f, fadeOutDuration).SetEase(Ease.InSine));
        sequence.Join(panel_2.DOFade(0f, fadeOutDuration).SetEase(Ease.InSine));
        sequence.Join(txt_YouDied.DOFade(0f, fadeOutDuration).SetEase(Ease.InSine));

        //  Khi animation hoàn thành, chuyển sang menu khác
        sequence.OnComplete(() =>
        {
            DOTween.Kill(txt_YouDied.rectTransform);
            if (UIManager.Instance != null)
                UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
            else
                gameObject.SetActive(false);
        });
    }
}
