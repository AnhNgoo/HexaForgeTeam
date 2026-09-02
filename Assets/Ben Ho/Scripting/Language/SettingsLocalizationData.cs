using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization.Settings;

public static class SettingsLocalizationData
{
    public static bool IsVietnamesePublic() => IsVietnamese();

    // (English, Vietnamese)
    private static readonly (string en, string vi)[] Entries = new (string, string)[]
    {
        // ==========================================
        // 1. CÀI ĐẶT ÂM THANH (AUDIO)
        // ==========================================
        ("Master Volume", "Âm lượng tổng"),
        ("Controls the overall volume of all game audio.", "Điều khiển âm lượng tổng thể của toàn bộ âm thanh trong game."),
        ("Music Volume", "Âm lượng nhạc"),
        ("Controls the volume of background music without affecting dialogue or sound effects.", "Điều khiển âm lượng nhạc nền mà không ảnh hưởng đến hội thoại hay hiệu ứng âm thanh."),
        ("Sound Effects Volume", "Âm lượng hiệu ứng"),
        ("Sounds Effects Volume", "Âm lượng hiệu ứng"),
        ("Controls gameplay sound effects such as attacks, impacts, enemies, and interface sounds.", "Điều khiển các hiệu ứng âm thanh trong game như tấn công, va chạm, kẻ địch và âm thanh giao diện."),
        ("Dialogue Volume", "Âm lượng hội thoại"),
        ("Controls the volume of spoken dialogue and character voice lines.", "Điều khiển âm lượng lời thoại và giọng nói của nhân vật."),
        ("Background Sound", "Âm thanh nền"),
        ("Enables or disables background music.", "Bật hoặc tắt nhạc nền."),
        ("Collision Sound", "Âm thanh va chạm"),
        ("Enables or disables collision and impact sounds.", "Bật hoặc tắt âm thanh va chạm và tác động."),

        // ==========================================
        // 2. CÀI ĐẶT ĐỒ HỌA (GRAPHICS)
        // ==========================================
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
        ("Full Screen", "Toàn màn hình"),
        ("Fullscreen", "Toàn màn hình"),
        ("Borderless", "Không viền"),
        ("Windowed", "Cửa sổ"),
        ("COLOR GRADING", "HIỆU CHỈNH MÀU"),
        ("Color Grading", "Hiệu chỉnh màu"),

        // ==========================================
        // 3. CÀI ĐẶT ĐIỀU KHIỂN & PHÍM BẤM (CONTROLLER & INPUTS)
        // ==========================================
        ("Control Type", "Loại điều khiển"),
        ("Selects the preferred input method between Keyboard and Mouse and Controller.", "Lựa chọn phương thức điều khiển ưu tiên giữa Chuột & Bàn phím và Tay cầm."),
        ("Horizontal Sensitivity", "Độ nhạy ngang"),
        ("Adjusts how quickly the camera turns left and right.", "Điều chỉnh tốc độ camera quay trái và phải."),
        ("Vertical Sensitivity", "Độ nhạy dọc"),
        ("Adjusts how quickly the camera looks up and down.", "Điều chỉnh tốc độ camera nhìn lên và xuống."),
        ("Aim Assist", "Hỗ trợ nhắm"),
        ("Aim Assist Type", "Loại hỗ trợ nhắm"),
        ("Enables or disables assistance when aiming at a target.", "Bật hoặc tắt hỗ trợ khi nhắm vào mục tiêu."),
        ("Vibration", "Rung"),
        ("Enables or disables controller vibration and haptic feedback.", "Bật hoặc tắt rung của tay cầm và phản hồi xúc giác."),
        ("Move Forward", "Tiến về trước"),
        ("Sets the key used to move the character forward.", "Đặt phím dùng để di chuyển nhân vật về phía trước."),
        ("Move Backward", "Lùi về sau"),
        ("Move Back", "Lùi về sau"),
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
        ("Keyboard & Mouse", "Bàn phím & Chuột"),
        ("Keyboard and Mouse", "Bàn phím và Chuột"),
        ("Left Button", "Chuột trái"),
        ("Right Button", "Chuột phải"),
        ("Middle Button", "Chuột giữa"),
        ("Left Shift", "Shift trái"),
        ("Right Shift", "Shift phải"),
        ("Space", "Phím cách"),

        // ==========================================
        // 4. GIAO DIỆN CHUNG & MENU (COMMON UI)
        // ==========================================
        ("Setting", "Cài đặt"),
        ("Graphics", "Đồ họa"),
        ("Controller", "Điều khiển"),
        ("Audio", "Âm thanh"),
        ("Confirm", "Xác nhận"),
        ("Back", "Quay lại"),
        ("Cancel", "Hủy"),
        ("On", "Bật"),
        ("Off", "Tắt"),
        ("Low", "Thấp"),
        ("Medium", "Trung bình"),
        ("High", "Cao"),
        ("Ultra", "Cực cao"),
        ("Unlimited", "Không giới hạn"),
        ("Exit", "Thoát"),
        ("Quit", "Thoát"),
        ("Are you sure you want to quit the game?", "Bạn có chắc muốn thoát trò chơi không?"),
        ("Logout", "Đăng xuất"),
        ("Are you sure you want to log out?", "Bạn có chắc muốn đăng xuất không?"),
        ("Return to Lobby", "Quay lại Sảnh chờ"),
        ("Are you sure you want to abandon this run and return to the Lobby?", "Bạn có chắc muốn từ bỏ lượt chạy này và quay lại Sảnh chờ không?"),
        ("Discard", "Vứt bỏ"),
        ("Use", "Sử dụng"),
        ("Escape", "Thoát"),

        // ==========================================
        // 5. HƯỚNG DẪN & SKIP TUTORIAL
        // ==========================================
        ("Tutorial", "Hướng dẫn"),
        ("Skip Tutorial", "Bỏ qua hướng dẫn"),
        ("Are you sure you want to Skip Tutorial?", "Bạn có chắc muốn bỏ qua Hướng dẫn không?"),
        ("Are you sure you want to skip tutorial?", "Bạn có chắc muốn bỏ qua Hướng dẫn không?"),
        ("Are you sure you want to skip the tutorial?", "Bạn có chắc muốn bỏ qua Hướng dẫn không?"),
        ("All basic mechanics will be fully unlocked and all milestone rewards will be claimed immediately.", "Tất cả các cơ chế cơ bản sẽ được mở khóa hoàn toàn và toàn bộ phần thưởng cột mốc sẽ được nhận ngay lập tức."),
        ("SKIP", "BỎ QUA"),
        ("Skip", "Bỏ qua"),
        ("Lock Target", "Khóa mục tiêu"),
        ("Battle", "Chiến đấu"),
        ("Receive Recovery Bottle", "Nhận bình hồi phục"),
        ("Use Skill", "Sử dụng kỹ năng"),
        ("Use Skill Ultimate", "Sử dụng kỹ năng cuối"),
        ("Pick Up Weapon", "Nhặt vũ khí"),
        ("Pick Up Item", "Nhặt vật phẩm"),
        ("Pick Up Gold", "Nhặt vàng"),
        ("Pick Up Health Potion", "Nhặt bình hồi máu"),

        // ==========================================
        // 6. CHỈ SỐ NHÂN VẬT & TĂNG CẤP (STATS & LEVEL)
        // ==========================================
        ("Stats", "Chỉ số"),
        ("Stat:", "Chỉ số:"),
        ("Stats:", "Chỉ số:"),
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
        ("Crit Chance", "Tỉ lệ chí mạng"),

        // ==========================================
        // 7. MENU CHỌN NHÂN VẬT (CHARACTER MENU)
        // ==========================================
        ("Character", "Nhân vật"),
        ("Coming soon", "Sắp ra mắt"),
        ("COMING SOON", "Sắp ra mắt"),
        ("DEPLOY HERO", "XUẤT TRẬN"),
        ("DEPLOYED", "ĐÃ XUẤT TRẬN"),
        ("PREVIEWING", "XEM TRƯỚC"),
        ("Select Character", "Chọn Nhân Vật"),

        // ==========================================
        // 8. MENU RUNE & KHO ĐỒ (RUNE INVENTORY)
        // ==========================================
        ("Equip Rune", "Trang bị Rune"),
        ("Fusion Rune", "Hợp nhất Rune"),
        ("Inventory Rune", "Kho Rune"),
        ("Rune Inventory", "Kho Rune"),
        ("Open Inventory", "Mở Túi Đồ"),
        ("Red", "Đỏ"),
        ("Green", "Lục"),
        ("Blue", "Lam"),
        ("Yellow", "Vàng"),
        ("Purple", "Tím"),
        ("Gacha Ticket", "Vé Gacha"),
        ("Protection Charm", "Bùa bảo vệ"),
        ("Reroll Scroll", "Cuộn Reroll"),
        ("Special Item", "Vật phẩm đặc biệt"),
        ("Origin of Creation", "Cội nguồn Sáng tạo"),
        ("The final proof that nothing remains unconquered.", "Bằng chứng cuối cùng rằng không gì là không thể chinh phục."),

        // ==========================================
        // 9. MENU CHỌN BOSS & ĐẶT CƯỢC (BOSS SELECT & WAGER)
        // ==========================================
        ("The Round Table", "Bàn Tròn"),
        ("Select Boss Map", "Chọn Bản Đồ Boss"),
        ("Select Base Map", "Chọn Bản Đồ Cơ Bản"),
        ("The Earthshaker", "The Earthshaker"),
        ("The DarkMage", "The DarkMage"),
        ("Lucky Cat", "Mèo May Mắn"),
        ("Medical Kit", "Túi Cứu Thương"),
        ("Brawn Elixir", "Dược Lực Chiến"),
        ("Defeat Boss 1 to Unlock", "Đánh bại Boss 1 để Mở khóa"),
        ("Defeat The Earthshaker first to unlock!", "Hãy đánh bại The Earthshaker trước để mở khóa!"),
        ("Item depleted! Please purchase more from the Shop.", "Đã hết vật phẩm! Vui lòng mua thêm tại Cửa hàng."),
        ("Tier", "Cấp độ"),
        ("Bet", "Cược"),
        ("Benefits", "Lợi ích"),
        ("Note", "Lưu ý"),
        ("Buffs", "Buff hỗ trợ"),
        ("Gems", "Ngọc"),
        ("Standard (Safe)", "Tiêu chuẩn (An toàn)"),
        ("Risky (Challenge)", "Mạo hiểm (Thử thách)"),
        ("Nightmare (Hardcore)", "Ác mộng (Khắc nghiệt)"),
        ("Standard combat, resources kept intact.", "Chiến đấu tiêu chuẩn, giữ nguyên toàn bộ tài nguyên."),
        ("No secondary buffs allowed.", "Không được sử dụng buff phụ trợ."),
        ("EXP & Resource gains +50%.", "Tăng +50% EXP & Tài nguyên thu được."),
        ("Gold Boost & Phoenix Charm available.", "Cho phép sử dụng Mèo May Mắn & Bùa Phượng Hoàng."),
        ("Massive rewards (x2.5) & Rune Shards.", "Nhận thưởng cực lớn (x2.5) & Mảnh Đá Rune."),
        ("All 3 Combat Elixirs unlocked!", "Mở khóa toàn bộ 3 loại Dược Phẩm Chiến Đấu!"),

        // ==========================================
        // 10. THÀNH TÍCH & BẢNG XẾP HẠNG (ACHIEVEMENTS & LEADERBOARDS)
        // ==========================================
        ("Achievement", "Thành tích"),
        ("Achievements", "Thành tích"),
        ("Archievement", "Thành tích"),
        ("Archievements", "Thành tích"),
        ("Claim All", "Nhận tất cả"),
        ("Claim", "Nhận"),
        ("Claimed", "Đã nhận"),
        ("Locked", "Chưa mở khóa"),
        ("(Locked)", "(Đã khóa)"),
        ("Unlocked", "Đã mở khóa"),
        ("Master Achievement", "Thành tích Bậc thầy"),
        ("Achievement Unlocked", "Mở khóa thành tựu"),
        ("All available achievement rewards claimed!", "Đã nhận toàn bộ phần thưởng thành tựu!"),
        ("Leader Board", "Bảng Xếp Hạng"),
        ("Leaderboard", "Bảng Xếp Hạng"),
        ("Leaderboards", "Bảng Xếp Hạng"),
        ("Power", "Lực chiến"),
        ("Hunter", "Thợ Săn"),
        ("Run", "Thám Hiểm"),
        ("Rank", "Hạng"),
        ("Detail", "Chi tiết"),
        ("Score", "Điểm"),
        ("Score :", "Điểm :"),
        ("Combat Power", "Lực chiến"),
        ("Combat Power:", "Lực chiến:"),
        ("Player Stat", "Chỉ số người chơi"),

        // Danh sách tên Thành tích
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

        // Mô tả Thành tích
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

        // ==========================================
        // 11. HỘI THOẠI & LỰA CHỌN NPC (NPC & DIALOGUE CHOICES)
        // ==========================================
        ("Shop", "Cửa hàng"),
        ("Gacha", "Gacha"),
        ("Bye", "Tạm biệt"),
        ("Quest", "Nhiệm vụ"),
        ("Quests", "Nhiệm vụ"),
        ("Reward", "Phần thưởng"),
        ("Rewards", "Phần thưởng"),
        ("Talk", "Trò chuyện"),
        ("Leave", "Rời đi"),
        ("Adventurer", "Nhà thám hiểm"),
        ("Manager", "Quản lý"),
        ("Accept Quest", "Nhận nhiệm vụ"),
        ("Claim Reward", "Nhận thưởng"),

        // Nút lựa chọn có trạng thái (Locked)
        ("Achievement (Locked)", "Thành tích (Đã khóa)"),
        ("Achievements (Locked)", "Thành tích (Đã khóa)"),
        ("Archievement (Locked)", "Thành tích (Đã khóa)"),
        ("Leaderboard (Locked)", "Bảng xếp hạng (Đã khóa)"),
        ("Leaderboards (Locked)", "Bảng xếp hạng (Đã khóa)"),
        ("Shop (Locked)", "Cửa hàng (Đã khóa)"),
        ("Gacha (Locked)", "Gacha (Đã khóa)"),
        ("Quest (Locked)", "Nhiệm vụ (Đã khóa)"),
        ("Quests (Locked)", "Nhiệm vụ (Đã khóa)"),

        // Tên tương tác phím F
        ("Talk - Ngoo", "Trò chuyện - Ngoo"),
        ("Talk - Dat", "Trò chuyện - Đạt"),
        ("Talk - Maria", "Trò chuyện - Maria"),
        ("Talk - Phuc", "Trò chuyện - Phúc"),
        ("Talk - Long", "Trò chuyện - Long"),
        ("Talk - Thu Khoa", "Trò chuyện - Thủ Khoa"),
        ("Talk - Trung", "Trò chuyện - Trung"),
        ("Talk - Bee Ngoo", "Trò chuyện - Bee Ngoo"),
        ("Talk - Dat Vit", "Trò chuyện - Đạt Vịt"),
        ("Talk - Maria Ozawa", "Trò chuyện - Maria Ozawa"),
        ("Talk - Thanh Phuc", "Trò chuyện - Thanh Phúc"),
        ("Talk - Lai Duy Long", "Trò chuyện - Lại Duy Long"),
        ("Talk - Uong Quoc Trung", "Trò chuyện - Uông Quốc Trung"),
        ("ThuKhoa", "Thủ Khoa"),
        ("Bee Ngoo", "Bee Ngoo"),
        ("BeeNgoo", "Bee Ngoo"),
        ("Gambler", "Kẻ Đánh Bạc"),
        ("Phuc", "Phúc"),
        ("Dat Vit", "Đạt Vịt"),
        ("Dat", "Đạt"),
        ("Maria Ozawa", "Maria Ozawa"),
        ("Maria", "Maria"),
        ("LaiDuyLon", "Lại Duy Long"),
        ("Lai Duy Long", "Lại Duy Long"),
        ("Uong Quoc Trung", "Uông Quốc Trung"),
        ("Trung", "Trung"),
        ("Thanh Phuc", "Thanh Phúc"),

        // Lời thoại NPC trong sảnh
        ("Rune power is volatile. If you don't fuse and reinforce your engraved stones regularly, your combat efficiency will drop drastically.", "Sức mạnh của Rune rất dễ biến động. Nếu không hợp nhất và cường hóa các viên đá khắc thường xuyên, hiệu quả chiến đấu của bạn sẽ giảm mạnh đấy."),
        ("Drop by the workshop before every expedition—proper maintenance is what keeps you alive out there.", "Hãy ghé qua xưởng trước mỗi chuyến thám hiểm—bảo dưỡng trang bị cẩn thận chính là thứ giúp bạn sống sót ngoài kia."),
        ("Stay on high alert! The rift's barrier has been fluctuating, and scout reports indicate ferocious beasts gathering near the outer border.", "Hãy luôn cảnh giác cao độ! Rào chắn của khe nứt đang dao động dữ dội, và báo cáo từ trinh sát cho thấy lũ quái thú hung tợn đang tụ tập gần biên giới bên ngoài."),
        ("Keep your weapons drawn and never drop your guard when stepping into unknown territory.", "Hãy luôn sẵn sàng vũ khí trong tay và đừng bao giờ lơ là cảnh giác khi bước vào lãnh thổ chưa từng được khám phá."),
        ("The name's Gambler. I only deal with adventurers who have seen real action outside.", "Ta là Kẻ Đánh Bạc. Ta chỉ giao dịch với những nhà thám hiểm đã từng trải qua chiến trận thực sự ngoài kia thôi."),
        ("Heading into an expedition without battle elixirs or medical kits is practically suicidal, rookie.", "Dấn thân vào một chuyến thám hiểm mà không mang theo dược phẩm chiến đấu hay túi cứu thương chẳng khác nào tự sát đâu, lính mới."),
        ("Always stock up on provisions at the shop beforehand—you'll thank yourself when you're cornered by the boss.", "Hãy luôn chuẩn bị đầy đủ nhu yếu phẩm tại cửa hàng trước—bạn sẽ phải cảm ơn bản thân khi bị dồn vào đường cùng trước mặt boss đấy."),
        ("The magical ley lines beneath the sanctuary have been behaving strangely today... An ancient, slumbering presence is definitely stirring.", "Các mạch năng lượng ma thuật bên dưới thánh địa hôm nay có biểu hiện rất kỳ lạ... Một thực thể cổ xưa đang say ngủ chắc chắn đang bắt đầu cựa mình."),
        ("I'm deciphering the ancient manuscripts to see if there's any way to suppress the rift's dark energy.", "Tôi đang giải mã các bản thảo cổ đại để tìm cách trấn áp nguồn năng lượng hắc ám của khe nứt."),
        ("Hey there! What brings you here?", "Chào bạn! Gì đưa bạn đến đây thế?"),
        ("I've got plenty of items available if you're looking to upgrade your gear.", "Tôi có rất nhiều món đồ nếu bạn muốn nâng cấp trang bị của mình."),
        ("And if you're feeling lucky, you can always try your chances with the gacha.", "Còn nếu thấy may mắn, bạn luôn có thể thử vận may với gacha."),
        ("Hey there! Looking to see how you're doing?", "Chào bạn! Đến xem tình hình của bạn dạo này chứ?"),
        ("You can check the leaderboard and see how you rank against other players.", "Bạn có thể xem bảng xếp hạng để biết thứ hạng của mình so với người chơi khác."),
        ("Or, if you want to see what you've accomplished, take a look at your achievements.", "Hoặc nếu muốn xem những gì mình đã đạt được, hãy ngắm qua thành tích của bạn."),
        ("Don't let your weapons get dull. A broken blade out there means certain death.", "Đừng để vũ khí bị cùn. Một lưỡi kiếm gãy ngoài kia đồng nghĩa với cái chết chắc chắn."),
        ("I'll keep that in mind. Keep your forge hot!", "Tôi sẽ ghi nhớ. Giữ lửa lò rèn luôn nóng nhé!"),
        ("Hey there! The world out there is treacherous, but full of unseen treasures. Stay sharp on your expeditions!", "Chào bạn! Thế giới ngoài kia đầy hiểm nguy nhưng cũng lắm kho báu chưa ai thấy. Hãy luôn cảnh giác trong những chuyến thám hiểm!"),
        ("Thanks for the advice, traveler!", "Cảm ơn lời khuyên của bạn, lữ khách!"),
        ("Ah, fresh blood! I manage the supplies and mysterious relics here. Need stronger gear?", "À, gương mặt mới! Tôi quản lý vật phẩm và những di vật bí ẩn ở đây. Cần trang bị mạnh hơn không?"),
        ("Good to know! I will definitely come back when I need equipment.", "Hay đấy! Tôi chắc chắn sẽ quay lại khi cần trang bị."),
        ("Feel the resonance in the air? This altar channels pure rune energy. Bring me rune shards and I'll awaken their power.", "Cảm nhận được sự cộng hưởng trong không khí chứ? Bàn thờ này dẫn nguồn năng lượng rune tinh khiết. Hãy mang cho tôi những mảnh rune, tôi sẽ đánh thức sức mạnh của chúng."),
        ("Fascinating! I will make sure to bring runes here.", "Thật cuốn hút! Tôi nhất định sẽ mang rune đến đây."),
        ("Welcome to the archives. If you wish to learn about the ancient history of this realm, you know where to find me.", "Chào mừng đến với kho lưu trữ. Nếu muốn tìm hiểu lịch sử cổ xưa của vương quốc này, bạn biết nơi nào để tìm tôi rồi đấy."),
        ("Thank you! I'll read through the records when I have time.", "Cảm ơn! Tôi sẽ đọc các ghi chép khi có thời gian."),

        // ==========================================
        // 12. NHIỆM VỤ CỐT TRUYỆN (QUESTS)
        // ==========================================
        ("Trial of the Expedition", "Thử Thách Thám Hiểm"),
        ("Follow Dat Vit to the Expedition Gate and complete your first combat run (victory or defeat).", "Theo chân Đạt Vịt đến Cổng Thám Hiểm và hoàn thành lượt chiến đấu đầu tiên (thắng hoặc thua)."),
        ("The sanctuary is safe, but true glory lies beyond the portal. Follow me to the Expedition Gate.", "Thánh địa tuy an toàn, nhưng vinh quang thực sự nằm phía sau cánh cổng. Hãy theo tôi đến Cổng Thám Hiểm."),
        ("Are there dangerous monsters out there?", "Ngoài đó có những quái vật nguy hiểm không?"),
        ("Fierce beasts and ancient bosses await. Complete a full combat run to prove your mettle!", "Những mãnh thú hung dữ và các trùm cổ xưa đang chờ đón. Hãy hoàn thành một lượt chiến đấu trọn vẹn để chứng tỏ bản lĩnh của bạn!"),
        ("The portal is active right beside us! Step through, fight your way through the hordes, and see how far you can push.", "Cánh cổng đã kích hoạt ngay cạnh chúng ta rồi! Hãy bước qua, chiến đấu vượt qua lũ quái vật và xem bạn có thể tiến xa đến đâu."),
        ("I will enter the portal and face the trial right now.", "Tôi sẽ bước qua cánh cổng và đối mặt với thử thách ngay bây giờ."),
        ("I've returned from the expedition. That was an intense battle!", "Tôi đã trở về từ chuyến thám hiểm. Đó quả là một trận chiến khốc liệt!"),
        ("Outstanding resilience! Whether you conquer the boss or fall in battle, every trial sharpens your power. Take your reward!", "Sự kiên cường phi thường! Dù bạn hạ gục boss hay ngã xuống trên chiến trường, mỗi thử thách đều tôi luyện sức mạnh của bạn. Hãy nhận phần thưởng!"),
        ("You are now a true expeditioner. Speak with Uong Quoc Trung to review your battle records and rankings.", "Giờ bạn đã là một nhà thám hiểm thực thụ. Hãy nói chuyện với Uông Quốc Trung để xem lại kỷ lục chiến đấu và thứ hạng của bạn nhé."),

        ("Record of Valour", "Chiến Tích Quả Cảm"),
        ("Talk to Trung to unlock and inspect the Achievement records.", "Nói chuyện với Trung để mở khóa và xem các chiến tích Thành tựu."),
        ("I see you've survived your first expedition! It's time to track your heroic milestones.", "Tôi thấy bạn đã sống sót sau chuyến thám hiểm đầu tiên! Đã đến lúc theo dõi các cột mốc anh hùng của bạn rồi."),
        ("Where can I see what milestones I've achieved?", "Tôi có thể xem các cột mốc mình đã đạt được ở đâu?"),
        ("Open the Achievement panel from my menu to claim your milestone rewards.", "Mở bảng Thành tích từ menu của tôi để nhận các phần thưởng cột mốc."),
        ("Check the Achievement list to see what feats you have accomplished.", "Kiểm tra danh sách Thành tích để xem bạn đã hoàn thành những chiến công nào."),
        ("I've checked the achievements. There are many goals to pursue!", "Tôi đã kiểm tra các thành tích rồi. Có rất nhiều mục tiêu để theo đuổi!"),
        ("Keep completing them as you venture deeper. Here is your reward for this milestone.", "Hãy tiếp tục hoàn thành chúng khi bạn tiến sâu hơn. Đây là phần thưởng cho cột mốc này của bạn."),
        ("Check back often to claim more achievement rewards as you progress.", "Hãy thường xuyên quay lại để nhận thêm nhiều phần thưởng thành tích khi bạn tiến bộ nhé."),

        ("Hall of Fame", "Đền Danh Vọng"),
        ("Talk to Trung to unlock and view the global Leaderboard rankings.", "Nói chuyện với Trung để mở khóa và xem bảng xếp hạng toàn cầu."),
        ("Beyond personal achievements, you can also compete with warriors worldwide.", "Bên cạnh thành tích cá nhân, bạn còn có thể so tài cùng các chiến binh trên toàn thế giới."),
        ("Is there a global ranking system?", "Có hệ thống xếp hạng toàn cầu sao?"),
        ("Indeed! Open the Leaderboard panel to inspect the top-ranked adventurers.", "Chính xác! Mở bảng Xếp hạng để xem các nhà thám hiểm đứng đầu bảng nhé."),
        ("Inspect the Leaderboard rankings to see who sits at the apex.", "Hãy xem bảng Xếp hạng để biết ai đang đứng ở đỉnh cao danh vọng."),
        ("The competition is fierce! I need to keep growing stronger.", "Sự cạnh tranh thật khốc liệt! Tôi cần phải tiếp tục mạnh mẽ hơn nữa."),
        ("That's the spirit of a true champion. Take this reward, and make your name known across the realm!", "Đó chính là tinh thần của một nhà vô địch thực thụ. Hãy nhận phần thưởng này và vang danh khắp vương quốc nhé!"),
        ("Climb higher in the rankings and make our sanctuary proud.", "Hãy leo cao hơn trên bảng xếp hạng và làm rạng danh thánh địa của chúng ta."),

        ("Getting Acquainted", "Làm Quen Đồng Đội"),
        ("Explore the base and talk to all 6 residents.", "Khám phá căn cứ và trò chuyện với tất cả 6 cư dân."),
        ("Greetings, newcomer! Welcome to our sanctuary. You look a bit disoriented.", "Xin chào người mới! Chào mừng đến với thánh địa của chúng ta. Trông bạn có vẻ hơi bỡ ngỡ."),
        ("I just arrived here. Could you show me around or tell me who to meet?", "Tôi vừa mới tới đây. Bạn có thể dẫn tôi đi quanh hoặc chỉ tôi nên gặp ai không?"),
        ("Take your time to explore the base first. Go introduce yourself to the other 6 companions around here.", "Hãy cứ thong thả khám phá căn cứ trước. Hãy đi giới thiệu bản thân với 6 người đồng hành xung quanh đây nhé."),
        ("Have you met everyone yet? There are still people waiting to get acquainted with you.", "Bạn đã gặp hết mọi người chưa? Vẫn còn những người đang chờ làm quen với bạn đấy."),
        ("I'm on my way to talk to them right now.", "Tôi đang trên đường đi trò chuyện với họ đây."),
        ("I've met everyone in the base. They seem quite helpful!", "Tôi đã gặp tất cả mọi người trong căn cứ. Họ có vẻ rất nhiệt tình giúp đỡ!"),
        ("Splendid! Knowing your allies is the first step before venturing into danger. Here is a small welcome gift for you.", "Tuyệt vời! Hiểu rõ đồng minh là bước đầu tiên trước khi dấn thân vào hiểm nguy. Đây là món quà chào mừng nhỏ dành cho bạn."),

        ("Awaken the Relic", "Đánh Thức Di Vật"),
        ("Perform a Rune Summon at LaiDuyLon's station to obtain your first power.", "Thực hiện Triệu hồi Rune tại quầy của Lại Duy Long để nhận sức mạnh đầu tiên."),
        ("Ah, the new face! I manage our ancient relics and mysterious supply boxes.", "À, gương mặt mới! Tôi quản lý các di vật cổ đại và những hộp tiếp tế bí ẩn."),
        ("How do these relics work?", "Những di vật này hoạt động như thế nào?"),
        ("Channel your energy to summon one. Go ahead, perform your first summon right here!", "Tập trung năng lượng của bạn để triệu hồi một cái xem nào. Tiến lên, hãy thực hiện lần triệu hồi đầu tiên ngay tại đây!"),
        ("What are you waiting for? Open the Gacha menu and awaken your first relic!", "Còn chờ gì nữa? Mở menu Gacha và đánh thức di vật đầu tiên của bạn đi!"),
        ("Let me try it right now.", "Để tôi thử ngay xem sao."),
        ("The resonance worked! I've summoned a new relic.", "Sự cộng hưởng đã có tác dụng! Tôi vừa triệu hồi được một di vật mới."),
        ("Impressive resonance! Equip it well to strengthen your battle prowess. Here is your reward.", "Khả năng cộng hưởng thật ấn tượng! Hãy trang bị thật tốt để tăng cường sức mạnh chiến đấu. Đây là phần thưởng của bạn."),
        ("Feel free to summon more whenever you have enough Tickets or Gems.", "Cứ thoải mái triệu hồi thêm bất cứ khi nào bạn có đủ Vé hoặc Gem nhé."),

        ("Merchant's Wares", "Hàng Hóa Thương Gia"),
        ("Browse the supply shop and check out the essential adventurer goods.", "Xem qua cửa hàng tiếp tế và kiểm tra các món đồ thiết yếu cho nhà thám hiểm."),
        ("Now that you know how relics work, let me open my general supply store for you.", "Giờ bạn đã hiểu cách di vật hoạt động, để tôi mở cửa hàng tiếp tế tổng hợp cho bạn xem nhé."),
        ("What kind of items do you sell?", "Bạn bán những loại vật phẩm nào thế?"),
        ("Potions, enhancement materials, and rare tickets. Take a look inside the Shop menu!", "Bình thuốc, nguyên liệu cường hóa và cả vé hiếm nữa. Hãy xem thử bên trong menu Cửa hàng nhé!"),
        ("Check the shop catalog and see if anything suits your upcoming expeditions.", "Hãy xem danh mục cửa hàng để xem có món nào phù hợp cho các chuyến thám hiểm sắp tới của bạn không."),
        ("Your shop has quite a variety of useful supplies.", "Cửa hàng của bạn có rất nhiều món tiếp tế hữu dụng."),
        ("Glad you like it. Restock often, and stay prepared for the battlefield!", "Rất vui vì bạn thích. Hãy ghé mua bổ sung thường xuyên và luôn sẵn sàng cho chiến trường!"),
        ("Welcome to the shop anytime you need consumables or upgrades.", "Chào mừng bạn đến cửa hàng bất cứ lúc nào cần vật phẩm tiêu hao hoặc nâng cấp."),

        ("Rune Resonance", "Cộng Hưởng Phù Văn"),
        ("Follow the ThuKhoa to the Rune Altar and open your Rune Vault to inspect your power.", "Theo chân Thủ Khoa đến Bàn Thờ Rune và mở Kho Rune để kiểm tra sức mạnh của bạn."),
        ("You've gathered runes from summoning, but do you know how to wield them? Follow me to the Rune Altar.", "Bạn đã thu thập được rune từ việc triệu hồi, nhưng bạn đã biết cách sử dụng chúng chưa? Hãy theo tôi đến Bàn Thờ Rune."),
        ("Lead the way! I want to see where runes are stored and infused.", "Dẫn đường đi! Tôi muốn xem nơi rune được cất giữ và dung nạp sức mạnh."),
        ("We are right at the Rune Altar. Interact with it to inspect your equipped runes.", "Chúng ta đang ở ngay Bàn Thờ Rune rồi. Hãy tương tác với nó để kiểm tra các rune đã trang bị của bạn."),
        ("I've checked the Rune Vault. The stat boosts are incredible!", "Tôi đã kiểm tra Kho Rune rồi. Các chỉ số được gia tăng thật đáng kinh ngạc!"),
        ("Indeed. Align your runes properly to maximize your combat efficiency in expeditions. Here is your reward.", "Chính xác. Hãy sắp xếp rune hợp lý để tối đa hóa hiệu quả chiến đấu trong các chuyến thám hiểm. Đây là phần thưởng của bạn."),
        ("Return to the Altar anytime you need to equip or dismantle runes.", "Hãy quay lại Bàn Thờ bất cứ lúc nào bạn cần trang bị hoặc tháo dỡ rune nhé."),

        ("Choose Your Champion", "Chọn Chiến Binh Của Bạn"),
        ("Follow Trung to the Character Pedestal and select your combat avatar.", "Theo chân Trung đến Bệ Nhân Vật và chọn hóa thân chiến đấu của bạn."),
        ("Every expedition demands the right fighter. Follow me to the Character Pedestal.", "Mỗi chuyến thám hiểm đều cần một chiến binh phù hợp. Hãy theo tôi đến Bệ Chọn Nhân Vật."),
        ("Can I switch to different fighters with unique skills?", "Tôi có thể đổi sang các chiến binh khác nhau với bộ kỹ năng độc đáo không?"),
        ("Precisely. Let's head over and review your available champions.", "Chính xác. Hãy cùng qua đó và xem các anh hùng bạn đang sở hữu nhé."),
        ("Interact with the pedestal right here to inspect and switch your character.", "Tương tác với bệ đỡ ngay đây để kiểm tra và chuyển đổi nhân vật của bạn."),
        ("I have reviewed my characters and selected my preferred fighter.", "Tôi đã xem qua các nhân vật và chọn được chiến binh yêu thích của mình rồi."),
        ("Excellent choice! Mastery of your champion's mechanics is vital for survival. Here is your reward.", "Lựa chọn tuyệt vời! Thuần thục cơ chế của tướng là điều sống còn để sinh tồn. Đây là phần thưởng của bạn."),
        ("You can return to the pedestal whenever you wish to swap characters.", "Bạn có thể quay lại bệ bất cứ lúc nào muốn đổi nhân vật."),

        ("High Roller's Gamble", "Canh Bạc Tay Chơi"),
        ("Risk your hard-earned gems for double or triple returns after an expedition.", "Đặt cược số gem vất vả kiếm được để có cơ hội nhân đôi hoặc nhân ba sau chuyến thám hiểm."),
        ("Hehehe... Just crawled out of the rift alive, did you?", "Hê hê hê... Vừa bò ra khỏi vết nứt hầm ngục mà còn sống sót đấy à?"),
        ("Care to double those shiny gems in your pouch? One roll of fate, all or nothing!", "Có muốn nhân đôi số gem lấp lánh trong túi không? Một lần quay định mệnh, được ăn cả ngã về không!"),
        ("Come back after your next run if your pockets feel heavy again.", "Hãy quay lại sau chuyến đi tiếp theo nếu túi tiền của bạn lại rủng rỉnh nhé."),

        // ==========================================
        // 13. MẸO VÀ ĐỊA ĐIỂM (LOADING TIPS & DESTINATIONS)
        // ==========================================
        ("TIP", "MẸO"),
        ("Loading...", "Đang tải..."),
        ("Sharpening weapons... Steel meets darkness in the trials ahead.", "Đang mài sắc vũ khí... Lưỡi thép sẽ chạm bóng tối trong thử thách phía trước."),
        ("Lyra is weaving ancient Arcane glyphs. Do not interrupt her incantations.", "Lyra đang kết dệt những phù văn Arcane cổ xưa. Đừng làm gián đoạn lời chú của cô ấy."),
        ("Kael claims that dodging is easier than blocking. Learn his rhythm well.", "Kael khẳng định né đòn dễ hơn đỡ đòn. Hãy học thật kỹ nhịp điệu của anh ấy."),
        ("Ares channels brute rage. When low on health, his strikes become deadlier.", "Ares tụ hội cơn cuồng nộ. Khi máu thấp, đòn đánh của anh ta càng thêm chí mạng."),
        ("Elara never misses a target from the shadows. Keep your distance and kite.", "Elara chưa từng trượt mục tiêu từ trong bóng tối. Giữ khoảng cách và thả diều."),
        ("Transmuting rune affixes costs precious Shards. Plan your endgame build wisely!", "Chuyển hóa phụ tố rune tiêu tốn những Mảnh quý giá. Hãy hoạch định lối build cuối game thật khôn ngoan!"),
        ("Every rune dropped from the dungeon harbors latent elemental power.", "Mọi rune rơi ra từ hầm ngục đều ẩn chứa sức mạnh nguyên tố tiềm tàng."),
        ("Equipping matching elemental runes awakens formidable synergy passives.", "Trang bị các rune cùng nguyên tố sẽ đánh thức những nội tại cộng hưởng đáng gờm."),
        ("Higher Wager tiers drastically empower enemies but yield massive bonus Gems.", "Bậc Cược càng cao càng khiến kẻ địch mạnh lên khủng khiếp, nhưng phần thưởng Gem cũng khổng lồ."),
        ("Dying in a high-tier Wager run costs your entire bet. Retreat when overwhelmed!", "Chết trong lượt Cược bậc cao sẽ mất trắng tiền cược. Bị áp đảo thì hãy rút lui!"),
        ("Stamina management is key: sprinting and dodging recklessly leaves you defenseless.", "Quản lý thể lực là chìa khóa: chạy nhanh và né đòn bừa bãi sẽ khiến bạn không còn gì để phòng thủ."),
        ("Elite foes have relentless super-armor. Break their guard before committing combos.", "Kẻ địch tinh anh có lớp siêu giáp lì lợm. Hãy phá thế thủ của chúng trước khi tung combo."),
        ("Defeating The Earthshaker unlocks the forbidden domain of The DarkMage.", "Đánh bại The Earthshaker sẽ mở khóa lãnh địa cấm của The DarkMage."),
        ("Gacha duplicates are automatically converted into valuable Shards and Crystals.", "Trùng lặp gacha sẽ tự động chuyển hóa thành Mảnh và Tinh thể quý giá."),
        ("Pay attention to the red indicator markers on the ground to dodge devastating boss AoEs.", "Để ý các vạch chỉ báo đỏ trên mặt đất để né những chiêu AoE hủy diệt của boss."),
        ("Mana does not replenish instantly. Drink potions or manage spell cooldowns carefully.", "Năng lượng không hồi phục tức thì. Hãy uống thuốc hoặc canh hồi chiêu thật cẩn thận."),
        ("Equipping defensive Runes can turn fragile spellcasters into resilient battle-mages.", "Trang bị Rune phòng thủ có thể biến pháp sư mỏng manh thành chiến pháp sư kiên cường."),
        ("Bosses enter an enrage state at low HP. Save your ultimate skills for the final phase!", "Boss sẽ rơi vào trạng thái cuồng nộ khi máu thấp. Hãy dành kỹ năng cuối cho giai đoạn cuối!"),
        ("Safe zones shrink progressively in deep runs. Stay within the perimeter to survive.", "Vùng an toàn thu hẹp dần ở những tầng sâu. Hãy ở trong ranh giới để sống sót."),
        ("Gold earned inside dungeons is temporary, but Gems and Shards remain forever.", "Vàng kiếm trong hầm ngục chỉ là tạm thời, nhưng Gem và Mảnh thì còn mãi mãi."),
        ("Cleanse corrupted altars to gain temporary blessings before facing the domain Boss.", "Thanh tẩy những bàn thờ bị tha hóa để nhận phước lành tạm thời trước khi đối đầu Boss lãnh địa."),
        ("TRAVELING THROUGH THE VOID", "ĐANG DU HÀNH XUYÊN HƯ KHÔNG"),
        ("RETURNING TO: HEROES' SANCTUARY", "QUAY VỀ: THÁNH ĐỊA ANH HÙNG"),
        ("DESCENDING INTO: THE ABYSSAL DUNGEON", "TIẾN SÂU VÀO: HẦM NGỤC VỰC THẲM"),
        ("APPROACHING: NIGHTMARE LORD'S THRONE", "TIẾP CẬN: NGAI CỦA CHÚA TỂ ÁC MỘNG"),
        ("ENTERING: TRIAL OF ASCENSION", "BƯỚC VÀO: THỬ THÁCH THĂNG THIÊN"),
        ("CONNECTING TO: ASTRAL GATEWAY", "KẾT NỐI TỚI: CỔNG TINH TÚ"),

        // ==========================================
        // 14. ĐĂNG NHẬP / ĐĂNG KÝ (LOGIN / REGISTER)
        // ==========================================
        ("LOGIN", "ĐĂNG NHẬP"),
        ("Login", "Đăng nhập"),
        ("REGISTER", "ĐĂNG KÝ"),
        ("Register", "Đăng ký"),
        ("User or Email", "Người dùng hoặc Email"),
        ("Password", "Mật khẩu"),
        ("Remember Account ?", "Ghi nhớ tài khoản ?"),
        ("Remember Account?", "Ghi nhớ tài khoản?"),
        ("Forgot Password?", "Quên mật khẩu?"),
        ("Forgot Password ?", "Quên mật khẩu ?"),
        ("Confirm Password", "Xác nhận mật khẩu"),
        ("Email", "Email"),
        ("Username", "Tên đăng nhập"),
        ("Connecting...", "Đang kết nối..."),
        ("Connected", "Đã kết nối"),
        ("Reconnecting...", "Đang kết nối lại..."),
        ("Logging in...", "Đang đăng nhập..."),
        ("Authenticating...", "Đang xác thực..."),
        ("Verifying account...", "Đang xác minh tài khoản..."),
        ("Loading account data...", "Đang tải dữ liệu tài khoản..."),
        ("Please wait...", "Vui lòng chờ..."),
        ("Return to Lobby", "Quay lại Sảnh chờ"),
        ("Are you sure you want to abandon this battle and return to the Lobby?", "Bạn có chắc muốn từ bỏ trận chiến này và quay lại Sảnh chờ không?"),
        
    };

    // ✅ TEXT ĐỘNG (Có số / thẻ màu thay đổi) - dùng Regex
    private static readonly (Regex regex, string replacement)[] RegexEntries = new (Regex, string)[]
    {
        // "Level: 11 -> 12" → "Cấp độ: 11 -> 12"
        (new Regex(@"^Level:\s*(\d+)\s*->\s*(\d+)$", RegexOptions.IgnoreCase), "Cấp độ: $1 -> $2"),

        // "Need 211/7111 Gold" → "Cần 211/7111 Vàng"
        (new Regex(@"^Need\s+(\S+)\s+Gold$", RegexOptions.IgnoreCase), "Cần $1 Vàng"),

        // "Loading... 42%" → "Đang tải... 42%"
        (new Regex(@"^Loading\.\.\.\s*(\d+)%$", RegexOptions.IgnoreCase), "Đang tải... $1%"),

        // "ACCESSING: SHOP" → "TRUY CẬP: SHOP"
        (new Regex(@"^ACCESSING:\s*(.+)$", RegexOptions.IgnoreCase), "TRUY CẬP: $1"),

        // "JOURNEYING TO: XYZ" → "HÀNH TRÌNH TỚI: XYZ"
        (new Regex(@"^JOURNEYING TO:\s*(.+)$", RegexOptions.IgnoreCase), "HÀNH TRÌNH TỚI: $1"),

        // Dịch Skip Tutorial khi có gắn thẻ màu: "Are you sure you want to <color=...>Skip Tutorial</color>?"
        (new Regex(@"^Are you sure you want to\s+(<color=[^>]+>)?Skip Tutorial(</color>)?\??$", RegexOptions.IgnoreCase), "Bạn có chắc muốn bỏ qua Hướng dẫn không?"),

        // Dịch tự động mọi nút lựa chọn có đuôi (Locked) hoặc thẻ màu xám: "XYZ <color=#888888>(Locked)</color>"
        (new Regex(@"^(.*)\s*<color=[^>]+>\(Locked\)</color>$", RegexOptions.IgnoreCase), "$1 <color=#888888>(Đã khóa)</color>"),
        (new Regex(@"^(.*)\s*\(Locked\)$", RegexOptions.IgnoreCase), "$1 (Đã khóa)"),

        // Tự động dịch mọi tương tác dạng "Talk - XYZ" thành "Trò chuyện - XYZ"
        (new Regex(@"^Talk\s*-\s*(.+)$", RegexOptions.IgnoreCase), "Trò chuyện - $1"),
        (new Regex(@"^Talk\s+to\s+(.+)$", RegexOptions.IgnoreCase), "Nói chuyện với $1"),
        (new Regex(@"^Talk\s+with\s+(.+)$", RegexOptions.IgnoreCase), "Trò chuyện cùng $1"),

        // Dịch hiển thị sức chứa kho Rune: "Slots: 0 / 100" -> "Ô chứa: 0 / 100"
        (new Regex(@"^Slots:\s*(\d+)\s*/\s*(\d+)$", RegexOptions.IgnoreCase), "Ô chứa: $1 / $2"),

                // ========== DỊCH SỐ LƯỢNG VẬT PHẨM (DÙNG $1 ĐỂ GIỮ ĐÚNG SỐ LƯỢNG THẬT) ==========
        (new Regex(@"^Lucky Cat\s*x(\d+)$", RegexOptions.IgnoreCase), "Mèo May Mắn x$1"),
        (new Regex(@"^Medical Kit\s*x(\d+)$", RegexOptions.IgnoreCase), "Túi Cứu Thương x$1"),
        (new Regex(@"^Brawn Elixir\s*x(\d+)$", RegexOptions.IgnoreCase), "Dược Lực Chiến x$1"),

        // ========== DỊCH THÔNG TIN CẤP ĐỘ, CƯỢC & LỢI ÍCH TRONG MENU BOSS ==========
        // Cấp 1: Tiêu chuẩn (Standard)
        (new Regex(@"<b>Tier:\s*<color=[^>]+>Standard \(Safe\)</color></b>\s*\|\s*Bet:\s*<color=[^>]+>(\d+)\s*Gems</color>\s*\(x1\.0\)\s*<color=[^>]+>•\s*Benefits:</color>\s*Standard combat,\s*resources kept intact\.\s*<color=[^>]+>•\s*Note:</color>\s*No secondary buffs allowed\.", RegexOptions.IgnoreCase | RegexOptions.Singleline),
         "<b>Cấp: <color=#00FF00>Tiêu chuẩn (An toàn)</color></b> | Cược: <color=#00FFFF>$1 Ngọc</color> (x1.0)\n<color=#00FF00>• Lợi ích:</color> Chiến đấu tiêu chuẩn, giữ nguyên toàn bộ tài nguyên.\n<color=#FF5555>• Lưu ý:</color> Không được dùng buff phụ trợ."),

        // Cấp 2: Mạo hiểm (Risky)
        (new Regex(@"<b>Tier:\s*<color=[^>]+>Risky \(Challenge\)</color></b>\s*\|\s*Bet:\s*<color=[^>]+>(\d+)\s*Gems</color>\s*\(x1\.5\)\s*<color=[^>]+>•\s*Benefits:</color>\s*EXP & Resource gains \+50%\.\s*<color=[^>]+>•\s*Buffs:</color>\s*Gold Boost & Phoenix Charm available\.", RegexOptions.IgnoreCase | RegexOptions.Singleline),
         "<b>Cấp: <color=#FFFF00>Mạo hiểm (Thử thách)</color></b> | Cược: <color=#00FFFF>$1 Ngọc</color> (x1.5)\n<color=#00FF00>• Lợi ích:</color> Tăng +50% EXP & Tài nguyên nhận được.\n<color=#00FFFF>• Buff:</color> Mở khóa Mèo May Mắn & Bùa Phượng Hoàng."),

        // Cấp 3: Ác mộng (Nightmare)
        (new Regex(@"<b>Tier:\s*<color=[^>]+>Nightmare \(Hardcore\)</color></b>\s*\|\s*Bet:\s*<color=[^>]+>(\d+)\s*Gems</color>\s*\(x2\.5\)\s*<color=[^>]+>•\s*Benefits:</color>\s*Massive rewards \(x2\.5\) & Rune Shards\.\s*<color=[^>]+>•\s*Buffs:</color>\s*All 3 Combat Elixirs unlocked!", RegexOptions.IgnoreCase | RegexOptions.Singleline),
         "<b>Cấp: <color=#FF3333>Ác mộng (Hardcore)</color></b> | Cược: <color=#00FFFF>$1 Ngọc</color> (x2.5)\n<color=#00FF00>• Lợi ích:</color> Thưởng cực lớn (x2.5) & Mảnh Đá Rune.\n<color=#00FFFF>• Buff:</color> Mở khóa toàn bộ 3 loại Dược Phẩm Chiến Đấu!"),

        // Dịch hiển thị thứ hạng & điểm số cá nhân
        (new Regex(@"^Rank\s*#(\d+)$", RegexOptions.IgnoreCase), "Hạng #$1"),
        (new Regex(@"^Score\s*:\s*(.+)$", RegexOptions.IgnoreCase), "Điểm : $1"),
        (new Regex(@"^Combat Power:\s*(.+)$", RegexOptions.IgnoreCase), "Lực chiến: $1")
    };

    private static Dictionary<string, string> mapVI;

    private static void EnsureMap()
    {
        if (mapVI != null) return;
        mapVI = new Dictionary<string, string>();
        foreach (var e in Entries)
        {
            if (!mapVI.ContainsKey(e.en))
            {
                mapVI.Add(e.en, e.vi);
            }
        }
    }

    // ✅ Gộp khoảng trắng / xuống dòng / tab thành 1 khoảng trắng duy nhất
    private static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return Regex.Replace(text.Trim(), @"\s+", " ");
    }

    private static bool IsVietnamese()
    {
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
        {
            if (entry.regex.IsMatch(normalized))
                return entry.regex.Replace(normalized, entry.replacement);
        }

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
        {
            if (entry.regex.IsMatch(normalized))
                return true;
        }

        return false;
    }

    public static void Refresh()
    {
        mapVI = null;
        EnsureMap();
    }
}