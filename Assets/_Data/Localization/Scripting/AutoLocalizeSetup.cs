using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Linq;

public class AutoLocalizeSetup : MonoBehaviour
{
    [Header("Cấu hình")]
    [Tooltip("Tên table đã tạo")]
    public string tableName = "UI";
    
    [Tooltip("Ngôn ngữ mặc định của text trong scene")]
    public string defaultLanguage = "en";

    [ContextMenu("🔍 Tìm và Setup tất cả Text")]
    public void ScanAndSetupAllTexts()
    {
        Debug.Log("🔍 Bắt đầu quét TextMeshPro...");
        
        // Tìm tất cả TextMeshPro trong scene
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>();
        Debug.Log($"✅ Tìm thấy {allTexts.Length} TextMeshPro");

        int count = 0;
        foreach (TMP_Text text in allTexts)
        {
            // Bỏ qua text rỗng
            if (string.IsNullOrWhiteSpace(text.text))
                continue;
                
            // Bỏ qua text đã có localization
            if (text.GetComponent<UnityEngine.Localization.Components.LocalizeStringEvent>() != null)
                continue;
            
            // Tạo key từ text
            string key = GenerateKey(text.text);
            
            // Add component LocalizeStringEvent
            var localizeEvent = text.gameObject.AddComponent<UnityEngine.Localization.Components.LocalizeStringEvent>();
            
            // FIX: Dùng LocalizedString thay vì LocalizedStringReference
            localizeEvent.StringReference = new LocalizedString(key, tableName);
            
            // FIX: LocalizeStringEvent tự động update text, không cần add listener thủ công
            
            count++;
            Debug.Log($"✅ [{count}] Added: {key} = \"{text.text}\"");
        }
        
        Debug.Log($"🎉 HOÀN THÀNH! Đã setup {count} texts.");
        Debug.Log(" BÂY GIỜ: Mở Window → Localization → Tables để nhập bản dịch tiếng Việt");
    }

    private string GenerateKey(string text)
    {
        // Làm sạch text
        string clean = text.Trim();
        if (clean.Length > 30) clean = clean.Substring(0, 30);
        
        // Replace khoảng trắng và ký tự đặc biệt bằng _
        clean = System.Text.RegularExpressions.Regex.Replace(clean, @"[^a-zA-Z0-9]", "_");
        
        // Thêm timestamp để unique
        return $"Text_{clean}_{UnityEngine.Random.Range(1000, 9999)}";
    }
}