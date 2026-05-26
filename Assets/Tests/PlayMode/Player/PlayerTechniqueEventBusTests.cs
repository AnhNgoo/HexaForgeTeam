using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;

[TestReport]
[Category("Techniques")]
public class PlayerTechniqueEventBusTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("Tech_EventBus");
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
        id: "PL-TEC-002",
        title: "Kỹ thuật: EventBus subscribe/notify/unsubscribe",
        expected: "Listener được gọi khi subscribe và không được gọi sau unsubscribe.",
        steps: "1) Subscribe. 2) Notify. 3) Assert called. 4) Unsubscribe. 5) Notify. 6) Assert not called.")]
    public IEnumerator EventBus_Subscribe_Notify_Unsubscribe()
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
}
