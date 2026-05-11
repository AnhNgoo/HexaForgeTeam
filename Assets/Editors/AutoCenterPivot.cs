using UnityEngine;
using UnityEditor;
using System.IO;

public class AutoCenterPivot : EditorWindow
{
    private enum OffsetSpace
    {
        Local,
        World
    }

    private const string DefaultFolderPath = "Assets/GeneratedMeshes";

    private Vector3 pivotOffset = Vector3.zero;
    private bool useMeshCenter = true;
    private string folderPath = DefaultFolderPath;
    private OffsetSpace offsetSpace = OffsetSpace.Local;
    private Vector3 rotationOffset = Vector3.zero;
    private bool keepWorldRotation = true;

    [MenuItem("Tools/Center Pivot and Save Mesh")]
    public static void Open()
    {
        GetWindow<AutoCenterPivot>("Pivot Offset");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Pivot Offset", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Chỉnh pivot theo giá trị XYZ, hoặc dùng tâm mesh làm gốc.", MessageType.Info);

        useMeshCenter = EditorGUILayout.Toggle("Use Mesh Center", useMeshCenter);
        using (new EditorGUI.DisabledScope(!useMeshCenter))
        {
            EditorGUILayout.LabelField("(Nếu bật, offset sẽ tính từ tâm của Mesh)");
        }

        pivotOffset = EditorGUILayout.Vector3Field("Offset (XYZ)", pivotOffset);
        offsetSpace = (OffsetSpace)EditorGUILayout.EnumPopup("Offset Space", offsetSpace);
        rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset (XYZ)", rotationOffset);
        keepWorldRotation = EditorGUILayout.Toggle("Keep World Rotation", keepWorldRotation);
        folderPath = EditorGUILayout.TextField("Save Folder", folderPath);

        EditorGUILayout.Space(6);
        if (GUILayout.Button("Apply to Selected"))
        {
            CenterPivotAndSave(pivotOffset, useMeshCenter, folderPath, offsetSpace, rotationOffset, keepWorldRotation);
        }
    }

    private static void CenterPivotAndSave(
        Vector3 offset,
        bool fromMeshCenter,
        string saveFolder,
        OffsetSpace space,
        Vector3 rotationOffsetEuler,
        bool preserveWorldRotation
    )
    {
        if (string.IsNullOrWhiteSpace(saveFolder))
            saveFolder = DefaultFolderPath;

        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
            AssetDatabase.Refresh();
        }

        foreach (GameObject obj in Selection.gameObjects)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            Mesh mesh = mf.sharedMesh;
            Vector3 baseOffset = fromMeshCenter ? mesh.bounds.center : Vector3.zero;
            Vector3 offsetLocal = space == OffsetSpace.World
                ? obj.transform.InverseTransformVector(offset)
                : offset;
            Vector3 totalOffset = baseOffset + offsetLocal;

            Quaternion rotationOffset = Quaternion.Euler(rotationOffsetEuler);

            Mesh newMesh = Instantiate(mesh);
            Vector3[] vertices = newMesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i] - totalOffset;
                v = rotationOffset * v;
                vertices[i] = v;
            }

            newMesh.vertices = vertices;
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();

            string meshPath = $"{saveFolder}/{obj.name}_{obj.GetInstanceID()}_Pivot.asset";
            AssetDatabase.CreateAsset(newMesh, meshPath);
            AssetDatabase.SaveAssets();

            mf.sharedMesh = newMesh;
            obj.transform.position += obj.transform.TransformVector(totalOffset);

            if (preserveWorldRotation)
            {
                obj.transform.localRotation = obj.transform.localRotation * Quaternion.Inverse(rotationOffset);
            }

            EditorUtility.SetDirty(obj);

            Debug.Log($"Đã lưu Mesh mới tại: {meshPath}");
        }
    }
}