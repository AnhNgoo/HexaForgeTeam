using UnityEngine;

[DisallowMultipleComponent]
public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerTag = "Player";

    [Header("Position")]
    [SerializeField] private float height = 20f;
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Rotation")]
    [SerializeField] private bool rotateWithPlayer;
    [SerializeField] private float northRotation;

    private void Awake()
    {
        FindPlayer();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindPlayer();
            return;
        }

        Vector3 desiredPosition = new Vector3(
            target.position.x,
            target.position.y + height,
            target.position.z
        );

        float smoothAmount =
            1f - Mathf.Exp(
                -smoothSpeed * Time.unscaledDeltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothAmount
        );

        float mapRotation = rotateWithPlayer
            ? target.eulerAngles.y
            : northRotation;

        transform.rotation = Quaternion.Euler(
            90f,
            mapRotation,
            0f
        );
    }

    private void FindPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            target = player.transform;
        }
    }
}