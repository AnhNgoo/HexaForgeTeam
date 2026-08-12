using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreditsMenu : MenuBase
{
    public override MenuType menuType => MenuType.CreditsMenu;

    [Header("References")]
    [SerializeField] private Button btn_Back;
    [SerializeField] private ScrollRect creditsScroll;
    [SerializeField] private RectTransform content;

    [Header("Auto Scroll")]
    [SerializeField] private float delayBeforeScroll = 1.5f;
    [SerializeField] private float autoScrollSpeed = 35f;

    private Coroutine autoScrollRoutine;

    protected override void LoadComponent()
    {
        if (btn_Back == null)
            btn_Back = FindDeepChild("Btn_Back")?.GetComponent<Button>();

        if (creditsScroll == null)
            creditsScroll =
                FindDeepChild("CreditsScrollView")?.GetComponent<ScrollRect>();

        if (content == null && creditsScroll != null)
            content = creditsScroll.content;
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    public override void Open(object data = null)
    {
        base.Open(data);
        LoadComponentRuntime();

        if (btn_Back != null)
        {
            btn_Back.onClick.RemoveListener(OnBackClicked);
            btn_Back.onClick.AddListener(OnBackClicked);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(btn_Back.gameObject);
        }

        if (autoScrollRoutine != null)
            StopCoroutine(autoScrollRoutine);

        autoScrollRoutine = StartCoroutine(AutoScrollRoutine());
    }

    public override void Close()
    {
        if (autoScrollRoutine != null)
        {
            StopCoroutine(autoScrollRoutine);
            autoScrollRoutine = null;
        }

        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(OnBackClicked);

        base.Close();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            OnBackClicked();
        }
    }

    private IEnumerator AutoScrollRoutine()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        if (creditsScroll == null || content == null)
            yield break;

        creditsScroll.StopMovement();
        creditsScroll.verticalNormalizedPosition = 1f;

        yield return new WaitForSecondsRealtime(delayBeforeScroll);

        RectTransform viewport = creditsScroll.viewport != null
            ? creditsScroll.viewport
            : creditsScroll.GetComponent<RectTransform>();

        float scrollableHeight = Mathf.Max(
            1f,
            content.rect.height - viewport.rect.height
        );

        while (creditsScroll.verticalNormalizedPosition > 0.001f)
        {
            float normalizedSpeed =
                autoScrollSpeed / scrollableHeight;

            creditsScroll.verticalNormalizedPosition =
                Mathf.MoveTowards(
                    creditsScroll.verticalNormalizedPosition,
                    0f,
                    normalizedSpeed * Time.unscaledDeltaTime
                );

            yield return null;
        }

        autoScrollRoutine = null;
    }

    private void OnBackClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }

    private Transform FindDeepChild(string childName)
    {
        Transform[] children =
            GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}