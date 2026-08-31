using System.Collections;
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

    // Lưu text GỐC tiếng Anh
    private readonly Dictionary<TMP_Text, string> originalTexts = new Dictionary<TMP_Text, string>();

    [Header("Quét định kỳ (fallback cho text động)")]
    [SerializeField] private float scanInterval = 2f;

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

        // ✅ Quét + dịch NGAY trong Awake — trước frame render đầu tiên, KHÔNG chờ init
        ScanAndApply();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        StartCoroutine(PeriodicScan());
        ScanAndApply();
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    // ✅ Scene load xong là quét + dịch NGAY LẬP TỨC (bỏ WaitForSecondsRealtime 0.25s)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ScanAndApply();
    }

    private void OnLocaleChanged(Locale locale)
    {
        ApplyTranslation();
    }

    /// <summary>Gọi mỗi khi mở menu/panel để dịch trong CÙNG frame → không chớp tiếng Anh.</summary>
    public void ScanAndApply()
    {
        ScanAll();
        ApplyTranslation();
    }

    // Fallback cho text ĐỘNG sinh ra giữa chừng khi chơi
    private IEnumerator PeriodicScan()
    {
        while (true)
        {
            yield return new WaitForSeconds(scanInterval);
            ScanAndApply();
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
                // Text ĐỘNG: game vừa đặt giá trị tiếng Anh mới → cập nhật bản gốc
                if (t.text != originalTexts[t] && SettingsLocalizationData.HasTranslation(t.text))
                    originalTexts[t] = t.text;
            }
            else if (SettingsLocalizationData.HasTranslation(t.text))
            {
                // Text MỚI → gỡ LocalizeStringEvent tàn dư rồi theo dõi
                var loc = t.GetComponent<UnityEngine.Localization.Components.LocalizeStringEvent>();
                if (loc != null)
                    Destroy(loc);

                originalTexts[t] = t.text;
            }
        }
    }

    private void ApplyTranslation()
    {
        if (originalTexts.Count == 0) return;

        int count = 0;
        foreach (var kv in originalTexts)
        {
            if (kv.Key == null) continue;

            // Translate() tự kiểm tra VI/EN bằng PlayerPrefs (sync) →
            // chạy được cả khi Localization CHƯA init xong; locale EN thì trả nguyên bản gốc
            string target = SettingsLocalizationData.Translate(kv.Value);
            if (kv.Key.text != target)
            {
                kv.Key.text = target;
                count++;
            }
        }

        if (count > 0)
            Debug.Log($"✅ [AutoTranslateUI] Đã update {count} texts");
    }

    [ContextMenu("⚡ Force Full Reset")]
    public void ForceFullReset()
    {
        originalTexts.Clear();
        ScanAndApply();
    }
}