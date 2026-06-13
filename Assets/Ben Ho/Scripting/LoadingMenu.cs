using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingMenu : MenuBase
{
    public override MenuType menuType => MenuType.LoadingMenu;

    [Header("Loading UI")]
    [SerializeField] private Slider slider_Loading;
    [SerializeField] private Image img_Fill;
    [SerializeField] private TextMeshProUGUI txt_Loading;

    [Header("Setting")]
    [SerializeField] private float loadingTime = 2f;

    private Coroutine loadingCoroutine;

    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        Time.timeScale = 1f;

        if (loadingCoroutine != null)
            StopCoroutine(loadingCoroutine);

        loadingCoroutine = StartCoroutine(LoadingRoutine());
    }

    public override void Close()
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }

        base.Close();
    }

    private IEnumerator LoadingRoutine()
    {
        float timer = 0f;

        SetProgress(0f);

        while (timer < loadingTime)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / loadingTime);
            SetProgress(progress);

            yield return null;
        }

        SetProgress(1f);

        yield return new WaitForSecondsRealtime(0.2f);

        UIManager.Instance.ChangeMenu(LoadingData.TargetMenu);
    }

    private void SetProgress(float value)
    {
        if (slider_Loading != null)
            slider_Loading.value = value;

        if (img_Fill != null)
            img_Fill.fillAmount = value;

        if (txt_Loading != null)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            txt_Loading.text = "Loading... " + percent + "%";
        }
    }
}