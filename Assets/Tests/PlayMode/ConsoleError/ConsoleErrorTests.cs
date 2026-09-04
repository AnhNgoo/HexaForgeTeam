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
        private const string TesterName = "Huynh Ngoc Thanh Phuoc";
        private const string StartDate = "31/05/2026";
        private const string RunMode = "Tu dong";
        private static readonly List<TestResultRecord> records = new List<TestResultRecord>();

        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (spawned[i] != null) UnityEngine.Object.DestroyImmediate(spawned[i]);
            }
            spawned.Clear();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestResultCsvExporter.Export("ConsoleError", records);
        }

        [UnityTest, Category("ConsoleError"), Category("Tu dong"), Description("CE-001: Load scene Run Scene.")]
        public IEnumerator CE_001_LoadGameplaySceneKhongLoiDo()
        {
            yield return RunUnity("CE-001", "Load scene Run Scene", "Scene Run Scene cua Trung load duoc trong PlayMode.", "High", LoadSceneNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tu dong"), Description("CE-002: Spawn Player prefab that.")]
        public IEnumerator CE_002_SpawnPlayerKhongLoiDo()
        {
            yield return RunUnity("CE-002", "Spawn Player", "Player prefab that spawn duoc.", "High", SpawnPlayerNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tu dong"), Description("CE-003: Spawn Enemy prefab that.")]
        public IEnumerator CE_003_SpawnEnemyKhongLoiDo()
        {
            yield return RunUnity("CE-003", "Spawn Enemy", "Enemy prefab that spawn duoc.", "High", SpawnEnemyNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tu dong"), Description("CE-004: Player thao tac co ban.")]
        public IEnumerator CE_004_PlayerThaoTacCoBanKhongLoiDo()
        {
            yield return RunUnity("CE-004", "Player thao tac co ban", "Goi thu movement/jump/dodge/attack/skill neu co.", "Medium", PlayerBasicNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tu dong"), Description("CE-005: Enemy thao tac co ban.")]
        public IEnumerator CE_005_EnemyThaoTacCoBanKhongLoiDo()
        {
            yield return RunUnity("CE-005", "Enemy thao tac co ban", "Goi thu damage/attack Enemy neu co.", "Medium", EnemyBasicNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tu dong"), Description("CE-006: Combat co ban.")]
        public IEnumerator CE_006_CombatCoBanKhongLoiDo()
        {
            yield return RunUnity("CE-006", "Combat co ban", "Spawn Player/Enemy va goi combat best-effort.", "Medium", CombatBasicNoError);
        }

        [UnityTest, Category("ConsoleError"), Category("Tu dong"), Description("CE-007: Chuoi thao tac Player/Enemy.")]
        public IEnumerator CE_007_ChuoiThaoTacPlayerEnemyKhongLoiDo()
        {
            yield return RunUnity("CE-007", "Chuoi thao tac Player/Enemy", "Chay chuoi thao tac co ban best-effort.", "Medium", PlayerEnemySequenceNoError);
        }

        private IEnumerator LoadSceneNoError(Ctx c)
        {
            string scenePath = FindGameplayScenePath();
            Assert.IsFalse(string.IsNullOrEmpty(scenePath), "Khong tim thay scene Run Scene cua Trung trong project.");
#if UNITY_EDITOR
            AsyncOperation op = EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            while (op != null && !op.isDone) yield return null;
#else
            yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
#endif
            yield return null;
            c.Actual = $"Scene da load={scenePath}, activeScene={SceneManager.GetActiveScene().name}.";
            Assert.IsTrue(SceneManager.GetActiveScene().IsValid(), "Scene Run Scene load ra khong hop le.");
        }

        private IEnumerator SpawnPlayerNoError(Ctx c)
        {
            GameObject player = SpawnPlayer(Vector3.zero);
            yield return null; yield return null; yield return null;
            c.Actual = $"Player prefab={player.name}, vi tri={F(player.transform.position)}.";
        }

        private IEnumerator SpawnEnemyNoError(Ctx c)
        {
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return null; yield return null; yield return null;
            c.Actual = $"Enemy prefab={enemy.name}, vi tri={F(enemy.transform.position)}.";
        }

        private IEnumerator PlayerBasicNoError(Ctx c)
        {
            CreateCameraAndGround();
            GameObject player = SpawnPlayer(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            List<string> actions = new List<string>
            {
                InvokeMovement(player, new Vector2(0f, 1f)),
                InvokeJump(player),
                InvokeDodge(player),
                InvokePlayerAttack(player),
                InvokeSkill1(player)
            };
            yield return new WaitForSeconds(0.5f);
            c.Actual = $"Player prefab={player.name}, chuoi thao tac={string.Join(", ", actions.ToArray())}.";
        }

        private IEnumerator EnemyBasicNoError(Ctx c)
        {
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 0f));
            yield return new WaitForSeconds(0.2f);
            string damage = ApplyEnemyDamage(enemy, 1f);
            string attack = InvokeEnemyAttack(enemy);
            for (int i = 0; i < 20; i++) yield return null;
            c.Actual = $"Enemy prefab={enemy.name}, damage method={damage}, attack method={attack}, frame cho=20.";
        }

        private IEnumerator CombatBasicNoError(Ctx c)
        {
            CreateCameraAndGround();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 1.5f));
            yield return new WaitForSeconds(0.2f);
            string pAttack = InvokePlayerAttack(player);
            string eAttack = InvokeEnemyAttack(enemy);
            string damage = ApplyEnemyDamage(enemy, 3f);
            yield return new WaitForSeconds(0.5f);
            c.Actual = $"Player={player.name}, Enemy={enemy.name}, khoang cach={Vector3.Distance(player.transform.position, enemy.transform.position):0.00}, PlayerAttack={pAttack}, EnemyAttack={eAttack}, damage={damage}.";
        }

        private IEnumerator PlayerEnemySequenceNoError(Ctx c)
        {
            CreateCameraAndGround();
            GameObject player = SpawnPlayer(Vector3.zero);
            GameObject enemy = SpawnEnemy(new Vector3(0f, 1f, 2f));
            yield return new WaitForSeconds(0.2f);
            List<string> actions = new List<string>
            {
                InvokeMovement(player, new Vector2(0f, 1f)),
                InvokePlayerAttack(player),
                InvokeSkill1(player),
                ApplyEnemyDamage(enemy, 2f),
                InvokeEnemyAttack(enemy),
                InvokeDodge(player)
            };
            for (int i = 0; i < 30; i++) yield return null;
            c.Actual = $"Player={player.name}, Enemy={enemy.name}, chuoi thao tac={string.Join(", ", actions.ToArray())}, frame cho=30.";
        }

        private string FindGameplayScenePath()
        {
#if UNITY_EDITOR
            string configuredPath = TestSceneConfig.GameplayScenePath;
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(configuredPath), "Khong tim thay scene Run Game: " + configuredPath);
            return configuredPath;
#else
            return TestSceneConfig.GameplayScenePath;
#endif
        }

        private GameObject SpawnPlayer(Vector3 pos)
        {
            GameObject prefab = TestPrefabFinder.FindPlayerPrefab();
            Assert.IsNotNull(prefab, "Khong tim thay Player prefab that trong project.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);
            go.name = prefab.name + "_ConsoleTest";
            go.tag = "Player";
            spawned.Add(go);
            return go;
        }

        private GameObject SpawnEnemy(Vector3 pos)
        {
            GameObject prefab = TestPrefabFinder.FindEnemyPrefab();
            Assert.IsNotNull(prefab, "Khong tim thay Enemy prefab that trong project.");
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
            if (movement == null) return "Khong co CharacterMovement";
            return TestReflectionHelper.TryInvokeMethod(movement, "Run", dir, 5f) ||
                   TestReflectionHelper.TryInvokeMethod(movement, "Walk", dir, 5f)
                ? "CharacterMovement.Run/Walk"
                : "Khong goi duoc movement";
        }

        private string InvokeJump(GameObject player)
        {
            Component movement = TestReflectionHelper.FindComponentByClassName(player, "CharacterMovement");
            if (movement == null) return "Khong co CharacterMovement";
            TestReflectionHelper.TrySetValue(movement, "IsGrounded", true);
            return TestReflectionHelper.TryInvokeMethod(movement, "Jump") ? "CharacterMovement.Jump" : "Khong goi duoc Jump";
        }

        private string InvokeDodge(GameObject player)
        {
            Component movement = TestReflectionHelper.FindComponentByClassName(player, "CharacterMovement");
            if (movement == null) return "Khong co CharacterMovement";
            return TestReflectionHelper.TryInvokeMethod(movement, "Dodge", new Vector2(0f, 1f), 5f) ? "CharacterMovement.Dodge" : "Khong goi duoc Dodge";
        }

        private string InvokePlayerAttack(GameObject player)
        {
            Component combat = TestReflectionHelper.FindComponentByClassName(player, "CharacterCombat");
            return combat != null && TestReflectionHelper.TryInvokeMethod(combat, "TryAttack") ? "CharacterCombat.TryAttack" : "Khong goi duoc Player Attack";
        }

        private string InvokeSkill1(GameObject player)
        {
            Component skill = TestReflectionHelper.FindComponentByClassName(player, "CharacterSkill");
            return skill != null && TestReflectionHelper.TryInvokeMethod(skill, "UseSkill1") ? "CharacterSkill.UseSkill1" : "Khong goi duoc Skill1";
        }

        private string InvokeEnemyAttack(GameObject enemy)
        {
            Component combat = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyCombat");
            if (combat == null) return "Khong co EnemyCombat";
            object arsenal;
            if (TestReflectionHelper.TryGetValue(combat, "AttackArsenal", out arsenal) && arsenal is Array attacks && attacks.Length > 0)
            {
                object attack = attacks.GetValue(0);
                if (attack != null && TestReflectionHelper.TryInvokeMethod(combat, "PerformAttack", attack)) return "EnemyCombat.PerformAttack";
            }
            return TestReflectionHelper.TryInvokeMethod(combat, "OpenHitbox") ? "EnemyCombat.OpenHitbox" : "Khong goi duoc Enemy attack";
        }

        private string ApplyEnemyDamage(GameObject enemy, float damage)
        {
            Component receiver = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyDamageReceiver");
            if (receiver != null && TestReflectionHelper.TryInvokeMethod(receiver, "TakeHit", damage, 0f)) return "EnemyDamageReceiver.TakeHit";
            Component health = TestReflectionHelper.FindComponentByClassName(enemy, "EnemyHealth");
            return health != null && TestReflectionHelper.TryInvokeMethod(health, "TakeDamage", damage) ? "EnemyHealth.TakeDamage" : "Khong goi duoc Enemy damage";
        }

        private string F(Vector3 v) => $"({v.x:0.00},{v.y:0.00},{v.z:0.00})";

        private IEnumerator RunUnity(string id, string title, string expected, string severity, Func<Ctx, IEnumerator> body)
        {
            Ctx c = new Ctx();
            Exception failure = null;
            IEnumerator routine = null;
            try { routine = body(c); } catch (Exception exception) { failure = exception; }
            while (failure == null)
            {
                bool next = false;
                object current = null;
                try { next = routine != null && routine.MoveNext(); if (next) current = routine.Current; }
                catch (Exception exception) { failure = exception; }
                if (failure != null || !next) break;
                yield return current;
            }

            if (failure == null) Record(id, title, expected, c.Actual, "Pass", "", "Tu dong kiem tra Tutorial/Console bang Unity Test Runner.");
            else { Record(id, title, expected, (c.Actual + " Không đạt - " + failure.Message).Trim(), "Fail", severity, "Tự động kiểm tra Tutorial/Console bằng Unity Test Runner."); throw failure; }
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
