using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class OpenChest : InteractBase
{
    [SerializeField] private Animator chestAnim;
    [SerializeField] private Vector2 forceDropItem = new Vector2(3f, 5f);

    [Header("List of Pick Up Items")]
    [SerializeField] private List<PoolType> pickUpItems = new List<PoolType>();
    public override string InteractionName => "Open Chest";
    private bool isOpened = false;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (chestAnim == null)
        {
            chestAnim = modelItem.GetComponent<Animator>();
        }
    }

    protected override void Update()
    {
        if (isOpened) return;
        base.Update();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isOpened) return;
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (isOpened) return;
        base.OnTriggerExit(other);
    }

    protected override void InteractAction()
    {
        isOpened = true;
        chestAnim.CrossFade("OpenChest", 0.1f);
        InteractionManager.Instance?.UnregisterInteractable(this);
        SpawnPickUpItems();
    }

    public async void SpawnPickUpItems()
    {
        await UniTask.Delay(1500); // Delay 0.5s để chờ animation mở rương hoàn tất
        foreach (PoolType item in pickUpItems)
        {
            GameObject pickUpItem = ObjectPooling.Instance.SpawnFromPool(item, transform.position + Vector3.up * 1f, Quaternion.identity);
            if (pickUpItem != null)
            {
                Rigidbody rb = pickUpItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float x = Random.Range(-1f, 1f);
                    Vector3 direction = transform.forward + transform.right * x + Vector3.up;

                    float forceDropItem = Random.Range(this.forceDropItem.x, this.forceDropItem.y);
                    rb.AddForce(direction.normalized * forceDropItem, ForceMode.Impulse);
                }
            }
        }
    }

    public override void ResetInteraction()
    {
        isOpened = false;
        chestAnim.CrossFade("CloseChest", 0.1f);
    }
}
