using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using System.Reflection;

[Category("Integration")]
[Category("Enemy")]
public class EnemyDamageDeathTests
{
    private EnemyTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new EnemyTestWorld("EnemyDamageDead");
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
    [Description("TC EN-INT-003: Enemy nhận sát thương đủ lớn sẽ chết (Dead) và tắt collider chính.")]
    [TestCaseMeta(
        id: "EN-INT-003",
        title: "Nhận sát thương dẫn tới Dead + disable collider",
        expected: "OnTakeDamage & OnDead được gọi; state chuyển Dead; collider bị tắt.",
        steps: "1) Set HP thấp. 2) Gọi TakeHit sát thương lớn. 3) Verify event/state/collider.")]
    public IEnumerator TakeHit_KillsEnemy_AndDisablesCollider()
    {
        bool takeDamageCalled = false;
        float lastDamage = -1f;
        bool deadCalled = false;

        var eventManager = EnemyTestUtils.GetProperty(_world.Enemy, "EventManager", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.AddEventHandler(eventManager, "OnTakeDamage", (Action<float>)(dmg => { takeDamageCalled = true; lastDamage = dmg; }));
        EnemyTestUtils.AddEventHandler(eventManager, "OnDead", (Action)(() => deadCalled = true));

        _world.SetEnemyDataField("maxDefense", 0f);
        _world.SetEnemyDataField("maxHealth", 10f);

        var health = EnemyTestUtils.GetProperty(_world.Enemy, "Heath", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.Call(health, "Initialize", BindingFlags.Instance | BindingFlags.Public, _world.Enemy);

        var damageReceiver = EnemyTestUtils.GetProperty(_world.Enemy, "DamageReceiver", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.Call(damageReceiver, "TakeHit", BindingFlags.Instance | BindingFlags.Public, 25f, 0f);
        yield return null;

        Assert.IsTrue(takeDamageCalled);
        Assert.AreEqual(25f, lastDamage);
        Assert.IsTrue(deadCalled);

        var stateMachine = EnemyTestUtils.GetProperty(_world.Enemy, "StateMachine", BindingFlags.Instance | BindingFlags.Public);
        var currentState = EnemyTestUtils.GetProperty(stateMachine, "CurrentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual("EnemyState_Dead", currentState.GetType().Name);

        var mainCollider = (Collider)EnemyTestUtils.GetProperty(_world.Enemy, "MainCollider", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsFalse(mainCollider.enabled);
    }
}
