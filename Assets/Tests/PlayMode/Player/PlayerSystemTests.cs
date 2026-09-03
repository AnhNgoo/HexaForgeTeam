using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class PlayerSystemTests
    {
        private const string TesterName = "Huỳnh Ngọc Thanh Phước";
        private const string StartDate = "31/05/2026";
        private const string RunMode = "Tự động";
        private const float MovementTestDuration = 1.0f;
        private const float DefaultMoveSpeed = 5.0f;

        private static readonly List<TestResultRecord> records = new List<TestResultRecord>();

        private readonly List<UnityEngine.Object> spawnedObjects = new List<UnityEngine.Object>();
        private TestLogWatcher logWatcher;

        [TearDown]
        public void TearDown()
        {
            Cleanup();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TestResultCsvExporter.Export("Player_TestResults", records);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player prefab co the khoi tao trong scene test.")]
        public IEnumerator PL_001_PlayerPrefab_CoTheKhoiTao()
        {
            yield return RunUnityTest(
                "PL-001",
                "Kiem tra Player prefab co the khoi tao trong scene test",
                "Player duoc tao thanh cong, khong bi null va khong phat sinh loi khi Instantiate.",
                "High",
                "1. Load Player prefab that. 2. Instantiate Player vao scene test. 3. Kiem tra Player khac null. 4. Cho vai frame va kiem tra khong co loi Console.",
                delegate(TestRunContext context)
                {
                    return PlayerInstantiateRoutine(context);
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player va object con khong bi Missing Script.")]
        public void PL_002_Player_KhongBiMissingScript()
        {
            RunTest("PL-002", "Kiem tra Player khong bi Missing Script",
                "Player va cac object con khong co component bi Missing Script.", "High",
                "1. Instantiate Player. 2. Duyet component cua Player va object con. 3. Kiem tra khong co component null.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Transform[] children = player.GetComponentsInChildren<Transform>(true);
                    int missingCount = 0;

                    foreach (Transform child in children)
                    {
                        Component[] components = child.GetComponents<Component>();
                        for (int i = 0; i < components.Length; i++)
                        {
                            if (components[i] == null)
                            {
                                missingCount++;
                            }
                        }
                    }

                    context.Actual = string.Format("Da quet {0} GameObject con, tim thay {1} Missing Script.", children.Length, missingCount);
                    Assert.AreEqual(0, missingCount, "Player prefab that dang co Missing Script tren Player hoac object con.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong bi vo hinh.")]
        public void PL_003_Player_KhongBiVoHinh()
        {
            RunTest("PL-003", "Kiem tra Player khong bi vo hinh",
                "Player co Renderer hoac SkinnedMeshRenderer dang bat.", "High",
                "1. Instantiate Player. 2. Tim Renderer trong Player va object con. 3. Kiem tra co it nhat mot Renderer enabled.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Renderer[] renderers = player.GetComponentsInChildren<Renderer>(true);
                    int enabledCount = 0;
                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer != null && renderer.enabled)
                        {
                            enabledCount++;
                        }
                    }

                    context.Actual = string.Format("Tim thay {0} Renderer, trong do {1} Renderer dang enabled.", renderers.Length, enabledCount);
                    Assert.Greater(enabledCount, 0, "Player prefab that khong co Renderer nao dang bat nen co nguy co bi vo hinh.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong bi loi material.")]
        public void PL_004_Player_KhongBiLoiMaterial()
        {
            RunTest("PL-004", "Kiem tra Player khong bi loi material",
                "Renderer cua Player co material hop le, khong null.", "Medium",
                "1. Instantiate Player. 2. Duyet Renderer. 3. Kiem tra sharedMaterials khong null.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Renderer[] renderers = player.GetComponentsInChildren<Renderer>(true);
                    int nullMaterials = 0;

                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer == null)
                        {
                            continue;
                        }

                        Material[] materials = renderer.sharedMaterials;
                        if (materials == null || materials.Length == 0)
                        {
                            nullMaterials++;
                            continue;
                        }

                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (materials[i] == null)
                            {
                                nullMaterials++;
                            }
                        }
                    }

                    context.Actual = string.Format("Da kiem tra {0} Renderer, so material null la {1}.", renderers.Length, nullMaterials);
                    Assert.AreEqual(0, nullMaterials, "Renderer cua Player prefab that co material null.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterBase.")]
        public void PL_005_Player_CoCharacterBase()
        {
            RunComponentPresenceTest("PL-005", "Kiem tra Player co CharacterBase", "CharacterBase", "Player co component CharacterBase.", "High");
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterMovement.")]
        public void PL_006_Player_CoCharacterMovement()
        {
            RunComponentPresenceTest("PL-006", "Kiem tra Player co CharacterMovement", "CharacterMovement", "Player co component CharacterMovement de xu ly di chuyen.", "High");
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterRotate.")]
        public void PL_007_Player_CoCharacterRotate()
        {
            RunComponentPresenceTest("PL-007", "Kiem tra Player co CharacterRotate", "CharacterRotate", "Player co component CharacterRotate de xoay huong nhan vat.", "Medium");
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co Collider hoac CharacterController.")]
        public void PL_008_Player_CoColliderHoacCharacterController()
        {
            RunTest("PL-008", "Kiem tra Player co Collider hoac CharacterController",
                "Player co thanh phan va cham de khong xuyen map hoac xuyen object.", "High",
                "1. Instantiate Player. 2. Tim Collider hoac CharacterController. 3. Kiem tra ton tai it nhat mot component hop le.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Collider[] colliders = player.GetComponentsInChildren<Collider>(true);
                    CharacterController controller = player.GetComponentInChildren<CharacterController>(true);
                    context.Actual = string.Format("Tim thay {0} Collider, CharacterController={1}.", colliders.Length, controller != null ? "Co" : "Khong");
                    Assert.IsTrue(colliders.Length > 0 || controller != null, "Player prefab that khong co Collider hoac CharacterController.");
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong roi xuyen nen sau khi spawn.")]
        public IEnumerator PL_009_Player_KhongRoiXuyenNen()
        {
            yield return RunUnityTest("PL-009", "Kiem tra Player khong roi xuyen nen sau khi spawn",
                "Player dung on dinh sau khi spawn, khong roi xuong duoi map.", "High",
                "1. Tao ground collider. 2. Spawn Player tren mat dat. 3. Ghi nhan Y ban dau. 4. Cho 1 giay. 5. Kiem tra Y khong giam bat thuong.",
                delegate(TestRunContext context)
                {
                    return GroundStabilityRoutine(context);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player di chuyen khi nhan W.")]
        public IEnumerator PL_MOVE_001_Player_DiChuyen_W()
        {
            yield return RunMovementTest("PL-MOVE-001", "Kiem tra Player di chuyen khi nhan W", "W", new Vector2(0f, 1f), "High", true);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player di chuyen khi nhan S.")]
        public IEnumerator PL_MOVE_002_Player_DiChuyen_S()
        {
            yield return RunMovementTest("PL-MOVE-002", "Kiem tra Player di chuyen khi nhan S", "S", new Vector2(0f, -1f), "High", true);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player di chuyen khi nhan A.")]
        public IEnumerator PL_MOVE_003_Player_DiChuyen_A()
        {
            yield return RunMovementTest("PL-MOVE-003", "Kiem tra Player di chuyen khi nhan A", "A", new Vector2(-1f, 0f), "High", true);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player di chuyen khi nhan D.")]
        public IEnumerator PL_MOVE_004_Player_DiChuyen_D()
        {
            yield return RunMovementTest("PL-MOVE-004", "Kiem tra Player di chuyen khi nhan D", "D", new Vector2(1f, 0f), "High", true);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player di chuyen cheo W+D.")]
        public IEnumerator PL_MOVE_005_Player_DiChuyenCheo_WD()
        {
            yield return RunMovementTest("PL-MOVE-005", "Kiem tra Player di chuyen cheo W+D", "W+D", new Vector2(1f, 1f), "Medium", true);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player dung khi khong co input.")]
        public IEnumerator PL_MOVE_006_Player_DungKhiKhongCoInput()
        {
            yield return RunUnityTest("PL-MOVE-006", "Kiem tra Player dung khi khong co input",
                "Khi khong co input, Player khong tu troi qua xa.", "Medium",
                "1. Spawn Player. 2. Ghi nhan vi tri dau. 3. Khong truyen input. 4. Cho 1 giay. 5. Kiem tra do lech khong vuot nguong.",
                delegate(TestRunContext context)
                {
                    return IdleMovementRoutine(context);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra toc do di chuyen Player hop le.")]
        public IEnumerator PL_MOVE_007_Player_TocDoDiChuyenHopLe()
        {
            yield return RunUnityTest("PL-MOVE-007", "Kiem tra toc do di chuyen Player hop le",
                "Quang duong Player di chuyen trong thoi gian test nam trong nguong hop ly.", "Medium",
                "1. Spawn Player. 2. Goi input tien. 3. Tinh quang duong va toc do. 4. Kiem tra toc do > 0 va khong qua cao bat thuong.",
                delegate(TestRunContext context)
                {
                    return MovementSpeedRoutine(context);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player Jump duoc.")]
        public IEnumerator PL_JUMP_001_Player_JumpDuoc()
        {
            yield return RunUnityTest("PL-JUMP-001", "Kiem tra Player Jump duoc",
                "Khi goi Jump/input Jump, vi tri Y tang hoac state Jump duoc kich hoat.", "Medium",
                "1. Spawn Player tren ground. 2. Ghi nhan Y ban dau. 3. Goi method Jump. 4. Cho vai frame. 5. Kiem tra Y cao nhat.",
                delegate(TestRunContext context)
                {
                    return JumpRoutine(context, false);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player roi xuong lai mat dat sau khi Jump.")]
        public IEnumerator PL_JUMP_002_Player_RoiXuongSauJump()
        {
            yield return RunUnityTest("PL-JUMP-002", "Kiem tra Player roi xuong lai mat dat sau khi Jump",
                "Sau khi Jump, Player tro lai gan mat dat, khong bi treo tren khong.", "Medium",
                "1. Spawn Player tren ground. 2. Goi Jump. 3. Cho du thoi gian roi xuong. 4. Kiem tra Y cuoi khong cao bat thuong.",
                delegate(TestRunContext context)
                {
                    return JumpRoutine(context, true);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player Dodge duoc.")]
        public IEnumerator PL_DODGE_001_Player_DodgeDuoc()
        {
            yield return RunUnityTest("PL-DODGE-001", "Kiem tra Player Dodge duoc",
                "Khi goi Dodge/input Dodge, Player thay doi vi tri hoac chuyen state Dodge hop le.", "Medium",
                "1. Spawn Player. 2. Ghi nhan vi tri dau. 3. Goi method Dodge. 4. Cho vai frame. 5. Kiem tra vi tri thay doi.",
                delegate(TestRunContext context)
                {
                    return DodgeRoutine(context, false);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong Dodge xuyen vat can.")]
        public IEnumerator PL_DODGE_002_Player_KhongDodgeXuyenVatCan()
        {
            yield return RunUnityTest("PL-DODGE-002", "Kiem tra Player khong Dodge xuyen vat can",
                "Khi Dodge ve phia collider vat can, Player khong xuyen qua vat can.", "High",
                "1. Tao vat can collider. 2. Spawn Player gan vat can. 3. Goi Dodge ve huong vat can. 4. Cho vai frame. 5. Kiem tra Player khong vuot qua vat can.",
                delegate(TestRunContext context)
                {
                    return DodgeRoutine(context, true);
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterCombat.")]
        public void PL_ACT_001_Player_CoCharacterCombat()
        {
            RunComponentPresenceTest("PL-ACT-001", "Kiem tra Player co CharacterCombat", "CharacterCombat", "Player co component CharacterCombat de xu ly chien dau.", "High");
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterMelee.")]
        public void PL_ACT_002_Player_CoCharacterMelee()
        {
            RunComponentPresenceTest("PL-ACT-002", "Kiem tra Player co CharacterMelee", "CharacterMelee", "Player co component CharacterMelee de xu ly danh can chien.", "High");
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterWeapon.")]
        public void PL_ACT_003_Player_CoCharacterWeapon()
        {
            RunTest("PL-ACT-003", "Kiem tra Player co CharacterWeapon",
                "Player co component hoac du lieu vu khi hop le de tan cong.", "Medium",
                "1. Instantiate Player. 2. Tim CharacterWeapon hoac object vu khi tren Player. 3. Kiem tra reference hop le neu co.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Component weapon = FindRequiredComponent(player, "CharacterWeapon", false);
                    Transform weaponObject = FindChildContains(player.transform, "weapon");
                    context.Actual = string.Format("CharacterWeapon={0}, object vu khi gan tren Player={1}.",
                        weapon != null ? weapon.GetType().Name : "Khong tim thay",
                        weaponObject != null ? weaponObject.name : "Khong tim thay");
                    Assert.IsTrue(weapon != null || weaponObject != null, "Khong tim thay CharacterWeapon hoac object vu khi tren Player prefab that.");
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player goi duoc hanh dong Attack.")]
        public IEnumerator PL_ACT_004_Player_GoiDuocAttack()
        {
            yield return RunActionNoConsoleErrorTest("PL-ACT-004", "Kiem tra Player goi duoc hanh dong Attack",
                "Khi goi Attack, Player xu ly lenh tan cong va khong phat sinh loi.", "High",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = InvokeAttack(player);
                    context.Actual = "Method Attack goi duoc: " + method + ", loi Console se duoc kiem tra sau khi cho frame.";
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Attack khi dang Jump khong gay loi.")]
        public IEnumerator PL_ACT_005_AttackKhiDangJump_KhongGayLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-ACT-005", "Kiem tra Attack khi dang Jump khong gay loi",
                "Goi Attack trong luc Jump khong gay Error/Exception va khong ket state.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string jumpMethod = InvokeJump(player);
                    string attackMethod = InvokeAttack(player);
                    context.Actual = string.Format("Method Jump={0}, method Attack={1}.", jumpMethod, attackMethod);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Attack khi dang Dodge khong gay loi.")]
        public IEnumerator PL_ACT_006_AttackKhiDangDodge_KhongGayLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-ACT-006", "Kiem tra Attack khi dang Dodge khong gay loi",
                "Goi Attack trong luc Dodge khong gay Error/Exception va khong ket state.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string dodgeMethod = InvokeDodge(player, new Vector2(0f, 1f));
                    string attackMethod = InvokeAttack(player);
                    context.Actual = string.Format("Method Dodge={0}, method Attack={1}.", dodgeMethod, attackMethod);
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterSkill.")]
        public void PL_SKILL_001_Player_CoCharacterSkill()
        {
            RunComponentPresenceTest("PL-SKILL-001", "Kiem tra Player co CharacterSkill", "CharacterSkill", "Player co component CharacterSkill de xu ly skill nhan vat.", "High");
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co du lieu Skill 1.")]
        public IEnumerator PL_SKILL_002_Player_CoDuLieuSkill1()
        {
            yield return RunUnityTest("PL-SKILL-002", "Kiem tra Player co du lieu Skill 1",
                "Skill 1 ton tai, khong null.", "High",
                "1. Instantiate Player. 2. Tim CharacterSkill. 3. Kiem tra danh sach skill hoac field Skill 1 khong null bang reflection.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Component skill = FindRequiredComponent(player, "CharacterSkill", true);
                    Component characterBase = FindRequiredComponent(player, "CharacterBase", true);
                    object skillData1 = null;
                    object characterData = null;
                    object runtimeSkill1 = null;
                    bool hasSkillData1 = TestReflectionHelper.TryGetValue(skill, "SkillData1", out skillData1) && skillData1 != null;
                    bool hasCharacterDataSkill1 = TestReflectionHelper.TryGetValue(characterBase, "CharacterData", out characterData) &&
                                                  characterData != null &&
                                                  TestReflectionHelper.TryGetValue(characterData, "skill1Data", out skillData1) &&
                                                  skillData1 != null;
                    bool hasRuntimeSkill1 = TestReflectionHelper.TryGetValue(skill, "skill1", out runtimeSkill1) && runtimeSkill1 != null;
                    bool hasSkillAsset = Resources.LoadAll<CharacterSkillData>("ScriptableObjects/SkillData").Length > 0;
                    context.Actual = string.Format("SkillData1={0}, CharacterData.skill1Data={1}, runtime skill1={2}, SkillData assets={3}.",
                        hasSkillData1 ? "Khac null" : "Null/khong doc duoc",
                        hasCharacterDataSkill1 ? "Khac null" : "Null/khong doc duoc",
                        hasRuntimeSkill1 ? "Khac null" : "Null/khong doc duoc",
                        hasSkillAsset ? "Co" : "Khong co");
                    Assert.IsTrue(hasSkillData1 || hasCharacterDataSkill1 || hasRuntimeSkill1 || hasSkillAsset,
                        "Khong tim thay du lieu Skill 1 trong Player runtime hoac Resources/ScriptableObjects/SkillData.");
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player kich hoat duoc Skill 1.")]
        public IEnumerator PL_SKILL_003_Player_KichHoatDuocSkill1()
        {
            yield return RunActionNoConsoleErrorTest("PL-SKILL-003", "Kiem tra Player kich hoat duoc Skill 1",
                "Goi Skill 1 khong loi, co state/effect/logic tuong ung neu doc duoc.", "High",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = InvokeSkill1(player);
                    context.Actual = "Method Skill 1 goi duoc: " + method + ".";
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Skill 1 khong spam lien tuc ngoai thiet ke.")]
        public IEnumerator PL_SKILL_004_Skill1_KhongSpamGayLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-SKILL-004", "Kiem tra Skill 1 khong spam lien tuc ngoai thiet ke",
                "Skill 1 co cooldown/dieu kien hoac khong gay loi khi goi lien tuc.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = string.Empty;
                    for (int i = 0; i < 5; i++)
                    {
                        method = InvokeSkill1(player);
                    }

                    context.Actual = string.Format("Da goi Skill 1 5 lan lien tiep bang method {0}.", method);
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterLockTarget.")]
        public void PL_LOCK_001_Player_CoCharacterLockTarget()
        {
            RunComponentPresenceTest("PL-LOCK-001", "Kiem tra Player co CharacterLockTarget", "CharacterLockTarget", "Player co component CharacterLockTarget de xu ly khoa muc tieu.", "Medium");
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player lock duoc Enemy trong pham vi.")]
        public IEnumerator PL_LOCK_002_Player_LockDuocEnemyTrongPhamVi()
        {
            yield return RunUnityTest("PL-LOCK-002", "Kiem tra Player lock duoc Enemy trong pham vi",
                "Khi co Enemy trong pham vi, Player gan target hop le.", "Medium",
                "1. Spawn Player. 2. Spawn Enemy prefab that. 3. Goi logic lock target. 4. Kiem tra target hoac khong co loi.",
                delegate(TestRunContext context)
                {
                    return LockTargetRoutine(context, true, false, false);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong lock khi khong co Enemy.")]
        public IEnumerator PL_LOCK_003_Player_KhongLockKhiKhongCoEnemy()
        {
            yield return RunUnityTest("PL-LOCK-003", "Kiem tra Player khong lock khi khong co Enemy",
                "Khi khong co Enemy, lock target khong gay loi va khong gan target sai.", "Medium",
                "1. Spawn Player. 2. Khong spawn Enemy. 3. Goi logic lock target. 4. Kiem tra khong co Error/Exception.",
                delegate(TestRunContext context)
                {
                    return LockTargetRoutine(context, false, false, false);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player di chuyen khi dang lock target.")]
        public IEnumerator PL_LOCK_004_Player_DiChuyenKhiDangLockTarget()
        {
            yield return RunUnityTest("PL-LOCK-004", "Kiem tra Player di chuyen khi dang lock target",
                "Khi lock Enemy, Player van di chuyen duoc bang W/A/S/D, khong ket input.", "Medium",
                "1. Spawn Player. 2. Spawn Enemy. 3. Goi lock target. 4. Goi input W. 5. Kiem tra Player thay doi vi tri.",
                delegate(TestRunContext context)
                {
                    return LockTargetRoutine(context, true, true, false);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player xoay huong hop le khi dang lock target.")]
        public IEnumerator PL_LOCK_005_Player_XoayHuongKhiDangLockTarget()
        {
            yield return RunUnityTest("PL-LOCK-005", "Kiem tra Player xoay huong hop le khi dang lock target",
                "Khi lock target, Player hoac huong nhin xoay ve Enemy dung logic thiet ke.", "Medium",
                "1. Spawn Player. 2. Spawn Enemy. 3. Goi lock target. 4. Cho vai frame. 5. Kiem tra rotation/huong toi Enemy neu doc duoc.",
                delegate(TestRunContext context)
                {
                    return LockTargetRoutine(context, true, false, true);
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co Animator reference hop le.")]
        public void PL_REF_001_Player_CoAnimatorReferenceHopLe()
        {
            RunTest("PL-REF-001", "Kiem tra Player co Animator reference hop le",
                "Neu Player co/yeu cau Animator thi Animator va RuntimeAnimatorController khong null.", "Medium",
                "1. Instantiate Player. 2. Tim Animator. 3. Neu Animator ton tai thi kiem tra runtimeAnimatorController khac null.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Animator[] animators = player.GetComponentsInChildren<Animator>(true);
                    int nullControllers = 0;
                    foreach (Animator animator in animators)
                    {
                        if (animator != null && animator.runtimeAnimatorController == null)
                        {
                            nullControllers++;
                        }
                    }

                    context.Actual = string.Format("Tim thay {0} Animator, so RuntimeAnimatorController null la {1}.", animators.Length, nullControllers);
                    Assert.Greater(animators.Length, 0, "Khong tim thay Animator tren Player prefab that.");
                    Assert.AreEqual(0, nullControllers, "Animator tren Player prefab that co RuntimeAnimatorController null.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co Audio reference hop le neu co.")]
        public void PL_REF_002_Player_CoAudioReferenceHopLeNeuCo()
        {
            RunTest("PL-REF-002", "Kiem tra Player co Audio reference hop le neu co",
                "Neu Player co AudioSource thi AudioSource va AudioClip/reference can thiet khong null neu component yeu cau.", "Low",
                "1. Instantiate Player. 2. Tim AudioSource. 3. Kiem tra AudioClip/reference neu co.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    AudioSource[] audioSources = player.GetComponentsInChildren<AudioSource>(true);
                    int nullClips = 0;
                    foreach (AudioSource source in audioSources)
                    {
                        if (source != null && source.playOnAwake && source.clip == null)
                        {
                            nullClips++;
                        }
                    }

                    context.Actual = string.Format("Tim thay {0} AudioSource, so AudioClip null tren AudioSource playOnAwake la {1}.", audioSources.Length, nullClips);
                    Assert.AreEqual(0, nullClips, "AudioSource playOnAwake tren Player prefab that co AudioClip null.");
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong loi khi khong co Enemy.")]
        public IEnumerator PL_SUM_001_Player_KhongLoiKhiKhongCoEnemy()
        {
            yield return RunSummaryRoutine("PL-SUM-001", "Kiem tra Player khong loi khi khong co Enemy",
                "Player van chay logic co ban khong NullReferenceException khi scene khong co Enemy.", "High", 0);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong loi khi co nhieu Enemy gan do.")]
        public IEnumerator PL_SUM_002_Player_KhongLoiKhiCoNhieuEnemyGanDo()
        {
            yield return RunSummaryRoutine("PL-SUM-002", "Kiem tra Player khong loi khi co nhieu Enemy gan do",
                "Player khong loi khi co nhieu Enemy gan.", "Medium", 3);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra tong he thong Player khong phat sinh loi do.")]
        public IEnumerator PL_SUM_003_TongHeThongPlayer_KhongPhatSinhLoiDo()
        {
            yield return RunSummaryRoutine("PL-SUM-003", "Kiem tra tong he thong Player khong phat sinh loi do",
                "Spawn, joystick/vector input, Jump, Dodge, Attack, Skill 1, Lock Target khong gay Error/Exception.", "High", 1);
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player prefab that ton tai trong project.")]
        public void PL_010_PlayerPrefab_TonTai()
        {
            RunTest("PL-010", "Kiem tra Player prefab that ton tai trong project",
                "Player prefab that ton tai va duoc TestPrefabFinder tim thay.", "High",
                "1. Goi TestPrefabFinder.FindPlayerPrefab. 2. Kiem tra prefab khac null.",
                delegate(TestRunContext context)
                {
                    GameObject prefab = TestPrefabFinder.FindPlayerPrefab();
                    context.Actual = prefab != null ? "Tim thay Player prefab: " + prefab.name + "." : "Khong tim thay Player prefab that.";
                    Assert.IsNotNull(prefab, "Khong tim thay Player prefab that trong project de test.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Transform cua Player hop le sau khi spawn.")]
        public void PL_011_Player_TransformHopLe()
        {
            RunTest("PL-011", "Kiem tra Player co Transform hop le",
                "Player co Transform hop le, position/rotation/scale khong NaN hoac Infinity.", "High",
                "1. Instantiate Player. 2. Doc Transform. 3. Kiem tra position, rotation, scale hop le.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    context.Actual = string.Format("Position={0}, rotation={1}, scale={2}.", FormatVector3(player.transform.position), player.transform.rotation.eulerAngles, FormatVector3(player.transform.localScale));
                    AssertFinite(player.transform.position, "Position Player khong hop le.");
                    AssertFinite(player.transform.localScale, "Scale Player khong hop le.");
                    AssertFinite(player.transform.rotation.eulerAngles, "Rotation Player khong hop le.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra scale cua Player khong bang 0.")]
        public void PL_012_Player_ScaleKhongBang0()
        {
            RunTest("PL-012", "Kiem tra Player scale hop le, khong bang 0",
                "Player scale tren cac truc lon hon 0 va khong bi am bat thuong.", "High",
                "1. Instantiate Player. 2. Doc localScale. 3. Kiem tra x/y/z lon hon 0.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Vector3 scale = player.transform.localScale;
                    context.Actual = "Scale Player=" + FormatVector3(scale) + ".";
                    Assert.Greater(scale.x, 0f, "Scale X cua Player bang 0 hoac am.");
                    Assert.Greater(scale.y, 0f, "Scale Y cua Player bang 0 hoac am.");
                    Assert.Greater(scale.z, 0f, "Scale Z cua Player bang 0 hoac am.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player active sau khi spawn.")]
        public void PL_013_Player_ActiveSauKhiSpawn()
        {
            RunTest("PL-013", "Kiem tra Player active sau khi spawn",
                "Player instance activeInHierarchy sau khi Instantiate.", "High",
                "1. Instantiate Player. 2. Kiem tra activeSelf va activeInHierarchy.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    context.Actual = string.Format("activeSelf={0}, activeInHierarchy={1}.", player.activeSelf, player.activeInHierarchy);
                    Assert.IsTrue(player.activeSelf, "Player instance bi inactiveSelf sau khi spawn.");
                    Assert.IsTrue(player.activeInHierarchy, "Player instance khong activeInHierarchy sau khi spawn.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterData hoac du lieu nhan vat hop le.")]
        public void PL_014_Player_CoCharacterDataHopLe()
        {
            RunTest("PL-014", "Kiem tra Player co CharacterData hoac du lieu nhan vat hop le",
                "CharacterData ton tai va stats co gia tri hop le.", "High",
                "1. Instantiate Player. 2. Tim CharacterBase. 3. Doc CharacterData/stats bang reflection. 4. Kiem tra health/speed/stamina hop le neu doc duoc.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Component characterBase = FindRequiredComponent(player, "CharacterBase", true);
                    object characterData = null;
                    Assert.IsTrue(TestReflectionHelper.TryGetValue(characterBase, "CharacterData", out characterData) && characterData != null, "Khong tim thay CharacterData hop le tren Player prefab that.");
                    object stats = null;
                    Assert.IsTrue(TestReflectionHelper.TryGetValue(characterData, "stats", out stats) && stats != null, "CharacterData cua Player khong co stats hop le.");
                    float health = ReadFloatOrFail(stats, "health");
                    float speed = ReadFloatOrFail(stats, "speed");
                    float stamina = ReadFloatOrFail(stats, "stamina");
                    context.Actual = string.Format("CharacterData={0}, health={1}, speed={2}, stamina={3}.", characterData, FormatFloat(health), FormatFloat(speed), FormatFloat(stamina));
                    Assert.Greater(health, 0f, "Health ban dau trong CharacterData phai lon hon 0.");
                    Assert.Greater(speed, 0f, "Speed trong CharacterData phai lon hon 0.");
                    Assert.GreaterOrEqual(stamina, 0f, "Stamina trong CharacterData khong duoc am.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co Kael neu prefab la Kael.")]
        public void PL_015_Player_CoKaelNeuPrefabLaKael()
        {
            RunTest("PL-015", "Kiem tra Player co Kael neu prefab la Kael",
                "Neu prefab ten Kael hoac co visual Kael thi component Kael ton tai.", "Medium",
                "1. Instantiate Player. 2. Kiem tra ten prefab/visual. 3. Neu la Kael thi tim component Kael.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    bool looksLikeKael = player.name.IndexOf("Kael", StringComparison.OrdinalIgnoreCase) >= 0 || FindChildContains(player.transform, "Kael") != null;
                    Component kael = TestReflectionHelper.FindComponentByClassName(player, "Kael");
                    context.Actual = string.Format("looksLikeKael={0}, component Kael={1}.", looksLikeKael, kael != null ? kael.GetType().Name : "Khong tim thay");
                    if (looksLikeKael)
                    {
                        Assert.IsNotNull(kael, "Prefab co dau hieu la Kael nhung khong co component Kael.");
                    }
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra StateController cua Player duoc khoi tao sau Start.")]
        public IEnumerator PL_016_Player_StateControllerDuocKhoiTao()
        {
            yield return RunUnityTest("PL-016", "Kiem tra StateController cua Player duoc khoi tao sau Start",
                "Neu project dung StateController thi stateController khac null sau Start.", "Medium",
                "1. Spawn Player. 2. Cho Start chay. 3. Doc StateController bang reflection.",
                delegate(TestRunContext context)
                {
                    return StateControllerRoutine(context, false);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra state ban dau cua Player khong null neu doc duoc.")]
        public IEnumerator PL_017_Player_StateBanDauKhongNull()
        {
            yield return RunUnityTest("PL-017", "Kiem tra Player state ban dau khong null neu doc duoc",
                "StateController co currentState hop le sau khi Player Start.", "Medium",
                "1. Spawn Player. 2. Cho Start chay. 3. Doc currentState bang reflection.",
                delegate(TestRunContext context)
                {
                    return StateControllerRoutine(context, true);
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra CharacterController cua Player co cau hinh hop le.")]
        public void PL_018_Player_CharacterControllerCauHinhHopLe()
        {
            RunTest("PL-018", "Kiem tra CharacterController cua Player co cau hinh hop le",
                "CharacterController neu co thi radius/height/skinWidth lon hon 0.", "Medium",
                "1. Instantiate Player. 2. Tim CharacterController. 3. Kiem tra radius, height, skinWidth.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    CharacterController controller = player.GetComponentInChildren<CharacterController>(true);
                    Assert.IsNotNull(controller, "Player prefab that khong co CharacterController de kiem tra cau hinh.");
                    context.Actual = string.Format("radius={0}, height={1}, skinWidth={2}, center={3}.", FormatFloat(controller.radius), FormatFloat(controller.height), FormatFloat(controller.skinWidth), FormatVector3(controller.center));
                    Assert.Greater(controller.radius, 0f, "CharacterController radius phai lon hon 0.");
                    Assert.Greater(controller.height, 0f, "CharacterController height phai lon hon 0.");
                    Assert.Greater(controller.skinWidth, 0f, "CharacterController skinWidth phai lon hon 0.");
                });
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co CharacterAnimation.")]
        public void PL_019_Player_CoCharacterAnimation()
        {
            RunComponentPresenceTest("PL-019", "Kiem tra Player co CharacterAnimation", "CharacterAnimation", "Player co component CharacterAnimation de dieu khien animation.", "Medium");
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player co effect point co ban neu gameplay can.")]
        public void PL_020_Player_CoEffectPointCoBan()
        {
            RunTest("PL-020", "Kiem tra Player co effect point co ban",
                "Player co EffectPoints hoac cac point danh/skill neu component yeu cau.", "Low",
                "1. Instantiate Player. 2. Tim EffectPoints va cac child point quan trong. 3. Ghi nhan so point tim thay.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Transform effectRoot = FindChildContains(player.transform, "EffectPoints");
                    int pointCount = 0;
                    if (effectRoot != null)
                    {
                        pointCount = effectRoot.GetComponentsInChildren<Transform>(true).Length - 1;
                    }

                    context.Actual = string.Format("EffectPoints={0}, so point con={1}.", effectRoot != null ? effectRoot.name : "Khong tim thay", pointCount);
                    Assert.IsNotNull(effectRoot, "Khong tim thay EffectPoints tren Player prefab that.");
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player doi huong lien tuc khong gay loi.")]
        public IEnumerator PL_MOVE_008_Player_DoiHuongLienTucKhongLoi()
        {
            yield return RunUnityTest("PL-MOVE-008", "Kiem tra Player doi huong lien tuc khong gay loi",
                "Player xu ly chuyen huong W/A/S/D lien tuc khong Error/Exception va khong ket.", "Medium",
                "1. Spawn Player. 2. Goi W/A/S/D lien tuc. 3. Kiem tra khong loi Console va vi tri hop le.",
                delegate(TestRunContext context)
                {
                    return DirectionChangeRoutine(context);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra vi tri Player khong NaN hoac Infinity sau movement.")]
        public IEnumerator PL_MOVE_009_Player_PositionFiniteSauMovement()
        {
            yield return RunFiniteAfterActionRoutine("PL-MOVE-009", "Kiem tra vi tri Player khong NaN hoac Infinity sau movement", "Movement", "Medium",
                delegate(GameObject player)
                {
                    InvokeMovement(FindRequiredComponent(player, "CharacterMovement", true), new Vector2(0f, 1f), DefaultMoveSpeed);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra MoveDirection cua Player khong NaN hoac Infinity.")]
        public IEnumerator PL_MOVE_010_Player_MoveDirectionFinite()
        {
            yield return RunUnityTest("PL-MOVE-010", "Kiem tra MoveDirection cua Player khong NaN hoac Infinity",
                "CharacterMovement.MoveDirection hop le sau khi set input.", "Medium",
                "1. Spawn Player. 2. Goi SetMoveDirection/Run. 3. Doc MoveDirection bang reflection.",
                delegate(TestRunContext context)
                {
                    return MoveDirectionFiniteRoutine(context);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong teleport bat thuong sau va cham vat can.")]
        public IEnumerator PL_MOVE_011_Player_KhongTeleportSauVaCham()
        {
            yield return RunCollisionStabilityRoutine("PL-MOVE-011", "Kiem tra Player khong teleport bat thuong sau va cham vat can", true);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Player khong bi scale hoac rotation bat thuong sau va cham.")]
        public IEnumerator PL_MOVE_012_Player_KhongBienDoiScaleRotationSauVaCham()
        {
            yield return RunCollisionStabilityRoutine("PL-MOVE-012", "Kiem tra Player khong bi scale hoac rotation bat thuong sau va cham", false);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Attack khi dang di chuyen khong gay loi.")]
        public IEnumerator PL_ACT_007_AttackKhiDangDiChuyen_KhongGayLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-ACT-007", "Kiem tra Attack khi dang di chuyen khong gay loi",
                "Attack trong luc Player dang di chuyen khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    InvokeMovement(FindRequiredComponent(player, "CharacterMovement", true), new Vector2(0f, 1f), DefaultMoveSpeed);
                    string attackMethod = InvokeAttack(player);
                    context.Actual = "Dang di chuyen bang W, method Attack=" + attackMethod + ".";
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Attack khi dang lock target khong gay loi.")]
        public IEnumerator PL_ACT_008_AttackKhiDangLockTarget_KhongGayLoi()
        {
            yield return RunActionWithEnemyNoConsoleErrorTest("PL-ACT-008", "Kiem tra Attack khi dang lock target khong gay loi",
                "Attack khi Player dang lock target khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string lockMethod = InvokeLockTarget(player);
                    string attackMethod = InvokeAttack(player);
                    context.Actual = string.Format("Method lock={0}, method Attack={1}.", lockMethod, attackMethod);
                }, 1);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Attack nhieu lan lien tiep khong lam ket state.")]
        public IEnumerator PL_ACT_009_AttackNhieuLan_KhongKetState()
        {
            yield return RunActionNoConsoleErrorTest("PL-ACT-009", "Kiem tra Attack nhieu lan lien tiep khong lam ket state",
                "Goi Attack nhieu lan khong gay Error/Exception va khong lam Player inactive.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = string.Empty;
                    for (int i = 0; i < 5; i++)
                    {
                        method = InvokeAttack(player);
                    }

                    context.Actual = "Da goi Attack 5 lan bang method " + method + ", activeInHierarchy=" + player.activeInHierarchy + ".";
                    Assert.IsTrue(player.activeInHierarchy, "Player bi inactive sau khi spam Attack.");
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Attack khi khong co Enemy khong gay loi.")]
        public IEnumerator PL_ACT_010_AttackKhongCoEnemy_KhongGayLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-ACT-010", "Kiem tra Attack khi khong co Enemy khong gay loi",
                "Attack khong co Enemy trong scene khong gay Error/Exception.", "High",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = InvokeAttack(player);
                    context.Actual = "Khong spawn Enemy, method Attack=" + method + ".";
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Attack khi co nhieu Enemy gan do khong gay loi.")]
        public IEnumerator PL_ACT_011_AttackNhieuEnemyGan_KhongGayLoi()
        {
            yield return RunActionWithEnemyNoConsoleErrorTest("PL-ACT-011", "Kiem tra Attack khi co nhieu Enemy gan do khong gay loi",
                "Attack khi co nhieu Enemy gan khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = InvokeAttack(player);
                    context.Actual = "Da spawn 3 Enemy gan Player, method Attack=" + method + ".";
                }, 3);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Skill 1 khi dang di chuyen khong gay loi.")]
        public IEnumerator PL_SKILL_005_Skill1KhiDangDiChuyen_KhongGayLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-SKILL-005", "Kiem tra Skill 1 khi dang di chuyen khong gay loi",
                "Skill 1 trong luc Player dang di chuyen khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    InvokeMovement(FindRequiredComponent(player, "CharacterMovement", true), new Vector2(0f, 1f), DefaultMoveSpeed);
                    string method = InvokeSkill1(player);
                    context.Actual = "Dang di chuyen bang W, method Skill 1=" + method + ".";
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Skill 1 khi dang Jump khong gay loi hoac bi chan dung thiet ke.")]
        public IEnumerator PL_SKILL_006_Skill1KhiDangJump_KhongGayLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-SKILL-006", "Kiem tra Skill 1 khi dang Jump khong gay loi",
                "Skill 1 khi dang Jump khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string jumpMethod = InvokeJump(player);
                    string skillMethod = InvokeSkill1(player);
                    context.Actual = string.Format("Method Jump={0}, method Skill 1={1}.", jumpMethod, skillMethod);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Skill 1 khi dang Dodge khong gay loi hoac bi chan dung thiet ke.")]
        public IEnumerator PL_SKILL_007_Skill1KhiDangDodge_KhongGayLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-SKILL-007", "Kiem tra Skill 1 khi dang Dodge khong gay loi",
                "Skill 1 khi dang Dodge khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string dodgeMethod = InvokeDodge(player, new Vector2(0f, 1f));
                    string skillMethod = InvokeSkill1(player);
                    context.Actual = string.Format("Method Dodge={0}, method Skill 1={1}.", dodgeMethod, skillMethod);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Skill 1 khi dang lock target khong gay loi.")]
        public IEnumerator PL_SKILL_008_Skill1KhiDangLockTarget_KhongGayLoi()
        {
            yield return RunActionWithEnemyNoConsoleErrorTest("PL-SKILL-008", "Kiem tra Skill 1 khi dang lock target khong gay loi",
                "Skill 1 khi Player dang lock target khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string lockMethod = InvokeLockTarget(player);
                    string skillMethod = InvokeSkill1(player);
                    context.Actual = string.Format("Method lock={0}, method Skill 1={1}.", lockMethod, skillMethod);
                }, 1);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Skill 1 khong lam Player bi NaN hoac Infinity position.")]
        public IEnumerator PL_SKILL_009_Skill1_PositionFinite()
        {
            yield return RunFiniteAfterActionRoutine("PL-SKILL-009", "Kiem tra Skill 1 khong lam Player bi NaN hoac Infinity position", "Skill 1", "Medium",
                delegate(GameObject player)
                {
                    InvokeSkill1(player);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Skill 1 khong lam Player bien mat hoac disable bat thuong.")]
        public IEnumerator PL_SKILL_010_Skill1_KhongLamPlayerDisable()
        {
            yield return RunActionNoConsoleErrorTest("PL-SKILL-010", "Kiem tra Skill 1 khong lam Player bien mat hoac disable bat thuong",
                "Sau khi goi Skill 1, Player van active va con Renderer enabled.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = InvokeSkill1(player);
                    int enabledRenderers = CountEnabledRenderers(player);
                    context.Actual = string.Format("Method Skill 1={0}, activeInHierarchy={1}, Renderer enabled={2}.", method, player.activeInHierarchy, enabledRenderers);
                    Assert.IsTrue(player.activeInHierarchy, "Player bi disable sau khi goi Skill 1.");
                    Assert.Greater(enabledRenderers, 0, "Player khong con Renderer enabled sau khi goi Skill 1.");
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra goi lock target hai lan co the huy lock hoac khong gay loi.")]
        public IEnumerator PL_LOCK_006_LockTargetHaiLan_KhongLoi()
        {
            yield return RunActionWithEnemyNoConsoleErrorTest("PL-LOCK-006", "Kiem tra goi lock target hai lan co the huy lock hoac khong gay loi",
                "Toggle lock target hai lan khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    object before = ReadTarget(player);
                    string method1 = InvokeLockTarget(player);
                    object afterFirst = ReadTarget(player);
                    string method2 = InvokeLockTarget(player);
                    object afterSecond = ReadTarget(player);
                    context.Actual = string.Format("Target truoc={0}, sau lan 1={1}, sau lan 2={2}, method={3}/{4}.", before ?? "Null", afterFirst ?? "Null", afterSecond ?? "Null", method1, method2);
                }, 1);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Enemy bi disable thi lock target khong gay loi.")]
        public IEnumerator PL_LOCK_007_EnemyDisable_LockTargetKhongLoi()
        {
            yield return RunUnityTest("PL-LOCK-007", "Kiem tra Enemy bi disable thi lock target khong gay loi",
                "Khi Enemy target bi disable, Player khong giu target loi va khong Error/Exception.", "Medium",
                "1. Spawn Player va Enemy. 2. Lock target. 3. Disable Enemy. 4. Goi lock lai va kiem tra Console.",
                delegate(TestRunContext context)
                {
                    return DisabledEnemyLockRoutine(context);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra nhieu Enemy trong pham vi thi lock target khong gay loi.")]
        public IEnumerator PL_LOCK_008_NhieuEnemyTrongPhamVi_LockTargetKhongLoi()
        {
            yield return RunActionWithEnemyNoConsoleErrorTest("PL-LOCK-008", "Kiem tra nhieu Enemy trong pham vi thi lock target khong gay loi",
                "Khi co nhieu Enemy trong pham vi, lock target chon target hop le hoac khong phat sinh loi.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = InvokeLockTarget(player);
                    object target = ReadTarget(player);
                    context.Actual = string.Format("Da spawn 3 Enemy, method lock={0}, target sau lock={1}.", method, target ?? "Null/khong doc duoc");
                }, 3);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra khong co Enemy ma goi lock nhieu lan khong loi Console.")]
        public IEnumerator PL_LOCK_009_KhongEnemy_GoiLockNhieuLanKhongLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-LOCK-009", "Kiem tra khong co Enemy ma goi lock nhieu lan khong loi Console",
                "Goi lock target nhieu lan khi khong co Enemy khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = string.Empty;
                    for (int i = 0; i < 5; i++)
                    {
                        method = InvokeLockTarget(player);
                    }

                    context.Actual = "Khong spawn Enemy, da goi lock 5 lan bang method " + method + ".";
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra sau Dodge Player van tiep tuc di chuyen duoc.")]
        public IEnumerator PL_STATE_001_SauDodge_VanDiChuyenDuoc()
        {
            yield return RunUnityTest("PL-STATE-001", "Kiem tra sau Dodge Player van tiep tuc di chuyen duoc",
                "Sau Dodge, Player van co the di chuyen va khong ket state.", "Medium",
                "1. Spawn Player. 2. Goi Dodge. 3. Cho ket thuc Dodge. 4. Goi movement W. 5. Kiem tra vi tri thay doi.",
                delegate(TestRunContext context)
                {
                    return DodgeThenMoveRoutine(context);
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Jump nhieu lan khong gay Error hoac Exception.")]
        public IEnumerator PL_STATE_002_JumpNhieuLan_KhongLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-STATE-002", "Kiem tra Jump nhieu lan khong gay Error hoac Exception",
                "Goi Jump nhieu lan lien tiep khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = string.Empty;
                    for (int i = 0; i < 3; i++)
                    {
                        method = InvokeJump(player);
                    }

                    context.Actual = "Da goi Jump 3 lan bang method " + method + ".";
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra Dodge nhieu lan khong gay Error hoac Exception.")]
        public IEnumerator PL_STATE_003_DodgeNhieuLan_KhongLoi()
        {
            yield return RunActionNoConsoleErrorTest("PL-STATE-003", "Kiem tra Dodge nhieu lan khong gay Error hoac Exception",
                "Goi Dodge nhieu lan lien tiep khong gay Error/Exception.", "Medium",
                delegate(GameObject player, TestRunContext context)
                {
                    string method = string.Empty;
                    for (int i = 0; i < 3; i++)
                    {
                        method = InvokeDodge(player, new Vector2(0f, 1f));
                    }

                    context.Actual = "Da goi Dodge 3 lan bang method " + method + ".";
                });
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra HP hoac Health ton tai neu project co he thong mau Player.")]
        public IEnumerator PL_STATE_004_Player_CoHealthNeuProjectHoTro()
        {
            yield return RunHealthDataUnityTest("PL-STATE-004", "Kiem tra HP hoac Health ton tai neu project co he thong mau Player", false);
        }

        [UnityTest]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra HP ban dau hop le neu doc duoc.")]
        public IEnumerator PL_STATE_005_Player_HPBanDauHopLe()
        {
            yield return RunHealthDataUnityTest("PL-STATE-005", "Kiem tra HP ban dau hop le neu doc duoc", true);
        }

        [Test]
        [Category("Player")]
        [Category("Tự động")]
        [Description("Kiem tra he thong Death cua Player ton tai neu project ho tro.")]
        public void PL_STATE_006_Player_DeathSystemTonTaiNeuHoTro()
        {
            RunTest("PL-STATE-006", "Kiem tra he thong Death cua Player ton tai neu project ho tro",
                "Neu project co HP/Death thi co method hoac state death that de xu ly Player chet.", "High",
                "1. Instantiate Player. 2. Tim method/field lien quan Death/Dead/Die. 3. Fail ro neu project chua co he thong Death Player.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Component[] components = player.GetComponentsInChildren<Component>(true);
                    bool hasDeathSignal = HasMemberNamedLike(components, new[] { "Die", "Dead", "Death", "OnDead", "IsDead" });
                    context.Actual = "Co member Death/Dead/Die tren Player=" + hasDeathSignal + ".";
                    Assert.IsTrue(hasDeathSignal, "Khong tim thay he thong Death/Dead/Die that tren Player prefab de test.");
                });
        }

        private IEnumerator PlayerInstantiateRoutine(TestRunContext context)
        {
            StartLogWatcher();
            GameObject prefab = TestPrefabFinder.FindPlayerPrefab();
            Assert.IsNotNull(prefab, "Khong tim thay Player prefab that trong project de test.");
            GameObject player = InstantiatePlayerOrFail(Vector3.zero);
            yield return null;
            yield return null;
            yield return null;
            context.Actual = string.Format("Prefab={0}, vi tri spawn={1}, so loi Console={2}.", prefab.name, FormatVector3(player.transform.position), GetErrorCount());
            AssertNoConsoleErrors("Instantiate Player prefab that phat sinh Error/Exception.");
        }

        private IEnumerator GroundStabilityRoutine(TestRunContext context)
        {
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return null;
            float startY = player.transform.position.y;
            yield return new WaitForSeconds(1f);
            float endY = player.transform.position.y;
            float delta = endY - startY;
            context.Actual = string.Format("Y ban dau={0}, Y sau={1}, do lech Y={2}, thoi gian cho=1.00s.", FormatFloat(startY), FormatFloat(endY), FormatFloat(delta));
            Assert.Greater(endY, -2f, "Player roi xuong duoi map sau khi spawn tren ground collider.");
            Assert.GreaterOrEqual(delta, -2.5f, "Player giam Y bat thuong sau khi spawn tren ground collider.");
        }

        private IEnumerator RunMovementTest(string id, string title, string inputName, Vector2 input, string severity, bool requireMovement)
        {
            yield return RunUnityTest(id, title,
                "Khi co input " + inputName + ", Player di chuyen dung huong hoac thay doi vi tri hop le.",
                severity,
                "1. Spawn Player. 2. Ghi nhan vi tri ban dau. 3. Goi method di chuyen that cua CharacterMovement bang reflection. 4. Cho 1 giay. 5. Kiem tra vi tri Player thay doi.",
                delegate(TestRunContext context)
                {
                    return MovementRoutine(context, inputName, input, requireMovement);
                });
        }

        private IEnumerator MovementRoutine(TestRunContext context, string inputName, Vector2 input, bool requireMovement)
        {
            yield return TestSceneLoader.Load(TestSceneConfig.LobbyScenePath);
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return null;
            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            Vector3 start = player.transform.position;
            InvokeMovement(movement, input, DefaultMoveSpeed);
            yield return new WaitForSeconds(MovementTestDuration);
            Vector3 end = player.transform.position;
            float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(end.x, 0f, end.z));
            context.Actual = string.Format("Input={0}, vi tri dau={1}, vi tri sau={2}, quang duong={3}, thoi gian={4}s.", inputName, FormatVector3(start), FormatVector3(end), FormatFloat(distance), FormatFloat(MovementTestDuration));

            AssertNoConsoleErrors("Di chuyen Player bang input " + inputName + " phat sinh Error/Exception.");
            if (requireMovement)
            {
                Assert.Greater(distance, 0.05f, "Khong tim thay method/input di chuyen phu hop de test " + inputName + " hoac Player khong thay doi vi tri.");
            }
        }

        private IEnumerator IdleMovementRoutine(TestRunContext context)
        {
            yield return TestSceneLoader.Load(TestSceneConfig.LobbyScenePath);
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return null;
            Vector3 start = player.transform.position;
            yield return new WaitForSeconds(1f);
            Vector3 end = player.transform.position;
            float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(end.x, 0f, end.z));
            const float threshold = 0.25f;
            context.Actual = string.Format("Vi tri dau={0}, vi tri sau={1}, do lech={2}, thoi gian cho=1.00s, nguong cho phep={3}.", FormatVector3(start), FormatVector3(end), FormatFloat(distance), FormatFloat(threshold));
            AssertNoConsoleErrors("Player dung yen khong input nhung phat sinh Error/Exception.");
            Assert.LessOrEqual(distance, threshold, "Player tu troi qua xa khi khong co input.");
        }

        private IEnumerator MovementSpeedRoutine(TestRunContext context)
        {
            yield return TestSceneLoader.Load(TestSceneConfig.LobbyScenePath);
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return null;
            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            Vector3 start = player.transform.position;
            InvokeMovement(movement, new Vector2(0f, 1f), DefaultMoveSpeed);
            yield return new WaitForSeconds(MovementTestDuration);
            Vector3 end = player.transform.position;
            float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(end.x, 0f, end.z));
            float speed = distance / MovementTestDuration;
            context.Actual = string.Format("Thoi gian test={0}s, quang duong={1}, toc do tinh duoc={2}, nguong kiem tra=(0.05, 20.00).", FormatFloat(MovementTestDuration), FormatFloat(distance), FormatFloat(speed));
            AssertNoConsoleErrors("Kiem tra toc do di chuyen Player phat sinh Error/Exception.");
            Assert.Greater(speed, 0.05f, "Toc do Player bang 0 hoac qua thap khi goi di chuyen tien.");
            Assert.LessOrEqual(speed, 20f, "Toc do Player qua cao bat thuong trong thoi gian test.");
        }

        private IEnumerator JumpRoutine(TestRunContext context, bool waitForLanding)
        {
            yield return TestSceneLoader.Load(TestSceneConfig.LobbyScenePath);
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            float startY = player.transform.position.y;
            string method = InvokeJump(player);
            float maxY = startY;
            float duration = waitForLanding ? 2.0f : 0.6f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                maxY = Mathf.Max(maxY, player.transform.position.y);
                elapsed += Time.deltaTime;
                yield return null;
            }

            float endY = player.transform.position.y;
            context.Actual = string.Format("Method Jump={0}, Y ban dau={1}, Y cao nhat={2}, Y cuoi={3}, thoi gian test={4}s.", method, FormatFloat(startY), FormatFloat(maxY), FormatFloat(endY), FormatFloat(duration));
            AssertNoConsoleErrors("Goi Jump tren Player phat sinh Error/Exception.");
            Assert.Greater(maxY, startY + 0.05f, "Player khong tang Y sau khi goi Jump.");
            if (waitForLanding)
            {
                Assert.LessOrEqual(endY, startY + 1.5f, "Player bi treo tren khong sau khi Jump.");
            }
        }

        private IEnumerator DodgeRoutine(TestRunContext context, bool withObstacle)
        {
            yield return TestSceneLoader.Load(TestSceneConfig.LobbyScenePath);
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            if (withObstacle)
            {
                CreateObstacle(new Vector3(0f, 1f, 2.2f));
            }

            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            Vector3 start = player.transform.position;
            string method = InvokeDodge(player, new Vector2(0f, 1f));
            yield return new WaitForSeconds(0.6f);
            Vector3 end = player.transform.position;
            float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(end.x, 0f, end.z));

            if (withObstacle)
            {
                float remaining = 2.2f - end.z;
                context.Actual = string.Format("Method Dodge={0}, vi tri truoc Dodge={1}, vi tri sau Dodge={2}, vi tri vat can=(0.00,1.00,2.20), khoang cach con lai theo Z={3}.", method, FormatVector3(start), FormatVector3(end), FormatFloat(remaining));
                Assert.LessOrEqual(end.z, 2.25f, "Player Dodge vuot qua phia ben kia vat can.");
            }
            else
            {
                context.Actual = string.Format("Method Dodge={0}, vi tri dau={1}, vi tri sau={2}, quang duong Dodge={3}.", method, FormatVector3(start), FormatVector3(end), FormatFloat(distance));
                Assert.Greater(distance, 0.05f, "Player khong thay doi vi tri sau khi goi Dodge.");
            }

            AssertNoConsoleErrors("Goi Dodge tren Player phat sinh Error/Exception.");
        }

        private IEnumerator RunActionNoConsoleErrorTest(string id, string title, string expected, string severity, Action<GameObject, TestRunContext> action)
        {
            yield return RunUnityTest(id, title, expected, severity,
                "1. Spawn Player. 2. Goi logic that bang reflection neu co. 3. Cho vai frame. 4. Kiem tra khong co Error/Exception.",
                delegate(TestRunContext context)
                {
                    return ActionNoConsoleErrorRoutine(context, action);
                });
        }

        private IEnumerator ActionNoConsoleErrorRoutine(TestRunContext context, Action<GameObject, TestRunContext> action)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            action(player, context);
            yield return new WaitForSeconds(0.5f);
            context.Actual = context.Actual + " So loi Error/Exception=" + GetErrorCount() + ".";
            AssertNoConsoleErrors("Logic Player phat sinh Error/Exception.");
        }

        private IEnumerator LockTargetRoutine(TestRunContext context, bool spawnEnemy, bool moveWhileLocked, bool checkRotation)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            GameObject enemy = null;
            if (spawnEnemy)
            {
                enemy = InstantiateEnemyOrFail(new Vector3(0f, 1.2f, 5f));
            }

            yield return new WaitForSeconds(0.2f);
            Quaternion startRotation = player.transform.rotation;
            Vector3 start = player.transform.position;
            string method = InvokeLockTarget(player);
            yield return new WaitForSeconds(0.2f);

            if (moveWhileLocked)
            {
                Component movement = FindRequiredComponent(player, "CharacterMovement", true);
                InvokeMovement(movement, new Vector2(0f, 1f), DefaultMoveSpeed);
                yield return new WaitForSeconds(0.7f);
            }
            else
            {
                yield return new WaitForSeconds(0.4f);
            }

            object target = ReadTarget(player);
            Vector3 end = player.transform.position;
            float distance = enemy != null ? Vector3.Distance(player.transform.position, enemy.transform.position) : 0f;
            float moved = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(end.x, 0f, end.z));
            float angle = enemy != null ? Vector3.Angle(player.transform.forward, (enemy.transform.position - player.transform.position).normalized) : 0f;
            context.Actual = string.Format("Method lock={0}, Enemy={1}, khoang cach Player-Enemy={2}, target sau lock={3}, vi tri dau={4}, vi tri sau={5}, quang duong={6}, rotation dau={7}, rotation sau={8}, goc lech toi Enemy={9}.",
                method,
                enemy != null ? enemy.name : "Khong spawn",
                FormatFloat(distance),
                target != null ? target.ToString() : "Null/khong doc duoc",
                FormatVector3(start),
                FormatVector3(end),
                FormatFloat(moved),
                startRotation.eulerAngles,
                player.transform.rotation.eulerAngles,
                FormatFloat(angle));

            AssertNoConsoleErrors("Lock Target tren Player phat sinh Error/Exception.");
            if (spawnEnemy)
            {
                Assert.IsNotNull(enemy, "Khong tim thay Enemy prefab that de test lock target.");
            }

            if (moveWhileLocked)
            {
                Assert.Greater(moved, 0.05f, "Player khong di chuyen khi dang lock target.");
            }

            if (checkRotation && enemy != null)
            {
                Assert.LessOrEqual(angle, 135f, "Huong nhin Player lech qua lon so voi Enemy sau khi lock target.");
            }
        }

        private IEnumerator RunSummaryRoutine(string id, string title, string expected, string severity, int enemyCount)
        {
            yield return RunUnityTest(id, title, expected, severity,
                "1. Spawn Player. 2. Spawn Enemy that neu can. 3. Goi logic co ban. 4. Cho vai frame. 5. Kiem tra khong co Error/Exception.",
                delegate(TestRunContext context)
                {
                    return SummaryRoutine(context, enemyCount);
                });
        }

        private IEnumerator SummaryRoutine(TestRunContext context, int enemyCount)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            int spawnedEnemyCount = 0;
            for (int i = 0; i < enemyCount; i++)
            {
                GameObject enemy = InstantiateEnemyOrFail(new Vector3(-2f + i * 2f, 1.2f, 5f));
                if (enemy != null)
                {
                    spawnedEnemyCount++;
                }
            }

            yield return new WaitForSeconds(0.2f);
            List<string> called = new List<string>();
            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            InvokeMovement(movement, new Vector2(0f, 1f), DefaultMoveSpeed);
            called.Add("W");
            InvokeMovement(movement, new Vector2(-1f, 0f), DefaultMoveSpeed);
            called.Add("A");
            InvokeMovement(movement, new Vector2(0f, -1f), DefaultMoveSpeed);
            called.Add("S");
            InvokeMovement(movement, new Vector2(1f, 0f), DefaultMoveSpeed);
            called.Add("D");
            called.Add(InvokeJump(player));
            called.Add(InvokeDodge(player, new Vector2(0f, 1f)));
            called.Add(InvokeAttack(player));
            called.Add(InvokeSkill1(player));
            called.Add(InvokeLockTarget(player));
            yield return new WaitForSeconds(0.8f);
            context.Actual = string.Format("Cac thao tac da goi: {0}. So Enemy spawn={1}. So loi Error/Exception={2}.", string.Join(", ", called.ToArray()), spawnedEnemyCount, GetErrorCount());
            AssertNoConsoleErrors("Tong hop logic Player phat sinh Error/Exception.");
        }

        private IEnumerator StateControllerRoutine(TestRunContext context, bool requireCurrentState)
        {
            GameObject player = InstantiatePlayerOrFail();
            yield return null;
            yield return null;

            Component characterBase = FindRequiredComponent(player, "CharacterBase", true);
            object stateController = null;
            bool hasStateController = TestReflectionHelper.TryGetValue(characterBase, "StateController", out stateController) && stateController != null;
            object currentState = null;
            bool hasCurrentState = hasStateController && TestReflectionHelper.TryGetValue(stateController, "currentState", out currentState) && currentState != null;

            context.Actual = string.Format("StateController={0}, currentState={1}.",
                hasStateController ? stateController.GetType().Name : "Null/khong doc duoc",
                hasCurrentState ? currentState.GetType().Name : "Null/khong doc duoc");

            Assert.IsTrue(hasStateController, "StateController cua Player chua duoc khoi tao sau Start.");
            if (requireCurrentState)
            {
                Assert.IsTrue(hasCurrentState, "currentState cua Player bi null sau Start.");
            }
        }

        private IEnumerator DirectionChangeRoutine(TestRunContext context)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            Vector3 start = player.transform.position;
            Vector2[] directions = { new Vector2(0f, 1f), new Vector2(-1f, 0f), new Vector2(0f, -1f), new Vector2(1f, 0f), new Vector2(1f, 1f) };

            for (int i = 0; i < directions.Length; i++)
            {
                InvokeMovement(movement, directions[i], DefaultMoveSpeed);
                yield return new WaitForSeconds(0.2f);
            }

            Vector3 end = player.transform.position;
            float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(end.x, 0f, end.z));
            context.Actual = string.Format("Vi tri dau={0}, vi tri cuoi={1}, quang duong tong={2}, so lan doi huong={3}, loi Console={4}.",
                FormatVector3(start), FormatVector3(end), FormatFloat(distance), directions.Length, GetErrorCount());
            AssertFinite(end, "Position Player khong hop le sau khi doi huong lien tuc.");
            AssertNoConsoleErrors("Doi huong movement lien tuc phat sinh Error/Exception.");
        }

        private IEnumerator MoveDirectionFiniteRoutine(TestRunContext context)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            TestReflectionHelper.TryInvokeMethod(movement, "SetMoveDirection", new Vector2(1f, 1f));
            InvokeMovement(movement, new Vector2(1f, 1f), DefaultMoveSpeed);
            yield return null;

            object moveDirectionValue = null;
            Assert.IsTrue(TestReflectionHelper.TryGetValue(movement, "MoveDirection", out moveDirectionValue), "Khong doc duoc MoveDirection tren CharacterMovement.");
            Vector2 moveDirection = (Vector2)moveDirectionValue;
            context.Actual = string.Format("MoveDirection=({0},{1}), loi Console={2}.", FormatFloat(moveDirection.x), FormatFloat(moveDirection.y), GetErrorCount());
            AssertFinite(new Vector3(moveDirection.x, 0f, moveDirection.y), "MoveDirection cua Player khong hop le.");
            AssertNoConsoleErrors("SetMoveDirection/Run phat sinh Error/Exception.");
        }

        private IEnumerator RunCollisionStabilityRoutine(string id, string title, bool checkTeleport)
        {
            yield return RunUnityTest(id, title,
                checkTeleport ? "Player khong teleport bat thuong sau va cham vat can." : "Scale va rotation Player on dinh sau va cham vat can.",
                "Medium",
                "1. Tao ground va obstacle. 2. Spawn Player. 3. Cho Player di chuyen ve vat can. 4. Kiem tra vi tri/scale/rotation hop le.",
                delegate(TestRunContext context)
                {
                    return CollisionStabilityRoutine(context, checkTeleport);
                });
        }

        private IEnumerator CollisionStabilityRoutine(TestRunContext context, bool checkTeleport)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            CreateObstacle(new Vector3(0f, 1f, 2.2f));
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);

            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            Vector3 start = player.transform.position;
            Vector3 startScale = player.transform.localScale;
            Quaternion startRotation = player.transform.rotation;
            InvokeMovement(movement, new Vector2(0f, 1f), DefaultMoveSpeed);
            yield return new WaitForSeconds(1f);
            Vector3 end = player.transform.position;
            float distance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(end.x, 0f, end.z));
            float scaleDelta = Vector3.Distance(startScale, player.transform.localScale);
            float rotationDelta = Quaternion.Angle(startRotation, player.transform.rotation);
            float remaining = 2.2f - end.z;

            context.Actual = string.Format("Vi tri dau={0}, vi tri cuoi={1}, quang duong={2}, vi tri vat can=(0.00,1.00,2.20), khoang cach con lai Z={3}, scaleDelta={4}, rotationDelta={5}, co xuyen vat can={6}.",
                FormatVector3(start), FormatVector3(end), FormatFloat(distance), FormatFloat(remaining), FormatFloat(scaleDelta), FormatFloat(rotationDelta), end.z > 2.25f ? "Co" : "Khong");

            AssertNoConsoleErrors("Movement va cham vat can phat sinh Error/Exception.");
            AssertFinite(end, "Position Player khong hop le sau va cham.");
            if (checkTeleport)
            {
                Assert.LessOrEqual(distance, 12f, "Player teleport bat thuong sau va cham vat can.");
            }
            else
            {
                Assert.LessOrEqual(scaleDelta, 0.01f, "Scale Player thay doi bat thuong sau va cham.");
                Assert.LessOrEqual(rotationDelta, 90f, "Rotation Player thay doi qua lon sau va cham.");
            }
        }

        private IEnumerator RunFiniteAfterActionRoutine(string id, string title, string actionName, string severity, Action<GameObject> action)
        {
            yield return RunUnityTest(id, title,
                actionName + " khong lam position Player bi NaN hoac Infinity.", severity,
                "1. Spawn Player. 2. Goi action that. 3. Cho vai frame. 4. Kiem tra position hop le va khong co Error/Exception.",
                delegate(TestRunContext context)
                {
                    return FiniteAfterActionRoutine(context, actionName, action);
                });
        }

        private IEnumerator FiniteAfterActionRoutine(TestRunContext context, string actionName, Action<GameObject> action)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            Vector3 start = player.transform.position;
            action(player);
            yield return new WaitForSeconds(0.5f);
            Vector3 end = player.transform.position;
            context.Actual = string.Format("Action={0}, vi tri dau={1}, vi tri sau={2}, loi Console={3}.", actionName, FormatVector3(start), FormatVector3(end), GetErrorCount());
            AssertFinite(end, "Position Player bi NaN hoac Infinity sau " + actionName + ".");
            AssertNoConsoleErrors(actionName + " phat sinh Error/Exception.");
        }

        private IEnumerator RunActionWithEnemyNoConsoleErrorTest(string id, string title, string expected, string severity, Action<GameObject, TestRunContext> action, int enemyCount)
        {
            yield return RunUnityTest(id, title, expected, severity,
                "1. Spawn Player. 2. Spawn Enemy prefab that. 3. Goi logic that bang reflection. 4. Cho vai frame. 5. Kiem tra khong co Error/Exception.",
                delegate(TestRunContext context)
                {
                    return ActionWithEnemyNoConsoleErrorRoutine(context, action, enemyCount);
                });
        }

        private IEnumerator ActionWithEnemyNoConsoleErrorRoutine(TestRunContext context, Action<GameObject, TestRunContext> action, int enemyCount)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            for (int i = 0; i < enemyCount; i++)
            {
                InstantiateEnemyOrFail(new Vector3(-2f + i * 2f, 1.2f, 5f));
            }

            yield return new WaitForSeconds(0.2f);
            action(player, context);
            yield return new WaitForSeconds(0.5f);
            context.Actual = context.Actual + " So Enemy spawn=" + enemyCount + ", so loi Error/Exception=" + GetErrorCount() + ".";
            AssertNoConsoleErrors("Logic Player voi Enemy phat sinh Error/Exception.");
        }

        private IEnumerator DisabledEnemyLockRoutine(TestRunContext context)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            GameObject enemy = InstantiateEnemyOrFail(new Vector3(0f, 1.2f, 5f));
            yield return new WaitForSeconds(0.2f);
            string method1 = InvokeLockTarget(player);
            object targetBeforeDisable = ReadTarget(player);
            enemy.SetActive(false);
            yield return null;
            string method2 = InvokeLockTarget(player);
            object targetAfterDisable = ReadTarget(player);
            context.Actual = string.Format("Enemy={0}, method lock truoc/sau disable={1}/{2}, target truoc disable={3}, target sau disable={4}, loi Console={5}.",
                enemy.name, method1, method2, targetBeforeDisable ?? "Null", targetAfterDisable ?? "Null", GetErrorCount());
            AssertNoConsoleErrors("Enemy bi disable trong luc lock target phat sinh Error/Exception.");
        }

        private IEnumerator DodgeThenMoveRoutine(TestRunContext context)
        {
            StartLogWatcher();
            CreateMainCameraIfNeeded();
            CreateGround();
            GameObject player = InstantiatePlayerOrFail(new Vector3(0f, 1.2f, 0f));
            yield return new WaitForSeconds(0.2f);
            Vector3 start = player.transform.position;
            string dodgeMethod = InvokeDodge(player, new Vector2(0f, 1f));
            yield return new WaitForSeconds(0.7f);
            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            Vector3 afterDodge = player.transform.position;
            InvokeMovement(movement, new Vector2(0f, 1f), DefaultMoveSpeed);
            yield return new WaitForSeconds(0.7f);
            Vector3 end = player.transform.position;
            float movedAfterDodge = Vector3.Distance(new Vector3(afterDodge.x, 0f, afterDodge.z), new Vector3(end.x, 0f, end.z));
            context.Actual = string.Format("Method Dodge={0}, vi tri dau={1}, sau Dodge={2}, sau movement={3}, quang duong sau Dodge={4}, loi Console={5}.",
                dodgeMethod, FormatVector3(start), FormatVector3(afterDodge), FormatVector3(end), FormatFloat(movedAfterDodge), GetErrorCount());
            AssertNoConsoleErrors("Dodge roi di chuyen phat sinh Error/Exception.");
            Assert.Greater(movedAfterDodge, 0.05f, "Sau Dodge, Player khong tiep tuc di chuyen duoc.");
        }

        private IEnumerator RunHealthDataUnityTest(string id, string title, bool requirePositive)
        {
            yield return RunUnityTest(id, title,
                requirePositive ? "HP ban dau cua Player lon hon 0 neu doc duoc." : "Player co HP/Health that neu project ho tro.",
                "High",
                "1. Instantiate Player. 2. Doc CharacterData stats hoac field HP/Health bang reflection. 3. Fail ro neu khong co he thong HP Player.",
                delegate(TestRunContext context) { return HealthDataRoutine(context, requirePositive); });
        }

        private IEnumerator HealthDataRoutine(TestRunContext context, bool requirePositive)
        {
            GameObject player = InstantiatePlayerOrFail();
            yield return null;
            Component characterBase = FindRequiredComponent(player, "CharacterBase", true);
            object characterData = null;
            object stats = null;
            float health = -1f;
            bool hasHealth = TestReflectionHelper.TryGetValue(characterBase, "CharacterData", out characterData) &&
                             characterData != null &&
                             TestReflectionHelper.TryGetValue(characterData, "stats", out stats) &&
                             stats != null &&
                             TestReflectionHelper.TryGetValue<float>(stats, "health", out health);

            context.Actual = hasHealth ? "Doc duoc CharacterData.stats.health=" + FormatFloat(health) + "." : "Khong doc duoc HP/Health tren Player.";
            Assert.IsTrue(hasHealth, "Khong tim thay HP/Health that tren Player runtime trong RunGame.");
            if (requirePositive)
            {
                Assert.Greater(health, 0f, "HP ban dau cua Player phai lon hon 0.");
            }
        }

        private void RunHealthDataTest(string id, string title, bool requirePositive)
        {
            RunTest(id, title,
                requirePositive ? "HP ban dau cua Player lon hon 0 neu doc duoc." : "Player co HP/Health that neu project ho tro.",
                "High",
                "1. Instantiate Player. 2. Doc CharacterData stats hoac field HP/Health bang reflection. 3. Fail ro neu khong co he thong HP Player.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Component characterBase = FindRequiredComponent(player, "CharacterBase", true);
                    object characterData = null;
                    object stats = null;
                    float health = -1f;
                    bool hasHealth = TestReflectionHelper.TryGetValue(characterBase, "CharacterData", out characterData) &&
                                     characterData != null &&
                                     TestReflectionHelper.TryGetValue(characterData, "stats", out stats) &&
                                     stats != null &&
                                     TestReflectionHelper.TryGetValue<float>(stats, "health", out health);

                    context.Actual = hasHealth ? "Doc duoc CharacterData.stats.health=" + FormatFloat(health) + "." : "Khong doc duoc HP/Health tren Player.";
                    Assert.IsTrue(hasHealth, "Khong tim thay HP/Health that tren Player prefab de test.");
                    if (requirePositive)
                    {
                        Assert.Greater(health, 0f, "HP ban dau cua Player phai lon hon 0.");
                    }
                });
        }

        private void RunComponentPresenceTest(string id, string title, string componentName, string expected, string severity)
        {
            RunTest(id, title, expected, severity,
                "1. Instantiate Player. 2. Tim component " + componentName + ". 3. Kiem tra component khac null.",
                delegate(TestRunContext context)
                {
                    GameObject player = InstantiatePlayerOrFail();
                    Component component = FindRequiredComponent(player, componentName, false);
                    context.Actual = component != null
                        ? "DAT - Tim thay component " + component.GetType().Name + " tren Player prefab that."
                        : "KHONG DAT - Khong tim thay component " + componentName + " tren Player prefab that.";
                    Assert.IsNotNull(component, "Khong tim thay component " + componentName + " tren Player prefab that.");
                });
        }

        private void RunTest(string id, string title, string expected, string severity, string steps, Action<TestRunContext> body)
        {
            TestRunContext context = new TestRunContext();
            try
            {
                body(context);
                RecordPass(id, title, expected, context.Actual, steps);
            }
            catch (Exception exception)
            {
                RecordFail(id, title, expected, BuildFailActual(context, exception), severity, steps);
                throw;
            }
        }

        private IEnumerator RunUnityTest(string id, string title, string expected, string severity, string steps, Func<TestRunContext, IEnumerator> body)
        {
            TestRunContext context = new TestRunContext();
            IEnumerator routine = null;
            Exception failure = null;

            try
            {
                routine = RunAfterSceneLoad(context, body);
            }
            catch (Exception exception)
            {
                failure = exception;
            }

            while (failure == null)
            {
                object current = null;
                bool hasNext = false;
                try
                {
                    hasNext = routine != null && routine.MoveNext();
                    if (hasNext)
                    {
                        current = routine.Current;
                    }
                }
                catch (Exception exception)
                {
                    failure = exception;
                }

                if (failure != null || !hasNext)
                {
                    break;
                }

                yield return current;
            }

            if (failure == null)
            {
                RecordPass(id, title, expected, context.Actual, steps);
            }
            else
            {
                RecordFail(id, title, expected, BuildFailActual(context, failure), severity, steps);
                throw failure;
            }
        }

        private IEnumerator RunAfterSceneLoad(TestRunContext context, Func<TestRunContext, IEnumerator> body)
        {
            yield return TestSceneLoader.Load(TestSceneConfig.RunScenePath);
            IEnumerator routine = body(context);
            while (routine != null && routine.MoveNext()) yield return routine.Current;
        }

        private GameObject InstantiatePlayerOrFail()
        {
            return InstantiatePlayerOrFail(Vector3.zero);
        }

        private GameObject InstantiatePlayerOrFail(Vector3 position)
        {
            GameObject prefab = TestPrefabFinder.FindPlayerPrefab();
            Assert.IsNotNull(prefab, "Khong tim thay Player prefab that trong project de test.");
            GameObject player = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            player.name = prefab.name + "_TestInstance";
            spawnedObjects.Add(player);
            return player;
        }

        private GameObject InstantiateEnemyOrFail(Vector3 position)
        {
            GameObject prefab = TestPrefabFinder.FindEnemyPrefab();
            Assert.IsNotNull(prefab, "Khong tim thay Enemy prefab that trong project de test Player lock/combat.");
            GameObject enemy = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            enemy.name = prefab.name + "_TestInstance";
            spawnedObjects.Add(enemy);
            return enemy;
        }

        private Component FindRequiredComponent(GameObject root, string componentName, bool failIfMissing)
        {
            Component component = TestReflectionHelper.FindComponentByClassName(root, componentName);
            if (failIfMissing)
            {
                Assert.IsNotNull(component, "Khong tim thay component " + componentName + " tren Player prefab that.");
            }

            return component;
        }

        private void InvokeMovement(Component movement, Vector2 input, float speed)
        {
            if (movement == null)
            {
                Assert.Fail("Khong tim thay component CharacterMovement tren Player prefab that.");
            }

            bool invoked = TestReflectionHelper.TryInvokeMethod(movement, "Run", input, speed);
            if (!invoked)
            {
                invoked = TestReflectionHelper.TryInvokeMethod(movement, "Walk", input, speed);
            }

            if (!invoked)
            {
                invoked = TestReflectionHelper.TryInvokeMethod(movement, "Sprint", input, speed);
            }

            if (!invoked)
            {
                Assert.Fail("Khong tim thay method/input di chuyen phu hop de test W/A/S/D.");
            }
        }

        private string InvokeJump(GameObject player)
        {
            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            if (TestReflectionHelper.TrySetValue(movement, "IsGrounded", true) &&
                TestReflectionHelper.TryInvokeMethod(movement, "Jump"))
            {
                return "CharacterMovement.Jump";
            }

            Assert.Fail("Khong tim thay method Jump phu hop tren CharacterMovement.");
            return "Khong tim thay";
        }

        private string InvokeDodge(GameObject player, Vector2 direction)
        {
            Component movement = FindRequiredComponent(player, "CharacterMovement", true);
            if (TestReflectionHelper.TryInvokeMethod(movement, "Dodge", direction, DefaultMoveSpeed))
            {
                return "CharacterMovement.Dodge";
            }

            Assert.Fail("Khong tim thay method Dodge/Roll/Dash phu hop tren Player prefab that.");
            return "Khong tim thay";
        }

        private string InvokeAttack(GameObject player)
        {
            Component combat = FindRequiredComponent(player, "CharacterCombat", false);
            if (combat != null && TestReflectionHelper.TryInvokeMethod(combat, "TryAttack"))
            {
                return "CharacterCombat.TryAttack";
            }

            Component character = FindRequiredComponent(player, "CharacterBase", false);
            if (character != null && TestReflectionHelper.TryInvokeMethod(character, "OnAttack"))
            {
                return "CharacterBase.OnAttack";
            }

            Assert.Fail("Khong tim thay method Attack phu hop tren CharacterCombat/CharacterMelee.");
            return "Khong tim thay";
        }

        private string InvokeSkill1(GameObject player)
        {
            Component skill = FindRequiredComponent(player, "CharacterSkill", false);
            if (skill != null && TestReflectionHelper.TryInvokeMethod(skill, "UseSkill1"))
            {
                return "CharacterSkill.UseSkill1";
            }

            Component character = FindRequiredComponent(player, "CharacterBase", false);
            if (character != null && TestReflectionHelper.TryInvokeMethod(character, "OnSkill_1"))
            {
                return "CharacterBase.OnSkill_1";
            }

            Assert.Fail("Khong tim thay method kich hoat Skill 1 phu hop tren Player prefab that.");
            return "Khong tim thay";
        }

        private string InvokeLockTarget(GameObject player)
        {
            Component lockTarget = FindRequiredComponent(player, "CharacterLockTarget", true);
            if (TestReflectionHelper.TryInvokeMethod(lockTarget, "ToggleLockTarget"))
            {
                return "CharacterLockTarget.ToggleLockTarget";
            }

            Assert.Fail("Khong tim thay method lock target phu hop tren Player prefab that.");
            return "Khong tim thay";
        }

        private object ReadTarget(GameObject player)
        {
            Component lockTarget = FindRequiredComponent(player, "CharacterLockTarget", false);
            if (lockTarget == null)
            {
                return null;
            }

            object value = null;
            if (TestReflectionHelper.TryGetValue(lockTarget, "Target", out value))
            {
                return value;
            }

            if (TestReflectionHelper.TryGetValue(lockTarget, "lookAtTarget", out value))
            {
                return value;
            }

            return null;
        }

        private void StartLogWatcher()
        {
            if (logWatcher != null)
            {
                logWatcher.Stop();
            }

            logWatcher = new TestLogWatcher();
            logWatcher.Start();
        }

        private void AssertNoConsoleErrors(string message)
        {
            if (logWatcher == null)
            {
                return;
            }

            Assert.IsFalse(logWatcher.HasErrorOrException, message + " Loi: " + string.Join(" | ", CopyErrors().ToArray()));
        }

        private int GetErrorCount()
        {
            if (logWatcher == null)
            {
                return 0;
            }

            return logWatcher.GetErrors().Count;
        }

        private List<string> CopyErrors()
        {
            List<string> errors = new List<string>();
            if (logWatcher == null)
            {
                return errors;
            }

            IReadOnlyList<string> source = logWatcher.GetErrors();
            for (int i = 0; i < source.Count; i++)
            {
                errors.Add(source[i]);
            }

            return errors;
        }

        private void Cleanup()
        {
            if (logWatcher != null)
            {
                logWatcher.Stop();
                logWatcher = null;
            }

            for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object obj = spawnedObjects[i];
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }

            spawnedObjects.Clear();
        }

        private void CreateMainCameraIfNeeded()
        {
            if (Camera.main != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("Test_MainCamera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 6f, -8f);
            camera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
            spawnedObjects.Add(cameraObject);
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Test_Ground";
            ground.transform.position = new Vector3(0f, -0.55f, 0f);
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            spawnedObjects.Add(ground);
        }

        private void CreateObstacle(Vector3 position)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Test_Obstacle";
            obstacle.transform.position = position;
            obstacle.transform.localScale = new Vector3(3f, 2f, 0.5f);
            spawnedObjects.Add(obstacle);
        }

        private Transform FindChildContains(Transform root, string token)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return children[i];
                }
            }

            return null;
        }

        private float ReadFloatOrFail(object target, string memberName)
        {
            float value = 0f;
            Assert.IsTrue(TestReflectionHelper.TryGetValue<float>(target, memberName, out value), "Khong doc duoc gia tri " + memberName + " bang reflection.");
            Assert.IsFalse(float.IsNaN(value), memberName + " bi NaN.");
            Assert.IsFalse(float.IsInfinity(value), memberName + " bi Infinity.");
            return value;
        }

        private void AssertFinite(Vector3 value, string message)
        {
            Assert.IsFalse(float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z), message + " Gia tri bi NaN: " + FormatVector3(value));
            Assert.IsFalse(float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z), message + " Gia tri bi Infinity: " + FormatVector3(value));
        }

        private int CountEnabledRenderers(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            int count = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].enabled)
                {
                    count++;
                }
            }

            return count;
        }

        private bool HasMemberNamedLike(Component[] components, string[] tokens)
        {
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                Type type = component.GetType();
                for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
                {
                    string token = tokens[tokenIndex];
                    if (type.Name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        HasMemberNamedLike(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), token) ||
                        HasMemberNamedLike(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), token) ||
                        HasMemberNamedLike(type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), token))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasMemberNamedLike(MemberInfo[] members, string token)
        {
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].Name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private string FormatVector3(Vector3 value)
        {
            return string.Format("({0},{1},{2})", FormatFloat(value.x), FormatFloat(value.y), FormatFloat(value.z));
        }

        private string FormatFloat(float value)
        {
            return value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }

        private string BuildFailActual(TestRunContext context, Exception exception)
        {
            string prefix = string.IsNullOrEmpty(context.Actual) ? string.Empty : context.Actual + " ";
            return prefix + "KHONG DAT - " + exception.Message;
        }

        private void RecordPass(string id, string title, string expected, string actual, string steps)
        {
            records.Add(new TestResultRecord
            {
                MaTC = id,
                TieuDeTestcase = title,
                KetQuaMongDoi = expected,
                KetQuaThucTe = string.IsNullOrEmpty(actual) ? "DAT - Test hoan thanh dung dieu kien kiem tra." : "DAT - " + actual,
                TinhTrangThucThi = "Pass",
                MucDoNghiemTrongCuaLoi = string.Empty,
                KieuChay = RunMode,
                NguoiKiemThu = TesterName,
                NgayBatDau = StartDate,
                ChiTietBuocKiemThu = steps,
                GhiChu = string.Empty
            });
        }

        private void RecordFail(string id, string title, string expected, string actual, string severity, string steps)
        {
            records.Add(new TestResultRecord
            {
                MaTC = id,
                TieuDeTestcase = title,
                KetQuaMongDoi = expected,
                KetQuaThucTe = actual,
                TinhTrangThucThi = "Fail",
                MucDoNghiemTrongCuaLoi = severity,
                KieuChay = RunMode,
                NguoiKiemThu = TesterName,
                NgayBatDau = StartDate,
                ChiTietBuocKiemThu = steps,
                GhiChu = string.Empty
            });
        }

        private class TestRunContext
        {
            public string Actual;
        }
    }
}


