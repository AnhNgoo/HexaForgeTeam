using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractBase : MonoBehaviour
{
    protected void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            InteractAction();
        }
    }

    /// <summary>
    /// Hành động tương tác khi người chơi nhấn phím tương tác (F)
    /// </summary>

    protected abstract void InteractAction();

    protected bool playerInRange = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
