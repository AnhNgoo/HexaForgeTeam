using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicData", menuName = "ScriptableObjects/RelicData", order = 1)]
public abstract class RelicData : ItemDataBase
{
    public abstract void Use(CharacterBase characterBase);
}
