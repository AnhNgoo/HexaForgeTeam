using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

internal sealed class EnemyTestWorld
{
    internal Scene Scene { get; }
    internal PhysicsScene PhysicsScene { get; }

    internal GameObject MainCameraGo { get; private set; }
    internal GameObject LightGo { get; private set; }
    internal GameObject GroundGo { get; private set; }
    internal GameObject PlayerGo { get; private set; }
    internal GameObject EnemyGo { get; private set; }

    internal Component Enemy { get; private set; }
    internal ScriptableObject EnemyData { get; private set; }

    private readonly string _sceneName;

    internal EnemyTestWorld(string sceneNamePrefix = "EnemyTestScene")
    {
        _sceneName = $"{sceneNamePrefix}_{Guid.NewGuid():N}";

        // Use default physics scene because production code uses Physics.* static APIs.
        var createParams = new CreateSceneParameters(LocalPhysicsMode.None);
        Scene = SceneManager.CreateScene(_sceneName, createParams);
        SceneManager.SetActiveScene(Scene);
        PhysicsScene = Physics.defaultPhysicsScene;

        Assert.IsTrue(Scene.IsValid());
        Assert.IsTrue(PhysicsScene.IsValid());
    }

    internal IEnumerator BuildDefaultWorld(
        Vector3? cameraPos = null,
        Vector3? playerPos = null,
        Vector3? enemyPos = null)
    {
        MainCameraGo = new GameObject("Main Camera");
        MainCameraGo.tag = "MainCamera";
        var cam = MainCameraGo.AddComponent<Camera>();
        SceneManager.MoveGameObjectToScene(MainCameraGo, Scene);

        cam.transform.position = cameraPos ?? new Vector3(0f, 6f, -10f);
        cam.transform.LookAt(Vector3.zero);

        LightGo = new GameObject("Directional Light");
        var light = LightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        LightGo.transform.eulerAngles = new Vector3(50f, -30f, 0f);
        SceneManager.MoveGameObjectToScene(LightGo, Scene);

        GroundGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        GroundGo.name = "Ground";
        GroundGo.transform.position = Vector3.zero;
        SceneManager.MoveGameObjectToScene(GroundGo, Scene);

        PlayerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        PlayerGo.name = "Player_Test";
        PlayerGo.tag = "Player";
        PlayerGo.transform.position = playerPos ?? new Vector3(0f, 1f, 1.5f);
        SceneManager.MoveGameObjectToScene(PlayerGo, Scene);

        EnemyGo = BuildEnemy(enemyPos ?? new Vector3(0f, 1f, 0f));
        SceneManager.MoveGameObjectToScene(EnemyGo, Scene);
        EnemyGo.SetActive(true);

        Enemy = EnemyGo.GetComponent(EnemyTestUtils.FindType("EnemyBase"));
        Assert.IsNotNull(Enemy, "EnemyBase component missing.");

        // let Awake/OnEnable/Start run
        yield return null;
    }

    internal IEnumerator StepFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
    }

    internal IEnumerator CaptureScreenshot(string fileName)
    {
        var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var dir = Path.Combine(root, "TestScreenshots");
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, fileName);
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"[EnemyTestWorld] Screenshot saved: {path}");

        // Give Unity a few frames to flush.
        yield return null;
        yield return null;
        yield return null;
    }

    internal IEnumerator DisposeWorld()
    {
        if (EnemyGo != null) UnityEngine.Object.Destroy(EnemyGo);
        if (PlayerGo != null) UnityEngine.Object.Destroy(PlayerGo);
        if (GroundGo != null) UnityEngine.Object.Destroy(GroundGo);
        if (LightGo != null) UnityEngine.Object.Destroy(LightGo);
        if (MainCameraGo != null) UnityEngine.Object.Destroy(MainCameraGo);
        yield return null;

        if (Scene.IsValid())
        {
            var unload = SceneManager.UnloadSceneAsync(Scene);
            if (unload != null)
                yield return unload;
        }
    }

    private GameObject BuildEnemy(Vector3 position)
    {
        var root = new GameObject("Enemy_Test");
        root.SetActive(false);

        root.transform.position = position;

        // Required so Dead state can disable it.
        root.AddComponent<CapsuleCollider>();

        // Required by EnemyLocomotion.SetSpeed/StopMoving.
        var agent = root.AddComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = true;

        // Modules expected by EnemyBase.CacheReferences()
        EnemyTestUtils.AddComponent(root, "EnemyHealth");
        EnemyTestUtils.AddComponent(root, "EnemyCombat");
        var detection = EnemyTestUtils.AddComponent(root, "EnemyDetection");
        EnemyTestUtils.AddComponent(root, "EnemyStateMachine");
        EnemyTestUtils.AddComponent(root, "EnemyEventManager");
        EnemyTestUtils.AddComponent(root, "EnemyDamageReceiver");
        EnemyTestUtils.AddComponent(root, "EnemyPoiseSystem");
        var locomotion = EnemyTestUtils.AddComponent(root, "EnemyLocomotion");

        // Visuals/Animator controller (EnemyBase caches it via GetComponentInChildren)
        var visuals = new GameObject("Visuals");
        visuals.transform.SetParent(root.transform, false);
        visuals.AddComponent<Animator>();
        EnemyTestUtils.AddComponent(visuals, "EnemyAnimatorController");

        // Root orchestrator
        var enemyBase = EnemyTestUtils.AddComponent(root, "EnemyBase");

        // Create and inject EnemyData
        var enemyDataType = EnemyTestUtils.FindType("EnemyData");
        Assert.IsNotNull(enemyDataType, "Could not find EnemyData type.");
        EnemyData = ScriptableObject.CreateInstance(enemyDataType!);

        SetEnemyDataField("maxHealth", 50f);
        SetEnemyDataField("damage", 10f);
        SetEnemyDataField("maxDefense", 0f);
        SetEnemyDataField("maxPoise", 30f);
        SetEnemyDataField("moveSpeed", 3f);
        SetEnemyDataField("patrolSpeed", 2f);
        SetEnemyDataField("detectRange", 10f);
        SetEnemyDataField("loseTargetRange", 15f);
        SetEnemyDataField("povAngle", 180f);
        SetEnemyDataField("attackCooldown", 0f); // deterministic in tests
        SetEnemyDataField("staggerDuration", 0.05f);

        EnemyTestUtils.SetField(enemyBase, "enemyData", EnemyData, BindingFlags.Instance | BindingFlags.NonPublic);

        // Wire serialized private refs that are NOT auto-cached at runtime
        EnemyTestUtils.SetField(locomotion, "_navMeshAgent", agent, BindingFlags.Instance | BindingFlags.NonPublic);

        EnemyTestUtils.SetField(detection, "Player", PlayerGo.transform, BindingFlags.Instance | BindingFlags.NonPublic);
        EnemyTestUtils.SetField(detection, "obstacleLayerMask", (LayerMask)0, BindingFlags.Instance | BindingFlags.NonPublic);

        // Force-cache after we injected private fields while inactive.
        // (Awake will also cache, but this makes tests resilient to script execution order.)
        EnemyTestUtils.InvokeLoadComponent(enemyBase);

        return root;
    }

    internal void SetEnemyDataField(string fieldName, object value)
    {
        Assert.IsNotNull(EnemyData);
        EnemyTestUtils.SetField(EnemyData, fieldName, value, BindingFlags.Instance | BindingFlags.Public);
    }
}
