using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class AutoTranslateUI : MonoBehaviour
{
    // ✅ THÊM: singleton chống trùng lặp
    private static AutoTranslateUI instance;

    // ✅ Lưu text GỐC tiếng Anh - chỉ lưu 1 lần, không ghi đè
    private readonly Dictionary<TMP_Text, string> originalTexts = new Dictionary<TMP_Text, string>();
    private bool isInitialized = false;

    [Header("Quét định kỳ (panel mở sau)")]
    [SerializeField] private float scanInterval = 2f;

    // ✅ THÊM: Awake để setup singleton + DontDestroyOnLoad
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // ✅ Sống qua mọi scene

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        StartCoroutine(InitAndScan());
        StartCoroutine(PeriodicScan());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        StopAllCoroutines();
    }

    // ✅ THÊM: cleanup khi bị hủy
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    // ✅ THÊM: scene mới (Tutorial...) → quét ngay, không cần chờ 2s
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ScanAfterDelay(0.25f));
    }

    private IEnumerator ScanAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ScanAll();
        ApplyTranslation();
    }

    private IEnumerator InitAndScan()
    {
        yield return LocalizationSettings.InitializationOperation;
        yield return null;

        if (!isInitialized)
        {
            ScanAll();
            isInitialized = true;
            Debug.Log($"✅ [AutoTranslateUI] Đã lưu {originalTexts.Count} text gốc");
        }

        ApplyTranslation();
    }

    // ✅ Mỗi 2 giây quét lại: bắt text MỚI xuất hiện + cập nhật text ĐỘNG
    private IEnumerator PeriodicScan()
    {
        while (true)
        {
            yield return new WaitForSeconds(scanInterval);
            ScanAll();
            ApplyTranslation();
        }
    }

    private void ScanAll()
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);

        foreach (TMP_Text t in texts)
        {
            if (t == null) continue;
            if (t.name == "Item-Name" || t.name == "Description") continue;

            if (originalTexts.ContainsKey(t))
            {
                // ✅ Text ĐỘNG: game vừa đặt giá trị tiếng Anh mới → cập nhật text gốc
                if (t.text != originalTexts[t] && SettingsLocalizationData.HasTranslation(t.text))
                {
                    originalTexts[t] = t.text;
                }
            }
            else if (SettingsLocalizationData.HasTranslation(t.text))
            {
                // ✅ Text MỚI → gỡ LocalizeStringEvent tàn dư rồi theo dõi
                var loc = t.GetComponent<UnityEngine.Localization.Components.LocalizeStringEvent>();
                if (loc != null)
                    Destroy(loc);

                originalTexts[t] = t.text;
            }
        }
    }

    private void OnLocaleChanged(Locale locale)
    {
        Debug.Log($"🌍 [AutoTranslateUI] Locale changed: {locale?.Identifier.Code ?? "null"}");
        ApplyTranslation();
    }

    private void ApplyTranslation()
    {
        if (originalTexts.Count == 0) return;

        string currentLocale = "en";
        try
        {
            currentLocale = LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";
        }
        catch { }

        bool isVietnamese = currentLocale.ToLower().StartsWith("vi");

        int count = 0;
        foreach (var kv in originalTexts)
        {
            if (kv.Key == null) continue;

            if (isVietnamese)
            {
                string translated = SettingsLocalizationData.Translate(kv.Value);
                if (kv.Key.text != translated)
                {
                    kv.Key.text = translated;
                    count++;
                }
            }
            else
            {
                // ✅ Trả về text GỐC tiếng Anh
                if (kv.Key.text != kv.Value)
                {
                    kv.Key.text = kv.Value;
                    count++;
                }
            }
        }

        if (count > 0)
            Debug.Log($"✅ [AutoTranslateUI] Đã update {count} texts (Locale: {currentLocale})");
    }

    [ContextMenu("⚡ Force Full Reset")]
    public void ForceFullReset()
    {
        isInitialized = false;
        originalTexts.Clear();
        ScanAll();
        ApplyTranslation();
    }
}