using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public static class TMPTextSafeProtector
{
    /// <summary>
    /// Hàm mở rộng an toàn giúp gán Text cho TextMeshPro mà không bao giờ lo crash game do lỗi Font/Ký tự lạ
    /// </summary>
    public static void SetTextSafe(this TMP_Text tmpText, string content)
    {
        if (tmpText == null) return;

        try
        {
            if (string.IsNullOrEmpty(content))
            {
                tmpText.text = string.Empty;
                return;
            }

            // 1. Khóa bảo vệ: Tự động sửa/đóng các thẻ Rich Text bị viết thiếu cú pháp (tránh lỗi render TMPro)
            string sanitizedContent = SanitizeRichText(content);

            // 2. Kiểm tra lỗi Font tiềm ẩn, nếu có lỗi gán text, bắt try-catch để đưa về text không định dạng
            tmpText.text = sanitizedContent;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[TMP Safe Protector] Phát hiện lỗi render ký tự đặc biệt. Đang gán text dạng thô. Chi tiết: {ex.Message}");
            try
            {
                // Nếu lỗi render Rich Text, ta strip sạch toàn bộ tag và gán chữ thô (Plain Text)
                tmpText.text = Regex.Replace(content, "<[^>]*>", string.Empty);
            }
            catch
            {
                tmpText.text = "Error Text"; // Phương án dự phòng cuối cùng để tránh văng game
            }
        }
    }

    private static string SanitizeRichText(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Đếm số lượng thẻ đóng và mở để tránh lỗi thiếu thẻ làm treo TextMeshPro render
        int openTags = Regex.Matches(input, "<color=").Count + Regex.Matches(input, "<b>").Count + Regex.Matches(input, "<i>").Count;
        int closeTags = Regex.Matches(input, "</color>").Count + Regex.Matches(input, "</b>").Count + Regex.Matches(input, "</i>").Count;

        if (openTags != closeTags)
        {
            // Nếu phát hiện lệch thẻ mở/đóng, xóa sạch toàn bộ tag định dạng để bảo vệ UI khỏi crash
            return Regex.Replace(input, "<[^>]*>", string.Empty);
        }

        return input;
    }
}