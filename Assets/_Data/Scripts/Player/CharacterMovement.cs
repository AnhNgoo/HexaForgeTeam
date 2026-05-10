using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : LoadComponents
{
    [SerializeField] private Rigidbody rb;
    public Rigidbody Rb => rb;

    [Header("Walk Settings")]
    [SerializeField] private float walkSpeedMultiplier = 0.3f;
    [SerializeField] private float walkThreshold = 0.3f;
    public float WalkThreshold => walkThreshold;
    [Header("Run Settings")]
    [SerializeField] private float runSpeedMultiplier = 1f;
    [SerializeField] private float runThreshold = 0.75f;
    public float RunThreshold => runThreshold;
    [Header("Sprint Settings")]
    [SerializeField] private float sprintSpeedMultiplier = 1.3f;
    [SerializeField] private float sprintThreshold = 1.0f;
    public float SprintThreshold => sprintThreshold;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeSpeedMultiplier = 2f;
    [SerializeField] private float dodgeDuration = 0.5f;
    public float DodgeDuration => dodgeDuration;
    [SerializeField] private float dodgeCooldown = 1f;
    public float DodgeCooldown => dodgeCooldown;
    public bool IsDodging { get; set; } = false;
    public float dodgeTimer { get; set; } = 0f;



    [Header("Lunge Settings")]
    [SerializeField] private float lungeDistance = 1f;
    [SerializeField] private float lungeDuration = 0.2f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float airSpeedMultiplier = 1f;
    public bool JumpLanding { get; set; } = false;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    public bool IsGrounded { get; set; } = false;
    public bool CanMoveAttack { get; set; } = false;
    public Vector2 MoveDirection { get; private set; }

    protected override void LoadComponent()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void SetMoveDirection(Vector2 direction)
    {
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;
        Vector3 right = Camera.main.transform.right;
        right.y = 0f;
        Vector3 moveDirection3D = direction.x * right.normalized + direction.y * forward.normalized;
        MoveDirection = new Vector2(moveDirection3D.x, moveDirection3D.z);
    }

    private void Movement(Vector2 direction, float moveSpeed, float speedMultiplier)
    {
        Vector3 moveDirection = new Vector3(direction.x, 0, direction.y);
        Vector3 targetVelocity = moveDirection.normalized * moveSpeed * speedMultiplier;
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
    }

    public void Walk(Vector2 direction, float moveSpeed)
    {
        Movement(direction, moveSpeed, walkSpeedMultiplier);
    }

    public void Run(Vector2 direction, float moveSpeed)
    {
        Movement(direction, moveSpeed, runSpeedMultiplier);
    }

    public void Sprint(Vector2 direction, float moveSpeed)
    {
        Movement(direction, moveSpeed, sprintSpeedMultiplier);
    }

    public void Dodge(Vector2 direction, float moveSpeed)
    {
        Movement(direction, moveSpeed, dodgeSpeedMultiplier);
    }

    public void Lunge(Vector2 direction)
    {
        // Di chuyển nhân vật đến vị trí mục tiêu trong một khoảng thời gian ngắn
        StartCoroutine(LungeCoroutine(direction));
    }

    private IEnumerator LungeCoroutine(Vector2 direction)
    {
        Debug.Log("Lunge in direction: " + direction);
        float speed = lungeDistance / lungeDuration;
        float elapsedTime = 0f;

        while (elapsedTime < lungeDuration)
        {
            Movement(direction, speed, 1f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Stop();
    }

    public void Jump()
    {
        if (rb == null) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void MoveAir(Vector2 direction, float moveSpeed)
    {
        Movement(direction, moveSpeed, airSpeedMultiplier);
    }
    public void Stop()
    {
        if (rb == null) return;

        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    public void CheckGrounded()
    {
        IsGrounded = Physics.OverlapSphere(transform.position + Vector3.down * groundCheckDistance,
                                                groundCheckRadius, groundLayer).Length > 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
