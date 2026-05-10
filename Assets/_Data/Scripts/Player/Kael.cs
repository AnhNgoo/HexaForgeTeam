using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public class Kael : CharacterMelee
{
    [SerializeField] protected GameObject kaelObject;
    [SerializeField] protected Animator kaelAnimator;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (kaelObject == null)
            kaelObject = visuals.transform.Find("Kael").gameObject;
        if (kaelAnimator == null)
            kaelAnimator = visuals.transform.Find("Kael").GetComponent<Animator>();
    }
    protected override void Awake()
    {
        base.Awake();
        characterAnimation.Init(kaelAnimator);
    }

    // Override để khởi tạo các đòn tấn công riêng cho Kael
    protected override IAttackStep[] InitAttackCombos()
    {
        return attackCombos = new IAttackStep[4]
        {
            new KaelAttackStep_1(),
            new KaelAttackStep_2(),
            new KaelAttackStep_3(),
            new KaelAttackStep_4()
        };
    }
}
