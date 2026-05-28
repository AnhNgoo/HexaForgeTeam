using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[Category("Integration")]
[Category("Character")]
public class PlayerCameraLockTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("PlayerCameraLock");
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
    [Description("TC PL-INT-003: Bật/tắt camera lock và kiểm tra chọn đúng target trong layer mask.")]
    [TestCaseMeta(
        id: "PL-INT-003",
        title: "CameraLock lock/unlock và chọn đúng target",
        expected: "Lock bật/tắt được; LookAtTarget đúng capsule target.",
        steps: "1) Tạo Target layer. 2) Configure layer mask. 3) ToggleLockTarget. 4) Verify. 5) Toggle lại.")]
    public IEnumerator CameraLock_Toggles_And_SelectsBestTarget()
    {
        var kael = _world.Kael;

        const int targetLayerIndex = 8;
        var target = _world.CreateTarget(targetLayerIndex, new Vector3(0f, 1f, 6f));

        var characterCamera = PlayerTestUtils.GetProperty(kael, "CharacterCamera", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(characterCamera);

        _world.ConfigureCameraTargetLayers(targetLayerIndex, obstacleLayerMask: 0);

        PlayerTestUtils.Call(characterCamera, "ToggleLockTarget", BindingFlags.Instance | BindingFlags.Public);
        yield return null;

        var isLocking = (bool)PlayerTestUtils.GetProperty(characterCamera, "IsLockingTarget", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsTrue(isLocking);

        var lookAtTarget = (Transform)PlayerTestUtils.GetProperty(characterCamera, "LookAtTarget", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(lookAtTarget);
        Assert.AreEqual(target.transform, lookAtTarget);

        PlayerTestUtils.Call(characterCamera, "ToggleLockTarget", BindingFlags.Instance | BindingFlags.Public);
        yield return null;

        isLocking = (bool)PlayerTestUtils.GetProperty(characterCamera, "IsLockingTarget", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsFalse(isLocking);

        UnityEngine.Object.Destroy(target);
    }
}
