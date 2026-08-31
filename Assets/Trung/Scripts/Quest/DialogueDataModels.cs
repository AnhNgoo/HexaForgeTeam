using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpeakerType
{
    NPC,
    Player
}

[Serializable]
public class DialogueLine
{
    public SpeakerType speaker = SpeakerType.NPC;
    [TextArea(2, 4)] public string text;

    public DialogueLine(SpeakerType speaker, string text)
    {
        this.speaker = speaker;
        this.text = text;
    }
}