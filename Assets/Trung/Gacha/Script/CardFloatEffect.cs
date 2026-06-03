using UnityEngine;

public class CardFloatEffect :
    MonoBehaviour
{
    private RectTransform rect;

    private Vector2 basePosition;

    private float amplitude;

    private float speed;

    private float offset;

    public void Setup(
        float amplitude,
        float speed,
        float offset)
    {
        this.amplitude =
            amplitude;

        this.speed =
            speed;

        this.offset =
            offset;
    }

    private void Awake()
    {
        rect =
            transform as RectTransform;

        basePosition =
            rect.anchoredPosition;
    }

    private void Update()
    {
        float y =
            Mathf.Sin(
                Time.time * speed
                + offset)
            * amplitude;

        rect.anchoredPosition =
            new Vector2(
                basePosition.x,
                basePosition.y + y);
    }
}