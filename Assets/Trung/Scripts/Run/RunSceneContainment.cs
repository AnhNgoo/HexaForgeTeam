using UnityEngine;
using UnityEngine.SceneManagement;

public class RunSceneContainment : MonoBehaviour
{
    public static RunSceneContainment Instance { get; private set; }

    [Header("Run Scene Container")]
    [Tooltip("Kéo thả Run_Scene_Container trong Scene hầm ngục vào đây.")]
    [SerializeField] private Transform runContainerParent;

    private Scene myRunScene;

    private void Awake()
    {
        Instance = this;
        myRunScene = gameObject.scene;

        if (runContainerParent == null)
        {
            runContainerParent = this.transform;
        }

        ForceSetActiveScene();
    }

    private void Start()
    {
        ForceSetActiveScene();
    }

    private void OnEnable()
    {
        ForceSetActiveScene();
    }

    public Transform GetContainerParent()
    {
        return runContainerParent;
    }

    private void ForceSetActiveScene()
    {
        if (myRunScene.isLoaded && SceneManager.GetActiveScene() != myRunScene)
        {
            SceneManager.SetActiveScene(myRunScene);
            Debug.Log($"[Containment] Active Scene successfully forced to: {myRunScene.name}");
        }
    }
}