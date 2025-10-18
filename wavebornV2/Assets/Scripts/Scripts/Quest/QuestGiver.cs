using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private Button btnTasks;
    [SerializeField] private TextMeshProUGUI tasksStatusText;

    [Header("Quests")]
    [SerializeField] private List<QuestSO> quests = new();

    private void Awake()
    {
        if (!dialogueUI)
            dialogueUI = FindObjectOfType<DialogueUI>();

        if (btnTasks)
            btnTasks.onClick.AddListener(OnTasksPressed);
    }

    private void Start()
    {
        if (QuestManager.Instance == null)
            Debug.LogError("❌ QuestManager не знайдено в сцені!");
    }

    private void OnTasksPressed()
    {
        var qm = QuestManager.Instance;
        if (qm == null)
        {
            dialogueUI.ShowDialogueText("❌ Система квестів не знайдена.");
            return;
        }

        if (quests.Count == 0)
        {
            dialogueUI.ShowDialogueText("Наразі задач немає.");
            return;
        }

        QuestSO quest = quests[0];

        if (qm.HasActive(quest.questId))
        {
            var state = qm.GetState(quest.questId);
            dialogueUI.ShowDialogueText(
                $"🔸 Квест вже виконується: <b>{quest.title}</b>\n" +
                $"{quest.description}\n" +
                $"Прогрес: {state.Current}/{state.Required}"
            );
            return;
        }

        if (qm.IsCompleted(quest.questId))
        {
            if (quest.repeatable)
            {
                qm.Accept(quest);
                dialogueUI.ShowDialogueText($"🔁 Ви взяли повторно квест: <b>{quest.title}</b>");
            }
            else
            {
                dialogueUI.ShowDialogueText($"✅ Квест <b>{quest.title}</b> вже виконано.");
            }
            return;
        }

        qm.Accept(quest);
        dialogueUI.ShowDialogueText($"🆕 Прийнято квест: <b>{quest.title}</b>");
    }
}
