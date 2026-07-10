using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private InteractBase currentClosestInteraction;
    [SerializeField] private List<InteractBase> interactableObjects = new List<InteractBase>();

    private InteractBase previousClosestInteraction = null;

    public void Init(Transform player)
    {
        playerTransform = player;
    }

    private void Update()
    {
        UpdateClosestInteraction();
    }
    public void RegisterInteractable(InteractBase interactable)
    {
        interactableObjects.Add(interactable);
    }

    public void UnregisterInteractable(InteractBase interactable)
    {
        if (!interactableObjects.Contains(interactable)) return;

        if (currentClosestInteraction == interactable) // Nếu huỷ đăng ký mà đối tượng chính là đối tượng gần nhất hiện tại, thì cần ẩn highlight và panel
        {
            currentClosestInteraction.HideHighlight();
            EventManager.Notify(GameEvent.OnHidePickUpItemPanel);
            currentClosestInteraction = null;
            previousClosestInteraction = null;
        }

        interactableObjects.Remove(interactable);
    }

    private void UpdateClosestInteraction()
    {
        if (interactableObjects.Count == 0)// Nếu bằng 0 thì không cần so sánh khoảng cách
            return;

        if (interactableObjects.Count == 1)  // Nếu chỉ có 1 đối tượng tương tác, thì luôn chọn nó
        {
            SetClosestInteraction(interactableObjects[0]);
            return;
        }

        // Nếu nhiều hơn 1 đối tượng tương tác, thì tìm đối tượng gần nhất
        InteractBase closestInteraction = null;
        float closestDistance = Mathf.Infinity;

        foreach (var interactable in interactableObjects)
        {
            if (interactable == null) continue; // Bỏ qua các đối tượng null (khi bị hủy hoặc không còn tồn tại)

            float distance = Vector3.Distance(playerTransform.position, interactable.transform.position);
            if (distance < closestDistance) // Lấy đối tượng có khoảng cách gần nhất
            {
                closestDistance = distance;
                closestInteraction = interactable;
            }
        }

        SetClosestInteraction(closestInteraction);
    }

    private void SetClosestInteraction(InteractBase closestInteraction)
    {
        if (previousClosestInteraction == closestInteraction) return; // Nếu đối tượng trước giống đối tượng hiện tại, không cần cập nhật
        previousClosestInteraction = currentClosestInteraction;

        currentClosestInteraction?.HideHighlight();
        EventManager.Notify(GameEvent.OnHidePickUpItemPanel);

        currentClosestInteraction = closestInteraction;

        currentClosestInteraction?.ShowHighlight();
        EventManager.Notify(GameEvent.OnShowPickUpItemPanel, currentClosestInteraction.InteractionName);
    }

    // Kiểm tra đối tượng truyền vào có phải là đối tượng tương tác gần nhất hiện tại hay không
    public bool IsCurrentInteraction(InteractBase interactable)
    {
        return currentClosestInteraction == interactable;
    }
}
