using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [SerializeField]
    private DialogueData dialogueData =
        new DialogueData();

    public DialogueData GetDialogue()
    {
        return dialogueData;
    }
}