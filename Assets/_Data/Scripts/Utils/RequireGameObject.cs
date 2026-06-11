using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Giúp đảm bảo rằng các GameObject cần thiết luôn tồn tại trong scene. Nếu không tìm thấy, nó sẽ tự động tạo ra chúng từ prefab đã chỉ định.
/// </summary>
public class RequireGameObject : MonoBehaviour
{
    [SerializeField] private List<GameObject> requiredGameObjects;

    [Button("Setup Required GameObjects")]
    private void SetupRequiredGameObjects()
    {
        foreach (var obj in requiredGameObjects)
        {
            GameObject foundObj = GameObject.Find(obj.name);

            if (foundObj != null)
                continue;
            GameObject instantiatedObj =
                (GameObject)PrefabUtility.InstantiatePrefab(obj);
            instantiatedObj.name = obj.name;
        }
    }
}