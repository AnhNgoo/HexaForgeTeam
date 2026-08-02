using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialType
{
    None = 0,
    Move = 1,
    LockTarget = 2,
    Battle = 3,
    Dodge = 4,
    PickUpItem = 5,
    ReceiveRecoveryBottle = 6,
    UseSkill = 7,

}
public class TutorialSystem : Singleton<TutorialSystem>
{
    // Start is called before the first frame update
    void Start()
    {
        HideTutorial();
    }

    public void ShowTutorial(TutorialType tutorialType)
    {
        EventManager.Notify(GameEvent.OnShowTutorial, tutorialType);
    }

    public void HideTutorial()
    {
        EventManager.Notify(GameEvent.OnHideTutorial);
    }
}
