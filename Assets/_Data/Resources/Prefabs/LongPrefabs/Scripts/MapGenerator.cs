using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private PrefabRandomruins randomruins;
    public static bool IsNavMeshReady { get; private set; } = true;

    private void Awake()
    {
        IsNavMeshReady = false;
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        randomruins.SpawnRuins();

        Debug.Log("Spawn xong");

        yield return null;

        Debug.Log("Build NavMesh");

        navMeshSurface.BuildNavMesh();
        IsNavMeshReady = true;

        Debug.Log("Build xong");
    }

    private void OnDestroy()
    {
        IsNavMeshReady = true;
    }
}
