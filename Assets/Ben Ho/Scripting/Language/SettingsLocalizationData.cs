using System.Collections.Generic;
using UnityEngine.Localization.Settings;

public static class SettingsLocalizationData
{
    // (English, Vietnamese)
    private static readonly (string en, string vi)[] Entries = new (string, string)[]
    {
        // ========== AUDIO ==========
        ("Master Volume", "Âm lượng tổng"),
        ("Controls the overall volume of all game audio.", "Điều khiển âm lượng tổng thể của toàn bộ âm thanh trong game."),
        ("Music Volume", "Âm lượng nhạc"),
        ("Controls the volume of background music without affecting dialogue or sound effects.", "Điều khiển âm lượng nhạc nền mà không ảnh hưởng đến hội thoại hay hiệu ứng âm thanh."),
        ("Sound Effects Volume", "Âm lượng hiệu ứng"),
        ("Controls gameplay sound effects such as attacks, impacts, enemies, and interface sounds.", "Điều khiển các hiệu ứng âm thanh trong game như tấn công, va chạm, kẻ địch và âm thanh giao diện."),
        ("Dialogue Volume", "Âm lượng hội thoại"),
        ("Controls the volume of spoken dialogue and character voice lines.", "Điều khiển âm lượng lời thoại và giọng nói của nhân vật."),
        ("Background Sound", "Âm thanh nền"),
        ("Enables or disables background music.", "Bật hoặc tắt nhạc nền."),
        ("Collision Sound", "Âm thanh va chạm"),
        ("Enables or disables collision and impact sounds.", "Bật hoặc tắt âm thanh va chạm và tác động."),

        // ========== GRAPHICS ==========
        ("Screen Resolution", "Độ phân giải màn hình"),
        ("Changes the number of pixels displayed on screen. Higher resolutions look sharper but may reduce performance.", "Thay đổi số lượng điểm ảnh hiển thị trên màn hình. Độ phân giải cao giúp hình ảnh sắc nét hơn nhưng có thể giảm hiệu năng."),
        ("Frame Rate", "Tốc độ khung hình"),
        ("Sets the maximum frame rate. Higher values make motion smoother but use more processing power.", "Đặt tốc độ khung hình tối đa. Giá trị cao giúp chuyển động mượt mà hơn nhưng tốn nhiều sức mạnh xử lý hơn."),
        ("Display Mode", "Chế độ hiển thị"),
        ("Chooses between Full Screen, Borderless, and Windowed display modes.", "Lựa chọn giữa các chế độ hiển thị Toàn màn hình, Không viền và Cửa sổ."),
        ("Sharpening", "Độ làm nét"),
        ("Improves image clarity and edge detail. FidelityFX may make the picture look sharper.", "Cải thiện độ rõ nét và chi tiết cạnh của hình ảnh. FidelityFX có thể làm hình ảnh trông sắc nét hơn."),
        ("Vertical Synchronisation", "Đồng bộ dọc"),
        ("Synchronizes the frame rate with the monitor refresh rate to reduce screen tearing.", "Đồng bộ tốc độ khung hình với tần số quét của màn hình để giảm hiện tượng rách hình."),
        ("Motion Blur", "Làm mờ chuyển động"),
        ("Adds blur during fast camera or object movement for a smoother cinematic effect.", "Thêm hiệu ứng mờ khi camera hoặc vật thể chuyển động nhanh, tạo cảm giác điện ảnh mượt mà hơn."),
        ("Chromatic Aberration", "Quang sai màu"),
        ("Adds subtle color separation near screen edges as a visual effect.", "Thêm hiệu ứng tách màu nhẹ ở gần các cạnh màn hình như một hiệu ứng thị giác."),
        ("Brightness", "Độ sáng"),
        ("Adjusts the overall brightness of the image.", "Điều chỉnh độ sáng tổng thể của hình ảnh."),
        ("Contrast", "Độ tương phản"),
        ("Adjusts the difference between the darkest and brightest parts of the image.", "Điều chỉnh sự chênh lệch giữa vùng tối nhất và sáng nhất của hình ảnh."),
        ("Saturation", "Độ bão hòa"),
        ("Adjusts the intensity of colors. Lower values look faded; higher values look more vivid.", "Điều chỉnh cường độ màu sắc. Giá trị thấp làm màu nhạt hơn; giá trị cao làm màu rực rỡ hơn."),
        ("Field Of View", "Tầm nhìn"),
        ("Changes how much of the world the camera can see. A wider view shows more but may distort the edges.", "Thay đổi phạm vi thế giới mà camera nhìn thấy. Tầm nhìn rộng hiển thị nhiều hơn nhưng có thể làm méo các cạnh."),

                // ========== CONTROLLER (ControllerMenu) ==========
        ("Control Type", "Loại điều khiển"),
        ("Selects the preferred input method between Keyboard and Mouse and Controller.", "Lựa chọn phương thức điều khiển ưu tiên giữa Chuột & Bàn phím và Tay cầm."),
        ("Horizontal Sensitivity", "Độ nhạy ngang"),
        ("Adjusts how quickly the camera turns left and right.", "Điều chỉnh tốc độ camera quay trái và phải."),
        ("Vertical Sensitivity", "Độ nhạy dọc"),
        ("Adjusts how quickly the camera looks up và down.", "Điều chỉnh tốc độ camera nhìn lên và xuống."),
        ("Aim Assist", "Hỗ trợ nhắm"),
        ("Aim Assist Type", "Loại hỗ trợ nhắm"),
        ("Enables or disables assistance when aiming at a target.", "Bật hoặc tắt hỗ trợ khi nhắm vào mục tiêu."),
        ("Vibration", "Rung"),
        ("Enables or disables controller vibration and haptic feedback.", "Bật hoặc tắt rung của tay cầm và phản hồi xúc giác."),
        ("Move Forward", "Tiến về trước"),
        ("Sets the key used to move the character forward.", "Đặt phím dùng để di chuyển nhân vật về phía trước."),
        ("Move Backward", "Lùi về sau"),
        ("Sets the key used to move the character backward.", "Đặt phím dùng để di chuyển nhân vật về phía sau."),
        ("Move Left", "Sang trái"),
        ("Sets the key used to move the character to the left.", "Đặt phím dùng để di chuyển nhân vật sang trái."),
        ("Move Right", "Sang phải"),
        ("Sets the key used to move the character to the right.", "Đặt phím dùng để di chuyển nhân vật sang phải."),
        ("Jump", "Nhảy"),
        ("Sets the key used to make the character jump.", "Đặt phím dùng để làm nhân vật nhảy."),
        ("Dodge", "Né đòn"),
        ("Sets the key used to dodge or roll away from attacks.", "Đặt phím dùng để né đòn hoặc lăn người tránh tấn công."),
        ("Sprint", "Chạy nhanh"),
        ("Sets the key used to run or sprint.", "Đặt phím dùng để chạy hoặc chạy nhanh."),
        ("Sneak / Crouch", "Đi rón rén / Ngồi"),
        ("Sneak/Crouch", "Đi rón rén / Ngồi"),
        ("Sets the key used to crouch or move quietly.", "Đặt phím dùng để ngồi xuống hoặc di chuyển lặng lẽ."),
        ("Interact", "Tương tác"),
        ("Sets the key used to interact with objects, characters, and menus.", "Đặt phím dùng để tương tác với vật thể, nhân vật và menu."),

        // ========== UI COMMON (nhãn trái + tab trên) ==========
        ("Sounds Effects Volume", "Âm lượng hiệu ứng"),
        ("Setting", "Cài đặt"),
        ("Graphics", "Đồ họa"),
        ("Controller", "Điều khiển"),
        ("Audio", "Âm thanh"),
        ("Confirm", "Xác nhận"),
        ("Back", "Quay lại"),
    };

    private static Dictionary<string, string> mapVI;

    private static void EnsureMap()
    {
        if (mapVI != null) return;
        mapVI = new Dictionary<string, string>();
        foreach (var e in Entries)
            if (!mapVI.ContainsKey(e.en))
                mapVI.Add(e.en, e.vi);
    }

    /// <summary>
    /// Tự động dịch: nếu ngôn ngữ hiện tại là Tiếng Việt → trả về tiếng Việt,
    /// ngược lại giữ nguyên tiếng Anh.
    /// </summary>
    public static string Translate(string englishText)
    {
        if (string.IsNullOrEmpty(englishText))
            return englishText;

        bool isVietnamese = false;
        try
        {
            isVietnamese = LocalizationSettings.SelectedLocale != null &&
                           LocalizationSettings.SelectedLocale.Identifier.Code
                               .ToLower().StartsWith("vi");
        }
        catch
        {
            isVietnamese = false;
        }

        if (!isVietnamese)
            return englishText;

        EnsureMap();
        return mapVI.TryGetValue(englishText.Trim(), out string vi) ? vi : englishText;
    }
        public static bool HasTranslation(string englishText)
    {
        if (string.IsNullOrEmpty(englishText))
            return false;

        EnsureMap();
        return mapVI.ContainsKey(englishText.Trim());
    }
}