using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;
using System.Reflection;

[TestReport]
[Category("Integration")]
[Category("Enemy")]
public class EnemyDeadEventTests
{
    private EnemyTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new EnemyTestWorld("EnemyDeadEvent");
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
    [Description("TC EN-TEC-002: Gây sát thương cực lớn để xác nhận event OnDead và state Dead hoạt động đúng.")]
    [TestCaseMeta(
        id: "EN-TEC-002",
        title: "Event-driven: chết khi nhận sát thương cực lớn",
        expected: "OnDead được gọi và state chuyển Dead.",
        steps: "1) Set HP thấp. 2) TakeHit 999. 3) Verify event/state.")]
    public IEnumerator DeadEvent_Fires_OnHugeDamage()
    {
        bool deadCalled = false;
        var eventManager = EnemyTestUtils.GetProperty(_world.Enemy, "EventManager", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.AddEventHandler(eventManager, "OnDead", (Action)(() => deadCalled = true));

        _world.SetEnemyDataField("maxDefense", 0f);
        _world.SetEnemyDataField("maxHealth", 5f);

        var health = EnemyTestUtils.GetProperty(_world.Enemy, "Heath", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.Call(health, "Initialize", BindingFlags.Instance | BindingFlags.Public, _world.Enemy);

        var damageReceiver = EnemyTestUtils.GetProperty(_world.Enemy, "DamageReceiver", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.Call(damageReceiver, "TakeHit", BindingFlags.Instance | BindingFlags.Public, 999f, 0f);
        yield return null;

        Assert.IsTrue(deadCalled);
        var stateMachine = EnemyTestUtils.GetProperty(_world.Enemy, "StateMachine", BindingFlags.Instance | BindingFlags.Public);
        var currentState = EnemyTestUtils.GetProperty(stateMachine, "CurrentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual("EnemyState_Dead", currentState.GetType().Name);
    }
}
