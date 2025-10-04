using TMPro;
using UnityEngine;

public class QuestItemUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI progressText;

    /// <summary>Оновити контент елемента.</summary>
    public void Set(string title, string description, int current, int max)
    {
        if (titleText) titleText.text = title ?? "";
        if (descText) descText.text = description ?? "";
        if (progressText) progressText.text = $"{current}/{max}";

        // Можеш підсвічувати виконані квести:
        bool done = current >= max;
        if (progressText) progressText.color = done ? new Color(0.5f, 1f, 0.6f) : Color.white;
        if (titleText) titleText.fontStyle = done ? FontStyles.Bold : FontStyles.Normal;
    }
}
