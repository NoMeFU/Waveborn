using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private Button btnTasks;
    [SerializeField] private TextMeshProUGUI tasksStatusText;

    [Header("Quests")]
    [SerializeField] private List<QuestSO> quests = new();

    [Header("Auto Hide Settings")]
    [SerializeField] private float autoHideDelay = 10f;

    private Coroutine hideCoroutine;

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
            ShowDialogueWithAutoHide("❌ Система квестів не знайдена.");
            return;
        }

        if (quests.Count == 0)
        {
            ShowDialogueWithAutoHide("Наразі задач немає.");
            return;
        }

        QuestSO quest = quests[0];

        if (qm.HasActive(quest.questId))
        {
            var state = qm.GetState(quest.questId);
            ShowDialogueWithAutoHide(
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
                ShowDialogueWithAutoHide($"🔁 Ви взяли повторно квест: <b>{quest.title}</b>");
            }
            else
            {
                ShowDialogueWithAutoHide($"✅ Квест <b>{quest.title}</b> вже виконано.");
            }
            return;
        }

        qm.Accept(quest);
        ShowDialogueWithAutoHide($"🆕 Прийнято квест: <b>{quest.title}</b>");
    }

    private void ShowDialogueWithAutoHide(string text)
    {
        // Скасовуємо попередню корутину якщо вона існує
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // Показуємо діалог
        dialogueUI.ShowDialogueText(text);

        // Запускаємо нову корутину для автоматичного приховування
        hideCoroutine = StartCoroutine(AutoHideDialogue());
    }

    private IEnumerator AutoHideDialogue()
    {
        // Чекаємо вказану кількість секунд
        yield return new WaitForSeconds(autoHideDelay);

        // Ховаємо діалог
        if (dialogueUI != null)
        {
            dialogueUI.CloseAll();
        }

        hideCoroutine = null;
    }
}

/*
ЩО ДОДАНО:

1. ✅ Поле autoHideDelay (за замовчуванням 10 секунд) - можна змінити в Inspector
2. ✅ Метод ShowDialogueWithAutoHide() - показує текст і запускає таймер
3. ✅ Корутина AutoHideDialogue() - чекає 10 сек і ховає діалог
4. ✅ Скасування попереднього таймера якщо показується новий текст

ВАЖЛИВО:
Ваш DialogueUI має мати метод HideDialogue() для приховування.
Якщо у вас інша назва методу, змініть його в AutoHideDialogue().

Наприклад, якщо метод називається Close() або Hide(), замініть:
dialogueUI.HideDialogue() → dialogueUI.Close()
*/