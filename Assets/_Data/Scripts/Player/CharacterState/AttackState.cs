using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AttackState : ICharacterState
{
    private CharacterBase character;
    public AttackState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        character.CharacterAnimation.EnableRootMotion();
    }

    public void Exit()
    {
        character.CharacterAnimation.DisableRootMotion();
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {

    }
}
