using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdRandomSpawner : MonoBehaviour
{
    [System.Serializable]
    private sealed class BirdRoute
    {
        [Tooltip("Vị trí chim xuất hiện.")]
        public Transform spawnPoint = null;

        [Tooltip("Điểm chim bay tới.")]
        public Transform destination = null;

        public bool IsValid => spawnPoint != null && destination != null;
    }

    [Header("Bird")]
    [SerializeField] private BirdController birdPrefab;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float findPlayerTimeout = 10f;

    [Header("Random Routes")]
    [SerializeField] private BirdRoute[] routes;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private BirdController spawnedBird;
    private CharacterBase player;
    private int selectedRouteIndex = -1;

    private IEnumerator Start()
    {
        if (!TrySelectRoute(out BirdRoute route))
            yield break;

        SpawnBird(route);

        if (spawnedBird == null)
            yield break;

        yield return WaitForPlayerReady();

        if (player == null || player.StateController == null)
            yield break;

        // Cho CharacterBase.Start và các component liên quan hoàn tất trong frame hiện tại.
        yield return null;

        if (spawnedBird != null)
            spawnedBird.GrabPlayer(player);
    }

    private bool TrySelectRoute(out BirdRoute selectedRoute)
    {
        selectedRoute = null;

        if (birdPrefab == null)
        {
            Debug.LogError("BirdRandomSpawner chưa được gán Bird Prefab.", this);
            return false;
        }

        if (routes == null || routes.Length == 0)
        {
            Debug.LogError("BirdRandomSpawner chưa có route.", this);
            return false;
        }

        List<int> validIndexes = new List<int>();

        for (int i = 0; i < routes.Length; i++)
        {
            if (routes[i] != null && routes[i].IsValid)
                validIndexes.Add(i);
        }

        if (validIndexes.Count == 0)
        {
            Debug.LogError("BirdRandomSpawner không có route hợp lệ.", this);
            return false;
        }

        selectedRouteIndex = validIndexes[Random.Range(0, validIndexes.Count)];
        selectedRoute = routes[selectedRouteIndex];
        return true;
    }

    private void SpawnBird(BirdRoute route)
    {
        spawnedBird = Instantiate(
            birdPrefab,
            route.spawnPoint.position,
            route.spawnPoint.rotation
        );

        if (spawnedBird == null)
        {
            Debug.LogError("BirdRandomSpawner không thể spawn Bird Prefab.", this);
            return;
        }

        spawnedBird.SetupRoute(route.destination);

        if (showDebugLog)
        {
            Debug.Log(
                $"Bird route {selectedRouteIndex + 1}: " +
                $"{route.spawnPoint.name} -> {route.destination.name}",
                spawnedBird
            );
        }
    }

    private IEnumerator WaitForPlayerReady()
    {
        float elapsed = 0f;

        while (elapsed < findPlayerTimeout)
        {
            if (player == null)
            {
                GameObject playerObject;

                try
                {
                    playerObject = GameObject.FindGameObjectWithTag(playerTag);
                }
                catch (UnityException)
                {
                    Debug.LogError(
                        $"Tag \"{playerTag}\" chưa tồn tại. Hãy tạo tag và gắn cho Player.",
                        this
                    );
                    yield break;
                }

                player = FindCharacterBase(playerObject);
            }

            if (player != null && player.StateController != null)
                yield break;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.LogError(
            $"BirdRandomSpawner không tìm thấy Player sẵn sàng sau {findPlayerTimeout:0.##} giây.",
            this
        );
    }

    private static CharacterBase FindCharacterBase(GameObject playerObject)
    {
        if (playerObject == null)
            return null;

        CharacterBase foundPlayer = playerObject.GetComponent<CharacterBase>();

        if (foundPlayer == null)
            foundPlayer = playerObject.GetComponentInChildren<CharacterBase>();

        if (foundPlayer == null)
            foundPlayer = playerObject.GetComponentInParent<CharacterBase>();

        return foundPlayer;
    }

    private void OnDrawGizmosSelected()
    {
        if (routes == null)
            return;

        for (int i = 0; i < routes.Length; i++)
        {
            BirdRoute route = routes[i];

            if (route == null || !route.IsValid)
                continue;

            Gizmos.color = GetRouteColor(i);
            Gizmos.DrawWireSphere(route.spawnPoint.position, 0.4f);
            Gizmos.DrawWireSphere(route.destination.position, 0.4f);
            Gizmos.DrawLine(route.spawnPoint.position, route.destination.position);
        }
    }

    private static Color GetRouteColor(int index)
    {
        switch (index % 3)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            default: return Color.blue;
        }
    }
}
