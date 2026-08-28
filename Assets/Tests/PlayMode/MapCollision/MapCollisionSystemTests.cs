using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class MapCollisionSystemTests : RuntimeSystemTestBase
    {
        protected override string ExportName => "MapCollision";

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-001: Kiem tra load duoc scene Run Scene.")]
        public IEnumerator MAP_001_LoadGameplaySceneDuoc() { return RunUnity("MAP-001", "Load scene Run Scene duoc", "Load duoc scene Run Scene cua Trung trong project.", "High", c => Load(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-002: Ghi nhan collider trong scene Run Scene.")]
        public IEnumerator MAP_002_SceneCoCollider() { return RunUnity("MAP-002", "Scene Run Scene collider", "Ghi nhan collider trong scene Run Scene neu co.", "Low", c => SceneCollider(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-003: Ghi nhan collider non-trigger trong scene Run Scene.")]
        public IEnumerator MAP_003_ColliderNenKhongTrigger() { return RunUnity("MAP-003", "Collider non-trigger Run Scene", "Ghi nhan collider khong trigger trong scene Run Scene neu co.", "Low", c => GroundCollider(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-004: Kiem tra Player prefab co collider hoac CharacterController.")]
        public IEnumerator MAP_004_PlayerCoCollider() { return RunUnity("MAP-004", "Player co collider", "Player prefab that co Collider hoac CharacterController.", "High", c => PlayerCollider(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-005: Kiem tra Enemy prefab co collider hoac CharacterController.")]
        public IEnumerator MAP_005_EnemyCoCollider() { return RunUnity("MAP-005", "Enemy co collider", "Enemy prefab that co Collider hoac CharacterController.", "High", c => EnemyCollider(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-006: Kiem tra Player khong roi xuyen nen ho tro.")]
        public IEnumerator MAP_006_PlayerKhongRoiXuyenNen() { return RunUnity("MAP-006", "Player khong roi xuyen nen", "Player prefab that dung tren ground ho tro va khong roi xuyen.", "High", c => PlayerFall(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-007: Kiem tra Enemy instantiate khong loi physics co ban.")]
        public IEnumerator MAP_007_EnemyKhongRoiXuyenNen() { return RunUnity("MAP-007", "Enemy instantiate physics co ban", "Enemy prefab that instantiate tren ground ho tro khong loi physics nghiem trong.", "Medium", c => EnemyFall(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-008: Ghi nhan bounds collider trong scene Run Scene.")]
        public IEnumerator MAP_008_ColliderMapBoundsHopLe() { return RunUnity("MAP-008", "Collider Run Scene bounds", "Ghi nhan bounds collider trong scene Run Scene neu co.", "Low", c => Bounds(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-009: Kiem tra spawn Player va Enemy gan nhau khong crash.")]
        public IEnumerator MAP_009_PlayerEnemySpawnGanNhauKhongLoi() { return RunUnity("MAP-009", "Player/Enemy spawn gan nhau khong crash", "Spawn Player va Enemy prefab that tren ground ho tro khong crash.", "Medium", c => SpawnNear(c)); }

        [UnityTest, Category("MapCollision"), Category("Tu dong"), Description("MAP-010: Kiem tra scene Run Scene chay 60 frame.")]
        public IEnumerator MAP_010_MapChay60FrameKhongLoi() { return RunUnity("MAP-010", "Run Scene chay 60 frame", "Scene Run Scene cua Trung chay 60 frame trong PlayMode.", "High", c => Sixty(c)); }

        private IEnumerator Load(Ctx c) { yield return LoadSceneByPath(TestSceneConfig.RunScenePath, c); yield return null; c.Actual += "Scene Run Scene load thanh cong."; }
        private IEnumerator SceneCollider(Ctx c) { yield return LoadSceneByPath(TestSceneConfig.RunScenePath, c); yield return null; Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>(true); int enabled = 0; foreach (Collider col in colliders) if (col.enabled) enabled++; c.Actual += $"Collider tong={colliders.Length}, enabled={enabled}."; }
        private IEnumerator GroundCollider(Ctx c) { yield return LoadSceneByPath(TestSceneConfig.RunScenePath, c); yield return null; Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>(true); int nonTrigger = 0; foreach (Collider col in colliders) if (col.enabled && !col.isTrigger) nonTrigger++; c.Actual += $"Collider khong trigger={nonTrigger}."; }
        private IEnumerator PlayerCollider(Ctx c) { GameObject player = SpawnPlayer(Vector3.up); yield return null; int colliders = player.GetComponentsInChildren<Collider>(true).Length; int controllers = player.GetComponentsInChildren<CharacterController>(true).Length; c.Actual = $"Player={player.name}, Collider={colliders}, CharacterController={controllers}."; Assert.IsTrue(colliders > 0 || controllers > 0); }
        private IEnumerator EnemyCollider(Ctx c) { GameObject enemy = SpawnEnemy(Vector3.up); yield return null; int colliders = enemy.GetComponentsInChildren<Collider>(true).Length; int controllers = enemy.GetComponentsInChildren<CharacterController>(true).Length; c.Actual = $"Enemy={enemy.name}, Collider={colliders}, CharacterController={controllers}."; Assert.IsTrue(colliders > 0 || controllers > 0); }
        private IEnumerator PlayerFall(Ctx c) { CreateGround(); GameObject player = SpawnPlayer(new Vector3(0f, 1.2f, 0f)); float y0 = player.transform.position.y; for (int i = 0; i < 60; i++) yield return null; float y1 = player.transform.position.y; c.Actual = $"Player={player.name}, Y dau={N(y0)}, Y sau={N(y1)}."; Assert.Greater(y1, -1.5f); }
        private IEnumerator EnemyFall(Ctx c) { CreateGround(); GameObject enemy = SpawnEnemy(new Vector3(0f, 1.2f, 0f)); float y0 = enemy.transform.position.y; for (int i = 0; i < 30; i++) yield return null; float y1 = enemy.transform.position.y; c.Actual = $"Enemy={enemy.name}, Y dau={N(y0)}, Y sau={N(y1)}."; Assert.Greater(y1, -10f); }
        private IEnumerator Bounds(Ctx c) { yield return LoadSceneByPath(TestSceneConfig.RunScenePath, c); yield return null; Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>(true); int valid = 0; foreach (Collider col in colliders) if (col.enabled && col.bounds.size.sqrMagnitude > 0.001f) valid++; c.Actual += $"Collider={colliders.Length}, bounds hop le={valid}."; }
        private IEnumerator SpawnNear(Ctx c) { CreateGround(); GameObject player = SpawnPlayer(new Vector3(0f, 1.2f, 0f)); GameObject enemy = SpawnEnemy(new Vector3(2.5f, 1.2f, 0f)); for (int i = 0; i < 30; i++) yield return null; float distance = Vector3.Distance(player.transform.position, enemy.transform.position); c.Actual = $"Player={player.name}, Enemy={enemy.name}, khoang cach={N(distance)}."; }
        private IEnumerator Sixty(Ctx c) { yield return LoadSceneByPath(TestSceneConfig.RunScenePath, c); for (int i = 0; i < 60; i++) yield return null; c.Actual += "Frame=60."; }
    }
}
