using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRanged : CharacterBase
{
    [Header("Effect Points")]
    public GameObject fireEffectPoint;

    protected override void LoadEffectPoints()
    {
        base.LoadEffectPoints();
        if (fireEffectPoint == null)
            fireEffectPoint = effectPoints?.transform.Find("FireEffectPoint")?.gameObject;
    }
    public override void Attack()
    {
        if (characterCombat.CurrentComboIndex == 0 && !characterWeapon.HasWeapon) // Chỉ áp sát mục tiêu nếu đây là đòn tấn công đầu tiên trong chuỗi combo
            MeleeSnapToTarget();
        Debug.Log("Attack Ranged" + characterWeapon.HasWeapon);
        if (characterWeapon.HasWeapon) // nếu có vũ khí thì được vừa di chuyển vừa tấn công
            characterCombat?.TryAttack(true, 1);
        else // nếu không thì không được di chuyển khi tấn công
            characterCombat?.TryAttack(false);

    }

    // Hàm bắn cho nhân vật tầm xa
    public void CreateProjectile(PoolType characterProjectile, PoolType hitEffect = PoolType.None)
    {
        GameObject projectileObj = ObjectPooling.Instance.SpawnFromPool(characterProjectile,
                                             fireEffectPoint.transform.position,
                                             fireEffectPoint.transform.rotation);

        if (projectileObj.TryGetComponent(out Projectile projectile))
        {
            Vector3 direction = GetDirectionToTarget();
            projectile.Init(direction, this, hitEffect);
        }
    }
    public Vector3 GetDirectionToTarget()
    {
        if (characterLockTarget.Target == null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 direction = new Vector3(fireEffectPoint.transform.forward.x, cameraForward.y, fireEffectPoint.transform.forward.z).normalized;
            return direction;
        }


        Vector3 directionToTarget = (characterLockTarget.Target.position - fireEffectPoint.transform.position).normalized;
        return directionToTarget;
    }
}
