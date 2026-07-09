using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueData
{
    public string npcName;

    [TextArea]
public List<string> dialogues =
    new List<string>();

    public List<DialogueChoice> choices =
        new List<DialogueChoice>();
}