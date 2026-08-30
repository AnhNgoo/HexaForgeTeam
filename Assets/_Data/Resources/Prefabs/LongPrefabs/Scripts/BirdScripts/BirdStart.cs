using System.Collections;
using UnityEngine;

public class BirdStart : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BirdController bird;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Settings")]
    [SerializeField] private float findPlayerTimeout = 10f;

    private CharacterBase player;

    private IEnumerator Start()
    {
        FindBirdController();

        if (bird == null)
        {
            Debug.LogError(
                "BirdStart không tìm thấy BirdController " +
                "trên object hiện tại, object cha hoặc object con!",
                this
            );

            yield break;
        }

        yield return FindPlayerRoutine();

        if (player == null)
        {
            Debug.LogError(
                $"BirdStart không tìm thấy Player sau " +
                $"{findPlayerTimeout} giây!",
                this
            );

            yield break;
        }

        float timer = 0f;

        // Đợi CharacterBase.Start() khởi tạo StateController.
        while (player.StateController == null &&
               timer < findPlayerTimeout)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (player.StateController == null)
        {
            Debug.LogError(
                "Đã tìm thấy Player nhưng StateController chưa được khởi tạo!",
                player
            );

            yield break;
        }

        // Bird có thể được scene/spawner kích hoạt và gán route muộn hơn
        // BirdStart. Chờ tại object này vì coroutine không chạy được trên
        // chính BirdController khi GameObject Bird đang inactive.
        timer = 0f;

        while ((!bird.isActiveAndEnabled || !bird.HasDestination) &&
               timer < findPlayerTimeout)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!bird.isActiveAndEnabled || !bird.HasDestination)
        {
            Debug.LogWarning(
                "Bird chưa được kích hoạt hoặc chưa nhận Destination; " +
                "bỏ qua GrabPlayer để tránh khóa player.",
                bird
            );

            yield break;
        }

        Debug.Log(
            $"BirdStart đã tìm thấy Player: {player.name}. " +
            "Bắt đầu GrabPlayer().",
            this
        );

        bird.GrabPlayer(player);
    }

    private void FindBirdController()
    {
        if (bird != null)
            return;

        // Tìm trên chính GameObject này.
        bird = GetComponent<BirdController>();

        // Nếu BirdStart nằm ở object con.
        if (bird == null)
        {
            bird = GetComponentInParent<BirdController>();
        }

        // Nếu BirdController nằm ở object con.
        if (bird == null)
        {
            bird = GetComponentInChildren<BirdController>(true);
        }
    }

    private IEnumerator FindPlayerRoutine()
    {
        float timer = 0f;

        while (player == null &&
               timer < findPlayerTimeout)
        {
            GameObject playerObject = null;

            // Ưu tiên tìm bằng tag để không lấy nhầm NPC.
            try
            {
                playerObject =
                    GameObject.FindGameObjectWithTag(playerTag);
            }
            catch (UnityException)
            {
                Debug.LogError(
                    $"Tag \"{playerTag}\" chưa được tạo trong Unity. " +
                    "Hãy tạo tag Player và gắn vào GameObject player.",
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

                    if (player == null)
                    {
                        player =
                            playerObject.GetComponentInParent<CharacterBase>();
                    }
                }
            }

            if (player == null)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
