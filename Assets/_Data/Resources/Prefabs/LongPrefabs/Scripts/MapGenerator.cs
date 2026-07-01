using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private PrefabRandomruins randomruins;
    // Start is called before the first frame update
    private IEnumerator Start()
    {
        randomruins.SpawnRuin();

        Debug.Log("Spawn xong");

        yield return null;

        Debug.Log("Build NavMesh");

        navMeshSurface.BuildNavMesh();

        Debug.Log("Build xong");
    }
}
