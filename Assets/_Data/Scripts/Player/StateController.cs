using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController
{
    public ICharacterState currentState;

    public void ChangeState(ICharacterState newState)
    {
        if (newState == null)
            return;

        if (currentState != null && currentState.GetType() == newState.GetType())
        {
            return;
        }

        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }
}
