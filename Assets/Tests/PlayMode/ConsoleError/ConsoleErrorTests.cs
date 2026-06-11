using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace DuskBlade.Tests
{
    public class ConsoleErrorTests
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
            TestResultCsvExporter.Export("ConsoleError", records);
        }

        [UnityTest, Category("ConsoleError"), Category("Tự động")]
        [Description("Load gameplay scene thật không lỗi đỏ nếu tìm được scene.")]
        public IEnumerator CE_001_LoadGameplaySceneKhongLoiDo()
        {
            yield return RunUnity("CE-001", "Load gameplay scene không lỗi đỏ nếu tìm được", "Scene gameplay thật load được và không phát sinh Error/Exception.", "High", LoadSceneNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tự động")]
        [Description("Spawn Player prefab thật không lỗi đỏ.")]
        public IEnumerator CE_002_SpawnPlayerKhongLoiDo()
        {
            yield return RunUnity("CE-002", "Spawn Player không lỗi đỏ", "Player prefab thật spawn không phát sinh Error/Exception.", "High", SpawnPlayerNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tự động")]
        [Description("Spawn Enemy prefab thật không lỗi đỏ.")]
        public IEnumerator CE_003_SpawnEnemyKhongLoiDo()
        {
            yield return RunUnity("CE-003", "Spawn Enemy không lỗi đỏ", "Enemy prefab thật spawn không phát sinh Error/Exception.", "High", SpawnEnemyNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tự động")]
        [Description("Player thao tác cơ bản không lỗi đỏ.")]
        public IEnumerator CE_004_PlayerThaoTacCoBanKhongLoiDo()
        {
            yield return RunUnity("CE-004", "Player thao tác cơ bản không lỗi đỏ", "Movement, Jump, Dodge, Attack, Skill 1 nếu có không phát sinh Error/Exception.", "High", PlayerBasicNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tự động")]
        [Description("Enemy thao tác cơ bản không lỗi đỏ.")]
        public IEnumerator CE_005_EnemyThaoTacCoBanKhongLoiDo()
        {
            yield return RunUnity("CE-005", "Enemy thao tác cơ bản không lỗi đỏ", "Enemy chạy frame, nhận damage nhỏ và gọi attack nếu có không phát sinh Error/Exception.", "High", EnemyBasicNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tự động")]
        [Description("Combat cơ bản không lỗi đỏ.")]
        public IEnumerator CE_006_CombatCoBanKhongLoiDo()
        {
            yield return RunUnity("CE-006", "Combat cơ bản không lỗi đỏ", "Spawn Player/Enemy và gọi combat cơ bản không phát sinh Error/Exception.", "High", CombatBasicNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tự động")]
        [Description("Chuỗi thao tác Player/Enemy cơ bản không lỗi đỏ.")]
        public IEnumerator CE_007_ChuoiThaoTacPlayerEnemyKhongLoiDo()
        {
            yield return RunUnity("CE-007", "Chuỗi thao tác Player/Enemy cơ bản không lỗi đỏ", "Chuỗi spawn, movement, attack, skill, enemy damage, enemy attack không phát sinh Error/Exception.", "High", PlayerEnemySequenceNoError);
        }

        private IEnumerator LoadSceneNoError(Ctx c)
        {
            StartWatcher();
            string scenePath = FindGameplayScenePath();
            Assert.IsFalse(string.IsNullOrEmpty(scenePath), "Không tìm thấy scene gameplay thật trong Assets/Scenes hoặc Build Settings để load.");
#if UNITY_EDITOR
            AsyncOperation op = EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (op != null && !op.isDone) yield return null;
#else
            yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
#endif
            yield return null;
            c.Actual = $"Scene đã load={scenePath}, activeScene={SceneManager.GetActiveScene().name}, số Error/Exception={Errors()}.";
            AssertNoErrors("Load gameplay scene phát sinh Error/Exception.");
        }

        private IEnumerator SpawnPlayerNoError(Ctx c)
        {
            StartWatcher();
            GameObject player = SpawnPlayer(Vector3.zero);
            yield return null; yield return null; yield return null;
            c.Actual = $"Player prefab={player.name}, vị trí={F(player.transform.position)}, số Error/Exception={Errors()}.";
            AssertNoErrors("Spawn Player phát sinh Error/Exception.");
        }

        private IEnumerator SpawnEnemyNoError(Ctx c)
        {
            StartWatcher();
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return null; yield return null; yield return null;
            c.Actual = $"Enemy prefab={enemy.name}, vị trí={F(enemy.transform.position)}, số Error/Exception={Errors()}.";
            AssertNoErrors("Spawn Enemy phát sinh Error/Exception.");
        }

        private IEnumerator PlayerBasicNoError(Ctx c)
        {
            StartWatcher();
            CreateCameraAndGround();
            GameObject player = SpawnPlayer(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            List<string> actions = new List<string>();
            actions.Add(InvokeMovement(player, new Vector2(0f, 1f)));
            actions.Add(InvokeJump(player));
            actions.Add(InvokeDodge(player));
            actions.Add(InvokePlayerAttack(player));
            actions.Add(InvokeSkill1(player));
            yield return new WaitForSeconds(0.5f);
            c.Actual = $"Player prefab={player.name}, chuỗi thao tác={string.Join(", ", actions.ToArray())}, số Error/Exception={Errors()}.";
            AssertNoErrors("Player thao tác cơ bản phát sinh Error/Exception.");
        }

        private IEnumerator EnemyBasicNoError(Ctx c)
        {
            StartWatcher();
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return new WaitForSeconds(0.2f);
            string damage = ApplyEnemyDamage(enemy, 1f);
            string attack = InvokeEnemyAttack(enemy);
            for (int i = 0; i < 20; i++) yield return null;
            c.Actual = $"Enemy prefab={enemy.name}, damage method={damage}, attack method={attack}, frame chờ=20, số Error/Exception={Errors()}.";
            AssertNoErrors("Enemy thao tác cơ bản phát sinh Error/Exception.");
        }

        private IEnumerator CombatBasicNoError(Ctx c)
        {
            StartWatcher();
            CreateCameraAndGround();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 1.5f));
            yield return new WaitForSeconds(0.2f);
            string pAttack = InvokePlayerAttack(player);
            string eAttack = InvokeEnemyAttack(enemy);
            string damage = ApplyEnemyDamage(enemy, 3f);
            yield return new WaitForSeconds(0.5f);
            c.Actual = $"Player={player.name}, Enemy={enemy.name}, khoảng cách={Vector3.Distance(player.transform.position, enemy.transform.position):0.00}, PlayerAttack={pAttack}, EnemyAttack={eAttack}, damage={damage}, số Error/Exception={Errors()}.";
            AssertNoErrors("Combat cơ bản phát sinh Error/Exception.");
        }

        private IEnumerator PlayerEnemySequenceNoError(Ctx c)
        {
            StartWatcher();
            CreateCameraAndGround();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 2f));
            yield return new WaitForSeconds(0.2f);
            List<string> actions = new List<string>();
            actions.Add(InvokeMovement(player, new Vector2(0f, 1f)));
            actions.Add(InvokePlayerAttack(player));
            actions.Add(InvokeSkill1(player));
            actions.Add(ApplyEnemyDamage(enemy, 2f));
            actions.Add(InvokeEnemyAttack(enemy));
            actions.Add(InvokeDodge(player));
            for (int i = 0; i < 30; i++) yield return null;
            c.Actual = $"Player={player.name}, Enemy={enemy.name}, chuỗi thao tác={string.Join(", ", actions.ToArray())}, frame chờ=30, số Error/Exception={Errors()}.";
            AssertNoErrors("Chuỗi thao tác Player/Enemy phát sinh Error/Exception.");
        }

        private string FindGameplayScenePath()
        {
#if UNITY_EDITOR
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled && !string.IsNullOrEmpty(scene.path)) return scene.path;
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            if (guids.Length == 0) guids = AssetDatabase.FindAssets("t:Scene");
            return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : null;
#else
            return null;
#endif
        }

        private GameObject SpawnPlayer(Vector3 pos)
        {
            GameObject prefab = TestPrefabFinder.FindPlayerPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Player prefab thật trong project.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);
            go.name = prefab.name + "_ConsoleTest";
            go.tag = "Player";
            spawned.Add(go);
            return go;
        }

        private GameObject SpawnEnemy(Vector3 pos)
        {
            GameObject prefab = TestPrefabFinder.FindEnemyPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Enemy prefab thật trong project.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);
            go.name = prefab.name + "_ConsoleTest";
            spawned.Add(go);
            return go;
        }

        private void CreateCameraAndGround()
        {
            if (Camera.main == null)
            {
                GameObject cam = new GameObject("Test_MainCamera");
                cam.tag = "MainCamera";
                cam.AddComponent<Camera>();
                cam.transform.position = new Vector3(0f, 6f, -8f);
                cam.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
                spawned.Add(cam);
            }
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Test_Ground";
            ground.transform.position = new Vector3(0f, -0.55f, 0f);
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            spawned.Add(ground);
        }

        private string InvokeMovement(GameObject player, Vector2 dir)
        {
            Component movement = TestReflectionHelper.FindComponentByClassName(player, "CharacterMovement");
            Assert.IsNotNull(movement, "Không tìm thấy CharacterMovement trên Player prefab thật.");
            Assert.IsTrue(TestReflectionHelper.TryInvokeMethod(movement, "Run", dir, 5f) || TestReflectionHelper.TryInvokeMethod(movement, "Walk", dir, 5f), "Không gọi được method movement thật.");
            return "CharacterMovement.Run/Walk";
        }

        private string InvokeJump(GameObject player)
        {
            Component movement = TestReflectionHelper.FindComponentByClassName(player, "CharacterMovement");
            Assert.IsNotNull(movement, "Không tìm thấy CharacterMovement trên Player prefab thật.");
            TestReflectionHelper.TrySetValue(movement, "IsGrounded", true);
            Assert.IsTrue(TestReflectionHelper.TryInvokeMethod(movement, "Jump"), "Không gọi được CharacterMovement.Jump.");
            return "CharacterMovement.Jump";
        }

        private string InvokeDodge(GameObject player)
        {
            Component movement = TestReflectionHelper.FindComponentByClassName(player, "CharacterMovement");
            Assert.IsNotNull(movement, "Không tìm thấy CharacterMovement trên Player prefab thật.");
            Assert.IsTrue(TestReflectionHelper.TryInvokeMethod(movement, "Dodge", new Vector2(0f, 1f), 5f), "Không gọi được CharacterMovement.Dodge.");
            return "CharacterMovement.Dodge";
        }

        private string InvokePlayerAttack(GameObject player)
        {
            Component combat = TestReflectionHelper.FindComponentByClassName(player, "CharacterCombat");
            if (combat != null && TestReflectionHelper.TryInvokeMethod(combat, "TryAttack")) return "CharacterCombat.TryAttack";
            Assert.Fail("Không tìm thấy method Attack thật trên Player.");
            return "Không tìm thấy";
        }

        private string InvokeSkill1(GameObject player)
        {
            Component skill = TestReflectionHelper.FindComponentByClassName(player, "CharacterSkill");
            if (skill != null && TestReflectionHelper.TryInvokeMethod(skill, "UseSkill1")) return "CharacterSkill.UseSkill1";
            Assert.Fail("Không tìm thấy method Skill 1 thật trên Player.");
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
            Assert.Fail("Không gọi được Enemy attack thật.");
            return "Không tìm thấy";
        }

        private string ApplyEnemyDamage(GameObject enemy, float damage)
        {
            Component receiver = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyDamageReceiver");
            if (receiver != null && TestReflectionHelper.TryInvokeMethod(receiver, "TakeHit", damage, 0f)) return "EnemyDamageReceiver.TakeHit";
            Component health = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyHealth");
            if (health != null && TestReflectionHelper.TryInvokeMethod(health, "TakeDamage", damage)) return "EnemyHealth.TakeDamage";
            Assert.Fail("Không gọi được damage thật trên Enemy.");
            return "Không tìm thấy";
        }

        private void StartWatcher()
        {
            if (watcher != null) watcher.Stop();
            watcher = new TestLogWatcher();
            watcher.Start();
        }

        private int Errors() => watcher == null ? 0 : watcher.GetErrors().Count;
        private void AssertNoErrors(string msg) => Assert.IsFalse(watcher != null && watcher.HasErrorOrException, msg + " Lỗi: " + string.Join(" | ", watcher.GetErrors()));
        private string F(Vector3 v) => $"({v.x:0.00},{v.y:0.00},{v.z:0.00})";

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
            if (failure == null) Record(id, title, expected, c.Actual, "Pass", "", "Tự động kiểm tra lỗi Console bằng Unity Test Runner.");
            else { Record(id, title, expected, (c.Actual + " KHÔNG ĐẠT - " + failure.Message).Trim(), "Fail", severity, "Tự động kiểm tra lỗi Console bằng Unity Test Runner."); throw failure; }
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
