using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

internal sealed class EnemyTestWorld
{
    internal Scene Scene { get; private set; }
    internal PhysicsScene PhysicsScene { get; private set; }

    internal GameObject MainCameraGo { get; private set; }
    internal GameObject LightGo { get; private set; }
    internal GameObject GroundGo { get; private set; }
    internal GameObject PlayerGo { get; private set; }
    internal GameObject EnemyGo { get; private set; }

    internal Component Enemy { get; private set; }
    internal ScriptableObject EnemyData { get; private set; }

    private readonly string _scenePath;

    internal EnemyTestWorld(string sceneNamePrefix = "EnemyTestScene", string scenePath = TestSceneLoader.DefaultScenePath)
    {
        _scenePath = scenePath;
    }

    internal IEnumerator BuildDefaultWorld(
        Vector3? cameraPos = null,
        Vector3? playerPos = null,
        Vector3? enemyPos = null)
    {
        yield return TestSceneLoader.LoadScene(_scenePath);

        // Use default physics scene because production code uses Physics.* static APIs.
        Scene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(Scene);
        PhysicsScene = Physics.defaultPhysicsScene;

        Assert.IsTrue(Scene.IsValid());
        Assert.IsTrue(PhysicsScene.IsValid());

        MainCameraGo = Camera.main != null ? Camera.main.gameObject : (GameObject.FindWithTag("MainCamera") ?? GameObject.Find("Camera") ?? GameObject.Find("Main Camera"));
        Assert.IsNotNull(MainCameraGo, "Could not find Main Camera in scene.");
        if (cameraPos.HasValue)
            MainCameraGo.transform.position = cameraPos.Value;

        var kaelComp = FindComponentByTypeName("Kael");
        PlayerGo = kaelComp != null ? kaelComp.gameObject : (GameObject.Find("Kael") ?? GameObject.FindWithTag("Player") ?? FindGameObjectWithComponent("Kael"));
        Assert.IsNotNull(PlayerGo, "Could not find Player (Kael) in scene.");
        if (playerPos.HasValue)
            PlayerGo.transform.position = playerPos.Value;

        EnemyGo = FindEnemyGameObject();
        Assert.IsNotNull(EnemyGo, "Could not find an EnemyBase in scene.");
        if (enemyPos.HasValue)
            EnemyGo.transform.position = enemyPos.Value;

        var enemyBaseComp = FindComponentByTypeName("EnemyBase");
        if (enemyBaseComp != null)
            Enemy = enemyBaseComp;
        else
        {
            var enemyBaseType = EnemyTestUtils.FindType("EnemyBase");
            Enemy = enemyBaseType != null
                ? (EnemyGo.GetComponent(enemyBaseType) ?? EnemyGo.GetComponentInChildren(enemyBaseType, includeInactive: true))
                : null;
        }
        Assert.IsNotNull(Enemy, "EnemyBase component missing.");

        // Try to read existing EnemyData from EnemyBase.
        var enemyDataObj = EnemyTestUtils.GetField<object>(Enemy, "enemyData", BindingFlags.Instance | BindingFlags.NonPublic);
        EnemyData = enemyDataObj as ScriptableObject;

        // Fallback: create and inject if missing.
        if (EnemyData == null)
        {
            var enemyDataType = EnemyTestUtils.FindType("EnemyData");
            Assert.IsNotNull(enemyDataType, "Could not find EnemyData type.");
            EnemyData = ScriptableObject.CreateInstance(enemyDataType!);
            EnemyTestUtils.SetField(Enemy, "enemyData", EnemyData, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        yield return null;
    }

    internal IEnumerator StepFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
    }

    internal IEnumerator DisposeWorld()
    {
        // World is the loaded scene; we rely on reloading in next SetUp.
        yield return null;
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

    private static GameObject FindGameObjectWithComponent(string typeName)
    {
        var type = EnemyTestUtils.FindType(typeName);
        if (type == null) return null;

        var comps = UnityEngine.Object.FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (comps == null || comps.Length == 0) return null;
        return ((Component)comps[0]).gameObject;
    }

    private static Component FindComponentByTypeName(string typeName)
    {
        var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in behaviours)
        {
            if (b == null) continue;
            if (string.Equals(b.GetType().Name, typeName, StringComparison.Ordinal))
                return b;
        }

        return null;
    }

    private static GameObject FindEnemyGameObject()
    {
        var enemyBaseType = EnemyTestUtils.FindType("EnemyBase");
        if (enemyBaseType == null) return null;

        var comps = UnityEngine.Object.FindObjectsByType(enemyBaseType, FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (comps == null || comps.Length == 0) return null;

        // Prefer something named like Enemy_* (bat, etc.) to match scene layout.
        foreach (var c in comps)
        {
            var go = ((Component)c).gameObject;
            if (go != null && go.name.StartsWith("Enemy_", StringComparison.OrdinalIgnoreCase))
                return go;
        }

        return ((Component)comps[0]).gameObject;
    }

    internal void SetEnemyDataField(string fieldName, object value)
    {
        Assert.IsNotNull(EnemyData);
        EnemyTestUtils.SetField(EnemyData, fieldName, value, BindingFlags.Instance | BindingFlags.Public);
    }
}
