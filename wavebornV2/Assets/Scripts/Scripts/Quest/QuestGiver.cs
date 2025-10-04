using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestGiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private Button btnTasks;
    [SerializeField] private TextMeshProUGUI tasksStatusText;

    [Header("Available Quests")]
    [SerializeField] private List<QuestSO> quests = new();

    private void Awake()
    {
        if (!dialogueUI)
            dialogueUI = FindObjectOfType<DialogueUI>();

        if (btnTasks)
            btnTasks.onClick.AddListener(OnTasksPressed);
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted += OnQuestAccepted;
            QuestManager.Instance.OnQuestProgress += OnQuestProgress;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted -= OnQuestAccepted;
            QuestManager.Instance.OnQuestProgress -= OnQuestProgress;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
    }

    private void OnTasksPressed()
    {
        if (!dialogueUI)
            return;

        QuestSO quest = quests.Count > 0 ? quests[0] : null;

        if (quest == null)
        {
            dialogueUI.ShowDialogueText("Наразі задач немає.");
            StartCoroutine(CloseSoon());
            return;
        }

        var qm = QuestManager.Instance;
        if (qm == null)
        {
            dialogueUI.ShowDialogueText("Система квестів не знайдена.");
            return;
        }

        // Якщо квест вже активний
        if (qm.HasActive(quest.questId))
        {
            var state = qm.GetState(quest.questId);
            dialogueUI.ShowDialogueText(
                $"Квест вже виконується: <b>{quest.title}</b>\n" +
                $"{quest.description}\n" +
                $"Прогрес: {state.Current}/{state.Required}\n" +
                $"Нагорода: {quest.rewardText}"
            );
            StartCoroutine(CloseSoon());
            return;
        }

        // Якщо квест завершений
        if (qm.IsCompleted(quest.questId))
        {
            if (quest.repeatable)
            {
                qm.Accept(quest);
                dialogueUI.ShowDialogueText(
                    $"Ви взяли повторно квест: <b>{quest.title}</b>\n" +
                    $"Ціль: {quest.description} (0/{quest.requiredAmount})\n" +
                    $"Нагорода: {quest.rewardText}"
                );
            }
            else
            {
                dialogueUI.ShowDialogueText($"Цей квест вже виконано і він не повторюється.");
            }

            StartCoroutine(CloseSoon());
            return;
        }

        // Новий квест
        qm.Accept(quest);
        dialogueUI.ShowDialogueText(
            $"Прийнято квест: <b>{quest.title}</b>\n" +
            $"Ціль: {quest.description} (0/{quest.requiredAmount})\n" +
            $"Нагорода: {quest.rewardText}"
        );
        StartCoroutine(CloseSoon());
    }

    private IEnumerator CloseSoon()
    {
        yield return new WaitForSeconds(1.0f);
        if (dialogueUI != null && dialogueUI.IsOpen)
            dialogueUI.CloseAll();
    }

    private void OnQuestAccepted(QuestState state)
    {
        if (tasksStatusText != null)
            tasksStatusText.text = $"Прийнято: {state.Data.title}";
    }

    private void OnQuestProgress(QuestState state)
    {
        if (tasksStatusText != null && !state.Completed)
            tasksStatusText.text = $"{state.Data.title}: {state.Current}/{state.Required}";
    }

    private void OnQuestCompleted(QuestState state)
    {
        if (tasksStatusText != null)
            tasksStatusText.text = $"Завершено: {state.Data.title} → {state.Data.rewardText}";
    }
}
