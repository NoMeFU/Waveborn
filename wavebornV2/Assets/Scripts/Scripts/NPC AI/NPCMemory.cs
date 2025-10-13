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

    // 🧠 Пошук найсхожішого питання (з урахуванням форми слів)
    public NPCKnowledgeEntry GetBestMatch(string playerMessage)
    {
        if (data.knowledgeBase == null || data.knowledgeBase.Count == 0)
            return null;

        string cleaned = Clean(playerMessage);
        var playerWords = StemWords(cleaned.Split(' '));

        NPCKnowledgeEntry bestMatch = null;
        float bestScore = 0f;

        foreach (var entry in data.knowledgeBase)
        {
            var npcWords = StemWords(Clean(entry.question).Split(' '));
            float score = Similarity(playerWords, npcWords);

            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = entry;
            }
        }

        // ✅ Вважаємо, що схоже якщо ≥ 0.45
        return bestScore >= 0.45f ? bestMatch : null;
    }

    public void Learn(string question, string answer)
    {
        if (data.knowledgeBase.Any(e => Clean(e.question) == Clean(question)))
            return;

        data.knowledgeBase.Add(new NPCKnowledgeEntry { question = question, answer = answer });
        SaveMemory();
        Debug.Log($"🧠 NPC '{data.npcName}' запам’ятав нове: {question}");
    }

    // 🔍 Обчислення схожості (з урахуванням стемінгу)
    private float Similarity(List<string> a, List<string> b)
    {
        if (a.Count == 0 || b.Count == 0) return 0;

        int common = a.Count(w => b.Contains(w));
        int total = Mathf.Max(a.Count, b.Count);
        return (float)common / total;
    }

    // 🧹 Прибирає знаки, робить нижній регістр
    private string Clean(string input)
    {
        string cleaned = input.ToLower();
        cleaned = new string(cleaned.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
        return cleaned.Trim();
    }

    // 🇺🇦 Простий український стемінг — відкидає типові закінчення
    private List<string> StemWords(IEnumerable<string> words)
    {
        string[] endings = { "и", "і", "ї", "є", "ю", "я", "ти", "тися", "ш", "ся", "ів", "івся", "вся", "сь", "тись", "ому", "ими", "ому", "ам", "ах", "ів", "е", "а", "о", "у", "ь" };
        List<string> result = new List<string>();

        foreach (string word in words)
        {
            string stem = word;
            foreach (var end in endings)
            {
                if (stem.EndsWith(end) && stem.Length > end.Length + 1)
                {
                    stem = stem.Substring(0, stem.Length - end.Length);
                    break;
                }
            }
            result.Add(stem);
        }

        return result.Distinct().ToList();
    }
}
