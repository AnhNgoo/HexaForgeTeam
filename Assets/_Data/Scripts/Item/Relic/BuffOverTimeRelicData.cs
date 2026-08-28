using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "BuffOverTimeRelicData", menuName = "ScriptableObjects/RelicData/BuffOverTimeRelicData", order = 1)]
public abstract class BuffOverTimeRelicData : RelicData
{
    public float duration;

    public override async void Use(CharacterBase characterBase)
    {
        ApplyBuff(characterBase);
        await UniTask.Delay((int)(duration * 1000));
    }

    protected abstract void ApplyBuff(CharacterBase characterBase);
}
