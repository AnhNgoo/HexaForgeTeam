using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : LoadComponents
{
    [SerializeField] private CharacterController cc;
    public CharacterController CC => cc;
    [SerializeField] private bool debugMode = false;
    [SerializeField] private float strafeThreshold = 0.8f;
    public float StrafeThreshold => strafeThreshold;

    [Header("Walk Settings")]
    [SerializeField] private float walkSpeedMultiplier = 0.3f;

    [Header("Run Settings")]
    [SerializeField] private float runSpeedMultiplier = 1f;

    [Header("Sprint Settings")]
    [SerializeField] private float sprintSpeedMultiplier = 1.3f;

    [Header("Dodge Settings")]
    [SerializeField] private float dodgeSpeedMultiplier = 1f;
    [SerializeField] private float dodgeDuration = 0.5f;
    public float DodgeDuration => dodgeDuration;
    [SerializeField] private float dodgeCooldown = 1f;
    public float DodgeCooldown => dodgeCooldown;
    public bool IsDodging { get; set; } = false;

    [Header("Lunge Settings")]
    [SerializeField] private float lungeSpeedMultiplier = 10f;
    public bool IsLunging { get; set; } = false;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 40f;
    [SerializeField] private float airSpeedMultiplier = 1f;
    [SerializeField] private float fallThreshold = -50f;
    public float FallThreshold => fallThreshold;
    public bool JumpLanding { get; set; } = false;

    [Header("KnockBack Settings")]
    [SerializeField] private float knockBackForce = 10f;
    [SerializeField] private float knockBackDuration = 0.05f;

    [Header("Wall Edge Settings")]
    [SerializeField] private LayerMask wallEdgeLayer;
    [SerializeField] private float radiusCheck = 0.5f;
    [SerializeField] private float distanceCheck = 1f;
    public bool WallEdge { get; set; } = false;
    public bool CanWallJump { get; set; } = true;

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -100f;
    public bool UseGravity = true;

    public bool IsGrounded { get; set; } = false;
    public CollisionFlags flags { get; private set; } // Cờ giúp phát hiện va chạm với mặt đất, tường hoặc trần nhà
    public Vector2 MoveDirection { get; private set; }
    private Vector3 CurrentMove; // Hướng di chuyển cuối cùng sau khi áp dụng tất cả các hiệu ứng (dodge, lunge, jump, v.v.)

    private float verticalVelocity;
    public float VerticalVelocity => verticalVelocity;

    private float _movementLockedUntil;

    protected override void LoadComponent()
    {
        if (cc == null)
            cc = GetComponent<CharacterController>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void Update()
    {
        ApplyGravity();
        CheckGrounded();
        CheckWallEdge();
    }

    // Thiết lập hướng di chuyển dựa trên input và hướng camera
    public void SetMoveDirection(Vector2 direction)
    {
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;

        Vector3 right = Camera.main.transform.right;
        right.y = 0f;

        Vector3 moveDirection3D =
            direction.x * right.normalized +
            direction.y * forward.normalized;

        MoveDirection = new Vector2(
            moveDirection3D.x,
            moveDirection3D.z
        );
    }

    private void ApplyGravity()
    {
        if (UseGravity && IsGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (UseGravity)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 finalMove = CurrentMove;
        if (UseGravity)
        {
            finalMove.y = verticalVelocity;
        }

        flags =
            cc.Move(finalMove * Time.deltaTime);

        // Hit ceiling
        if ((flags & CollisionFlags.Above) != 0) // Nếu va chạm với trần nhà, đặt verticalVelocity về 0 để ngăn nhân vật tiếp tục tăng tốc lên trên
        {
            verticalVelocity = gravity * Time.deltaTime;
        }

        // Grounded
        IsGrounded =
            (flags & CollisionFlags.Below) != 0; // Cập nhật grounded sau khi di chuyển để đảm bảo chính xác
    }

    public void Movement(Vector3 direction, float moveSpeed, float speedMultiplier = 1)
    {
        Vector3 moveDirection = new Vector3(
            direction.x,
            direction.y,
            direction.z
        );

        CurrentMove =
            moveDirection.normalized *
            moveSpeed *
            speedMultiplier;
    }
    public void Walk(Vector2 direction, float moveSpeed)
    {
        if (Time.time < _movementLockedUntil)
        {
            Stop();
            return;
        }
        Movement(new Vector3(direction.x, 0, direction.y), moveSpeed, walkSpeedMultiplier);
    }

    public void Run(Vector2 direction, float moveSpeed)
    {
        if (Time.time < _movementLockedUntil)
        {
            Stop();
            return;
        }
        Movement(new Vector3(direction.x, 0, direction.y), moveSpeed, runSpeedMultiplier);
    }

    public void Sprint(Vector2 direction, float moveSpeed)
    {
        if (Time.time < _movementLockedUntil)
        {
            Stop();
            return;
        }
        Movement(new Vector3(direction.x, 0, direction.y), moveSpeed, sprintSpeedMultiplier);
    }

    public async void Dodge(Vector2 direction, float moveSpeed)
    {
        if (Time.time < _movementLockedUntil)
        {
            Stop();
            return;
        }

        IsDodging = true;
        float dodgeTimer = 0f;
        while (dodgeTimer < dodgeDuration)
        {
            Movement(new Vector3(direction.x, 0, direction.y), moveSpeed, dodgeSpeedMultiplier);
            dodgeTimer += Time.deltaTime;
            await UniTask.Yield();
        }
        Stop();
        IsDodging = false;
    }

    public void Lunge(Vector3 direction, float moveSpeed)
    {
        if (Time.time < _movementLockedUntil)
        {
            Stop();
            return;
        }

        CurrentMove =
            direction.normalized *
            moveSpeed *
            lungeSpeedMultiplier;
    }
    public void Jump()
    {
        if (!cc.isGrounded) return;

        verticalVelocity = jumpForce;
    }

    public void WallJump()
    {
        verticalVelocity = jumpForce;
    }

    public void KnockBack(GameObject attacker)
    {
        if (attacker == null)
        {
            Debug.LogWarning("Attacker is null. Cannot apply knockback.");
            return;
        }
        Vector3 knockBackDirection = (transform.position - attacker.transform.position).normalized;
        StartKnockBack(knockBackDirection);
    }

    public async void StartKnockBack(Vector3 knockBackDirection)
    {
        float timer = 0f;
        while (timer < knockBackDuration)
        {
            Vector3 knockBackMove = knockBackDirection * knockBackForce;
            cc.Move(knockBackMove * Time.deltaTime);
            timer += Time.deltaTime;
            await UniTask.Yield();
        }
    }
    public void MoveAir(Vector2 direction, float moveSpeed)
    {
        if (Time.time < _movementLockedUntil)
        {
            Stop();
            return;
        }
        Movement(new Vector3(direction.x, 0, direction.y), moveSpeed, airSpeedMultiplier);
    }

    public void Stop()
    {
        CurrentMove = Vector3.zero;
    }

    private void CheckGrounded()
    {
        IsGrounded = cc.isGrounded;
    }

    private void CheckWallEdge()
    {
        if (IsGrounded)
        {
            WallEdge = false;
            return;
        }

        WallEdge = Physics.CheckSphere(
            transform.position + transform.forward * distanceCheck,
            radiusCheck,
            wallEdgeLayer
        );
    }

    public void LockMovement(float duration)
    {
        _movementLockedUntil = Mathf.Max(_movementLockedUntil, Time.time + duration);
        Stop();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debugMode)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * distanceCheck, radiusCheck);
    }
#endif
}