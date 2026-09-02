using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quest System/Quest Database")]
public class QuestDatabaseSO : ScriptableObject
{
    [Header("All Game Quests Database")]
    [Tooltip("Assign all created QuestSO assets here")]
    public List<QuestSO> allQuests = new List<QuestSO>();

    public QuestSO GetQuestSO(string id)
    {
        if (allQuests == null) return null;
        for (int i = 0; i < allQuests.Count; i++)
        {
            if (allQuests[i] != null && allQuests[i].questID == id)
            {
                return allQuests[i];
            }
        }
        return null;
    }
}