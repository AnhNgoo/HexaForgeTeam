using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : ICharacterState
{
    private CharacterBase character;
    public HitState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        character.CharacterAnimation.CrossFade("Hit", 0.1f);
        character.CharacterMovement.Stop();
    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        if (character.CharacterAnimation.GetAnimationTime("Hit") >= 0.5f)
        {
            character.StateController.ChangeState(new IdleState(character));
            return;
        }
    }
}
