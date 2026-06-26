using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialZone : MonoBehaviour
{
    [SerializeField] private TutorialType tutorialType;
    private bool isShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isShown)
        {
            TutorialSystem.Instance.ShowTutorial(tutorialType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialSystem.Instance.HideTutorial();
            isShown = true;
        }
    }
}
