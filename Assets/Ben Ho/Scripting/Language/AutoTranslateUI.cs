using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// Tự động quét TẤT CẢ TextMeshPro trong scene và dịch những text
/// có trong từ điển SettingsLocalizationData.
/// </summary>
public class AutoTranslateUI : MonoBehaviour
{
    private readonly Dictionary<TMP_Text, string> originals = new Dictionary<TMP_Text, string>();

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        StartCoroutine(InitAndScan());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private IEnumerator InitAndScan()
    {
        yield return LocalizationSettings.InitializationOperation;
        Scan();
    }

    [ContextMenu("🔄 Quét lại text")]
    public void Scan()
    {
        originals.Clear();

        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text t in texts)
        {
            if (t == null) continue;

            // Bỏ qua 2 text description (đã có SettingsDescriptionPanel lo)
            if (t.name == "Item-Name" || t.name == "Description")
                continue;

            // Chỉ xử lý những text CÓ trong từ điển dịch
            if (SettingsLocalizationData.HasTranslation(t.text))
            {
                // ✅ Gỡ LocalizeStringEvent tàn dư (hỏng) để từ điển tự quản lý text này
                var loc = t.GetComponent<UnityEngine.Localization.Components.LocalizeStringEvent>();
                if (loc != null)
                    Destroy(loc);

                originals[t] = t.text;
            }
            // Text KHÔNG có trong từ điển → giữ nguyên (team tự quản lý)
        }

        Apply();
        Debug.Log($"✅ AutoTranslateUI: đang theo dõi {originals.Count} texts");
    }

    private void OnLocaleChanged(Locale locale)
    {
        Apply();
    }

    private void Apply()
    {
        foreach (var kv in originals)
        {
            if (kv.Key == null) continue;
            kv.Key.text = SettingsLocalizationData.Translate(kv.Value);
        }
    }
}