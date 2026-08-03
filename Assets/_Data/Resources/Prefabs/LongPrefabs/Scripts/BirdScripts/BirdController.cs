using System.Collections;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Transform destination;
    [SerializeField] private ParticleSystem disableBirdParticleSystem;

    [Header("Movement")]
    [SerializeField] private float flySpeed = 8f;
    [SerializeField] private float destinationThreshold = 0.1f;

    [Header("Drop")]
    [Tooltip("Khoảng cách đặt player thấp hơn Grab Point khi được thả.")]
    [SerializeField] private float dropOffset = 0.25f;

    private CharacterBase player;

    private bool isFlying;
    private bool isDropping;
    private bool playerReleased;
    private bool isCleaningUp;

    public void GrabPlayer(CharacterBase character)
    {
        // Ngăn chim bắt player nhiều lần.
        if (isFlying || isDropping || player != null)
            return;

        if (character == null ||
            grabPoint == null ||
            destination == null)
        {
            Debug.LogError(
                "BirdController đang thiếu Player, GrabPoint hoặc Destination!",
                this
            );

            return;
        }

        player = character;
        playerReleased = false;
        isCleaningUp = false;

        player.StateController.ChangeState(
            new BirdRideState(player)
        );

        CharacterMovement movement =
            player.CharacterMovement;

        if (movement != null)
        {
            CharacterController cc = movement.CC;

            // Tắt toàn bộ CharacterMovement để Update và
            // ApplyGravity không chạy trong lúc player đang bay.
            movement.enabled = false;

            if (cc != null)
                cc.enabled = false;
        }

        // Không parent player vào chim hoặc bone.
        player.transform.SetParent(null, true);

        player.transform.SetPositionAndRotation(
            grabPoint.position,
            grabPoint.rotation
        );

        transform.LookAt(destination.position);

        Physics.SyncTransforms();

        isFlying = true;
    }

    private void Update()
    {
        if (!isFlying || isDropping)
            return;

        if (destination == null)
        {
            Debug.LogError(
                "BirdController: Destination đã bị mất!",
                this
            );

            StartCoroutine(DropPlayerRoutine());
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination.position,
            flySpeed * Time.deltaTime
        );

        float remainingDistance = Vector3.Distance(
            transform.position,
            destination.position
        );

        if (remainingDistance <= destinationThreshold)
        {
            StartCoroutine(DropPlayerRoutine());
        }
    }

    private void LateUpdate()
    {
        if (!isFlying ||
            isDropping ||
            player == null ||
            grabPoint == null)
        {
            return;
        }

        // LateUpdate chạy sau Animator, do đó lấy được
        // vị trí cuối cùng của Grab Point trong frame.
        player.transform.SetPositionAndRotation(
            grabPoint.position,
            grabPoint.rotation
        );
    }

    private IEnumerator DropPlayerRoutine()
    {
        if (isDropping || playerReleased)
            yield break;

        isDropping = true;
        isFlying = false;

        if (player == null)
        {
            playerReleased = true;
            Destroy(gameObject);
            yield break;
        }

        CharacterBase releasedPlayer = player;

        CharacterMovement movement =
            releasedPlayer.CharacterMovement;

        CharacterController cc =
            movement != null ? movement.CC : null;

        // Tắt collider của chim để tránh collider chim
        // đẩy CharacterController của player.
        Collider[] birdColliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider birdCollider in birdColliders)
        {
            if (birdCollider != null)
                birdCollider.enabled = false;
        }

        Vector3 releasePosition =
            releasedPlayer.transform.position;

        Quaternion releaseRotation =
            releasedPlayer.transform.rotation;

        if (grabPoint != null)
        {
            releasePosition =
                grabPoint.position +
                Vector3.down * dropOffset;

            releaseRotation = grabPoint.rotation;
        }

        // CharacterController vẫn đang tắt nên có thể
        // thay đổi Transform trực tiếp.
        releasedPlayer.transform.SetParent(null, true);

        releasedPlayer.transform.SetPositionAndRotation(
            releasePosition,
            releaseRotation
        );

        if (movement != null)
        {
            // Xóa trạng thái grounded còn lưu từ trước.
            movement.IsGrounded = false;
        }

        Physics.SyncTransforms();

        // Cho Physics một frame để cập nhật vị trí mới.
        yield return null;

        if (releasedPlayer == null)
        {
            playerReleased = true;
            player = null;

            Destroy(gameObject);
            yield break;
        }

        // Bật CharacterController trước.
        if (cc != null)
            cc.enabled = true;

        Physics.SyncTransforms();

        // Sau đó mới bật CharacterMovement.
        if (movement != null)
        {
            movement.IsGrounded = false;
            movement.enabled = true;
        }

        // Đánh dấu trước khi Destroy để OnDisable và
        // OnDestroy không giải phóng player lần thứ hai.
        playerReleased = true;
        player = null;

        // Player được thả trên không nên chuyển sang FallState.
        releasedPlayer.StateController.ChangeState(
            new JumpState(releasedPlayer)
        );

        PlayDisableParticle();

        Destroy(gameObject);
    }

    private void ReleasePlayerIfNeeded()
    {
        // Phương thức này chạy khi chim biến mất vì đổi scene,
        // bị disable hoặc bị destroy ngoài DropPlayerRoutine.
        if (playerReleased || isCleaningUp)
            return;

        if (player == null)
            return;

        isCleaningUp = true;
        isFlying = false;
        isDropping = false;

        CharacterBase releasedPlayer = player;

        // Đánh dấu và xóa tham chiếu trước để tránh
        // OnDisable và OnDestroy xử lý hai lần.
        playerReleased = true;
        player = null;

        releasedPlayer.transform.SetParent(null, true);

        CharacterMovement movement =
            releasedPlayer.CharacterMovement;

        if (movement != null)
        {
            CharacterController cc = movement.CC;

            movement.IsGrounded = false;

            if (cc != null)
                cc.enabled = true;

            movement.enabled = true;
        }

        Physics.SyncTransforms();

        // Thoát khỏi BirdRideState để player không bị
        // kẹt animation khi sang scene khác.
        releasedPlayer.StateController.ChangeState(
            new FallState(releasedPlayer)
        );

        isCleaningUp = false;
    }

    private void OnDisable()
    {
        ReleasePlayerIfNeeded();
    }

    private void OnDestroy()
    {
        ReleasePlayerIfNeeded();
    }

    private void PlayDisableParticle()
    {
        if (disableBirdParticleSystem == null)
        {
            Debug.LogWarning(
                "BirdController chưa được gán particle biến mất!",
                this
            );

            return;
        }

        // Tạo một instance effect trong scene.
        ParticleSystem particleInstance = Instantiate(
            disableBirdParticleSystem,
            transform.position,
            transform.rotation
        );

        particleInstance.gameObject.SetActive(true);

        // Đảm bảo particle bắt đầu lại từ đầu.
        particleInstance.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        particleInstance.Play(true);

        ParticleSystem.MainModule main =
            particleInstance.main;

        float destroyDelay =
            main.duration +
            main.startLifetime.constantMax +
            0.5f;

        Destroy(
            particleInstance.gameObject,
            destroyDelay
        );

        Debug.Log(
            $"Đã phát effect chim biến mất tại {transform.position}",
            particleInstance
        );
    }

    public void SetupRoute(Transform newDestination)
    {
        destination = newDestination;

        if (destination == null)
        {
            Debug.LogError(
                "BirdController: Destination được truyền vào đang null!",
                this
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (grabPoint != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                grabPoint.position,
                0.15f
            );

            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(
                grabPoint.position,
                grabPoint.position +
                Vector3.down * dropOffset
            );
        }

        if (destination != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                destination.position,
                destinationThreshold
            );
        }
    }
}