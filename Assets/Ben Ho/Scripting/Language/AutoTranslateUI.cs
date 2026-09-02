using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class AutoTranslateUI : MonoBehaviour
{
    private static AutoTranslateUI instance;
    public static AutoTranslateUI Instance => instance;

    // Danh sách text do script khác tự dịch (DialogueUI, Typewriter...)
    public static readonly HashSet<TMP_Text> IgnoredTexts = new HashSet<TMP_Text>();

    // Lưu text GỐC tiếng Anh
    private readonly Dictionary<TMP_Text, string> originalTexts = new Dictionary<TMP_Text, string>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Dịch ngay lập tức khi vừa Awake
        ScanAndApplyInstant();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        ScanAndApplyInstant();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    // Khi vừa đổi ngôn ngữ trong cài đặt -> Áp dụng ngay
    private void OnLocaleChanged(Locale locale)
    {
        ApplyAllTranslations();
    }

    // Khi scene vừa load xong -> Dịch ngay frame đầu tiên
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ScanAndApplyInstant();
    }

    // ✅ LateUpdate chạy ở cuối mỗi frame TRƯỚC KHI vẽ lên màn hình
    // Đảm bảo không bao giờ bị chớp tiếng Anh dù panel vừa mới bật lên
    private void LateUpdate()
    {
        if (!SettingsLocalizationData.IsVietnamesePublic()) return;

        // Quét nhanh các text active đang hiển thị trên màn hình
        var activeTexts = FindObjectsOfType<TMP_Text>(false);
        for (int i = 0; i < activeTexts.Length; i++)
        {
            TMP_Text t = activeTexts[i];
            if (t == null || IgnoredTexts.Contains(t)) continue;
            if (t.name == "Item-Name" || t.name == "Description") continue;

            if (!originalTexts.TryGetValue(t, out string original))
            {
                if (SettingsLocalizationData.HasTranslation(t.text))
                {
                    original = t.text;
                    originalTexts[t] = original;
                    t.text = SettingsLocalizationData.Translate(original);
                }
            }
            else
            {
                // Nếu text bị script khác đổi lại tiếng Anh -> dịch lại ngay lập tức
                if (t.text == original)
                {
                    t.text = SettingsLocalizationData.Translate(original);
                }
                else if (SettingsLocalizationData.HasTranslation(t.text))
                {
                    originalTexts[t] = t.text;
                    t.text = SettingsLocalizationData.Translate(t.text);
                }
            }
        }
    }

    public void ScanAndApplyInstant()
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        bool isVI = SettingsLocalizationData.IsVietnamesePublic();

        foreach (TMP_Text t in texts)
        {
            if (t == null || IgnoredTexts.Contains(t)) continue;
            if (t.name == "Item-Name" || t.name == "Description") continue;

            if (!originalTexts.ContainsKey(t))
            {
                if (SettingsLocalizationData.HasTranslation(t.text))
                {
                    originalTexts[t] = t.text;
                    if (isVI)
                    {
                        t.text = SettingsLocalizationData.Translate(t.text);
                    }
                }
            }
            else
            {
                if (isVI)
                {
                    t.text = SettingsLocalizationData.Translate(originalTexts[t]);
                }
            }
        }
    }

    private void ApplyAllTranslations()
    {
        bool isVI = SettingsLocalizationData.IsVietnamesePublic();
        foreach (var kv in originalTexts)
        {
            if (kv.Key == null || IgnoredTexts.Contains(kv.Key)) continue;
            kv.Key.text = isVI ? SettingsLocalizationData.Translate(kv.Value) : kv.Value;
        }
    }

    public static void IgnoreText(TMP_Text t)
    {
        if (t == null) return;
        IgnoredTexts.Add(t);
        if (instance != null)
            instance.originalTexts.Remove(t);
    }
}