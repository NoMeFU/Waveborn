using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    [SerializeField] private NPCMemory memory;

    public string Think(string playerMessage)
    {
        if (memory == null)
        {
            Debug.LogWarning("❌ NPCMemory не підключено до NPCBrain!");
            return "У мене провал у пам’яті...";
        }

        var match = memory.GetBestMatch(playerMessage);
        if (match != null)
        {
            return match.answer;
        }
        else
        {
            memory.Learn(playerMessage, "Не знаю... але я запам’ятаю це.");
            return "Не знаю... але я запам’ятаю це.";
        }
    }
}
