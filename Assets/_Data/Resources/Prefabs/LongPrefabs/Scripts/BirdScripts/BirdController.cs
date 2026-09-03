using System.Collections;
using UnityEngine;

public class BirdController : Singleton<BirdController>
{
    [Header("References")]
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Transform destination;
    [SerializeField] private ParticleSystem disableBirdParticleSystem;

    [Header("Movement")]
    [SerializeField] private float flySpeed = 8f;
    [SerializeField] private float destinationThreshold = 0.1f;
    [Tooltip("Thời gian chờ route được gán nếu GrabPlayer được gọi quá sớm.")]
    [SerializeField] private float destinationWaitTimeout = 2f;

    [Header("Drop")]
    [Tooltip("Khoảng cách đặt player thấp hơn Grab Point khi được thả.")]
    [SerializeField] private float dropOffset = 0.25f;

    private CharacterBase player;
    private Coroutine waitForDestinationRoutine;
    private bool isFlying;
    private bool isDropping;
    private bool playerReleased;

    public bool HasDestination => destination != null;
    private bool IsBusy => isFlying || isDropping || player != null;

    public void SetupRoute(Transform newDestination)
    {
        destination = newDestination;

        if (destination == null)
            Debug.LogWarning("BirdController nhận Destination null.", this);
    }

    public void GrabPlayer(CharacterBase character)
    {
        if (character == null || IsBusy || waitForDestinationRoutine != null)
            return;

        if (!HasDestination)
        {
            if (isActiveAndEnabled)
                waitForDestinationRoutine = StartCoroutine(WaitForDestination(character));

            return;
        }

        BeginFlight(character);
    }

    private IEnumerator WaitForDestination(CharacterBase character)
    {
        float elapsed = 0f;

        while (!HasDestination && elapsed < destinationWaitTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        waitForDestinationRoutine = null;

        if (HasDestination)
        {
            GrabPlayer(character);
            yield break;
        }

        Debug.LogWarning(
            $"BirdController không nhận được Destination sau {destinationWaitTimeout:0.##} giây.",
            this
        );
    }

    private void BeginFlight(CharacterBase character)
    {
        if (grabPoint == null || character.StateController == null)
        {
            Debug.LogError("BirdController thiếu Grab Point hoặc Player chưa sẵn sàng.", this);
            return;
        }

        player = character;
        playerReleased = false;

        player.StateController.ChangeState(new BirdRideState(player));
        SetPlayerMovementEnabled(player, false);

        player.transform.SetParent(null, true);
        player.transform.SetPositionAndRotation(grabPoint.position, grabPoint.rotation);

        transform.LookAt(destination.position);
        Physics.SyncTransforms();
        isFlying = true;
    }

    private void Update()
    {
        if (!isFlying || isDropping)
            return;

        if (!HasDestination)
        {
            Debug.LogError("BirdController bị mất Destination khi đang bay.", this);
            StartDrop();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination.position,
            flySpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, destination.position) <= destinationThreshold)
            StartDrop();
    }

    private void LateUpdate()
    {
        if (!isFlying || isDropping || player == null || grabPoint == null)
            return;

        player.transform.SetPositionAndRotation(grabPoint.position, grabPoint.rotation);
    }

    private void StartDrop()
    {
        if (!isDropping && !playerReleased)
            StartCoroutine(DropPlayerRoutine());
    }

    private IEnumerator DropPlayerRoutine()
    {
        isDropping = true;
        isFlying = false;

        if (player == null)
        {
            playerReleased = true;
            Destroy(gameObject);
            yield break;
        }

        CharacterBase releasedPlayer = player;
        DisableBirdColliders();
        MovePlayerToDropPoint(releasedPlayer);

        yield return null;

        if (releasedPlayer == null)
        {
            FinishRelease();
            Destroy(gameObject);
            yield break;
        }

        RestorePlayerControl(releasedPlayer, useJumpState: true);
        FinishRelease();
        PlayDisableParticle();
        Destroy(gameObject);
    }

    private void MovePlayerToDropPoint(CharacterBase releasedPlayer)
    {
        Vector3 position = releasedPlayer.transform.position;
        Quaternion rotation = releasedPlayer.transform.rotation;

        if (grabPoint != null)
        {
            position = grabPoint.position + Vector3.down * dropOffset;
            rotation = grabPoint.rotation;
        }

        releasedPlayer.transform.SetParent(null, true);
        releasedPlayer.transform.SetPositionAndRotation(position, rotation);

        if (releasedPlayer.CharacterMovement != null)
            releasedPlayer.CharacterMovement.IsGrounded = false;

        Physics.SyncTransforms();
    }

    private void DisableBirdColliders()
    {
        foreach (Collider birdCollider in GetComponentsInChildren<Collider>())
        {
            if (birdCollider != null)
                birdCollider.enabled = false;
        }
    }

    public void ReleasePlayerIfNeeded()
    {
        if (playerReleased || player == null)
            return;

        CharacterBase releasedPlayer = player;
        isFlying = false;
        isDropping = false;

        releasedPlayer.transform.SetParent(null, true);
        RestorePlayerControl(releasedPlayer, useJumpState: false);
        FinishRelease();
    }

    private void RestorePlayerControl(CharacterBase releasedPlayer, bool useJumpState)
    {
        SetPlayerMovementEnabled(releasedPlayer, true);
        Physics.SyncTransforms();

        if (releasedPlayer.StateController != null)
        {
            if (useJumpState)
                releasedPlayer.StateController.ChangeState(new JumpState(releasedPlayer));
            else
                releasedPlayer.StateController.ChangeState(new FallState(releasedPlayer));
        }

        if (releasedPlayer.CharacterInput != null)
            releasedPlayer.CharacterInput.LockInput = false;
    }

    private static void SetPlayerMovementEnabled(CharacterBase character, bool enabled)
    {
        CharacterMovement movement = character.CharacterMovement;

        if (movement == null)
            return;

        movement.IsGrounded = false;

        if (enabled)
        {
            if (movement.CC != null)
                movement.CC.enabled = true;

            movement.enabled = true;
            return;
        }

        movement.enabled = false;

        if (movement.CC != null)
            movement.CC.enabled = false;
    }

    private void FinishRelease()
    {
        playerReleased = true;
        player = null;
    }

    private void OnDisable()
    {
        waitForDestinationRoutine = null;
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
            Debug.LogWarning("BirdController chưa được gán particle biến mất.", this);
            return;
        }

        ParticleSystem particle = Instantiate(
            disableBirdParticleSystem,
            transform.position,
            transform.rotation
        );

        particle.gameObject.SetActive(true);
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particle.Play(true);

        ParticleSystem.MainModule main = particle.main;
        float lifetime = main.duration + main.startLifetime.constantMax + 0.5f;
        Destroy(particle.gameObject, lifetime);
    }

    private void OnDrawGizmosSelected()
    {
        if (grabPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grabPoint.position, 0.15f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                grabPoint.position,
                grabPoint.position + Vector3.down * dropOffset
            );
        }

        if (destination == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(destination.position, destinationThreshold);
    }
}
