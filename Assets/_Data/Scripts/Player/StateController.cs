using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController
{
    public ICharacterState currentState;

    public void ChangeState(ICharacterState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}
