using UnityEngine;

public class BirdRideState : ICharacterState
{
    private CharacterBase character;

    public BirdRideState(CharacterBase character)
    {
        this.character = character;
    }

    public void Enter()
    {
        character.CharacterAnimation.CrossFade("HangOnBird");
    }

    public void Update()
    {
        Debug.Log("BirdRideState Update");
        // Không cho điều khiển
    }

    public void FixedUpdate()
    {

    }

    public void Exit()
    {

    }
}