using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotateDragHandler : MonoBehaviour, IDragHandler
{
    [Header("Target to Rotate")]
    [SerializeField] private Transform targetToRotate; // Bệ đá hoặc Khung chứa 3D Model
    [SerializeField] private float rotationSpeed = 0.4f;

    public void OnDrag(PointerEventData eventData)
    {
        if (targetToRotate == null) return;

        // eventData.delta.x lấy độ dịch chuyển chuột/ngón tay theo chiều ngang
        float rotateAmount = -eventData.delta.x * rotationSpeed;
        
        // Xoay quanh trục Y
        targetToRotate.Rotate(Vector3.up, rotateAmount, Space.World);
    }

    public void SetTarget(Transform newTarget)
    {
        targetToRotate = newTarget;
    }
}