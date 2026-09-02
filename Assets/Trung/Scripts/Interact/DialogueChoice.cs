using System;

[Serializable]
public class DialogueChoice
{
    public string choiceText;

    public DialogueAction action =
        DialogueAction.None;

    public MenuType menuType =
    MenuType.None;
}

public enum DialogueAction
{
    None,
    OpenPanel,
    CloseDialogue,
    GambleBet50,
    GambleBet100,
    GambleBet250
}