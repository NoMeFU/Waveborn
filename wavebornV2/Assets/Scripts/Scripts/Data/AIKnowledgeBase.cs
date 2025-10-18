using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIKnowledgeBase", menuName = "AI/NPC Knowledge Base")]
public class AIKnowledgeBase : ScriptableObject
{
    [System.Serializable]
    public class KnowledgeEntry
    {
        public string question;
        [TextArea(3, 10)] public string answer;
    }

    public List<KnowledgeEntry> entries = new List<KnowledgeEntry>();

    public string GetResponse(string input)
    {
        input = input.ToLower();
        foreach (var e in entries)
        {
            if (input.Contains(e.question.ToLower()))
                return e.answer;
        }
        return "Цікаве питання... але я ще думаю над відповіддю.";
    }
}
