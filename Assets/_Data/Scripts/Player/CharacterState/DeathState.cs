using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : ICharacterState
{
    private CharacterBase character;

    public DeathState(CharacterBase character)
    {
        this.character = character;
    }

    public void Enter()
    {
        character.CharacterAnimation.CrossFade("Death", 0.1f);
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

    }
}
