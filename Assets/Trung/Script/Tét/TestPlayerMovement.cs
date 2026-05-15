using UnityEngine;

public class TestPlayerMovement : MonoBehaviour
{
    [Header("Di Chuyển")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotate")]
    [SerializeField] private float rotateSpeed = 10f;

    private CharacterController characterController;

    private Vector3 moveDirection;

    public bool IsLockMovement { get; set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("Thiếu CharacterController");
        }
    }

    private void Update()
    {
        if (IsLockMovement)
            return;

        Movement();
    }

    private void Movement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );
        }
    }
}