using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class MeleeWeapon : WeaponBase
{
    [SerializeField] protected Collider hitboxCollider;
    protected override void LoadComponent()
    {
        if (hitboxCollider == null)
            hitboxCollider = GetComponent<Collider>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    protected virtual void Start()
    {
        DisableHitbox();
    }
    public virtual void EnableHitbox()
    {
        hitboxCollider.enabled = true;
    }

    public virtual void DisableHitbox()
    {
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            // Xử lý va chạm với đối tượng khác
            Debug.Log($"Hit {other.gameObject.name}");
        }

    }
}
