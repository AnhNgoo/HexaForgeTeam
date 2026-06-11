using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class GoldLevelSystemTests : RuntimeSystemTestBase
    {
        protected override string ExportName => "GoldLevel";

        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-001: Kiểm tra GoldManager thật khởi tạo được.")]
        public IEnumerator GL_001_GoldManagerKhoiTaoDuoc() { return RunUnity("GL-001", "GoldManager thật khởi tạo được", "GoldManager thật AddComponent được.", "High", c => GoldInit(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-002: Kiểm tra vàng ban đầu không âm.")]
        public IEnumerator GL_002_VangBanDauKhongAm() { return RunUnity("GL-002", "Vàng ban đầu không âm", "CurrentGold ban đầu >= 0.", "High", c => GoldInitial(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-003: Kiểm tra AddGold tăng vàng.")]
        public IEnumerator GL_003_AddGoldTangVang() { return RunUnity("GL-003", "AddGold tăng vàng", "AddGold làm CurrentGold tăng đúng.", "High", c => AddGold(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-004: Kiểm tra AddGold bỏ qua số âm.")]
        public IEnumerator GL_004_AddGoldBoQuaSoAm() { return RunUnity("GL-004", "AddGold bỏ qua số âm", "AddGold âm không đổi vàng.", "Medium", c => AddGoldNegative(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-005: Kiểm tra RemoveGold trừ vàng.")]
        public IEnumerator GL_005_RemoveGoldTruVang() { return RunUnity("GL-005", "RemoveGold trừ vàng", "RemoveGold làm CurrentGold giảm đúng.", "High", c => RemoveGold(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-006: Kiểm tra vàng không âm khi trừ quá số hiện có.")]
        public IEnumerator GL_006_VangKhongAm() { return RunUnity("GL-006", "Vàng không âm", "RemoveGold quá số không làm vàng âm.", "High", c => GoldClamp(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-007: Kiểm tra HasEnoughGold đúng.")]
        public IEnumerator GL_007_HasEnoughGoldDung() { return RunUnity("GL-007", "HasEnoughGold đúng", "HasEnoughGold trả đúng true/false.", "Medium", c => HasEnoughGold(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-008: Kiểm tra ResetGold về 0.")]
        public IEnumerator GL_008_ResetGoldVe0() { return RunUnity("GL-008", "ResetGold về 0", "ResetGold đưa CurrentGold về 0.", "Medium", c => ResetGold(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-009: Kiểm tra LevelManager thật khởi tạo được.")]
        public IEnumerator GL_009_LevelManagerKhoiTaoDuoc() { return RunUnity("GL-009", "LevelManager thật khởi tạo được", "CurrentLevel >= 1.", "High", c => LevelInitial(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-010: Kiểm tra chi phí lên level hợp lệ.")]
        public IEnumerator GL_010_ChiPhiLenLevelHopLe() { return RunUnity("GL-010", "Chi phí lên level hợp lệ", "GetCurrentLevelUpCost trả số không âm.", "Medium", c => LevelCost(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-011: Kiểm tra thiếu vàng không lên level.")]
        public IEnumerator GL_011_ThieuVangKhongLenLevel() { return RunUnity("GL-011", "Thiếu vàng không lên level", "CanLevelUp false khi thiếu vàng.", "High", c => CannotLevel(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-012: Kiểm tra đủ vàng có thể lên level.")]
        public IEnumerator GL_012_DuVangCoTheLenLevel() { return RunUnity("GL-012", "Đủ vàng có thể lên level", "CanLevelUp true khi đủ vàng.", "High", c => CanLevel(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-013: Kiểm tra LevelUp tăng level và trừ vàng.")]
        public IEnumerator GL_013_LevelUpTangLevelTruVang() { return RunUnity("GL-013", "LevelUp tăng level và trừ vàng", "LevelUp tăng level và trừ đúng cost.", "High", c => LevelUp(c)); }
        [UnityTest, Category("GoldLevel"), Category("Tự động"), Description("GL-014: Kiểm tra max level không tăng thêm.")]
        public IEnumerator GL_014_MaxLevelKhongTangThem() { return RunUnity("GL-014", "Max level không tăng thêm", "Ở maxLevel thì LevelUp không tăng tiếp.", "Medium", c => MaxLevel(c)); }

        private IEnumerator GoldInit(Ctx c) { StartWatcher(); Component g = Gold(); yield return null; c.Actual = $"Component={g.GetType().Name}, Error/Exception={ErrorCount()}."; AssertNoErrors("GoldManager khởi tạo không được lỗi đỏ."); }
        private IEnumerator GoldInitial(Ctx c) { Component g = Gold(); yield return null; int v = ReadInt(g, "CurrentGold"); c.Actual = $"CurrentGold={v}."; Assert.GreaterOrEqual(v, 0); }
        private IEnumerator AddGold(Ctx c) { Component g = Gold(); yield return null; int b = ReadInt(g, "CurrentGold"); Assert.IsTrue(TryInvoke(g, "AddGold", 25)); int a = ReadInt(g, "CurrentGold"); c.Actual = $"Gold {b}->{a}."; Assert.AreEqual(b + 25, a); }
        private IEnumerator AddGoldNegative(Ctx c) { Component g = Gold(); yield return null; TryInvoke(g, "AddGold", 30); int b = ReadInt(g, "CurrentGold"); TryInvoke(g, "AddGold", -10); int a = ReadInt(g, "CurrentGold"); c.Actual = $"Gold trước={b}, sau AddGold(-10)={a}."; Assert.AreEqual(b, a); }
        private IEnumerator RemoveGold(Ctx c) { Component g = Gold(); yield return null; TryInvoke(g, "AddGold", 100); int b = ReadInt(g, "CurrentGold"); Assert.IsTrue(TryInvoke(g, "RemoveGold", 35)); int a = ReadInt(g, "CurrentGold"); c.Actual = $"Gold {b}->{a}."; Assert.AreEqual(b - 35, a); }
        private IEnumerator GoldClamp(Ctx c) { Component g = Gold(); yield return null; TryInvoke(g, "AddGold", 20); TryInvoke(g, "RemoveGold", 999); int a = ReadInt(g, "CurrentGold"); c.Actual = $"Gold sau RemoveGold(999)={a}."; Assert.GreaterOrEqual(a, 0); }
        private IEnumerator HasEnoughGold(Ctx c) { Component g = Gold(); yield return null; TryInvoke(g, "AddGold", 50); Assert.IsTrue(TryInvoke(g, "HasEnoughGold", out object ok30, 30)); Assert.IsTrue(TryInvoke(g, "HasEnoughGold", out object ok80, 80)); c.Actual = $"Gold=50, HasEnoughGold(30)={ok30}, HasEnoughGold(80)={ok80}."; Assert.IsTrue((bool)ok30); Assert.IsFalse((bool)ok80); }
        private IEnumerator ResetGold(Ctx c) { Component g = Gold(); yield return null; TryInvoke(g, "AddGold", 75); Assert.IsTrue(TryInvoke(g, "ResetGold")); int a = ReadInt(g, "CurrentGold"); c.Actual = $"Gold sau ResetGold={a}."; Assert.AreEqual(0, a); }
        private IEnumerator LevelInitial(Ctx c) { Component l = Level(); yield return null; int current = ReadInt(l, "CurrentLevel"); c.Actual = $"CurrentLevel={current}, maxLevel={ReadInt(l, "maxLevel")}."; Assert.GreaterOrEqual(current, 1); }
        private IEnumerator LevelCost(Ctx c) { Component l = Level(); yield return null; Assert.IsTrue(TryInvoke(l, "GetCurrentLevelUpCost", out object cost)); int value = System.Convert.ToInt32(cost); c.Actual = $"Cost={value}."; Assert.GreaterOrEqual(value, 0); }
        private IEnumerator CannotLevel(Ctx c) { Component g = Gold(); Component l = Level(); yield return null; TryInvoke(g, "ResetGold"); Assert.IsTrue(TryInvoke(l, "CanLevelUp", out object can)); c.Actual = $"Gold=0, CanLevelUp={can}."; Assert.IsFalse((bool)can); }
        private IEnumerator CanLevel(Ctx c) { Component g = Gold(); Component l = Level(); yield return null; TryInvoke(l, "GetCurrentLevelUpCost", out object costObj); int cost = System.Convert.ToInt32(costObj); TryInvoke(g, "AddGold", cost); TryInvoke(l, "CanLevelUp", out object can); c.Actual = $"Gold={ReadInt(g, "CurrentGold")}, cost={cost}, CanLevelUp={can}."; Assert.IsTrue((bool)can); }
        private IEnumerator LevelUp(Ctx c) { Component g = Gold(); Component l = Level(); yield return null; int beforeLevel = ReadInt(l, "CurrentLevel"); TryInvoke(l, "GetCurrentLevelUpCost", out object costObj); int cost = System.Convert.ToInt32(costObj); TryInvoke(g, "AddGold", cost + 20); int beforeGold = ReadInt(g, "CurrentGold"); Assert.IsTrue(TryInvoke(l, "LevelUp")); int afterLevel = ReadInt(l, "CurrentLevel"); int afterGold = ReadInt(g, "CurrentGold"); c.Actual = $"Level {beforeLevel}->{afterLevel}, Gold {beforeGold}->{afterGold}, cost={cost}."; Assert.AreEqual(beforeLevel + 1, afterLevel); Assert.AreEqual(beforeGold - cost, afterGold); }
        private IEnumerator MaxLevel(Ctx c) { Component g = Gold(); Component l = Level(); yield return null; int max = ReadInt(l, "maxLevel"); Assert.IsTrue(TrySet(l, "currentLevel", max)); TryInvoke(g, "AddGold", 999999); TryInvoke(l, "LevelUp"); int after = ReadInt(l, "CurrentLevel"); c.Actual = $"maxLevel={max}, CurrentLevel sau LevelUp={after}."; Assert.AreEqual(max, after); }

        private Component Gold() { return CreateRealComponent("GoldManager", "Test_GoldManager"); }
        private Component Level() { return CreateRealComponent("LevelManager", "Test_LevelManager"); }
    }
}
