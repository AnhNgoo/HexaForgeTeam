using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestReport]
[Category("Integration")]
[Category("Character")]
public class PlayerCombatComboTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("PlayerCombatCombo");
        yield return _world.BuildDefaultWorld();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_world != null)
            yield return _world.DisposeWorld();
    }

    [UnityTest]
    [Category("P1")]
    [Description("TC PL-INT-004: Kiểm tra khởi tạo combo cận chiến và combo đấm của Kael (đủ bước, đúng type bước 1).")]
    [TestCaseMeta(
        id: "PL-INT-004",
        title: "Combo init tạo đủ melee/punch combos",
        expected: "weaponCombos và punchCombos có 4 phần tử và đúng type đầu tiên.",
        steps: "1) Lấy CharacterCombat. 2) Đọc private fields weaponCombos/punchCombos. 3) Verify length/type.")]
    public IEnumerator Combo_Init_SetsMeleeCombosAndPunchFallback()
    {
        var kael = _world.Kael;

        var characterCombat = PlayerTestUtils.GetProperty(kael, "CharacterCombat", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(characterCombat);

        var weaponCombos = (Array)PlayerTestUtils.GetField<object>(characterCombat, "weaponCombos", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(weaponCombos);
        Assert.AreEqual(4, weaponCombos.Length);
        Assert.AreEqual("AttackMeleeStep_1", weaponCombos.GetValue(0)!.GetType().Name);

        var punchCombos = (Array)PlayerTestUtils.GetField<object>(characterCombat, "punchCombos", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(punchCombos);
        Assert.AreEqual(4, punchCombos.Length);
        Assert.AreEqual("PunchStep_1", punchCombos.GetValue(0)!.GetType().Name);

        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-001: [Mẫu] Kiểm tra Kael tồn tại trong scene và đang active.")]
    [TestCaseMeta(
        id: "PL-SAMPLE-001",
        title: "Kael tồn tại và active trong scene",
        expected: "Tìm thấy Kael GameObject và Kael component.",
        steps: "1) Load GameDemo. 2) Find Kael. 3) Assert active + component không null.")]
    public IEnumerator Mau_01_Kael_Exists_AndActive()
    {
        Assert.IsNotNull(_world.PlayerGo);
        Assert.IsTrue(_world.PlayerGo.activeInHierarchy, "Kael phải active trong scene để test gameplay.");
        Assert.IsNotNull(_world.Kael);
        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-002: [Mẫu] Kiểm tra Main Camera tồn tại (Camera.main != null).")]
    [TestCaseMeta(
        id: "PL-SAMPLE-002",
        title: "Main Camera tồn tại",
        expected: "Camera.main != null.",
        steps: "1) Load scene. 2) Đọc Camera.main. 3) Assert không null.")]
    public IEnumerator Mau_02_MainCamera_Exists()
    {
        Assert.IsNotNull(Camera.main, "Scene phải có MainCamera tag để gameplay hoạt động đúng.");
        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-003: [Mẫu] Kiểm tra EventManager.Instance đã được khởi tạo.")]
    [TestCaseMeta(
        id: "PL-SAMPLE-003",
        title: "EventManager.Instance khác null",
        expected: "Instance != null.",
        steps: "1) Find type EventManager. 2) Lấy field Instance. 3) Assert != null.")]
    public IEnumerator Mau_03_EventManager_Instance_NotNull()
    {
        var eventManagerType = PlayerTestUtils.FindType("EventManager");
        Assert.IsNotNull(eventManagerType);

        var instanceField = eventManagerType!.BaseType != null
            ? eventManagerType.BaseType.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
            : eventManagerType.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

        Assert.IsNotNull(instanceField, "Không tìm thấy singleton Instance trên EventManager.");
        var instance = instanceField!.GetValue(null);
        Assert.IsNotNull(instance, "EventManager.Instance chưa init (Awake chưa chạy hoặc object thiếu).");

        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-004: [Mẫu] Kiểm tra các module cốt lõi của Kael (Movement/Combat/Camera) có tồn tại.")]
    [TestCaseMeta(
        id: "PL-SAMPLE-004",
        title: "Kael có đủ module core",
        expected: "CharacterMovement/CharacterCombat/CharacterCamera không null.",
        steps: "1) Get properties từ Kael. 2) Assert != null.")]
    public IEnumerator Mau_04_Kael_HasCoreModules()
    {
        var kael = _world.Kael;
        Assert.IsNotNull(PlayerTestUtils.GetProperty(kael, "CharacterMovement", BindingFlags.Instance | BindingFlags.Public));
        Assert.IsNotNull(PlayerTestUtils.GetProperty(kael, "CharacterCombat", BindingFlags.Instance | BindingFlags.Public));
        Assert.IsNotNull(PlayerTestUtils.GetProperty(kael, "CharacterCamera", BindingFlags.Instance | BindingFlags.Public));
        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-005: [Mẫu] weaponCombos không rỗng và mọi phần tử != null.")]
    [TestCaseMeta(
        id: "PL-SAMPLE-005",
        title: "weaponCombos không rỗng",
        expected: "weaponCombos != null, Length > 0, element != null.",
        steps: "1) Get CharacterCombat. 2) Get field weaponCombos. 3) Assert.")]
    public IEnumerator Mau_05_WeaponCombos_NotEmpty_ElementsNotNull()
    {
        var kael = _world.Kael;
        var characterCombat = PlayerTestUtils.GetProperty(kael, "CharacterCombat", BindingFlags.Instance | BindingFlags.Public);
        var weaponCombos = (Array)PlayerTestUtils.GetField<object>(characterCombat, "weaponCombos", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(weaponCombos);
        Assert.Greater(weaponCombos.Length, 0);
        for (int i = 0; i < weaponCombos.Length; i++)
            Assert.IsNotNull(weaponCombos.GetValue(i), $"weaponCombos[{i}] bị null");

        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-006: [Mẫu] punchCombos không rỗng và mọi phần tử != null.")]
    [TestCaseMeta(
        id: "PL-SAMPLE-006",
        title: "punchCombos không rỗng",
        expected: "punchCombos != null, Length > 0, element != null.",
        steps: "1) Get CharacterCombat. 2) Get field punchCombos. 3) Assert.")]
    public IEnumerator Mau_06_PunchCombos_NotEmpty_ElementsNotNull()
    {
        var kael = _world.Kael;
        var characterCombat = PlayerTestUtils.GetProperty(kael, "CharacterCombat", BindingFlags.Instance | BindingFlags.Public);
        var punchCombos = (Array)PlayerTestUtils.GetField<object>(characterCombat, "punchCombos", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(punchCombos);
        Assert.Greater(punchCombos.Length, 0);
        for (int i = 0; i < punchCombos.Length; i++)
            Assert.IsNotNull(punchCombos.GetValue(i), $"punchCombos[{i}] bị null");

        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-007: [Mẫu] weaponCombos và punchCombos có cùng số bước (dễ maintain combo design).")]
    [TestCaseMeta(
        id: "PL-SAMPLE-007",
        title: "weaponCombos và punchCombos cùng length",
        expected: "weaponCombos.Length == punchCombos.Length.",
        steps: "1) Get both arrays. 2) Compare length.")]
    public IEnumerator Mau_07_ComboArrays_SameLength()
    {
        var kael = _world.Kael;
        var characterCombat = PlayerTestUtils.GetProperty(kael, "CharacterCombat", BindingFlags.Instance | BindingFlags.Public);

        var weaponCombos = (Array)PlayerTestUtils.GetField<object>(characterCombat, "weaponCombos", BindingFlags.Instance | BindingFlags.NonPublic);
        var punchCombos = (Array)PlayerTestUtils.GetField<object>(characterCombat, "punchCombos", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(weaponCombos);
        Assert.IsNotNull(punchCombos);
        Assert.AreEqual(weaponCombos.Length, punchCombos.Length);
        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-008: [Mẫu] Kiểm tra StateController có currentState hợp lệ (không null).")]
    [TestCaseMeta(
        id: "PL-SAMPLE-008",
        title: "StateController.currentState không null",
        expected: "currentState != null.",
        steps: "1) Get StateController. 2) Read field currentState. 3) Assert != null.")]
    public IEnumerator Mau_08_StateController_CurrentState_NotNull()
    {
        var kael = _world.Kael;
        var stateController = PlayerTestUtils.GetProperty(kael, "StateController", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(stateController);

        var currentStateField = stateController.GetType().GetField("currentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(currentStateField, "Không tìm thấy field currentState.");

        var currentState = currentStateField!.GetValue(stateController);
        Assert.IsNotNull(currentState);
        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-009: [Mẫu] Gửi event OnMovement không throw exception.")]
    [TestCaseMeta(
        id: "PL-SAMPLE-009",
        title: "Notify OnMovement không lỗi",
        expected: "Không throw exception khi Notify OnMovement.",
        steps: "1) Call EventManager.Notify(GameEvent.OnMovement, Vector2). 2) Yield 1 frame.")]
    public IEnumerator Mau_09_Notify_OnMovement_DoesNotThrow()
    {
        NotifyGameEvent("OnMovement", new Vector2(0f, 1f));
        yield return null;
        NotifyGameEvent("OnMovement", Vector2.zero);
        yield return null;
    }

    [UnityTest]
    [Category("P2")]
    [Description("TC PL-SAMPLE-010: [Mẫu] MoveDirection được cập nhật sau khi gửi OnMovement.")]
    [TestCaseMeta(
        id: "PL-SAMPLE-010",
        title: "MoveDirection thay đổi sau OnMovement",
        expected: "MoveDirection.sqrMagnitude > 0 sau 1 frame.",
        steps: "1) Notify OnMovement (1,0). 2) Yield. 3) Đọc CharacterMovement.MoveDirection.")]
    public IEnumerator Mau_10_MoveDirection_Changes_AfterOnMovement()
    {
        var kael = _world.Kael;

        NotifyGameEvent("OnMovement", new Vector2(1f, 0f));
        yield return null;

        var characterMovement = PlayerTestUtils.GetProperty(kael, "CharacterMovement", BindingFlags.Instance | BindingFlags.Public);
        var moveDirection = (Vector2)PlayerTestUtils.GetProperty(characterMovement, "MoveDirection", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsTrue(moveDirection.sqrMagnitude > 0.0001f);
    }

    private void NotifyGameEvent(string gameEventName, object payload)
    {
        var eventManagerType = PlayerTestUtils.FindType("EventManager");
        Assert.IsNotNull(eventManagerType);

        var gameEventType = PlayerTestUtils.FindType("GameEvent");
        Assert.IsNotNull(gameEventType);

        var instanceField = eventManagerType!.BaseType != null
            ? eventManagerType.BaseType.GetField("Instance", BindingFlags.Static | BindingFlags.Public)
            : eventManagerType.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

        Assert.IsNotNull(instanceField);
        var instance = instanceField!.GetValue(null);
        Assert.IsNotNull(instance);

        var notifyMethod = eventManagerType.GetMethod("Notify", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(notifyMethod);

        var ev = Enum.Parse(gameEventType!, gameEventName);
        try
        {
            notifyMethod!.Invoke(instance, new[] { ev, payload });
        }
        catch (TargetInvocationException e)
        {
            Assert.Fail($"Notify threw: {e.InnerException?.Message ?? e.Message}");
        }
    }
}
