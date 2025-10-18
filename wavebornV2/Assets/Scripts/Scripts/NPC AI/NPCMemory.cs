using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class NPCKnowledgeEntry
{
    public string question;
    public string answer;
}

[System.Serializable]
public class NPCMemoryData
{
    public string npcName;
    public List<NPCKnowledgeEntry> knowledgeBase = new();
}

public class NPCMemory : MonoBehaviour
{
    [Header("Основна інформація NPC")]
    public Sprite avatarSprite;
    public NPCMemoryData data = new NPCMemoryData();

    private string filePath;

    private void Awake()
    {
        filePath = Path.Combine(Application.dataPath, "Resources/NPCMemory.json");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("⚠️ Не знайдено файл NPCMemory.json — створюю новий!");
            data = new NPCMemoryData
            {
                npcName = gameObject.name,
                knowledgeBase = new List<NPCKnowledgeEntry>()
            };
            SaveMemory();
        }
        else
        {
            LoadMemory();
        }
    }

    public void LoadMemory()
    {
        string json = File.ReadAllText(filePath);
        data = JsonUtility.FromJson<NPCMemoryData>(json);
        Debug.Log($"✅ Завантажено пам’ять NPC '{data.npcName}' ({data.knowledgeBase.Count} записів)");
    }

    public void SaveMemory()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // 🧠 Покращене порівняння запитань
    public NPCKnowledgeEntry GetBestMatch(string playerMessage)
    {
        if (data.knowledgeBase == null || data.knowledgeBase.Count == 0)
            return null;

        string cleaned = Clean(playerMessage);

        NPCKnowledgeEntry bestMatch = null;
        float bestScore = 0f;

        foreach (var entry in data.knowledgeBase)
        {
            float score = Similarity(cleaned, Clean(entry.question));
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = entry;
            }
        }

        // ✅ вважаємо збігом, якщо схожість > 0.4
        return bestScore > 0.4f ? bestMatch : null;
    }

    // 🧩 “Навчання” — запам’ятати нову фразу
    public void Learn(string question, string answer)
    {
        if (data.knowledgeBase.Any(e => Clean(e.question) == Clean(question)))
            return;

        data.knowledgeBase.Add(new NPCKnowledgeEntry { question = question, answer = answer });
        SaveMemory();
        Debug.Log($"🧠 NPC '{data.npcName}' запам’ятав нове: {question}");
    }

    // 🧩 Обчислення схожості
    private float Similarity(string a, string b)
    {
        var wordsA = a.Split(' ').Distinct().ToList();
        var wordsB = b.Split(' ').Distinct().ToList();

        int common = wordsA.Count(w => wordsB.Contains(w));
        int total = Mathf.Max(wordsA.Count, wordsB.Count);

        return (float)common / total;
    }

    // 🧹 Нормалізація тексту
    private string Clean(string input)
    {
        string cleaned = input.ToLower();
        cleaned = new string(cleaned.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
        cleaned = cleaned.Replace("  ", " ").Trim();
        return cleaned;
    }
}
