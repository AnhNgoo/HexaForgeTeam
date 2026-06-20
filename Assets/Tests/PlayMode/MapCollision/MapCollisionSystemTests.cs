using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class MapCollisionSystemTests : RuntimeSystemTestBase
    {
        protected override string ExportName => "MapCollision";

        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-001: Kiểm tra load được gameplay/map scene thật.")]
        public IEnumerator MAP_001_LoadGameplaySceneDuoc() { return RunUnity("MAP-001", "Load gameplay/map scene được", "Load được scene gameplay/map thật trong project.", "High", c => Load(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-002: Kiểm tra scene có Collider.")]
        public IEnumerator MAP_002_SceneCoCollider() { return RunUnity("MAP-002", "Scene có Collider", "Gameplay scene có Collider thật để chặn nhân vật.", "High", c => SceneCollider(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-003: Kiểm tra scene có collider nền/map không phải trigger.")]
        public IEnumerator MAP_003_ColliderNenKhongTrigger() { return RunUnity("MAP-003", "Collider nền/map không trigger", "Có ít nhất một Collider không trigger trên object map/nền.", "High", c => GroundCollider(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-004: Kiểm tra Player prefab có collider hoặc CharacterController.")]
        public IEnumerator MAP_004_PlayerCoCollider() { return RunUnity("MAP-004", "Player có collider", "Player prefab thật có Collider hoặc CharacterController.", "High", c => PlayerCollider(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-005: Kiểm tra Enemy prefab có collider hoặc CharacterController.")]
        public IEnumerator MAP_005_EnemyCoCollider() { return RunUnity("MAP-005", "Enemy có collider", "Enemy prefab thật có Collider hoặc CharacterController.", "High", c => EnemyCollider(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-006: Kiểm tra Player không rơi xuyên nền hỗ trợ.")]
        public IEnumerator MAP_006_PlayerKhongRoiXuyenNen() { return RunUnity("MAP-006", "Player không rơi xuyên nền", "Player prefab thật đứng trên ground hỗ trợ và không rơi xuyên.", "High", c => PlayerFall(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-007: Kiểm tra Enemy không rơi xuyên nền hỗ trợ.")]
        public IEnumerator MAP_007_EnemyKhongRoiXuyenNen() { return RunUnity("MAP-007", "Enemy không rơi xuyên nền", "Enemy prefab thật đứng trên ground hỗ trợ và không rơi xuyên.", "High", c => EnemyFall(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-008: Kiểm tra collider map có bounds hợp lệ.")]
        public IEnumerator MAP_008_ColliderMapBoundsHopLe() { return RunUnity("MAP-008", "Collider map có bounds hợp lệ", "Collider trong scene có bounds kích thước lớn hơn 0.", "Medium", c => Bounds(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-009: Kiểm tra spawn Player và Enemy gần nhau không tạo lỗi physics đỏ.")]
        public IEnumerator MAP_009_PlayerEnemySpawnGanNhauKhongLoi() { return RunUnity("MAP-009", "Player/Enemy spawn gần nhau không lỗi physics", "Spawn Player và Enemy prefab thật trên ground hỗ trợ không lỗi đỏ.", "Medium", c => SpawnNear(c)); }
        [UnityTest, Category("MapCollision"), Category("Tự động"), Description("MAP-010: Kiểm tra map scene chạy 60 frame không Error/Exception.")]
        public IEnumerator MAP_010_MapChay60FrameKhongLoi() { return RunUnity("MAP-010", "Map chạy 60 frame không lỗi đỏ", "Gameplay/map scene thật chạy 60 frame không lỗi đỏ.", "High", c => Sixty(c)); }

        private IEnumerator Load(Ctx c) { StartWatcher(); yield return LoadGameplayScene(c); yield return null; c.Actual += $"Error/Exception={ErrorCount()}."; AssertNoErrors("Load gameplay/map scene không được lỗi đỏ."); }
        private IEnumerator SceneCollider(Ctx c) { yield return LoadGameplayScene(c); yield return null; Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>(true); int enabled = 0; foreach (Collider col in colliders) if (col.enabled) enabled++; c.Actual += $"Collider tổng={colliders.Length}, enabled={enabled}."; Assert.Greater(enabled, 0); }
        private IEnumerator GroundCollider(Ctx c) { yield return LoadGameplayScene(c); yield return null; Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>(true); int nonTrigger = 0; foreach (Collider col in colliders) if (col.enabled && !col.isTrigger) nonTrigger++; c.Actual += $"Collider không trigger={nonTrigger}."; Assert.Greater(nonTrigger, 0); }
        private IEnumerator PlayerCollider(Ctx c) { GameObject p = SpawnPlayer(Vector3.up); yield return null; int colliders = p.GetComponentsInChildren<Collider>(true).Length; int controllers = p.GetComponentsInChildren<CharacterController>(true).Length; c.Actual = $"Player={p.name}, Collider={colliders}, CharacterController={controllers}."; Assert.IsTrue(colliders > 0 || controllers > 0); }
        private IEnumerator EnemyCollider(Ctx c) { GameObject e = SpawnEnemy(Vector3.up); yield return null; int colliders = e.GetComponentsInChildren<Collider>(true).Length; int controllers = e.GetComponentsInChildren<CharacterController>(true).Length; c.Actual = $"Enemy={e.name}, Collider={colliders}, CharacterController={controllers}."; Assert.IsTrue(colliders > 0 || controllers > 0); }
        private IEnumerator PlayerFall(Ctx c) { CreateGround(); GameObject p = SpawnPlayer(new Vector3(0f, 1.2f, 0f)); float y0 = p.transform.position.y; for (int i = 0; i < 60; i++) yield return null; float y1 = p.transform.position.y; c.Actual = $"Player={p.name}, Y đầu={N(y0)}, Y sau={N(y1)}."; Assert.Greater(y1, -1.5f); }
        private IEnumerator EnemyFall(Ctx c) { CreateGround(); GameObject e = SpawnEnemy(new Vector3(0f, 1.2f, 0f)); float y0 = e.transform.position.y; for (int i = 0; i < 60; i++) yield return null; float y1 = e.transform.position.y; c.Actual = $"Enemy={e.name}, Y đầu={N(y0)}, Y sau={N(y1)}."; Assert.Greater(y1, -1.5f); }
        private IEnumerator Bounds(Ctx c) { yield return LoadGameplayScene(c); yield return null; Collider[] colliders = UnityEngine.Object.FindObjectsOfType<Collider>(true); int valid = 0; foreach (Collider col in colliders) if (col.enabled && col.bounds.size.sqrMagnitude > 0.001f) valid++; c.Actual += $"Collider={colliders.Length}, bounds hợp lệ={valid}."; Assert.Greater(valid, 0); }
        private IEnumerator SpawnNear(Ctx c) { StartWatcher(); CreateGround(); GameObject p = SpawnPlayer(new Vector3(0f, 1.2f, 0f)); GameObject e = SpawnEnemy(new Vector3(2.5f, 1.2f, 0f)); for (int i = 0; i < 30; i++) yield return null; float distance = Vector3.Distance(p.transform.position, e.transform.position); c.Actual = $"Player={p.name}, Enemy={e.name}, khoảng cách={N(distance)}, Error/Exception={ErrorCount()}."; AssertNoErrors("Spawn Player/Enemy gần nhau không được lỗi đỏ."); }
        private IEnumerator Sixty(Ctx c) { StartWatcher(); yield return LoadGameplayScene(c); for (int i = 0; i < 60; i++) yield return null; c.Actual += $"Frame=60, Error/Exception={ErrorCount()}."; AssertNoErrors("Map scene chạy 60 frame không được lỗi đỏ."); }
    }
}
