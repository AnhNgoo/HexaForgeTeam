using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public class Kael : CharacterMelee
{
    [Header("Kael")]
    [SerializeField] protected GameObject kaelGiantVisual;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (characterVisual == null)
            characterVisual = visuals.transform.Find("Kael").gameObject;
        if (kaelGiantVisual == null)
            kaelGiantVisual = visuals.transform.Find("KaelGiant").gameObject;
    }
    protected override void Init(CharacterData data)
    {
        base.Init(data);
    }
}
