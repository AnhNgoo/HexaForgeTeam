using TMPro;
using UnityEngine;

public class WorldNameTag : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private string displayName;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (nameText != null)
        {
            nameText.SetTextSafe(displayName);
        }
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (nameText != null)
        {
            nameText.SetTextSafe(displayName);
        }
    }
#endif
}