using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerIntegrationTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld();
        yield return _world.BuildDefaultWorld();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_world != null)
            yield return _world.DisposeWorld();
    }

    [UnityTest]
    public IEnumerator Player_StartsInIdle_AndTransitionsToMove_ThenBackToIdle()
    {
        var kael = _world.Kael;

        // After Start() the state should be IdleState
        Assert.AreEqual("IdleState", GetCurrentStateName(kael));

        NotifyGameEvent("OnMovement", new Vector2(0f, 1f));
        yield return null; // allow Update() to run and change state

        Assert.AreEqual("MoveState", GetCurrentStateName(kael));

        NotifyGameEvent("OnMovement", Vector2.zero);
        yield return null;

        Assert.AreEqual("IdleState", GetCurrentStateName(kael));
    }

    [UnityTest]
    public IEnumerator MovementInput_ComputesMoveDirection_RelativeToCamera()
    {
        var kael = _world.Kael;

        NotifyGameEvent("OnMovement", new Vector2(1f, 0f));
        yield return null;

        // CharacterMovement.MoveDirection should not be zero
        var characterMovement = PlayerTestUtils.GetProperty(kael, "CharacterMovement", BindingFlags.Instance | BindingFlags.Public);
        var moveDirection = (Vector2)PlayerTestUtils.GetProperty(characterMovement, "MoveDirection", BindingFlags.Instance | BindingFlags.Public);

        Assert.IsTrue(moveDirection.sqrMagnitude > 0.0001f);
    }

    [UnityTest]
    public IEnumerator CameraLock_TogglesAndSelectsBestTarget()
    {
        var kael = _world.Kael;

        // create a target with collider on a dedicated layer
        const int targetLayerIndex = 8;
        var target = _world.CreateTarget(targetLayerIndex, new Vector3(0f, 1f, 6f));

        var characterCamera = PlayerTestUtils.GetProperty(kael, "CharacterCamera", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(characterCamera);

        _world.ConfigureCameraTargetLayers(targetLayerIndex, obstacleLayerMask: 0);

        // Lock on
        PlayerTestUtils.Call(characterCamera, "ToggleLockTarget", BindingFlags.Instance | BindingFlags.Public);
        yield return null;

        var isLocking = (bool)PlayerTestUtils.GetProperty(characterCamera, "IsLockingTarget", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsTrue(isLocking);

        var lookAtTarget = (Transform)PlayerTestUtils.GetProperty(characterCamera, "LookAtTarget", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(lookAtTarget);
        Assert.AreEqual(target.transform, lookAtTarget);

        // Unlock
        PlayerTestUtils.Call(characterCamera, "ToggleLockTarget", BindingFlags.Instance | BindingFlags.Public);
        yield return null;

        isLocking = (bool)PlayerTestUtils.GetProperty(characterCamera, "IsLockingTarget", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsFalse(isLocking);

        UnityEngine.Object.Destroy(target);
    }

    [UnityTest]
    public IEnumerator DebugArtifact_CaptureScreenshot_AfterBootstrap()
    {
        // This is mainly to "see" the test world; check project-root/TestScreenshots after running tests.
        yield return _world.CaptureScreenshot("PlayerTestWorld.png");
        Assert.Pass("Screenshot captured (see console log for path)." );
    }

    [UnityTest]
    public IEnumerator Combo_Init_SetsMeleeCombosAndPunchFallback()
    {
        // This is a fast check that doesn't run the actual AttackRoutine (which depends on Animator states)
        var kael = _world.Kael;

        var characterCombat = PlayerTestUtils.GetProperty(kael, "CharacterCombat", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(characterCombat);

        // private fields in CharacterCombat
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
