using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

internal sealed class PlayerTestWorld
{
    internal Scene Scene { get; private set; }
    internal PhysicsScene PhysicsScene { get; private set; }

    internal GameObject EventManagerGo { get; private set; }
    internal GameObject MainCameraGo { get; private set; }
    internal GameObject GroundGo { get; private set; }
    internal GameObject PlayerGo { get; private set; }

    internal Component Kael { get; private set; }

    private readonly string _scenePath;
    private readonly List<GameObject> _spawned = new();

    internal PlayerTestWorld(string sceneNamePrefix = "PlayerTestScene", string scenePath = TestSceneLoader.DefaultScenePath)
    {
        _scenePath = scenePath;
    }

    internal IEnumerator BuildDefaultWorld(Vector3? cameraPos = null)
    {
        yield return TestSceneLoader.LoadScene(_scenePath);

        // Use default physics scene because production code uses Physics.* static APIs.
        Scene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(Scene);
        PhysicsScene = Physics.defaultPhysicsScene;

        Assert.IsTrue(Scene.IsValid());
        Assert.IsTrue(PhysicsScene.IsValid());

        var eventManager = FindComponentByTypeName("EventManager");
        EventManagerGo = eventManager != null ? eventManager.gameObject : (GameObject.Find("EventManager") ?? FindGameObjectWithComponent("EventManager"));
        Assert.IsNotNull(EventManagerGo, "Could not find EventManager in scene. Ensure GameDemo scene contains it.");

        MainCameraGo = Camera.main != null ? Camera.main.gameObject : (GameObject.FindWithTag("MainCamera") ?? GameObject.Find("Camera") ?? GameObject.Find("Main Camera"));
        Assert.IsNotNull(MainCameraGo, "Could not find Main Camera in scene.");

        if (cameraPos.HasValue)
            MainCameraGo.transform.position = cameraPos.Value;

        Kael = FindComponentByTypeName("Kael");
        if (Kael != null)
        {
            PlayerGo = Kael.gameObject;
        }
        else
        {
            PlayerGo = GameObject.Find("Kael") ?? FindGameObjectWithComponent("Kael");
            Assert.IsNotNull(PlayerGo, "Could not find Kael GameObject in scene.");

            var kaelType = PlayerTestUtils.FindType("Kael");
            if (kaelType != null)
            {
                Kael = PlayerGo.GetComponent(kaelType)
                       ?? PlayerGo.GetComponentInChildren(kaelType, includeInactive: true);
            }
        }

        Assert.IsNotNull(Kael, "Could not find Kael component in scene.");

        yield return null;
    }

    internal IEnumerator StepFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
    }

    internal void SimulateFixedSteps(int steps, float fixedDeltaTime = 0.02f)
    {
        // For deterministic tests: turn off global auto simulation and manually simulate this scene's PhysicsScene.
        // Note: CharacterController movement happens in scripts; this is mainly for collisions/grounding when needed.
        var prevMode = Physics.simulationMode;
        var prevFixed = Time.fixedDeltaTime;
        try
        {
            Physics.simulationMode = SimulationMode.Script;
            Time.fixedDeltaTime = fixedDeltaTime;
            for (int i = 0; i < steps; i++)
            {
                // Use default Physics.Simulate so CharacterController/Physics.* stay consistent.
                Physics.Simulate(fixedDeltaTime);
            }
        }
        finally
        {
            Physics.simulationMode = prevMode;
            Time.fixedDeltaTime = prevFixed;
        }
    }

    internal GameObject CreateTarget(int layerIndex, Vector3 position)
    {
        var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        target.name = "Target";
        target.layer = layerIndex;
        target.transform.position = position;
        _spawned.Add(target);
        return target;
    }

    internal void ConfigureCameraTargetLayers(int targetLayerIndex, int obstacleLayerMask = 0)
    {
        var characterCamera = PlayerTestUtils.GetProperty(Kael, "CharacterCamera", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(characterCamera);

        var targetLayerMask = (LayerMask)(1 << targetLayerIndex);
        PlayerTestUtils.SetField(characterCamera, "targetLayer", targetLayerMask, BindingFlags.Instance | BindingFlags.NonPublic);
        PlayerTestUtils.SetField(characterCamera, "obstacleLayer", (LayerMask)obstacleLayerMask, BindingFlags.Instance | BindingFlags.NonPublic);
    }

    internal IEnumerator DisposeWorld()
    {
        foreach (var go in _spawned)
        {
            if (go != null)
                UnityEngine.Object.Destroy(go);
        }

        _spawned.Clear();
        yield return null;
    }

    private static GameObject FindGameObjectWithComponent(string typeName)
    {
        var type = PlayerTestUtils.FindType(typeName);
        if (type == null) return null;

        // Include inactive because scene objects might be disabled during boot.
        var comps = UnityEngine.Object.FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (comps == null || comps.Length == 0) return null;
        return ((Component)comps[0]).gameObject;
    }

    private static Component FindComponentByTypeName(string typeName)
    {
        // Most runtime scripts are MonoBehaviours; scanning by runtime type name avoids reflection/assembly timing issues.
        var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in behaviours)
        {
            if (b == null) continue;
            if (string.Equals(b.GetType().Name, typeName, StringComparison.Ordinal))
                return b;
        }

        return null;
    }

    private GameObject BuildKaelPlayer()
    {
        var root = new GameObject("Kael_Test");
        root.SetActive(false);

        var visuals = new GameObject("Visuals");
        visuals.transform.SetParent(root.transform, false);

        var kaelVisual = new GameObject("Kael");
        kaelVisual.transform.SetParent(visuals.transform, false);
        kaelVisual.AddComponent<Animator>();

        var kaelGiant = new GameObject("KaelGiant");
        kaelGiant.transform.SetParent(visuals.transform, false);

        root.AddComponent<CharacterController>();

        var characterAnimation = PlayerTestUtils.AddComponent(root, "CharacterAnimation");
        var characterMovement = PlayerTestUtils.AddComponent(root, "CharacterMovement");
        var characterRotate = PlayerTestUtils.AddComponent(root, "CharacterRotate");
        var characterWeapon = PlayerTestUtils.AddComponent(root, "CharacterWeapon");
        var characterCombat = PlayerTestUtils.AddComponent(root, "CharacterCombat");
        var characterCamera = PlayerTestUtils.AddComponent(root, "CharacterCamera");

        var kael = PlayerTestUtils.AddComponent(root, "Kael");

        PlayerTestUtils.InvokeLoadComponent((Component)kael);
        PlayerTestUtils.InvokeLoadComponent((Component)characterMovement);
        PlayerTestUtils.InvokeLoadComponent((Component)characterCamera);

        var characterData = PlayerTestUtils.CreateCharacterData(speed: 6f, attackSpeed: 120f);
        PlayerTestUtils.SetField(kael, "characterData", characterData, BindingFlags.Instance | BindingFlags.NonPublic);

        root.transform.position = new Vector3(0f, 1f, 0f);
        return root;
    }
}
