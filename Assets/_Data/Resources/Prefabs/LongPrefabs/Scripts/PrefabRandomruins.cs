using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PrefabRandomruins : MonoBehaviour
{
    public SpawnData[] spawnDatas;
    public Transform target;

    [SerializeField] private Transform spawnedParent;

    [ContextMenu("Spawn Ruins")]
    public void SpawnRuins()
    {
        ClearRuins();

        Transform parent = GetOrCreateSpawnedParent();

        foreach (SpawnData data in spawnDatas)
        {
            if (data.prefabs == null || data.spawnPoints == null)
                continue;

            int spawnCount = Mathf.Min(
                data.spawnPoints.Length,
                data.prefabs.Length
            );

            List<int> prefabIndexes =
                Enumerable.Range(0, data.prefabs.Length).ToList();

            Shuffle(prefabIndexes);

            List<int> pointIndexes =
                Enumerable.Range(0, data.spawnPoints.Length).ToList();

            Shuffle(pointIndexes);

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject prefab = data.prefabs[prefabIndexes[i]];
                Transform point = data.spawnPoints[pointIndexes[i]];

                if (prefab == null || point == null)
                    continue;

                Ray ray = new Ray(
                    point.position + Vector3.up * 40f,
                    Vector3.down
                );

                if (!TryGetGroundHitInThisScene(ray, out RaycastHit hit))
                {
                    Debug.LogWarning(
                        $"Không tìm thấy mặt đất thuộc scene '{gameObject.scene.name}' " +
                        $"cho điểm spawn '{point.name}'.",
                        point
                    );
                    continue;
                }

                GameObject spawnedObject = Instantiate(
                    prefab,
                    hit.point + Vector3.up * data.yOffset,
                    Quaternion.identity,
                    parent
                );

                spawnedObject.name = prefab.name;

                if (target != null)
                {
                    Vector3 direction =
                        target.position - spawnedObject.transform.position;

                    direction.y = 0;

                    if (direction.sqrMagnitude > 0.001f)
                    {
                        spawnedObject.transform.rotation =
                            Quaternion.LookRotation(direction);
                    }
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.RegisterCreatedObjectUndo(
                        spawnedObject,
                        "Spawn Ruin"
                    );
                }
#endif
            }
        }
    }

    [ContextMenu("Clear Ruins")]
    public void ClearRuins()
    {
        if (spawnedParent == null)
            return;

        for (int i = spawnedParent.childCount - 1; i >= 0; i--)
        {
            GameObject child =
                spawnedParent.GetChild(i).gameObject;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(child);
            }
            else
#endif
            {
                Destroy(child);
            }
        }
    }

    private Transform GetOrCreateSpawnedParent()
    {
        if (spawnedParent != null)
            return spawnedParent;

        GameObject parentObject =
            new GameObject("Spawned Ruins");

        parentObject.transform.SetParent(transform);
        spawnedParent = parentObject.transform;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RegisterCreatedObjectUndo(
                parentObject,
                "Create Spawned Ruins Parent"
            );

            EditorUtility.SetDirty(this);
        }
#endif

        return spawnedParent;
    }

    private void Shuffle(List<int> indexes)
    {
        for (int i = 0; i < indexes.Count; i++)
        {
            int randomIndex =
                Random.Range(i, indexes.Count);

            (indexes[i], indexes[randomIndex]) =
                (indexes[randomIndex], indexes[i]);
        }
    }

    private bool TryGetGroundHitInThisScene(Ray ray, out RaycastHit closestHit)
    {
        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            100f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        closestHit = default;
        float closestDistance = float.PositiveInfinity;
        bool foundHit = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.scene != gameObject.scene)
                continue;

            if (hit.distance >= closestDistance)
                continue;

            closestHit = hit;
            closestDistance = hit.distance;
            foundHit = true;
        }

        return foundHit;
    }
}
