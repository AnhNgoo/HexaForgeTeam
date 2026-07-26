using System.Collections;
using UnityEngine;

public class BirdRandomSpawner : MonoBehaviour
{
    [System.Serializable]
    private class BirdRoute
    {
        [Tooltip("Vị trí chim xuất hiện.")]
        public Transform spawnPoint;

        [Tooltip("Điểm chim bay tới.")]
        public Transform destination;
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
        if (!ValidateSpawner())
            yield break;

        BirdRoute selectedRoute = SelectRandomRoute();

        if (selectedRoute == null)
        {
            Debug.LogError(
                "BirdRandomSpawner không tìm thấy route hợp lệ!",
                this
            );

            yield break;
        }

        // Spawn chim.
        spawnedBird = Instantiate(
            birdPrefab,
            selectedRoute.spawnPoint.position,
            selectedRoute.spawnPoint.rotation
        );

        if (spawnedBird == null)
        {
            Debug.LogError(
                "Không thể spawn Bird Prefab!",
                this
            );

            yield break;
        }

        // Phải gán destination ngay sau khi spawn,
        // trước khi gọi GrabPlayer.
        spawnedBird.SetupRoute(
            selectedRoute.destination
        );

        if (showDebugLog)
        {
            Debug.Log(
                $"Đã spawn chim tại route {selectedRouteIndex + 1}: " +
                $"{selectedRoute.spawnPoint.name} -> " +
                $"{selectedRoute.destination.name}",
                spawnedBird
            );
        }

        // Tìm player sau khi chim và route đã sẵn sàng.
        yield return FindPlayerRoutine();

        if (player == null)
        {
            Debug.LogError(
                $"Không tìm thấy Player sau " +
                $"{findPlayerTimeout} giây!",
                this
            );

            yield break;
        }

        // Đợi CharacterBase khởi tạo StateController.
        float stateTimer = 0f;

        while (player.StateController == null &&
               stateTimer < findPlayerTimeout)
        {
            stateTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (player.StateController == null)
        {
            Debug.LogError(
                "Player đã tồn tại nhưng StateController chưa được khởi tạo!",
                player
            );

            yield break;
        }

        if (showDebugLog)
        {
            Debug.Log(
                $"Đã tìm thấy Player {player.name}. " +
                $"Đưa player lên chim {spawnedBird.name}.",
                player
            );
        }

        // Đợi thêm một frame để CharacterBase.Start()
        // và các component player hoàn thành khởi tạo.
        yield return null;

        spawnedBird.GrabPlayer(player);
    }

    private bool ValidateSpawner()
    {
        if (birdPrefab == null)
        {
            Debug.LogError(
                "BirdRandomSpawner chưa được gán Bird Prefab!",
                this
            );

            return false;
        }

        if (routes == null || routes.Length == 0)
        {
            Debug.LogError(
                "BirdRandomSpawner chưa có route!",
                this
            );

            return false;
        }

        return true;
    }

    private BirdRoute SelectRandomRoute()
    {
        int validRouteCount = 0;

        for (int i = 0; i < routes.Length; i++)
        {
            if (IsRouteValid(routes[i]))
                validRouteCount++;
        }

        if (validRouteCount == 0)
            return null;

        int randomValidIndex =
            Random.Range(0, validRouteCount);

        int validIndex = 0;

        for (int i = 0; i < routes.Length; i++)
        {
            if (!IsRouteValid(routes[i]))
                continue;

            if (validIndex == randomValidIndex)
            {
                selectedRouteIndex = i;
                return routes[i];
            }

            validIndex++;
        }

        return null;
    }

    private IEnumerator FindPlayerRoutine()
    {
        float timer = 0f;

        while (player == null &&
               timer < findPlayerTimeout)
        {
            GameObject playerObject = null;

            try
            {
                playerObject =
                    GameObject.FindGameObjectWithTag(playerTag);
            }
            catch (UnityException)
            {
                Debug.LogError(
                    $"Tag \"{playerTag}\" chưa tồn tại. " +
                    "Hãy tạo tag Player và gắn cho player.",
                    this
                );

                yield break;
            }

            if (playerObject != null)
            {
                player =
                    playerObject.GetComponent<CharacterBase>();

                if (player == null)
                {
                    player =
                        playerObject.GetComponentInChildren<CharacterBase>();
                }

                if (player == null)
                {
                    player =
                        playerObject.GetComponentInParent<CharacterBase>();
                }
            }

            if (player == null)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    private bool IsRouteValid(BirdRoute route)
    {
        return route != null &&
               route.spawnPoint != null &&
               route.destination != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (routes == null)
            return;

        for (int i = 0; i < routes.Length; i++)
        {
            BirdRoute route = routes[i];

            if (!IsRouteValid(route))
                continue;

            Gizmos.color = GetRouteColor(i);

            Gizmos.DrawWireSphere(
                route.spawnPoint.position,
                0.4f
            );

            Gizmos.DrawWireSphere(
                route.destination.position,
                0.4f
            );

            Gizmos.DrawLine(
                route.spawnPoint.position,
                route.destination.position
            );
        }
    }

    private Color GetRouteColor(int index)
    {
        switch (index % 3)
        {
            case 0:
                return Color.red;

            case 1:
                return Color.green;

            default:
                return Color.blue;
        }
    }
}