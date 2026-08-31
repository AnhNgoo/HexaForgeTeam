using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
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

                // ========== NPC: SHOP ==========
        ("Hey there! What brings you here?", "Chào bạn! Gì đưa bạn đến đây thế?"),
        ("I've got plenty of items available if you're looking to upgrade your gear.", "Tôi có rất nhiều món đồ nếu bạn muốn nâng cấp trang bị của mình."),
        ("And if you're feeling lucky, you can always try your chances with the gacha.", "Còn nếu thấy may mắn, bạn luôn có thể thử vận may với gacha."),
        ("Shop", "Cửa hàng"),
        ("Gacha", "Gacha"),
        ("Bye", "Tạm biệt"),

        // ========== NPC: ARCHIE ==========
        ("Hey there! Looking to see how you're doing?", "Chào bạn! Đến xem tình hình của bạn dạo này chứ?"),
        ("You can check the leaderboard and see how you rank against other players.", "Bạn có thể xem bảng xếp hạng để biết thứ hạng của mình so với người chơi khác."),
        ("Or, if you want to see what you've accomplished, take a look at your achievements.", "Hoặc nếu muốn xem những gì mình đã đạt được, hãy ngắm qua thành tích của bạn."),
        ("Achievement", "Thành tích"),
        ("Leaderboard", "Bảng xếp hạng"),

        // ========== TUTORIAL ==========
        ("Skip Tutorial", "Bỏ qua hướng dẫn"),
        ("Move Forward", "Tiến về trước"),
        ("Move Left", "Sang trái"),
        ("Move Right", "Sang phải"),
        ("Move Back", "Lùi về sau"),
        ("Lock Target", "Khóa mục tiêu"),
        ("Battle", "Chiến đấu"),
        ("Dodge", "Né đòn"),
        ("Receive Recovery Bottle", "Nhận bình hồi phục"),
        ("Use Skill", "Sử dụng kỹ năng"),
        ("Use Skill Ultimate", "Sử dụng kỹ năng cuối"),
        ("Tutorial", "Hướng dẫn"),

        // ========== STATS / LEVEL UP ==========
        ("Stats", "Chỉ số"),
        ("Level", "Cấp độ"),
        ("Level:", "Cấp độ:"),
        ("Level Up", "Tăng cấp"),
        ("Not enough gold to level up.", "Không đủ vàng để tăng cấp."),
        ("Health", "Máu"),
        ("Speed", "Tốc độ"),
        ("Damage", "Sát thương"),
        ("Defense", "Giáp"),
        ("Poison Damage", "Sát thương độc"),
        ("Stamina", "Thể lực"),
        ("Stamina Regen", "Hồi thể lực"),
        ("MP", "Năng lượng"),
        ("MP Regen", "Hồi năng lượng"),
        ("Gold", "Vàng"),
        ("Need", "Cần"),

        // ========== EXIT / PAUSE MENU ==========
        ("Exit", "Thoát"),
        ("Are you sure you want to quit the game?", "Bạn có chắc muốn thoát trò chơi không?"),
        ("Quit", "Thoát"),

         // ========== TUTORIAL: TÊN PHÍM ==========
        ("Left Button", "Chuột trái"),
        ("Right Button", "Chuột phải"),
        ("Middle Button", "Chuột giữa"),
        ("Left Shift", "Shift trái"),
        ("Right Shift", "Shift phải"),
        ("Space", "Phím cách"),
        ("Pick Up Weapon", "Nhặt vũ khí"),
        ("Pick Up Item", "Nhặt vật phẩm"),
        ("Pick Up Gold", "Nhặt vàng"),
        ("Pick Up Health Potion", "Nhặt bình hồi máu"),

        // ========== INVENTORY / ITEM ACTIONS ==========
        ("Discard", "Vứt bỏ"),
        ("Use", "Sử dụng"),
        ("Escape", "Thoát"),

        // ========== LOGOUT / RETURN LOBBY ==========
        ("Logout", "Đăng xuất"),
        ("Are you sure you want to log out?", "Bạn có chắc muốn đăng xuất không?"),
        ("Return to Lobby", "Quay lại Sảnh chờ"),
        ("Are you sure you want to abandon this run and return to the Lobby?", "Bạn có chắc muốn từ bỏ lượt chạy này và quay lại Sảnh chờ không?"),
        ("Cancel", "Hủy"),

        // ========== GRAPHICS: giá trị + tiêu đề mục ==========
        ("Full Screen", "Toàn màn hình"),
        ("Fullscreen", "Toàn màn hình"),
        ("Borderless", "Không viền"),
        ("Windowed", "Cửa sổ"),
        ("COLOR GRADING", "HIỆU CHỈNH MÀU"),
        ("Color Grading", "Hiệu chỉnh màu"),

        // ========== CONTROLLER: loại điều khiển ==========
        ("Keyboard & Mouse", "Bàn phím & Chuột"),
        ("Keyboard and Mouse", "Bàn phím và Chuột"),
        ("Controller", "Tay cầm"),

        // ========== GIÁ TRỊ CHUNG (dropdown/toggle) ==========
        ("On", "Bật"),
        ("Off", "Tắt"),
        ("Low", "Thấp"),
        ("Medium", "Trung bình"),
        ("High", "Cao"),
        ("Ultra", "Cực cao"),
        ("Unlimited", "Không giới hạn"),

        // ========== ACHIEVEMENT ==========
        ("Claim All", "Nhận tất cả"),
        ("Claim", "Nhận"),
        ("Locked", "Chưa mở khóa"),
        ("Unlocked", "Đã mở khóa"),
        ("Achievement", "Thành tích"),
        ("Achievements", "Thành tích"),

        // ========== ACHIEVEMENT: TÊN (title) ==========
        ("New Traveler", "Lữ khách mới"),
        ("First Steps", "Bước chân đầu tiên"),
        ("Rising Hero", "Anh hùng trỗi dậy"),
        ("Seasoned Warrior", "Chiến binh dày dạn"),
        ("Veteran Adventurer", "Nhà thám hiểm kỳ cựu"),
        ("Grand Commander", "Đại Thống Soái"),
        ("Legend of the Realm", "Huyền thoại Vương quốc"),
        ("First Fortune", "Vận may đầu tiên"),
        ("First Gambler", "Con bạc đầu tiên"),
        ("Lucky Seeker", "Kẻ tìm kiếm may mắn"),
        ("Rune Enthusiast", "Tín đồ Rune"),
        ("Master Summoner", "Bậc thầy Triệu hồi"),
        ("Golden Touch", "Bàn tay vàng"),
        ("Rune Hunter", "Thợ săn Rune"),
        ("Mythic Collector", "Nhà sưu tập Thần thoại"),
        ("First Blood", "Chiến công đầu"),
        ("Monster Slayer", "Sát thủ quái vật"),
        ("Dungeon Cleaner", "Dọn dẹp hầm ngục"),
        ("Executioner", "Đao phủ"),
        ("Fiend Nemesis", "Khắc tinh Ác quỷ"),
        ("Boss Crusher", "Kẻ nghiền nát Boss"),
        ("Boss Hunter", "Thợ săn Boss"),
        ("Dungeon Dominator", "Thống lĩnh Hầm ngục"),
        ("Alchemist Apprentice", "Học giả Giả kim"),
        ("Forge Enthusiast", "Tín đồ Rèn đúc"),
        ("Master Transmuter", "Bậc thầy Chuyển hóa"),
        ("Power Unleashed", "Giải phóng sức mạnh"),
        ("Affinities Tinkerer", "Nhà mày mò Thuộc tính"),
        ("Scrap Recycler", "Nhà tái chế phế liệu"),
        ("Master of Achievements", "Bậc thầy Thành tựu"),

        // ========== ACHIEVEMENT: MÔ TẢ (description) ==========
        ("Reach Account Level 2", "Đạt cấp tài khoản 2"),
        ("Reach Account Level 5", "Đạt cấp tài khoản 5"),
        ("Reach Account Level 10", "Đạt cấp tài khoản 10"),
        ("Reach Account Level 15", "Đạt cấp tài khoản 15"),
        ("Reach Account Level 20", "Đạt cấp tài khoản 20"),
        ("Reach Account Level 25", "Đạt cấp tài khoản 25"),
        ("Reach Account Level 30", "Đạt cấp tài khoản 30"),
        ("Roll 1 Time", "Quay 1 lần"),
        ("Roll 10 Times", "Quay 10 lần"),
        ("Roll 30 Times", "Quay 30 lần"),
        ("Roll 50 Times", "Quay 50 lần"),
        ("Roll 100 Times", "Quay 100 lần"),
        ("Obtain 1 Legendary Rune", "Sở hữu 1 Rune Huyền thoại"),
        ("Obtain 5 Legendary Runes", "Sở hữu 5 Rune Huyền thoại"),
        ("Obtain 10 Legendary Runes", "Sở hữu 10 Rune Huyền thoại"),
        ("Defeat 20 Monsters", "Đánh bại 20 quái vật"),
        ("Defeat 100 Monsters", "Đánh bại 100 quái vật"),
        ("Defeat 300 Monsters", "Đánh bại 300 quái vật"),
        ("Defeat 500 Monsters", "Đánh bại 500 quái vật"),
        ("Defeat 1,000 Monsters", "Đánh bại 1.000 quái vật"),
        ("Defeat 1 Boss", "Đánh bại 1 Boss"),
        ("Defeat 5 Bosses", "Đánh bại 5 Boss"),
        ("Defeat 10 Bosses", "Đánh bại 10 Boss"),
        ("Fuse Runes 1 Time", "Hợp nhất Rune 1 lần"),
        ("Fuse Runes 5 Times", "Hợp nhất Rune 5 lần"),
        ("Fuse Runes 10 Times", "Hợp nhất Rune 10 lần"),
        ("Equip 3 Runes on a Hero", "Trang bị 3 Rune cho một Anh hùng"),
        ("Reroll Rune Affix 1 Time", "Reroll thuộc tính Rune 1 lần"),
        ("Dismantle 10 Runes", "Tháo dỡ 10 Rune"),
        ("Complete all other achievements", "Hoàn thành tất cả các thành tựu khác"),

        // ========== ACHIEVEMENT UI & TOAST ==========
        ("Achievement Unlocked", "Mở khóa thành tựu"),
        ("All available achievement rewards claimed!", "Đã nhận toàn bộ phần thưởng thành tựu!"),

        // ========== ACHIEVEMENT: ITEM REWARDS ==========
        ("Gacha Ticket", "Vé Gacha"),
        ("Protection Charm", "Bùa bảo vệ"),
        ("Reroll Scroll", "Cuộn Reroll"),
        ("Special Item", "Vật phẩm đặc biệt"),

        // ========== ULTIMATE RUNE ==========
        ("Origin of Creation", "Cội nguồn Sáng tạo"),
        ("The final proof that nothing remains unconquered.", "Bằng chứng cuối cùng rằng không gì là không thể chinh phục."),
    };

        // ✅ TEXT ĐỘNG (có số thay đổi) - dùng Regex
    private static readonly (Regex regex, string replacement)[] RegexEntries = new (Regex, string)[]
    {
        // "Level: 11 -> 12" → "Cấp độ: 11 -> 12"
        (new Regex(@"^Level:\s*(\d+)\s*->\s*(\d+)$", RegexOptions.IgnoreCase), "Cấp độ: $1 -> $2"),

        // "Need 211/7111 Gold" → "Cần 211/7111 Vàng"
        (new Regex(@"^Need\s+(\S+)\s+Gold$", RegexOptions.IgnoreCase), "Cần $1 Vàng"),
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

    // ✅ Gộp khoảng trắng / xuống dòng / tab thành 1 khoảng trắng
    private static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return Regex.Replace(text.Trim(), @"\s+", " ");
    }

    private static bool IsVietnamese()
    {
        // Đọc thẳng PlayerPrefs — luôn có sẵn, không phải chờ Localization init
    return PlayerPrefs.GetInt("LANGUAGE", 0) == 1;
    }

    public static string Translate(string englishText)
    {
        if (string.IsNullOrEmpty(englishText))
            return englishText;

        if (!IsVietnamese())
            return englishText;

        EnsureMap();
        string normalized = Normalize(englishText);

        // 1. Khớp chính xác trong dictionary
        if (mapVI.TryGetValue(normalized, out string vi))
            return vi;

        // 2. Khớp Regex (text động có số)
        foreach (var entry in RegexEntries)
            if (entry.regex.IsMatch(normalized))
                return entry.regex.Replace(normalized, entry.replacement);

        // ✅ Chỉ log chuỗi có chữ cái thật sự (bỏ qua số như "1920 x 1080", "60")
        if (Regex.IsMatch(normalized, @"[a-zA-Z]{3,}"))
            Debug.LogWarning($"[THIẾU KEY VI] \"{normalized}\"");

        return englishText;
    }

    public static bool HasTranslation(string englishText)
    {
        if (string.IsNullOrEmpty(englishText))
            return false;

        EnsureMap();
        string normalized = Normalize(englishText);

        if (mapVI.ContainsKey(normalized))
            return true;

        foreach (var entry in RegexEntries)
            if (entry.regex.IsMatch(normalized))
                return true;

        return false;
    }

    public static void Refresh()
    {
        mapVI = null;
        EnsureMap();
    }
}