using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITakeDamage
{
    void TakeDamage(DamageInfo damageInfo);
}

public class DamageInfo
{
    public float damageAmount;
    public GameObject attacker = null;
    public bool isCritical = false;
    public bool isFromSafeZoneEffect = false;

}