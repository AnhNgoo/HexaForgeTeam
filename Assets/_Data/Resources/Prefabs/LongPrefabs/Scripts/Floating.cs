using UnityEngine;

public class Floating : MonoBehaviour
{
    [SerializeField] float floatHeight = 0.2f;
    [SerializeField] float floatSpeed = 1f;

    [SerializeField] float swayAmount = 0.05f;
    [SerializeField] float swaySpeed = 0.7f;

    [SerializeField] float rotateSpeed = 15f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        float x = Mathf.Sin(Time.time * swaySpeed) * swayAmount;

        transform.position = startPos + new Vector3(x, y, 0);

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}