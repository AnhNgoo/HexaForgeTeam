using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTestingTechniquesExamples
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("Techniques");
        yield return _world.BuildDefaultWorld();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_world != null)
            yield return _world.DisposeWorld();
    }

    [UnityTest]
    public IEnumerator Technique_TimeBased_Cooldown_Expires()
    {
        var cooldownType = PlayerTestUtils.FindType("Cooldown");
        Assert.IsNotNull(cooldownType);

        var cooldown = Activator.CreateInstance(cooldownType!);
        var startCooldown = cooldownType!.GetMethod("StartCooldown", BindingFlags.Instance | BindingFlags.Public);
        var isOnCooldownProp = cooldownType.GetProperty("IsOnCooldown", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(startCooldown);
        Assert.IsNotNull(isOnCooldownProp);

        startCooldown!.Invoke(cooldown, new object[] { 0.05f });
        Assert.IsTrue((bool)isOnCooldownProp!.GetValue(cooldown));

        // wait a bit longer than cooldown
        yield return new WaitForSeconds(0.1f);

        Assert.IsFalse((bool)isOnCooldownProp.GetValue(cooldown));
    }

    [UnityTest]
    public IEnumerator Technique_EventBus_Subscribe_Notify_Unsubscribe()
    {
        var eventManagerType = PlayerTestUtils.FindType("EventManager");
        var gameEventType = PlayerTestUtils.FindType("GameEvent");
        Assert.IsNotNull(eventManagerType);
        Assert.IsNotNull(gameEventType);

        var instanceField = eventManagerType!.BaseType!.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
        var instance = instanceField!.GetValue(null);
        Assert.IsNotNull(instance);

        var subscribe = eventManagerType.GetMethod("Subscribe", BindingFlags.Instance | BindingFlags.Public);
        var unsubscribe = eventManagerType.GetMethod("Unsubscribe", BindingFlags.Instance | BindingFlags.Public);
        var notify = eventManagerType.GetMethod("Notify", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(subscribe);
        Assert.IsNotNull(unsubscribe);
        Assert.IsNotNull(notify);

        bool called = false;
        Action<object> listener = _ => called = true;

        var ev = Enum.Parse(gameEventType!, "OnAttack");

        subscribe!.Invoke(instance, new object[] { ev, listener });
        notify!.Invoke(instance, new object[] { ev, null });
        yield return null;
        Assert.IsTrue(called);

        called = false;
        unsubscribe!.Invoke(instance, new object[] { ev, listener });
        notify.Invoke(instance, new object[] { ev, null });
        yield return null;
        Assert.IsFalse(called);
    }

    [UnityTest]
    public IEnumerator Technique_VirtualScene_IsolatedBootstrap_And_PhysicsSimulate()
    {
        // This demonstrates a "virtual" scene per test plus deterministic-ish physics stepping.
        // We mainly assert no exceptions and the player exists.
        Assert.IsNotNull(_world.PlayerGo);
        Assert.IsNotNull(_world.Kael);

        _world.SimulateFixedSteps(steps: 5, fixedDeltaTime: 0.02f);
        yield return null;

        // Capture artifact so you can visually inspect the world after running tests
        yield return _world.CaptureScreenshot("PlayerTechniquesWorld.png");
    }

    [UnityTest]
    public IEnumerator Technique_StateTransition_ViaEvent_JumpState()
    {
        // Avoid Dodge/Attack end-to-end: they depend on Animator states that tests don't set up.
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
