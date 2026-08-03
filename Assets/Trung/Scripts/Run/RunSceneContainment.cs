using UnityEngine;
using UnityEngine.SceneManagement;

public class RunSceneContainment : MonoBehaviour
{
    public static RunSceneContainment Instance { get; private set; }

    [Header("Run Scene Container")]
    [Tooltip("Kéo thả Run_Scene_Container trong Scene hầm ngục vào đây.")]
    [SerializeField] private Transform runContainerParent;
    [SerializeField] private string lobbySceneName = "LobbyMain Scene";

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
        ContainExistingLooseObjects();
    }

    private void OnEnable()
    {
        ForceSetActiveScene();
    }

    private void LateUpdate()
    {
        // Liên tục quét bảo vệ: Hốt sạch các Clone lỡ bị đẻ vào LobbyMainScene đưa về Run Scene Container
        ContainExistingLooseObjects();
    }

    public Transform GetContainerParent()
    {
        return runContainerParent;
    }

    public void ForceSetActiveScene()
    {
        if (myRunScene.isLoaded && SceneManager.GetActiveScene() != myRunScene)
        {
            SceneManager.SetActiveScene(myRunScene);
            Debug.Log($"[Containment] Active Scene successfully forced to: {myRunScene.name}");
        }
    }

    /// <summary>
    /// Chuyển đối tượng bị đẻ nhầm vào đúng Run Scene Container
    /// </summary>
    public void ContainObject(GameObject obj)
    {
        if (obj == null) return;

        if (runContainerParent != null)
        {
            obj.transform.SetParent(runContainerParent, true);
        }

        if (myRunScene.isLoaded && obj.scene != myRunScene)
        {
            SceneManager.MoveGameObjectToScene(obj, myRunScene);
        }
    }

    /// <summary>
    /// Quét LobbyMainScene để giải cứu toàn bộ object thuộc hầm ngục bị Instantiate tràn ra ngoài
    /// </summary>
    private void ContainExistingLooseObjects()
    {
        if (!myRunScene.isLoaded) return;

        Scene lobbyScene = SceneManager.GetSceneByName(lobbySceneName);
        if (!lobbyScene.isLoaded) return;

        GameObject[] rootObjects = lobbyScene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject obj = rootObjects[i];
            if (obj == null) continue;

            string name = obj.name;
            // Nhận diện các Object dạng (Clone) thuộc Run Scene bị đẻ nhầm ra ngoài
            if (name.Contains("(Clone)") && !name.Contains("Player") && !name.Contains("UI"))
            {
                ContainObject(obj);
            }
        }
    }
}