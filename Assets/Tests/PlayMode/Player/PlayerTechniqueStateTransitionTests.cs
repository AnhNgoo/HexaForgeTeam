using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestReport]
[Category("Techniques")]
public class PlayerTechniqueStateTransitionTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("Tech_StateTransition");
        yield return _world.BuildDefaultWorld();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_world != null)
            yield return _world.DisposeWorld();
    }

    [UnityTest]
    [Category("P2")]
    [TestCaseMeta(
        id: "PL-TEC-004",
        title: "Kỹ thuật: ép chuyển state qua Event (Jump)",
        expected: "State hiện tại là JumpState sau khi Notify OnJump.",
        steps: "1) Notify OnJump. 2) Chờ 1 frame. 3) Đọc currentState.")]
    public IEnumerator StateTransition_ViaEvent_JumpState()
    {
        NotifyGameEvent("OnJump", null);
        yield return null;

        var stateName = GetCurrentStateName(_world.Kael);
        Assert.AreEqual("JumpState", stateName);
    }

    private string GetCurrentStateName(Component characterBase)
    {
        var stateController = PlayerTestUtils.GetProperty(characterBase, "StateController", BindingFlags.Instance | BindingFlags.Public);
        var currentStateField = stateController.GetType().GetField("currentState", BindingFlags.Instance | BindingFlags.Public);
        var currentState = currentStateField!.GetValue(stateController);
        return currentState!.GetType().Name;
    }

    private void NotifyGameEvent(string gameEventName, object payload)
    {
        var eventManagerType = PlayerTestUtils.FindType("EventManager");
        var gameEventType = PlayerTestUtils.FindType("GameEvent");

        var instanceField = eventManagerType!.BaseType!.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
        var instance = instanceField!.GetValue(null);

        var notifyMethod = eventManagerType.GetMethod("Notify", BindingFlags.Instance | BindingFlags.Public);
        var ev = Enum.Parse(gameEventType!, gameEventName);
        notifyMethod!.Invoke(instance, new object[] { ev, payload });
    }
}
