using UnityEngine;
using TMPro;

[System.Serializable]
public class CompassLabel
{
    public string labelName;
    public float worldAngle;
    public RectTransform rect;
}

public class CompassDirectionUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Direction UI")]
    [SerializeField] private RectTransform directionLine;
    [SerializeField] private RectTransform playerDirectionMarker;

    [Header("Ping UI")]
    [SerializeField] private RectTransform directionPingMarker;
    [SerializeField] private float visibleAngle = 90f;
    [SerializeField] private float markerY = 20f;

    [Header("Optional Direction Labels")]
    [SerializeField] private CompassLabel[] compassLabels;

    private void OnEnable()
    {
        FindPlayer();

        MapPingService.OnPingChanged += HandlePingChanged;
        MapPingService.OnPingCleared += HidePingMarker;

        HidePingMarker();
    }

    private void OnDisable()
    {
        MapPingService.OnPingChanged -= HandlePingChanged;
        MapPingService.OnPingCleared -= HidePingMarker;
    }

    private void LateUpdate()
    {
        if (player == null)
            FindPlayer();

        if (player == null || directionLine == null)
            return;

        // Thanh line luôn đứng yên, không xoay.
        directionLine.localEulerAngles = Vector3.zero;

        // Player marker đứng giữa line. Nếu muốn icon xoay theo hướng nhân vật thì bật dòng rotation.
        if (playerDirectionMarker != null)
        {
            playerDirectionMarker.anchoredPosition =
                new Vector2(0f, markerY);

            playerDirectionMarker.localEulerAngles =
                new Vector3(0f, 0f, -player.eulerAngles.y);
        }

        UpdatePingMarker();
        UpdateCompassLabels();
    }

    private void UpdatePingMarker()
    {
        if (directionPingMarker == null ||
            directionLine == null ||
            !MapPingService.HasPing)
        {
            HidePingMarker();
            return;
        }

        Vector3 toPing =
            MapPingService.PingWorldPosition - player.position;

        toPing.y = 0f;

        if (toPing.sqrMagnitude < 0.1f)
        {
            HidePingMarker();
            return;
        }

        float pingAngle =
            Mathf.Atan2(toPing.x, toPing.z) * Mathf.Rad2Deg;

        float angleDiff =
            Mathf.DeltaAngle(player.eulerAngles.y, pingAngle);

        bool visible =
            Mathf.Abs(angleDiff) <= visibleAngle;

        directionPingMarker.gameObject.SetActive(visible);

        if (!visible)
            return;

        float halfWidth = directionLine.rect.width * 0.5f;

        float x =
            Mathf.Clamp(angleDiff / visibleAngle, -1f, 1f)
            * halfWidth;

        directionPingMarker.anchoredPosition =
            new Vector2(x, markerY);

        directionPingMarker.localEulerAngles = Vector3.zero;
    }

    private void UpdateCompassLabels()
    {
        if (compassLabels == null || directionLine == null)
            return;

        float halfWidth = directionLine.rect.width * 0.5f;

        foreach (CompassLabel label in compassLabels)
        {
            if (label == null || label.rect == null)
                continue;

            float angleDiff =
                Mathf.DeltaAngle(player.eulerAngles.y, label.worldAngle);

            bool visible =
                Mathf.Abs(angleDiff) <= visibleAngle;

            label.rect.gameObject.SetActive(visible);

            if (!visible)
                continue;

            float x =
                Mathf.Clamp(angleDiff / visibleAngle, -1f, 1f)
                * halfWidth;

            label.rect.anchoredPosition =
                new Vector2(x, markerY + 25f);

            label.rect.localEulerAngles = Vector3.zero;

            TMP_Text text = label.rect.GetComponent<TMP_Text>();
            if (text != null)
                text.text = label.labelName;
        }
    }

    private void HandlePingChanged(Vector3 worldPosition)
    {
        if (directionPingMarker != null)
            directionPingMarker.gameObject.SetActive(true);
    }

    private void HidePingMarker()
    {
        if (directionPingMarker != null)
            directionPingMarker.gameObject.SetActive(false);
    }

    private void FindPlayer()
    {
        GameObject target =
            GameObject.FindGameObjectWithTag(playerTag);

        if (target != null)
            player = target.transform;
    }
}