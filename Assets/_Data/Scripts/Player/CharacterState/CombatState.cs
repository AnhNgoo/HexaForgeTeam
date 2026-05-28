using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CombatState : ICharacterState
{
    private CharacterBase character;
    public CombatState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        bool canEnableRootMotion = true;
        if (character is CharacterMelee characterMelee)
            canEnableRootMotion = !characterMelee.CheckForNearEnemy();

        if (canEnableRootMotion)
            character.CharacterAnimation.EnableRootMotion();
        character.CharacterMovement.Stop();
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
