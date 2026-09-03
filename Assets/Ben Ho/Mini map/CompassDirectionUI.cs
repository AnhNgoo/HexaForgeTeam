using TMPro;
using UnityEngine;

public class CompassDirectionUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform headingSource;
    [SerializeField] private string playerTag = "Player";

    [Header("Compass")]
    [SerializeField] private RectTransform directionLine;
    [SerializeField] private RectTransform playerDirectionMarker;
    [SerializeField] private RectTransform directionPingMarker;
    [SerializeField] private bool useMainCameraHeading = true;

    [Header("Cardinal Directions")]
    [SerializeField] private TMP_Text northText;
    [SerializeField] private TMP_Text eastText;
    [SerializeField] private TMP_Text southText;
    [SerializeField] private TMP_Text westText;

    [Header("Layout")]
    [SerializeField, Range(90f, 180f)]
    private float displayedHalfAngle = 180f;

    [SerializeField] private float edgePadding = 30f;
    [SerializeField] private float playerMarkerY;
    [SerializeField] private float pingMarkerY;
    [SerializeField] private float cardinalTextY = 25f;

    [Header("Ping")]
    [SerializeField] private float arrivalDistance = 5f;
    [SerializeField] private TMP_Text pingDistanceText;

    private void OnEnable()
    {
        FindPlayerIfMissing();
        RefreshFixedMarker();
    }

    private void LateUpdate()
    {
        FindPlayerIfMissing();

        if (player == null || directionLine == null)
            return;

        Transform directionTarget = player;

        if (useMainCameraHeading && Camera.main != null)
            directionTarget = Camera.main.transform;
        else if (headingSource != null)
            directionTarget = headingSource;

        float headingAngle = directionTarget.eulerAngles.y;

        // The line never rotates.
        directionLine.localEulerAngles = Vector3.zero;

        RefreshFixedMarker();
        UpdateCardinalDirections(headingAngle);
        UpdatePingMarker(headingAngle);
    }

    private void RefreshFixedMarker()
    {
        if (playerDirectionMarker == null)
            return;

        playerDirectionMarker.gameObject.SetActive(true);
        playerDirectionMarker.anchoredPosition =
            new Vector2(0f, playerMarkerY);

        // The player marker stays fixed and does not rotate.
        playerDirectionMarker.localEulerAngles = Vector3.zero;
    }

    private void UpdateCardinalDirections(float headingAngle)
    {
        PositionCardinal(northText, "N", 0f, headingAngle);
        PositionCardinal(eastText, "E", 90f, headingAngle);
        PositionCardinal(southText, "S", 180f, headingAngle);
        PositionCardinal(westText, "W", 270f, headingAngle);
    }

    private void PositionCardinal(
        TMP_Text label,
        string value,
        float worldAngle,
        float headingAngle)
    {
        if (label == null)
            return;

        label.text = value;

        float angleDifference =
            Mathf.DeltaAngle(headingAngle, worldAngle);

        SetHorizontalPosition(
            label.rectTransform,
            angleDifference,
            cardinalTextY
        );

        label.gameObject.SetActive(true);
        label.rectTransform.localEulerAngles = Vector3.zero;
    }

    private void UpdatePingMarker(float headingAngle)
    {
        if (directionPingMarker == null || !MapPingService.HasPing)
        {
            HidePingMarker();
            return;
        }

        Vector3 direction =
            MapPingService.PingWorldPosition - player.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= arrivalDistance)
        {
            MapPingService.ClearPing();
            HidePingMarker();
            return;
        }

        float pingWorldAngle =
            Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        float angleDifference =
            Mathf.DeltaAngle(headingAngle, pingWorldAngle);

        directionPingMarker.gameObject.SetActive(true);

        SetHorizontalPosition(
            directionPingMarker,
            angleDifference,
            pingMarkerY
        );

        directionPingMarker.localEulerAngles = Vector3.zero;

        if (pingDistanceText != null)
        {
            pingDistanceText.gameObject.SetActive(true);
            pingDistanceText.text =
                Mathf.CeilToInt(distance) + "m";
        }
    }

    private void SetHorizontalPosition(
        RectTransform target,
        float angleDifference,
        float y)
    {
        float halfWidth =
            directionLine.rect.width * 0.5f - edgePadding;

        float normalizedPosition =
            Mathf.Clamp(
                angleDifference / displayedHalfAngle,
                -1f,
                1f
            );

        target.anchoredPosition =
            new Vector2(normalizedPosition * halfWidth, y);
    }

    private void HidePingMarker()
    {
        if (directionPingMarker != null)
            directionPingMarker.gameObject.SetActive(false);

        if (pingDistanceText != null)
            pingDistanceText.gameObject.SetActive(false);
    }

    private void FindPlayerIfMissing()
    {
        if (player == null)
        {
            CharacterBase character = FindObjectOfType<CharacterBase>();

            if (character != null)
                player = character.transform;
        }
    }
}