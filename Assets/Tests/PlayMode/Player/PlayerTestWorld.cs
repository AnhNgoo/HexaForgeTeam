using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

internal sealed class PlayerTestWorld
{
    internal Scene Scene { get; }
    internal PhysicsScene PhysicsScene { get; }

    internal GameObject EventManagerGo { get; private set; }
    internal GameObject MainCameraGo { get; private set; }
    internal GameObject GroundGo { get; private set; }
    internal GameObject PlayerGo { get; private set; }

    internal Component Kael { get; private set; }

    private readonly string _sceneName;

    internal PlayerTestWorld(string sceneNamePrefix = "PlayerTestScene")
    {
        _sceneName = $"{sceneNamePrefix}_{Guid.NewGuid():N}";

        // Use default physics scene because production code uses Physics.* static APIs.
        // A local physics scene would require PhysicsScene.* APIs in production to stay visible.
        var createParams = new CreateSceneParameters(LocalPhysicsMode.None);
        Scene = SceneManager.CreateScene(_sceneName, createParams);
        SceneManager.SetActiveScene(Scene);
        PhysicsScene = Physics.defaultPhysicsScene;

        Assert.IsTrue(Scene.IsValid());
        Assert.IsTrue(PhysicsScene.IsValid());
    }

    internal IEnumerator BuildDefaultWorld(Vector3? cameraPos = null)
    {
        EventManagerGo = new GameObject("EventManager");
        PlayerTestUtils.AddComponent(EventManagerGo, "EventManager");
        SceneManager.MoveGameObjectToScene(EventManagerGo, Scene);
        EventManagerGo.SetActive(true);

        MainCameraGo = new GameObject("Main Camera");
        MainCameraGo.tag = "MainCamera";
        var cam = MainCameraGo.AddComponent<Camera>();
        SceneManager.MoveGameObjectToScene(MainCameraGo, Scene);

        cam.transform.position = cameraPos ?? new Vector3(0f, 6f, -10f);
        cam.transform.LookAt(Vector3.zero);

        GroundGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        GroundGo.name = "Ground";
        SceneManager.MoveGameObjectToScene(GroundGo, Scene);
        GroundGo.transform.position = Vector3.zero;

        PlayerGo = BuildKaelPlayer();
        SceneManager.MoveGameObjectToScene(PlayerGo, Scene);
        PlayerGo.SetActive(true);

        Kael = PlayerGo.GetComponent(PlayerTestUtils.FindType("Kael"));
        Assert.IsNotNull(Kael);

        // let Awake/OnEnable/Start run
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
        bool prevAuto = Physics.autoSimulation;
        var prevFixed = Time.fixedDeltaTime;
        try
        {
            Physics.autoSimulation = false;
            Time.fixedDeltaTime = fixedDeltaTime;
            for (int i = 0; i < steps; i++)
            {
                // Use default Physics.Simulate so CharacterController/Physics.* stay consistent.
                Physics.Simulate(fixedDeltaTime);
            }
        }
        finally
        {
            Physics.autoSimulation = prevAuto;
            Time.fixedDeltaTime = prevFixed;
        }
    }

    internal GameObject CreateTarget(int layerIndex, Vector3 position)
    {
        var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        target.name = "Target";
        target.layer = layerIndex;
        target.transform.position = position;
        SceneManager.MoveGameObjectToScene(target, Scene);
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

    internal IEnumerator CaptureScreenshot(string fileName)
    {
        // Saves under project root /TestScreenshots so you can open it after tests.
        var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var dir = Path.Combine(root, "TestScreenshots");
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, fileName);
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log($"[PlayerTestWorld] Screenshot saved: {path}");

        // Give Unity a few frames to flush.
        yield return null;
        yield return null;
        yield return null;
    }

    internal IEnumerator DisposeWorld()
    {
        if (PlayerGo != null) UnityEngine.Object.Destroy(PlayerGo);
        if (GroundGo != null) UnityEngine.Object.Destroy(GroundGo);
        if (MainCameraGo != null) UnityEngine.Object.Destroy(MainCameraGo);
        if (EventManagerGo != null) UnityEngine.Object.Destroy(EventManagerGo);
        yield return null;

        if (Scene.IsValid())
        {
            var unload = SceneManager.UnloadSceneAsync(Scene);
            if (unload != null)
                yield return unload;
        }
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
