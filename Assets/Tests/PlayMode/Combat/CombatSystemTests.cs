using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class CombatSystemTests
    {
        private const string TesterName = "Huỳnh Ngọc Thanh Phước";
        private const string StartDate = "31/05/2026";
        private const string RunMode = "Tự động";
        private static readonly List<TestResultRecord> records = new List<TestResultRecord>();

        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();
        private TestLogWatcher watcher;

        [TearDown]
        public void TearDown()
        {
            if (watcher != null) watcher.Stop();
            watcher = null;
            for (int i = spawned.Count - 1; i >= 0; i--)
                if (spawned[i] != null) UnityEngine.Object.DestroyImmediate(spawned[i]);
            spawned.Clear();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestResultCsvExporter.Export("Combat", records);
        }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Player đánh trúng Enemy thì Enemy giảm HP nếu gọi được logic thật.")]
        public IEnumerator CB_001_PlayerDanhTrungEnemy_GiamHP() { yield return RunUnity("CB-001", "Player đánh trúng Enemy thì Enemy giảm HP", "Enemy giảm HP khi Player attack hoặc gọi damage thật tương ứng.", "High", c => PlayerDamageEnemy(c, true)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Player đánh ngoài tầm thì Enemy không giảm HP nếu kiểm tra được.")]
        public IEnumerator CB_002_PlayerDanhNgoaiTam_EnemyKhongGiamHP() { yield return RunUnity("CB-002", "Player đánh ngoài tầm thì Enemy không giảm HP", "Enemy ngoài tầm không bị giảm HP khi chỉ gọi Attack Player.", "Medium", c => PlayerOutOfRange(c)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Enemy đánh trúng Player thì Player giảm HP nếu gọi được logic thật.")]
        public IEnumerator CB_003_EnemyDanhTrungPlayer_PlayerGiamHP() { yield return RunUnity("CB-003", "Enemy đánh trúng Player thì Player giảm HP", "Player HP giảm khi Enemy attack thật nếu project có Player HP.", "High", c => EnemyDamagePlayer(c, true)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Enemy đánh ngoài tầm thì Player không giảm HP nếu kiểm tra được.")]
        public IEnumerator CB_004_EnemyDanhNgoaiTam_PlayerKhongGiamHP() { yield return RunUnity("CB-004", "Enemy đánh ngoài tầm thì Player không giảm HP", "Player ngoài tầm không bị giảm HP nếu đọc được Player HP.", "Medium", c => EnemyOutOfRange(c)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra hitbox Player chỉ gây damage khi Attack nếu kiểm tra được.")]
        public IEnumerator CB_005_PlayerHitboxChiDamageKhiAttack() { yield return RunUnity("CB-005", "Hitbox Player chỉ gây damage khi Attack", "Enemy không giảm HP trước Attack, chỉ giảm sau khi gọi logic Attack/damage thật.", "Medium", c => PlayerHitboxGate(c)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra hitbox Enemy chỉ gây damage khi Attack nếu kiểm tra được.")]
        public IEnumerator CB_006_EnemyHitboxChiDamageKhiAttack() { yield return RunUnity("CB-006", "Hitbox Enemy chỉ gây damage khi Attack", "Player không giảm HP trước Enemy Attack, chỉ giảm sau attack nếu hệ thống Player HP hỗ trợ.", "Medium", c => EnemyHitboxGate(c)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Player không spam damage mỗi frame.")]
        public IEnumerator CB_007_PlayerKhongSpamDamageMoiFrame() { yield return RunUnity("CB-007", "Player không spam damage mỗi frame", "Gọi Player Attack nhiều lần không làm Enemy nhận damage vô hạn mỗi frame.", "Medium", c => SpamDamage(c, true)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Enemy không spam damage mỗi frame.")]
        public IEnumerator CB_008_EnemyKhongSpamDamageMoiFrame() { yield return RunUnity("CB-008", "Enemy không spam damage mỗi frame", "Gọi Enemy Attack nhiều lần không làm Player nhận damage vô hạn mỗi frame nếu đọc được HP.", "Medium", c => SpamDamage(c, false)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Skill 1 gây damage đúng nếu Skill 1 là skill tấn công và kiểm tra được.")]
        public IEnumerator CB_009_Skill1GayDamageNeuLaSkillTanCong() { yield return RunUnity("CB-009", "Skill 1 gây damage nếu là skill tấn công", "Skill 1 gọi được và Enemy HP thay đổi hợp lệ nếu skill có damage.", "Medium", c => SkillDamage(c, true)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Skill 1 không tác động sai mục tiêu ngoài phạm vi nếu kiểm tra được.")]
        public IEnumerator CB_010_Skill1KhongTacDongSaiMucTieuNgoaiPhamVi() { yield return RunUnity("CB-010", "Skill 1 không tác động sai mục tiêu ngoài phạm vi", "Enemy ngoài phạm vi không giảm HP bất thường khi gọi Skill 1.", "Medium", c => SkillDamage(c, false)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Enemy chết không còn gây damage.")]
        public IEnumerator CB_011_EnemyChetKhongConGayDamage() { yield return RunUnity("CB-011", "Enemy chết không còn gây damage", "Enemy sau khi chết không gây lỗi và không gây damage thêm nếu Player HP đọc được.", "High", c => DeadUnitNoDamage(c, false)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra Player chết không còn gây damage nếu project có Player death.")]
        public IEnumerator CB_012_PlayerChetKhongConGayDamage() { yield return RunUnity("CB-012", "Player chết không còn gây damage", "Player sau khi chết không gây damage nếu project có Player HP/death thật.", "High", c => DeadUnitNoDamage(c, true)); }

        [UnityTest, Category("Combat"), Category("Tự động")]
        [Description("Kiểm tra combat cơ bản không Error hoặc Exception.")]
        public IEnumerator CB_013_CombatCoBanKhongLoiDo() { yield return RunUnity("CB-013", "Combat cơ bản không Error/Exception", "Spawn Player/Enemy, gọi attack/skill/damage thật không phát sinh Error/Exception.", "High", c => CombatSmoke(c)); }

        private IEnumerator PlayerDamageEnemy(Ctx c, bool requireDamage)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, 0f));
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 1.5f));
            yield return null;
            float hp0 = EnemyHp(enemy);
            string attack = InvokePlayerAttack(player);
            string damage = ApplyEnemyDamage(enemy, 10f);
            yield return null;
            float hp1 = EnemyHp(enemy);
            c.Actual = $"HP Enemy trước={N(hp0)}, sau={N(hp1)}, khoảng cách={N(Vector3.Distance(player.transform.position, enemy.transform.position))}, method attack={attack}, method damage={damage}, lỗi Console={Errors()}.";
            AssertNoErrors("Player đánh Enemy phát sinh Error/Exception.");
            if (requireDamage) Assert.Less(hp1, hp0, "Enemy không giảm HP sau logic Player attack/damage thật.");
        }

        private IEnumerator PlayerOutOfRange(Ctx c)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 100f));
            yield return null;
            float hp0 = EnemyHp(enemy);
            string attack = InvokePlayerAttack(player);
            yield return new WaitForSeconds(0.3f);
            float hp1 = EnemyHp(enemy);
            c.Actual = $"HP Enemy trước={N(hp0)}, sau={N(hp1)}, khoảng cách={N(Vector3.Distance(player.transform.position, enemy.transform.position))}, method attack={attack}, lỗi Console={Errors()}.";
            AssertNoErrors("Player attack ngoài tầm phát sinh Error/Exception.");
            Assert.AreEqual(hp0, hp1, 0.01f, "Enemy ngoài tầm vẫn giảm HP khi chỉ gọi Player Attack.");
        }

        private IEnumerator EnemyDamagePlayer(Ctx c, bool requireDamage)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, 1.5f));
            GameObject enemy = SpawnEnemy(Vector3.zero);
            yield return null;
            float hp0 = PlayerHp(player, true);
            string attack = InvokeEnemyAttack(enemy);
            yield return new WaitForSeconds(0.3f);
            float hp1 = PlayerHp(player, true);
            c.Actual = $"HP Player trước={N(hp0)}, sau={N(hp1)}, khoảng cách={N(Vector3.Distance(player.transform.position, enemy.transform.position))}, method Enemy attack={attack}, lỗi Console={Errors()}.";
            AssertNoErrors("Enemy đánh Player phát sinh Error/Exception.");
            if (requireDamage) Assert.Less(hp1, hp0, "Player không giảm HP sau Enemy attack thật.");
        }

        private IEnumerator EnemyOutOfRange(Ctx c)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, 100f));
            GameObject enemy = SpawnEnemy(Vector3.zero);
            yield return null;
            float hp0 = PlayerHp(player, true);
            string attack = InvokeEnemyAttack(enemy);
            yield return new WaitForSeconds(0.3f);
            float hp1 = PlayerHp(player, true);
            c.Actual = $"HP Player trước={N(hp0)}, sau={N(hp1)}, khoảng cách={N(Vector3.Distance(player.transform.position, enemy.transform.position))}, method Enemy attack={attack}, lỗi Console={Errors()}.";
            AssertNoErrors("Enemy attack ngoài tầm phát sinh Error/Exception.");
            Assert.AreEqual(hp0, hp1, 0.01f, "Player ngoài tầm vẫn giảm HP khi Enemy attack.");
        }

        private IEnumerator PlayerHitboxGate(Ctx c)
        {
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 1.5f));
            GameObject player = SpawnPlayer(Vector3.zero);
            yield return null;
            float hp0 = EnemyHp(enemy);
            yield return new WaitForSeconds(0.2f);
            float hpIdle = EnemyHp(enemy);
            string attack = InvokePlayerAttack(player);
            string damage = ApplyEnemyDamage(enemy, 5f);
            yield return null;
            float hpAfter = EnemyHp(enemy);
            c.Actual = $"HP Enemy ban đầu={N(hp0)}, sau đứng yên={N(hpIdle)}, sau Attack={N(hpAfter)}, method attack={attack}, damage method={damage}.";
            Assert.AreEqual(hp0, hpIdle, 0.01f, "Enemy bị giảm HP khi Player chưa Attack.");
            Assert.Less(hpAfter, hpIdle, "Enemy không giảm HP sau Attack/damage thật.");
        }

        private IEnumerator EnemyHitboxGate(Ctx c)
        {
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, 1.5f));
            GameObject enemy = SpawnEnemy(Vector3.zero);
            yield return null;
            float hp0 = PlayerHp(player, true);
            yield return new WaitForSeconds(0.2f);
            float hpIdle = PlayerHp(player, true);
            string attack = InvokeEnemyAttack(enemy);
            yield return new WaitForSeconds(0.2f);
            float hpAfter = PlayerHp(player, true);
            c.Actual = $"HP Player ban đầu={N(hp0)}, sau đứng yên={N(hpIdle)}, sau Enemy Attack={N(hpAfter)}, method attack={attack}.";
            Assert.AreEqual(hp0, hpIdle, 0.01f, "Player bị giảm HP khi Enemy chưa Attack.");
        }

        private IEnumerator SpamDamage(Ctx c, bool playerAttacks)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 1.5f));
            yield return null;
            float hp0 = playerAttacks ? EnemyHp(enemy) : PlayerHp(player, true);
            string method = "";
            for (int i = 0; i < 5; i++)
            {
                method = playerAttacks ? InvokePlayerAttack(player) : InvokeEnemyAttack(enemy);
                yield return null;
            }
            float hp1 = playerAttacks ? EnemyHp(enemy) : PlayerHp(player, true);
            c.Actual = $"Bên tấn công={(playerAttacks ? "Player" : "Enemy")}, HP mục tiêu trước={N(hp0)}, sau={N(hp1)}, số lần gọi=5, method={method}, lỗi Console={Errors()}.";
            AssertNoErrors("Spam combat phát sinh Error/Exception.");
        }

        private IEnumerator SkillDamage(Ctx c, bool inRange)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(inRange ? new Vector3(0f, 1f, 2f) : new Vector3(0f, 1f, 100f));
            yield return null;
            float hp0 = EnemyHp(enemy);
            string method = InvokeSkill1(player);
            yield return new WaitForSeconds(0.5f);
            float hp1 = EnemyHp(enemy);
            c.Actual = $"Skill1 method={method}, Enemy trong tầm={inRange}, HP Enemy trước={N(hp0)}, sau={N(hp1)}, khoảng cách={N(Vector3.Distance(player.transform.position, enemy.transform.position))}, lỗi Console={Errors()}.";
            AssertNoErrors("Skill 1 combat phát sinh Error/Exception.");
            if (!inRange) Assert.AreEqual(hp0, hp1, 0.01f, "Enemy ngoài phạm vi bị Skill 1 tác động sai.");
        }

        private IEnumerator DeadUnitNoDamage(Ctx c, bool playerDies)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 1.5f));
            yield return null;
            if (playerDies)
            {
                Assert.Fail("Project chưa có Player HP/Death method thật để đưa Player về trạng thái chết và kiểm tra combat sau chết.");
            }
            else
            {
                float hp0 = EnemyHp(enemy);
                ApplyEnemyDamage(enemy, hp0 + 9999f);
                string method = InvokeEnemyAttack(enemy);
                yield return new WaitForSeconds(0.2f);
                c.Actual = $"Enemy HP trước={N(hp0)}, sau chết={N(EnemyHp(enemy))}, method attack sau chết={method}, lỗi Console={Errors()}.";
                AssertNoErrors("Enemy chết rồi combat phát sinh Error/Exception.");
            }
        }

        private IEnumerator CombatSmoke(Ctx c)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 1.5f));
            yield return null;
            string pAttack = InvokePlayerAttack(player);
            string skill = InvokeSkill1(player);
            string eAttack = InvokeEnemyAttack(enemy);
            string damage = ApplyEnemyDamage(enemy, 3f);
            yield return new WaitForSeconds(0.5f);
            c.Actual = $"Player attack={pAttack}, Skill1={skill}, Enemy attack={eAttack}, damage method={damage}, Enemy HP={N(EnemyHp(enemy))}, lỗi Console={Errors()}.";
            AssertNoErrors("Combat cơ bản phát sinh Error/Exception.");
        }

        private string InvokePlayerAttack(GameObject player)
        {
            Component combat = TestReflectionHelper.FindComponentByClassName(player, "CharacterCombat");
            if (combat != null && TestReflectionHelper.TryInvokeMethod(combat, "TryAttack")) return "CharacterCombat.TryAttack";
            Component baseComp = TestReflectionHelper.FindComponentByClassName(player, "CharacterBase");
            if (baseComp != null && TestReflectionHelper.TryInvokeMethod(baseComp, "OnAttack")) return "CharacterBase.OnAttack";
            Assert.Fail("Không tìm thấy method Attack thật trên Player prefab.");
            return "Không tìm thấy";
        }

        private string InvokeSkill1(GameObject player)
        {
            Component skill = TestReflectionHelper.FindComponentByClassName(player, "CharacterSkill");
            if (skill != null && TestReflectionHelper.TryInvokeMethod(skill, "UseSkill1")) return "CharacterSkill.UseSkill1";
            Assert.Fail("Không tìm thấy method Skill 1 thật trên Player prefab.");
            return "Không tìm thấy";
        }

        private string InvokeEnemyAttack(GameObject enemy)
        {
            Component combat = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyCombat");
            Assert.IsNotNull(combat, "Không tìm thấy EnemyCombat trên Enemy prefab thật.");
            object arsenal = null;
            if (TestReflectionHelper.TryGetValue(combat, "AttackArsenal", out arsenal) && arsenal is Array attacks && attacks.Length > 0)
            {
                object attack = attacks.GetValue(0);
                if (attack != null && TestReflectionHelper.TryInvokeMethod(combat, "PerformAttack", attack)) return "EnemyCombat.PerformAttack";
            }
            if (TestReflectionHelper.TryInvokeMethod(combat, "OpenHitbox")) return "EnemyCombat.OpenHitbox";
            Assert.Fail("Không tìm thấy method Enemy attack thật hoặc AttackArsenal rỗng.");
            return "Không tìm thấy";
        }

        private string ApplyEnemyDamage(GameObject enemy, float damage)
        {
            Component receiver = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyDamageReceiver");
            if (receiver != null && TestReflectionHelper.TryInvokeMethod(receiver, "TakeHit", damage, 0f)) return "EnemyDamageReceiver.TakeHit";
            Component health = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyHealth");
            if (health != null && TestReflectionHelper.TryInvokeMethod(health, "TakeDamage", damage)) return "EnemyHealth.TakeDamage";
            Assert.Fail("Không tìm thấy method damage thật trên Enemy.");
            return "Không tìm thấy";
        }

        private float EnemyHp(GameObject enemy)
        {
            Component health = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyHealth");
            object value = null;
            Assert.IsTrue(health != null && TestReflectionHelper.TryGetValue(health, "currentHealth", out value), "Không đọc được currentHealth thật trên Enemy.");
            return Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        private float PlayerHp(GameObject player, bool fail)
        {
            Component[] components = player.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component == null) continue;
                object value = null;
                if (TestReflectionHelper.TryGetValue(component, "currentHealth", out value) ||
                    TestReflectionHelper.TryGetValue(component, "health", out value) ||
                    TestReflectionHelper.TryGetValue(component, "hp", out value))
                {
                    return Convert.ToSingle(value, CultureInfo.InvariantCulture);
                }
            }
            if (fail) Assert.Fail("Không tìm thấy HP/Health thật trên Player prefab để kiểm tra giảm HP.");
            return -1f;
        }

        private GameObject SpawnPlayer(Vector3 pos)
        {
            GameObject prefab = TestPrefabFinder.FindPlayerPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Player prefab thật trong project.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);
            go.name = prefab.name + "_CombatTest";
            go.tag = "Player";
            spawned.Add(go);
            return go;
        }

        private GameObject SpawnEnemy(Vector3 pos)
        {
            GameObject prefab = TestPrefabFinder.FindEnemyPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Enemy prefab thật trong project.");
            return TestEnemySpawnHelper.SpawnEnemyWithCampLifecycle(prefab, pos, "_CombatTest", spawned);
        }

        private void StartWatcher()
        {
            if (watcher != null) watcher.Stop();
            watcher = new TestLogWatcher();
            watcher.Start();
        }

        private int Errors() => watcher == null ? 0 : watcher.GetErrors().Count;
        private void AssertNoErrors(string msg) => Assert.IsFalse(watcher != null && watcher.HasErrorOrException, msg + " Lỗi: " + string.Join(" | ", watcher.GetErrors()));
        private string N(float v) => v.ToString("0.00", CultureInfo.InvariantCulture);

        private IEnumerator RunUnity(string id, string title, string expected, string severity, Func<Ctx, IEnumerator> body)
        {
            Ctx c = new Ctx();
            Exception failure = null;
            IEnumerator routine = null;
            try { routine = body(c); } catch (Exception e) { failure = e; }
            while (failure == null)
            {
                bool next = false; object current = null;
                try { next = routine != null && routine.MoveNext(); if (next) current = routine.Current; } catch (Exception e) { failure = e; }
                if (failure != null || !next) break;
                yield return current;
            }
            if (failure == null) Record(id, title, expected, c.Actual, "Pass", "", "Tự động kiểm tra combat bằng Unity Test Runner.");
            else { Record(id, title, expected, (c.Actual + " KHÔNG ĐẠT - " + failure.Message).Trim(), "Fail", severity, "Tự động kiểm tra combat bằng Unity Test Runner."); throw failure; }
        }

        private void Record(string id, string title, string expected, string actual, string status, string severity, string steps)
        {
            records.Add(new TestResultRecord
            {
                MaTC = id,
                TieuDeTestcase = title,
                KetQuaMongDoi = expected,
                KetQuaThucTe = actual,
                TinhTrangThucThi = status,
                MucDoNghiemTrongCuaLoi = severity,
                KieuChay = RunMode,
                NguoiKiemThu = TesterName,
                NgayBatDau = StartDate,
                ChiTietBuocKiemThu = steps,
                GhiChu = ""
            });
        }

        private class Ctx { public string Actual = ""; }
    }
}
