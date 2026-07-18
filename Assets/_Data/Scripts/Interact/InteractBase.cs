using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractBase : LoadComponents
{
    [Tooltip("Có thể override InteractionName để thay đổi tên hiển thị của đối tượng tương tác cụ thể này. Nếu không override thì sẽ lấy interactionName mặc định")]
    [SerializeField] protected string interactionName = "Pick Up Item";
    public virtual string InteractionName => interactionName;
    [SerializeField] protected Transform modelItem;

    protected bool playerInRange = false;
    protected GameObject lockTargetMarker;
    protected CharacterBase character;

    protected override void LoadComponent()
    {
        if (modelItem == null)
        {
            GameObject visual = transform.Find("Visuals")?.gameObject;

            if (visual != null)
            {
                if (visual.transform.childCount > 0)
                    modelItem = visual?.transform?.GetChild(0);
            }
        }
    }

    protected override void LoadComponentRuntime()
    {

    }
    protected virtual void Update()
    {
        if (!playerInRange || !InteractionManager.Instance.IsCurrentInteraction(this)) return;

        if (InputManager.InputActions.Keyboard.Interact.triggered &&
            UIManager.Instance.CurrentMenuType == MenuType.GameplayMenu)
        {
            InteractAction();
        }
    }

    /// <summary>
    /// Hành động tương tác khi người chơi nhấn phím tương tác (F)
    /// </summary>

    protected abstract void InteractAction();


    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (character == null)
            {
                if (other.TryGetComponent(out CharacterBase characterBase))
                {
                    character = characterBase;
                }
            }
            playerInRange = true;
            InteractionManager.Instance?.RegisterInteractable(this);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (character != null &&
                other.TryGetComponent(out CharacterBase characterBase) && characterBase == character)
            {
                character = null;
            }
            playerInRange = false;
            InteractionManager.Instance?.UnregisterInteractable(this);
        }
    }

    public void ShowHighlight()
    {
        lockTargetMarker = ObjectPooling.Instance.SpawnFromPool(PoolType.LockTargetMarker, modelItem.position, modelItem.rotation, modelItem);
    }

    public void HideHighlight()
    {
        if (lockTargetMarker != null)
        {
            ObjectPooling.Instance.ReturnToPool(PoolType.LockTargetMarker, lockTargetMarker);
            lockTargetMarker = null;
        }
    }

    public abstract void ResetInteraction();
}
