using UnityEngine;
using UnityEngine.SceneManagement;

public class StartRunTable : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField]
    private string sceneName = "TétTaoLao";

    public void OnInteract()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning(
                "Scene Name Empty");

            return;
        }

        SceneManager.LoadScene(
            sceneName);
    }
}