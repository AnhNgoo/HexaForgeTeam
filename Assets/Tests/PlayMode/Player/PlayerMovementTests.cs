using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[Category("Integration")]
[Category("Character")]
public class PlayerMovementTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("PlayerMovement");
        yield return _world.BuildDefaultWorld();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_world != null)
            yield return _world.DisposeWorld();
    }

    [UnityTest]
    [Category("P0")]
    [Description("TC PL-INT-001: Gửi input di chuyển để kiểm tra state Idle → Move → Idle của Kael.")]
    [TestCaseMeta(
        id: "PL-INT-001",
        title: "Player Idle → Move → Idle theo input",
        expected: "State đổi đúng: IdleState → MoveState → IdleState.",
        steps: "1) Bootstrap Player world. 2) Notify OnMovement (0,1). 3) Chờ 1 frame. 4) Notify OnMovement (0,0). 5) Chờ 1 frame.")]
    public IEnumerator Idle_To_Move_To_Idle_ByMovementInput()
    {
        var kael = _world.Kael;

        Assert.AreEqual("IdleState", GetCurrentStateName(kael));

        NotifyGameEvent("OnMovement", new Vector2(0f, 1f));
        yield return null;
        Assert.AreEqual("MoveState", GetCurrentStateName(kael));

        NotifyGameEvent("OnMovement", Vector2.zero);
        yield return null;
        Assert.AreEqual("IdleState", GetCurrentStateName(kael));
    }

    [UnityTest]
    [Category("P1")]
    [Description("TC PL-INT-002: MoveDirection được tính theo hướng camera sau khi gửi OnMovement.")]
    [TestCaseMeta(
        id: "PL-INT-002",
        title: "MovementInput tính hướng di chuyển theo camera",
        expected: "MoveDirection khác Vector2.zero.",
        steps: "1) Notify OnMovement (1,0). 2) Chờ 1 frame. 3) Đọc CharacterMovement.MoveDirection.")]
    public IEnumerator MoveDirection_Computed_RelativeToCamera()
    {
        var kael = _world.Kael;

        NotifyGameEvent("OnMovement", new Vector2(1f, 0f));
        yield return null;

        var characterMovement = PlayerTestUtils.GetProperty(kael, "CharacterMovement", BindingFlags.Instance | BindingFlags.Public);
        var moveDirection = (Vector2)PlayerTestUtils.GetProperty(characterMovement, "MoveDirection", BindingFlags.Instance | BindingFlags.Public);

        Assert.IsTrue(moveDirection.sqrMagnitude > 0.0001f);
    }

    private string GetCurrentStateName(Component characterBase)
    {
        var stateController = PlayerTestUtils.GetProperty(characterBase, "StateController", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(stateController);

        var currentStateField = stateController.GetType().GetField("currentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(currentStateField);

        var currentState = currentStateField!.GetValue(stateController);
        Assert.IsNotNull(currentState);
        return currentState!.GetType().Name;
    }

    private void NotifyGameEvent(string gameEventName, object payload)
    {
        var eventManagerType = PlayerTestUtils.FindType("EventManager");
        Assert.IsNotNull(eventManagerType);

        var gameEventType = PlayerTestUtils.FindType("GameEvent");
        Assert.IsNotNull(gameEventType);

        var instanceField = eventManagerType!.BaseType!.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
        var instance = instanceField!.GetValue(null);
        Assert.IsNotNull(instance);

        var notifyMethod = eventManagerType.GetMethod("Notify", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(notifyMethod);

        var ev = Enum.Parse(gameEventType!, gameEventName);
        notifyMethod!.Invoke(instance, new object[] { ev, payload });
    }
}
