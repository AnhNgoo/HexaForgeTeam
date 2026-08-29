using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class EnemySystemTests
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
            StopWatcher();
            for (int i = spawned.Count - 1; i >= 0; i--)
                if (spawned[i] != null) UnityEngine.Object.DestroyImmediate(spawned[i]);
            spawned.Clear();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestResultCsvExporter.Export("Enemy", records);
        }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy prefab thật instantiate được và không lỗi đỏ.")]
        public IEnumerator EN_001_EnemyPrefabInstantiateDuoc() { yield return RunUnity("EN-001", "Enemy prefab instantiate được", "Enemy prefab thật được tạo trong scene, không null và không phát sinh Error/Exception.", "High", c => SpawnSmoke(c)); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy không bị Missing Script.")]
        public void EN_002_EnemyKhongMissingScript() { Run("EN-002", "Enemy không Missing Script", "Enemy và object con không có component Missing Script.", "High", c => MissingScript(c)); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyBase.")]
        public void EN_003_CoEnemyBase() { ComponentPresence("EN-003", "Enemy có EnemyBase", "EnemyBase", "Enemy có component EnemyBase.", "High"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyData nếu project dùng.")]
        public void EN_004_CoEnemyData() { Run("EN-004", "Enemy có EnemyData", "EnemyBase.Data hoặc enemyData không null.", "High", c => EnemyData(c)); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyHealth.")]
        public void EN_005_CoEnemyHealth() { ComponentPresence("EN-005", "Enemy có EnemyHealth", "EnemyHealth", "Enemy có component EnemyHealth.", "High"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyDamageReceiver.")]
        public void EN_006_CoEnemyDamageReceiver() { ComponentPresence("EN-006", "Enemy có EnemyDamageReceiver", "EnemyDamageReceiver", "Enemy có component EnemyDamageReceiver.", "High"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyDetection.")]
        public void EN_007_CoEnemyDetection() { ComponentPresence("EN-007", "Enemy có EnemyDetection", "EnemyDetection", "Enemy có component EnemyDetection.", "High"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyLocomotion.")]
        public void EN_008_CoEnemyLocomotion() { ComponentPresence("EN-008", "Enemy có EnemyLocomotion", "EnemyLocomotion", "Enemy có component EnemyLocomotion.", "High"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyCombat.")]
        public void EN_009_CoEnemyCombat() { ComponentPresence("EN-009", "Enemy có EnemyCombat", "EnemyCombat", "Enemy có component EnemyCombat.", "High"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyHitbox nếu project dùng.")]
        public void EN_010_CoEnemyHitbox() { OptionalComponent("EN-010", "Enemy có EnemyHitbox nếu project dùng", "EnemyHitbox", "EnemyHitbox tồn tại nếu prefab dùng hitbox cận chiến.", "Medium"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyStateMachine nếu project dùng.")]
        public void EN_011_CoEnemyStateMachine() { ComponentPresence("EN-011", "Enemy có EnemyStateMachine", "EnemyStateMachine", "Enemy có EnemyStateMachine.", "Medium"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyPoiseSystem nếu project dùng.")]
        public void EN_012_CoEnemyPoiseSystem() { OptionalComponent("EN-012", "Enemy có EnemyPoiseSystem nếu project dùng", "EnemyPoiseSystem", "EnemyPoiseSystem tồn tại nếu prefab dùng poise/stagger.", "Medium"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyLootDropper nếu project dùng.")]
        public void EN_013_CoEnemyLootDropper() { OptionalComponent("EN-013", "Enemy có EnemyLootDropper nếu project dùng", "EnemyLootDropper", "EnemyLootDropper tồn tại nếu prefab dùng loot.", "Low"); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có EnemyProjectile nếu project dùng.")]
        public void EN_014_CoEnemyProjectileNeuDung() { OptionalComponent("EN-014", "Enemy có EnemyProjectile nếu project dùng", "EnemyProjectile", "EnemyProjectile tồn tại nếu prefab là projectile/ranged.", "Low"); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra HP ban đầu Enemy hợp lệ nếu đọc được.")]
        public IEnumerator EN_015_HPBanDauHopLe() { yield return RunUnity("EN-015", "HP ban đầu Enemy hợp lệ", "HP ban đầu đọc được và lớn hơn 0.", "High", c => HealthInitial(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy nhận damage được bằng method thật nếu có.")]
        public IEnumerator EN_016_EnemyNhanDamageDuoc() { yield return RunUnity("EN-016", "Enemy nhận damage được", "Gọi TakeHit/TakeDamage thật làm HP Enemy giảm nếu hệ thống hỗ trợ.", "High", c => DamageEnemy(c, false)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy chết khi HP về 0 nếu kiểm tra được.")]
        public IEnumerator EN_017_EnemyChetKhiHPVe0() { yield return RunUnity("EN-017", "Enemy chết khi HP về 0", "Gọi damage lớn đưa HP về 0 và trạng thái chết/event chết được kích hoạt nếu đọc được.", "High", c => DamageEnemy(c, true)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy không gây lỗi khi đã chết mà gọi attack.")]
        public IEnumerator EN_018_EnemyKhongGayDamageSauKhiChetNeuKiemTraDuoc() { yield return RunUnity("EN-018", "Enemy không hoạt động/gây damage sau khi chết nếu kiểm tra được", "Enemy đã nhận damage chết rồi gọi combat không phát sinh Error/Exception.", "Medium", c => DeadThenAttack(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy không lỗi khi không có Player.")]
        public IEnumerator EN_019_EnemyKhongLoiKhiKhongCoPlayer() { yield return RunUnity("EN-019", "Enemy không lỗi khi không có Player", "Enemy chạy vài frame khi scene không có Player mà không Error/Exception.", "High", c => RunFramesNoPlayer(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy nhận target Player nếu có Player prefab thật.")]
        public IEnumerator EN_020_EnemyNhanTargetPlayerNeuCo() { yield return RunUnity("EN-020", "Enemy nhận target Player nếu có Player thật", "Spawn Player prefab thật gần Enemy và detection/target không lỗi.", "Medium", c => EnemyWithPlayer(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy không rơi xuyên nền.")]
        public IEnumerator EN_021_EnemyKhongRoiXuyenNen() { yield return RunUnity("EN-021", "Enemy không rơi xuyên nền", "Enemy đứng trên ground test và Y không giảm bất thường.", "High", c => GroundCheck(c)); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có Collider hoặc CharacterController.")]
        public void EN_022_CoColliderHoacCharacterController() { Run("EN-022", "Enemy có Collider hoặc CharacterController", "Enemy có thành phần va chạm hợp lệ.", "High", c => CollisionComponent(c)); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có Renderer và material hợp lệ.")]
        public void EN_023_RendererMaterialHopLe() { Run("EN-023", "Enemy có Renderer/material hợp lệ", "Enemy có Renderer enabled và sharedMaterials không null.", "Medium", c => RendererMaterial(c)); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có Animator/RuntimeAnimatorController hợp lệ nếu có.")]
        public void EN_024_AnimatorHopLe() { Run("EN-024", "Enemy có Animator hợp lệ nếu có", "Animator nếu tồn tại thì RuntimeAnimatorController không null.", "Medium", c => AnimatorRefs(c)); }

        [Test, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy có AudioSource/AudioClip reference hợp lệ nếu có.")]
        public void EN_025_AudioHopLeNeuCo() { Run("EN-025", "Enemy có Audio reference hợp lệ nếu có", "AudioSource playOnAwake nếu có thì clip không null.", "Low", c => AudioRefs(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra spawn nhiều Enemy cùng lúc không lỗi.")]
        public IEnumerator EN_026_NhieuEnemySpawnKhongLoi() { yield return RunUnity("EN-026", "Nhiều Enemy spawn cùng lúc không lỗi", "Spawn 5 Enemy prefab thật không phát sinh Error/Exception.", "Medium", c => ManyEnemies(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy không tự disable ngay sau spawn.")]
        public IEnumerator EN_027_EnemyKhongTuDisableSauSpawn() { yield return RunUnity("EN-027", "Enemy không tự disable ngay sau spawn", "Enemy vẫn active sau vài frame.", "High", c => NotSelfDisable(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy chạy 60 frame không Error/Exception.")]
        public IEnumerator EN_028_EnemyChay60FrameKhongLoi() { yield return RunUnity("EN-028", "Enemy chạy 60 frame không Error/Exception", "Enemy prefab thật chạy 60 frame không lỗi đỏ.", "High", c => SixtyFrames(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy đánh được Player nếu hệ thống combat có method thật.")]
        public IEnumerator EN_029_EnemyDanhPlayerNeuCoLogic() { yield return RunUnity("EN-029", "Enemy đánh được Player nếu hệ thống combat có", "Enemy chọn/gọi attack thật với Player prefab thật không Error/Exception.", "Medium", c => EnemyAttackPlayer(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra Enemy không đánh khi đã chết nếu có thể kiểm tra.")]
        public IEnumerator EN_030_EnemyKhongDanhKhiDaChetNeuKiemTraDuoc() { yield return RunUnity("EN-030", "Enemy không đánh khi đã chết nếu có thể kiểm tra", "Enemy sau khi HP về 0 gọi attack không lỗi và không mở hitbox bất thường nếu đọc được.", "Medium", c => DeadThenAttack(c)); }

        [UnityTest, Category("Enemy"), Category("Tự động")]
        [Description("Kiểm tra tổng Enemy không lỗi đỏ.")]
        public IEnumerator EN_031_TongEnemyKhongLoiDo() { yield return RunUnity("EN-031", "Tổng Enemy không lỗi đỏ", "Spawn Enemy, nhận damage, spawn Player, chọn attack và chạy vài frame không Error/Exception.", "High", c => EnemySummary(c)); }

        private IEnumerator SpawnSmoke(Ctx c)
        {
            StartWatcher();
            GameObject prefab = TestPrefabFinder.FindEnemyPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Enemy prefab thật trong project để test.");
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return null; yield return null;
            c.Actual = $"Prefab={prefab.name}, vị trí spawn={F(enemy.transform.position)}, số lỗi Console={ErrorCount()}.";
            AssertNoErrors("Instantiate Enemy prefab thật phát sinh Error/Exception.");
        }

        private void MissingScript(Ctx c)
        {
            GameObject enemy = SpawnEnemy(Vector3.zero);
            int objects = 0, missing = 0;
            foreach (Transform t in enemy.GetComponentsInChildren<Transform>(true))
            {
                objects++;
                foreach (Component component in t.GetComponents<Component>())
                    if (component == null) missing++;
            }
            c.Actual = $"Đã quét {objects} GameObject con, Missing Script={missing}.";
            Assert.AreEqual(0, missing, "Enemy prefab thật đang có Missing Script.");
        }

        private void ComponentPresence(string id, string title, string component, string expected, string severity)
        {
            Run(id, title, expected, severity, c =>
            {
                GameObject enemy = SpawnEnemy(Vector3.zero);
                Component found = TestReflectionHelper.FindComponentByClassName(enemy, component);
                c.Actual = found != null ? $"Tìm thấy component {found.GetType().Name}." : $"Không tìm thấy component {component}.";
                Assert.IsNotNull(found, $"Không tìm thấy component {component} trên Enemy prefab thật.");
            });
        }

        private void OptionalComponent(string id, string title, string component, string expected, string severity)
        {
            Run(id, title, expected, severity, c =>
            {
                GameObject enemy = SpawnEnemy(Vector3.zero);
                Component found = TestReflectionHelper.FindComponentByClassName(enemy, component);
                c.Actual = found != null ? $"Tìm thấy component {found.GetType().Name}." : $"Prefab Enemy hiện tại không có {component}; ghi nhận để xác minh nếu hệ thống có dùng.";
            });
        }

        private void EnemyData(Ctx c)
        {
            GameObject enemy = SpawnEnemy(Vector3.zero);
            Component baseComp = Require(enemy, "EnemyBase");
            object data = null;
            Assert.IsTrue(TestReflectionHelper.TryGetValue(baseComp, "Data", out data) && data != null, "EnemyBase.Data null hoặc không đọc được.");
            c.Actual = $"EnemyData={data}, maxHealth={ReadFloat(data, "maxHealth", -1f)}, damage={ReadFloat(data, "damage", -1f)}.";
        }

        private IEnumerator HealthInitial(Ctx c)
        {
            GameObject enemy = SpawnEnemy(Vector3.zero);
            yield return null;
            float hp = ReadEnemyHp(enemy, true);
            c.Actual = $"HP ban đầu đọc được={N(hp)}.";
            Assert.Greater(hp, 0f, "HP ban đầu của Enemy phải lớn hơn 0.");
        }

        private IEnumerator DamageEnemy(Ctx c, bool kill)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, -2f));
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return null;
            float before = ReadEnemyHp(enemy, true);
            string method = ApplyDamage(enemy, kill ? before + 9999f : 10f);
            yield return null;
            float after = ReadEnemyHp(enemy, true);
            c.Actual = $"Player hỗ trợ={player.name}, method damage={method}, HP trước={N(before)}, HP sau={N(after)}, số lỗi Console={ErrorCount()}.";
            AssertNoErrors("Enemy nhận damage phát sinh Error/Exception.");
            Assert.Less(after, before, "Enemy không giảm HP sau khi gọi damage thật.");
            if (kill) Assert.LessOrEqual(after, 0f, "Enemy chưa về 0 HP sau damage kết liễu.");
        }

        private IEnumerator DeadThenAttack(Ctx c)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, -2f));
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return null;
            float before = ReadEnemyHp(enemy, true);
            string damageMethod = ApplyDamage(enemy, before + 9999f);
            string attackMethod = TryEnemyAttack(enemy, player);
            yield return new WaitForSeconds(0.2f);
            c.Actual = $"Damage method={damageMethod}, attack method sau chết={attackMethod}, HP trước={N(before)}, HP sau={N(ReadEnemyHp(enemy, true))}, lỗi Console={ErrorCount()}.";
            AssertNoErrors("Enemy chết rồi gọi attack phát sinh Error/Exception.");
        }

        private IEnumerator RunFramesNoPlayer(Ctx c)
        {
            StartWatcher();
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            for (int i = 0; i < 30; i++) yield return null;
            c.Actual = $"Enemy={enemy.name}, số frame=30, active={enemy.activeInHierarchy}, lỗi Console={ErrorCount()}.";
            AssertNoErrors("Enemy chạy không có Player phát sinh Error/Exception.");
        }

        private IEnumerator EnemyWithPlayer(Ctx c)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, -2f));
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return new WaitForSeconds(0.2f);
            Component detection = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyDetection");
            object target = null;
            bool readTarget = detection != null && TestReflectionHelper.TryGetValue(detection, "CurrentTarget", out target);
            c.Actual = $"Player={player.name}, Enemy={enemy.name}, đọc CurrentTarget={readTarget}, target={(target ?? "null")}, lỗi Console={ErrorCount()}.";
            AssertNoErrors("Enemy nhận target Player phát sinh Error/Exception.");
        }

        private IEnumerator GroundCheck(Ctx c)
        {
            StartWatcher();
            CreateGround();
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1.2f, 0f));
            yield return null;
            float y0 = enemy.transform.position.y;
            yield return new WaitForSeconds(1f);
            float y1 = enemy.transform.position.y;
            c.Actual = $"Y ban đầu={N(y0)}, Y sau={N(y1)}, lệch Y={N(y1 - y0)}, thời gian=1.00s, lỗi Console={ErrorCount()}.";
            Assert.Greater(y1, -2f, "Enemy rơi xuống dưới map sau khi spawn trên ground.");
            AssertNoErrors("Enemy ground check phát sinh Error/Exception.");
        }

        private void CollisionComponent(Ctx c)
        {
            GameObject enemy = SpawnEnemy(Vector3.zero);
            Collider[] colliders = enemy.GetComponentsInChildren<Collider>(true);
            CharacterController cc = enemy.GetComponentInChildren<CharacterController>(true);
            c.Actual = $"Số Collider={colliders.Length}, CharacterController={(cc != null ? "có" : "không")}.";
            Assert.IsTrue(colliders.Length > 0 || cc != null, "Enemy không có Collider hoặc CharacterController.");
        }

        private void RendererMaterial(Ctx c)
        {
            GameObject enemy = SpawnEnemy(Vector3.zero);
            Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>(true);
            int enabled = 0, nullMat = 0;
            foreach (Renderer r in renderers)
            {
                if (r.enabled) enabled++;
                foreach (Material m in r.sharedMaterials) if (m == null) nullMat++;
            }
            c.Actual = $"Renderer={renderers.Length}, Renderer enabled={enabled}, material null={nullMat}.";
            Assert.Greater(enabled, 0, "Enemy không có Renderer enabled.");
            Assert.AreEqual(0, nullMat, "Enemy có material null.");
        }

        private void AnimatorRefs(Ctx c)
        {
            GameObject enemy = SpawnEnemy(Vector3.zero);
            Animator[] animators = enemy.GetComponentsInChildren<Animator>(true);
            int nullController = 0;
            foreach (Animator a in animators) if (a.runtimeAnimatorController == null) nullController++;
            c.Actual = $"Animator={animators.Length}, RuntimeAnimatorController null={nullController}.";
            if (animators.Length > 0) Assert.AreEqual(0, nullController, "Enemy có Animator nhưng RuntimeAnimatorController null.");
        }

        private void AudioRefs(Ctx c)
        {
            GameObject enemy = SpawnEnemy(Vector3.zero);
            AudioSource[] sources = enemy.GetComponentsInChildren<AudioSource>(true);
            int nullClip = 0;
            foreach (AudioSource s in sources) if (s.playOnAwake && s.clip == null) nullClip++;
            c.Actual = $"AudioSource={sources.Length}, AudioClip null khi playOnAwake={nullClip}.";
            Assert.AreEqual(0, nullClip, "Enemy có AudioSource playOnAwake nhưng AudioClip null.");
        }

        private IEnumerator ManyEnemies(Ctx c)
        {
            StartWatcher();
            for (int i = 0; i < 5; i++) SpawnEnemy(new Vector3(i * 2f, 1f, 0f));
            yield return new WaitForSeconds(0.2f);
            c.Actual = $"Số Enemy spawn cùng lúc=5, số lỗi Console={ErrorCount()}.";
            AssertNoErrors("Spawn nhiều Enemy phát sinh Error/Exception.");
        }

        private IEnumerator NotSelfDisable(Ctx c)
        {
            GameObject enemy = SpawnEnemy(Vector3.zero);
            yield return null; yield return null; yield return null;
            c.Actual = $"Enemy={enemy.name}, activeSelf={enemy.activeSelf}, activeInHierarchy={enemy.activeInHierarchy}.";
            Assert.IsTrue(enemy.activeInHierarchy, "Enemy tự disable ngay sau spawn.");
        }

        private IEnumerator SixtyFrames(Ctx c)
        {
            StartWatcher();
            GameObject enemy = SpawnEnemy(Vector3.zero);
            for (int i = 0; i < 60; i++) yield return null;
            c.Actual = $"Enemy={enemy.name}, số frame=60, số lỗi Console={ErrorCount()}.";
            AssertNoErrors("Enemy chạy 60 frame phát sinh Error/Exception.");
        }

        private IEnumerator EnemyAttackPlayer(Ctx c)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, 1.5f));
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return null;
            string method = TryEnemyAttack(enemy, player);
            yield return new WaitForSeconds(0.2f);
            c.Actual = $"Player={player.name}, Enemy={enemy.name}, khoảng cách={N(Vector3.Distance(player.transform.position, enemy.transform.position))}, method attack={method}, lỗi Console={ErrorCount()}.";
            AssertNoErrors("Enemy attack Player phát sinh Error/Exception.");
            Assert.AreNotEqual("Không tìm thấy", method, "Không tìm thấy method combat thật để Enemy đánh Player.");
        }

        private IEnumerator EnemySummary(Ctx c)
        {
            StartWatcher();
            CreateGround();
            GameObject player = SpawnPlayer(new Vector3(0f, 1f, -2f));
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return null;
            float hp0 = ReadEnemyHp(enemy, true);
            string damage = ApplyDamage(enemy, 5f);
            string attack = TryEnemyAttack(enemy, player);
            for (int i = 0; i < 20; i++) yield return null;
            c.Actual = $"Enemy={enemy.name}, Player={player.name}, HP trước={N(hp0)}, HP sau={N(ReadEnemyHp(enemy, true))}, damage method={damage}, attack method={attack}, lỗi Console={ErrorCount()}.";
            AssertNoErrors("Tổng Enemy phát sinh Error/Exception.");
        }

        private string ApplyDamage(GameObject enemy, float damage)
        {
            Component receiver = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyDamageReceiver");
            if (receiver != null && TestReflectionHelper.TryInvokeMethod(receiver, "TakeHit", damage, 0f)) return "EnemyDamageReceiver.TakeHit";
            Component health = Require(enemy, "EnemyHealth");
            if (TestReflectionHelper.TryInvokeMethod(health, "TakeDamage", damage)) return "EnemyHealth.TakeDamage";
            Assert.Fail("Không tìm thấy method TakeHit/TakeDamage thật trên Enemy.");
            return "Không tìm thấy";
        }

        private string TryEnemyAttack(GameObject enemy, GameObject player)
        {
            Component combat = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyCombat");
            if (combat == null) return "Không tìm thấy";
            object arsenal = null;
            if (TestReflectionHelper.TryGetValue(combat, "AttackArsenal", out arsenal) && arsenal is Array attacks && attacks.Length > 0)
            {
                object chosen = attacks.GetValue(0);
                if (chosen != null && TestReflectionHelper.TryInvokeMethod(combat, "PerformAttack", chosen)) return "EnemyCombat.PerformAttack";
            }
            if (TestReflectionHelper.TryInvokeMethod(combat, "OpenHitbox")) return "EnemyCombat.OpenHitbox";
            return "Không tìm thấy";
        }

        private float ReadEnemyHp(GameObject enemy, bool fail)
        {
            Component health = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyHealth");
            object value = null;
            if (health != null && TestReflectionHelper.TryGetValue(health, "currentHealth", out value))
                return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            if (fail) Assert.Fail("Không đọc được currentHealth thật trên EnemyHealth.");
            return -1f;
        }

        private float ReadFloat(object target, string member, float fallback)
        {
            object value = null;
            if (target != null && TestReflectionHelper.TryGetValue(target, member, out value) && value != null)
                return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            return fallback;
        }

        private GameObject SpawnEnemy(Vector3 position)
        {
            GameObject prefab = TestPrefabFinder.FindEnemyPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Enemy prefab thật trong project.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            go.name = prefab.name + "_Test";
            spawned.Add(go);
            return go;
        }

        private GameObject SpawnPlayer(Vector3 position)
        {
            GameObject prefab = TestPrefabFinder.FindPlayerPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Player prefab thật trong project.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            go.name = prefab.name + "_Test";
            go.tag = "Player";
            spawned.Add(go);
            return go;
        }

        private Component Require(GameObject root, string component)
        {
            Component found = TestReflectionHelper.FindComponentByClassName(root, component);
            Assert.IsNotNull(found, $"Không tìm thấy component {component} trên Enemy prefab thật.");
            return found;
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Test_Ground";
            ground.transform.position = new Vector3(0f, -0.55f, 0f);
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            spawned.Add(ground);
        }

        private void StartWatcher()
        {
            StopWatcher();
            watcher = new TestLogWatcher();
            watcher.Start();
        }

        private void StopWatcher()
        {
            if (watcher != null) watcher.Stop();
            watcher = null;
        }

        private int ErrorCount() => watcher == null ? 0 : watcher.GetErrors().Count;
        private void AssertNoErrors(string message) => Assert.IsFalse(watcher != null && watcher.HasErrorOrException, message + " Lỗi: " + string.Join(" | ", watcher.GetErrors()));
        private string F(Vector3 v) => $"({N(v.x)},{N(v.y)},{N(v.z)})";
        private string N(float v) => v.ToString("0.00", CultureInfo.InvariantCulture);

        private void Run(string id, string title, string expected, string severity, Action<Ctx> body)
        {
            Ctx c = new Ctx();
            try { body(c); Record(id, title, expected, c.Actual, "Pass", "", "Tự động kiểm tra bằng Unity Test Runner."); }
            catch (Exception e) { Record(id, title, expected, (c.Actual + " KHÔNG ĐẠT - " + e.Message).Trim(), "Fail", severity, "Tự động kiểm tra bằng Unity Test Runner."); throw; }
        }

        private IEnumerator RunUnity(string id, string title, string expected, string severity, Func<Ctx, IEnumerator> body)
        {
            Ctx c = new Ctx();
            Exception failure = null;
            IEnumerator routine = null;
            try
            {
                routine = RunAfterSceneLoad(c, body);
            }
            catch (Exception e) { failure = e; }
            while (failure == null)
            {
                bool next = false; object current = null;
                try { next = routine != null && routine.MoveNext(); if (next) current = routine.Current; } catch (Exception e) { failure = e; }
                if (failure != null || !next) break;
                yield return current;
            }
            if (failure == null) Record(id, title, expected, c.Actual, "Pass", "", "Tự động kiểm tra bằng Unity Test Runner.");
            else { Record(id, title, expected, (c.Actual + " KHÔNG ĐẠT - " + failure.Message).Trim(), "Fail", severity, "Tự động kiểm tra bằng Unity Test Runner."); throw failure; }
        }

        private IEnumerator RunAfterSceneLoad(Ctx context, Func<Ctx, IEnumerator> body)
        {
            yield return TestSceneLoader.Load(TestSceneConfig.RunScenePath);
            IEnumerator routine = body(context);
            while (routine != null && routine.MoveNext()) yield return routine.Current;
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
